using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Animation Settings")]
    [SerializeField] float ballMoveDuration = 0.2f;
    [SerializeField] float animationStaggerDelay = 0.05f;
    [SerializeField] Ease moveEase = Ease.OutQuad;
    [SerializeField] float hoverHeight = 15f;
    [SerializeField] float hoverDuration = 0.7f;

    [Header("Selection Logic")]
    [SerializeField] Transform heldBallPosition;
    [SerializeField] float heldBallYOffset = 2.0f;

    [Header("Scene References")]
    [SerializeField] Transform animationParent;

    private TubeController selectedTube;
    private List<BallController> heldBalls = new List<BallController>();

    private int currentLevelIndex = 0;
    private bool isLevelComplete = false;
    private bool isAnimating = false;
    public float tubeTopYOffset = 2.5f;

    [SerializeField] LevelManager levelManager;
    [SerializeField] UIManager uiManager;
    private List<Tween> activeHoverTweens = new List<Tween>();

    [Header("Pfx Reference")]
    [SerializeField] Camera uiCamera;
    [SerializeField] GameObject particlePrefab;
    [SerializeField] Transform parent;

    [SerializeField] AudioClip tubeCompleteSfx;



    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    void Start()
    {
        LoadLevel(currentLevelIndex);
    }

    public void OnTubeSelected(TubeController tube)
    {
        if (isLevelComplete || isAnimating) return;

        if (selectedTube == null)
        {
            if (!tube.IsEmpty())
            {
                selectedTube = tube;
                heldBallPosition.position = tube.transform.position + new Vector3(0, heldBallYOffset, 0);
                StartCoroutine(AnimateAndRemoveBallsFromTube());
            }
        }
        else
        {
            if (tube == selectedTube)
            {
                StartCoroutine(AnimateAndReturnBalls());
            }
            else
            {
                int emptySlots = tube.GetEmptySlotCount();
                bool canPlace = tube.IsEmpty() || tube.GetTopBallColor() == heldBalls[0].color;
                if (emptySlots > 0 && canPlace)
                {
                    StartCoroutine(AnimateAndPlaceBallsInTube(tube, emptySlots));
                }
                else
                {
                    StartCoroutine(AnimateAndReturnBalls());
                }
            }
        }
    }

    private IEnumerator AnimateAndRemoveBallsFromTube()
    {
        isAnimating = true;

        heldBalls = selectedTube.RemoveTopBalls(selectedTube.GetTopBallBlock().Count);

        Sequence sequence = DOTween.Sequence();
        for (int i = 0; i < heldBalls.Count; i++)
        {
            var ball = heldBalls[i];
            ball.transform.SetParent(animationParent, true);
            Vector3 targetPos = heldBallPosition.position + new Vector3(0, i * (ball.GetComponent<RectTransform>().rect.height * 0.8f), 0);
            sequence.Append(ball.transform.DOMove(targetPos, ballMoveDuration).SetEase(moveEase));
            if (i < heldBalls.Count - 1) sequence.AppendInterval(animationStaggerDelay);
        }

        yield return sequence.WaitForCompletion();

        isAnimating = false;
        StartHoveringEffect();
    }

    private IEnumerator AnimateAndPlaceBallsInTube(TubeController destinationTube, int emptySlots)
    {
        StopHoveringEffect();
        isAnimating = true;

        int ballsToMoveCount = Mathf.Min(heldBalls.Count, emptySlots);
        var ballsToMove = heldBalls.Take(ballsToMoveCount).ToList();

        Sequence mainSequence = DOTween.Sequence();
        List<Vector3> finalPositions = destinationTube.GetWorldPositionsForSlots(ballsToMove.Count);

        for (int i = 0; i < ballsToMove.Count; i++)
        {
            var ball = ballsToMove[i];

            Vector3 tubeTopPosition = destinationTube.transform.position + new Vector3(0, tubeTopYOffset, 0);

            Sequence ballSequence = DOTween.Sequence();
            ballSequence.Append(ball.transform.DOMove(tubeTopPosition, ballMoveDuration).SetEase(moveEase));
            ballSequence.Append(ball.transform.DOMove(finalPositions[i], ballMoveDuration).SetEase(moveEase));

            mainSequence.Insert(i * animationStaggerDelay, ballSequence);
        }

        yield return mainSequence.WaitForCompletion();

        destinationTube.AddBalls(ballsToMove);

        var ballsToReturn = heldBalls.Skip(ballsToMoveCount).ToList();
        if (ballsToReturn.Count > 0)
        {
            StartCoroutine(AnimateAndReturnBalls());
        }
        else
        {
            heldBalls.Clear();
            selectedTube = null;
            isAnimating = false;
            CheckForWin();
        }
    }

    private IEnumerator AnimateAndReturnBalls()
    {
        StopHoveringEffect();
        isAnimating = true;

        Sequence sequence = DOTween.Sequence();
        List<Vector3> targetPositions = selectedTube.GetWorldPositionsForSlots(heldBalls.Count);

        for (int i = 0; i < heldBalls.Count; i++)
        {
            sequence.Append(heldBalls[i].transform.DOMove(targetPositions[i], ballMoveDuration).SetEase(moveEase));
            if (i < heldBalls.Count - 1) sequence.AppendInterval(animationStaggerDelay);
        }

        yield return sequence.WaitForCompletion();

        selectedTube.ReturnBallsToTop(heldBalls);
        heldBalls.Clear();
        selectedTube = null;
        isAnimating = false;
    }

    private void StartHoveringEffect()
    {
        foreach (var ball in heldBalls)
        {
            Vector3 startLocalPos = ball.transform.localPosition;
            Tween hoverTween = ball.transform.DOLocalMoveY(startLocalPos.y + hoverHeight, hoverDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
            activeHoverTweens.Add(hoverTween);
        }
    }

    private void StopHoveringEffect()
    {
        foreach (var tween in activeHoverTweens)
        {
            tween.Kill();
        }
        activeHoverTweens.Clear();
    }
    public void PlayEffectOnButton(RectTransform TubeRect)
    {
        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(null, TubeRect.position);

        Vector3 worldPos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            TubeRect, screenPos, uiCamera, out worldPos
        );

        Quaternion rot = Quaternion.Euler(new Vector3(-90,0,0));

        GameObject effect = Instantiate(particlePrefab, worldPos, rot);
        effect.transform.localScale = new Vector3(0.3f,0.3f,0.3f);

        ParticleSystem ps = effect.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            Destroy(effect, ps.main.duration + ps.main.startLifetime.constantMax);
        }
        else
        {
            Destroy(effect, 2f);
        }
    }

    void CheckForWin()
    {
        var allTubes = FindObjectsOfType<TubeController>();

        foreach (var tube in allTubes)
        {
            tube.SetInteractable(!tube.IsCompleteAndFull());
            if (tube.IsCompleteAndFull())
            {
                if(tube.GetIslocked()==true)
                {
                    PlayEffectOnButton(tube.GetComponent<RectTransform>());
                    tube.SetIslocked(false);
                    AudioManager.Instance.PlaySfxOnShot(tubeCompleteSfx);
                }
            }
        }
        if (allTubes.All(tube => tube.IsSolved()))
        {
            isLevelComplete = true;
            if (uiManager != null) uiManager.ShowNextLevelPanel("Victory");
        }
    }

    void LoadLevel(int index)
    {
        StopHoveringEffect();
        if (heldBalls.Count > 0)
        {
            foreach (var ball in heldBalls)
            {
                if (ball != null) Destroy(ball.gameObject);
            }
            heldBalls.Clear();
        }
        selectedTube = null;
        isAnimating = false;

        currentLevelIndex = index;
        levelManager.LoadLevel(currentLevelIndex);
        uiManager.UpdateLevelText(currentLevelIndex + 1);
        isLevelComplete = false;
        CheckForWin();
    }

    public void LoadNextLevel()
    {
        int nextLevel = currentLevelIndex + 1;
        if (nextLevel >= 5) nextLevel = 2;
        LoadLevel(nextLevel);
    }

    public void ReloadLevel()
    {
        LoadLevel(currentLevelIndex);
    }
}