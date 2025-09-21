using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static Ball;

public class TubeController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] int tubeCapacity = 4;

    [Header("Manual Layout Settings")]
    [SerializeField] Vector2 bottomOffset;
    [SerializeField] float ballSpacing = 50f;

    [Header("Scene References")]
    [SerializeField] Transform ballContainer;
    [SerializeField] Button tubeButton;
    [SerializeField] GameObject particlePrefab;

    [SerializeField] AudioClip tubeSfx, ballPlaceSfx, tubeCompleteSfx;

    Camera uiCamera;
    private List<BallController> balls = new List<BallController>();
    public bool IsLocked { get; private set; } = true;


    private void Awake()
    {
        uiCamera= Camera.main;
        GameEvents.OnTubeCompleted += HandleTubeCompletion;
    }
    private void OnDestroy()
    {
        GameEvents.OnTubeCompleted -= HandleTubeCompletion;
    }
    void Start()
    {
        tubeButton.onClick.AddListener(OnTubeClicked);
    }
    public void OnTubeClicked()
    {
        if (!GameManager.Instance.IsPlaying())
        {
            return;
        }
        AudioManager.Instance.PlaySfxOnShot(tubeSfx);
        GameEvents.OnTubeSelected?.Invoke(this);
    }
    private void HandleTubeCompletion(TubeController completedTube)
    {
        if (completedTube == this && IsLocked)
        {
            RectTransform tubeRectTransform = completedTube.GetComponent<RectTransform>();
            PlayEffectOnButton(tubeRectTransform);
            IsLocked = false;
        }
    }
    public void PlayEffectOnButton(RectTransform tubeRect)
    {
        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(null, tubeRect.position);

        Vector3 worldPos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            tubeRect, screenPos, uiCamera, out worldPos
        );

        Quaternion rot = Quaternion.Euler(new Vector3(-90, 0, 0));

        GameObject effect = Instantiate(particlePrefab, worldPos, rot);
        effect.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

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
    private void PositionBalls()
    {
        for (int i = 0; i < balls.Count; i++)
        {
            balls[i].GetComponent<RectTransform>().anchoredPosition = bottomOffset + new Vector2(0, i * ballSpacing);
        }
    }
    public void UpdateTubeState()
    {
        bool isComplete = IsCompleteAndFull();
        tubeButton.interactable = !isComplete;

        if (isComplete && IsLocked)
        {
            GameEvents.OnTubeCompleted?.Invoke(this);
        }
    }
    public void ClearBalls()
    {
        foreach (var ball in balls)
        {
            ObjectPooler.Instance.ReturnToPool("Ball", ball.gameObject);
        }
        balls.Clear();
        IsLocked = true;
    }
    public void AddBalls(List<BallController> newBalls)
    {
        foreach (var ball in newBalls)
        {
            balls.Add(ball);
            ball.transform.SetParent(ballContainer, false);
            ball.transform.localScale = Vector3.one;
        }
            PositionBalls();
    }
    public void SetInteractable(bool isInteractable)
    {
        if (tubeButton != null)
        {
            tubeButton.interactable = isInteractable;
        }
    }
    public List<BallController> RemoveTopBalls()
    {
        if ( IsEmpty()) return new List<BallController>();

        var topBallBlock = GetTopBallBlock();

        balls.RemoveRange(balls.Count - topBallBlock.Count, topBallBlock.Count);
        PositionBalls();

        return topBallBlock;
    }
    public List<BallController> GetTopBallBlock()
    {
        if (IsEmpty()) return new List<BallController>();

        var block = new List<BallController>();
        BallColor topColor = GetTopBallColor();

        for (int i = balls.Count - 1; i >= 0; i--)
        {
            if (balls[i].color == topColor)
            {
                block.Add(balls[i]);
            }
            else
            {
                break;
            }
        }
        block.Reverse();
        return block;
    }
    public void ReturnBallsToTop(List<BallController> returnedBalls)
    {
        balls.AddRange(returnedBalls);

        foreach (var ball in returnedBalls)
        {
            ball.transform.SetParent(ballContainer, false);
            ball.transform.localScale = Vector3.one;
        }
        PositionBalls();
    }

    public void AddBallForLevelSetup(BallController ball)
    {
        ball.transform.SetParent(ballContainer, false);
        ball.transform.localScale = Vector3.one;
        balls.Add(ball);
        PositionBalls();
    }

    public List<Vector3> GetWorldPositionsForSlots(int count)
    {
        var positions = new List<Vector3>();
        for (int i = 0; i < count; i++)
        {
            int slotIndex = balls.Count + i;
            if (slotIndex >= tubeCapacity) break;
            Vector2 localPos = bottomOffset + new Vector2(0, slotIndex * ballSpacing);
            Vector3 worldPos = ballContainer.TransformPoint(localPos);
            positions.Add(worldPos);
        }
        return positions;
    }

    public bool IsEmpty() => balls.Count == 0;
    public int GetEmptySlotCount() => tubeCapacity - balls.Count;

    public BallColor GetTopBallColor()
    {
        if (IsEmpty()) return (BallColor)(-1);
        return balls[balls.Count - 1].color;
    }

    public bool IsSolved()
    {
        return IsEmpty() || IsCompleteAndFull();
    }
    public bool IsCompleteAndFull()
    {
        if (balls.Count < tubeCapacity) return false;

        BallColor firstColor = balls[0].color;
        return balls.All(ball => ball.color == firstColor);
    }

}