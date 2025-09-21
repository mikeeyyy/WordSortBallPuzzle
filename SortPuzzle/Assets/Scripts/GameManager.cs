using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    private enum GameState { Loading, Playing, Animating, LevelComplete }
    private GameState _currentState;
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
    [SerializeField] LevelManager levelManager;
    [SerializeField] UIManager uiManager;
 
    private List<Tween> activeHoverTweens = new List<Tween>();
    private List<TubeController> activeTubes = new List<TubeController>();
    private TubeController selectedTube;
    private List<BallController> heldBalls = new List<BallController>();

    private int currentLevelIndex = 0;
    public float tubeTopYOffset = 2.5f;



    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            GameEvents.OnTubeSelected += HandleTubeSelection;
            GameEvents.OnReloadLevel += ReloadLevel;
            GameEvents.OnLoadNextLevel += LoadNextLevel;
        }
    }
    private void OnDestroy()
    {
        GameEvents.OnTubeSelected -= HandleTubeSelection;
        GameEvents.OnReloadLevel -= ReloadLevel;
        GameEvents.OnLoadNextLevel -= LoadNextLevel;
    }
    void Start()
    {
        LoadLevel(currentLevelIndex);
    }

    public void RegisterTube(TubeController tube)
    {
        if (!activeTubes.Contains(tube)) activeTubes.Add(tube);
    }

    public void HandleTubeSelection(TubeController tube)
    {
        if (_currentState != GameState.Playing) return;

        if (selectedTube == null)
        {
            if (!tube.IsEmpty())
            {
                selectedTube = tube;
                heldBallPosition.position = tube.transform.position + new Vector3(0, heldBallYOffset, 0);
                AnimateAndRemoveBallsFromTube();
            }
        }
        else
        {
            if (tube == selectedTube)
            {
                AnimateAndReturnBalls();
            }
            else
            {
                int emptySlots = tube.GetEmptySlotCount();
                bool canPlace = tube.IsEmpty() || tube.GetTopBallColor() == heldBalls[0].color;
                if (emptySlots > 0 && canPlace)
                {
                    AnimateAndPlaceBallsInTube(tube, emptySlots);
                }
                else
                {
                    AnimateAndReturnBalls();
                }
            }
        }
    }

    private void AnimateAndRemoveBallsFromTube()
    {
        _currentState = GameState.Animating;
        heldBalls = selectedTube.RemoveTopBalls();

        Sequence sequence = DOTween.Sequence();
        for (int i = heldBalls.Count - 1; i >= 0; i--)
        {
            var ball = heldBalls[i];
            ball.transform.SetParent(animationParent, true);
            Vector3 targetPos = heldBallPosition.position + new Vector3(0, i * (ball.GetComponent<RectTransform>().rect.height * 0.8f), 0);

            int animationDelayIndex = (heldBalls.Count - 1) - i;
            sequence.Insert(animationDelayIndex * animationStaggerDelay, ball.transform.DOMove(targetPos, ballMoveDuration).SetEase(moveEase));
        }
        sequence.OnComplete(() =>
        {
            _currentState = GameState.Playing;
            StartHoveringEffect();
        });
    }

    private void  AnimateAndPlaceBallsInTube(TubeController destinationTube, int emptySlots)
    {
        _currentState = GameState.Animating;
        StopHoveringEffect();

        int ballsToMoveCount = Mathf.Min(heldBalls.Count, emptySlots);
        var ballsToMove = heldBalls.Take(ballsToMoveCount).ToList();
        var ballsToReturn = heldBalls.Skip(ballsToMoveCount).ToList();

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

        if (ballsToReturn.Count > 0)
        {
            List<Vector3> targetPositions = selectedTube.GetWorldPositionsForSlots(ballsToReturn.Count);
            for (int i = 0; i < ballsToReturn.Count; i++)
            {
                mainSequence.Insert(i * animationStaggerDelay, ballsToReturn[i].transform.DOMove(targetPositions[i], ballMoveDuration).SetEase(moveEase));
            }
        }

        mainSequence.OnComplete(() =>
        {
            destinationTube.AddBalls(ballsToMove);
            if (ballsToReturn.Count > 0)
            {
                selectedTube.AddBalls(ballsToReturn);
            }

            ResetSelection();
            CheckForWin();
        });
    }
    
    private void AnimateAndReturnBalls()
    {
        _currentState = GameState.Animating;
        StopHoveringEffect();

        Sequence sequence = DOTween.Sequence();
        List<Vector3> targetPositions = selectedTube.GetWorldPositionsForSlots(heldBalls.Count);

        for (int i = heldBalls.Count - 1; i >= 0; i--)
        {
            sequence.Insert(i * animationStaggerDelay, heldBalls[i].transform.DOMove(targetPositions[i], ballMoveDuration).SetEase(moveEase));
        }
        sequence.OnComplete(() =>
        {
            selectedTube.AddBalls(heldBalls);
            ResetSelection();
        });
    }
    void CheckForWin()
    {
        bool allTubesSolved = true;
        foreach (var tube in activeTubes)
        {
            tube.UpdateTubeState();
            if (!tube.IsSolved())
            {
                allTubesSolved = false;
            }
        }

        if (allTubesSolved)
        {
            _currentState = GameState.LevelComplete;
            GameEvents.OnLevelComplete?.Invoke();
        }
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
    private void ResetSelection()
    {
        heldBalls.Clear();
        selectedTube = null;
        _currentState = GameState.Playing;
    }

    private void ClearLevel()
    {
        DOTween.KillAll();
        activeTubes.Clear();
        StopHoveringEffect();
        heldBalls.ForEach(ball => ball.gameObject.SetActive(false));
        ResetSelection();
    }
    void LoadLevel(int index)
    {
        _currentState = GameState.Loading;
        ClearLevel();
        currentLevelIndex = index;

        GameEvents.OnLoadLevel?.Invoke(currentLevelIndex);

        _currentState = GameState.Playing;
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