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

    [Header("References")]
    [SerializeField] Transform ballContainer;
    [SerializeField] private Button tubeButton;

    [SerializeField] AudioClip tubeSfx, ballPlaceSfx;

    private List<BallController> balls = new List<BallController>();
    bool locked=true;
    void Start()
    {
        tubeButton.onClick.AddListener(OnTubeClicked);
    }

    public void SetIslocked(bool value)
        { locked = value; }
    public bool GetIslocked()   
        { return locked; }
    private void PositionBalls()
    {
        for (int i = 0; i < balls.Count; i++)
        {
            int visualIndex = i;
            balls[i].GetComponent<RectTransform>().anchoredPosition = bottomOffset + new Vector2(0, visualIndex * ballSpacing);
          
        }
    }

    public void OnTubeClicked()
    {
        AudioManager.Instance.PlaySfxOnShot(tubeSfx);
        GameManager.Instance.OnTubeSelected(this);
    }
    public void SetInteractable(bool isInteractable)
    {
        if (tubeButton != null)
        {
            tubeButton.interactable = isInteractable;
        }
    }

    public void AddBalls(List<BallController> newBalls)
    {
        foreach (var ball in newBalls)
        {
            balls.Add(ball); 
        }
        foreach (var ball in newBalls)
        {
            ball.transform.SetParent(ballContainer, false);
            ball.transform.localScale = Vector3.one;
        }
        PositionBalls();
    }

    public List<BallController> RemoveTopBalls(int count)
    {
        if (count <= 0 || IsEmpty()) return new List<BallController>();

        int removalCount = Mathf.Min(count, balls.Count);

        List<BallController> removedBalls = balls.GetRange(balls.Count - removalCount, removalCount);
        balls.RemoveRange(balls.Count - removalCount, removalCount);
        PositionBalls();

        return removedBalls;
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
        if (IsEmpty())
        {
            return true;
        }

        if (balls.Count < tubeCapacity)
        {
            return false;
        }

        BallColor firstColor = balls[0].color;
        return balls.All(ball => ball.color == firstColor);
    }
    public bool IsCompleteAndFull()
    {
        if (balls.Count < tubeCapacity) return false;

        BallColor firstColor = balls[0].color;
        return balls.All(ball => ball.color == firstColor);
    }
}