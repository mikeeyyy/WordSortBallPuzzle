using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static Ball;

public class TubeController : MonoBehaviour
{
    [Header("Settings")]
    public int tubeCapacity = 4;

    [Header("Manual Layout Settings")]
    public Vector2 bottomOffset;
    public float ballSpacing = 50f;

    [Header("References")]
    [SerializeField] Transform ballContainer;
    [SerializeField] private Button tubeButton;
    private List<BallController> balls = new List<BallController>();

    void Start()
    {
        if (tubeButton != null)
        {
            tubeButton.onClick.AddListener(OnTubeClicked);
        }
    }
    private void PositionBalls()
    {
        for (int i = 0; i < balls.Count; i++)
        {
            int visualIndex = balls.Count - 1 - i;
            balls[i].GetComponent<RectTransform>().anchoredPosition = bottomOffset + new Vector2(0, visualIndex * ballSpacing);
        }
    }
    public void OnTubeClicked()
    {
        GameManager.Instance.OnTubeSelected(this);
    }
    public void AddBalls(List<BallController> newBalls)
    {
        foreach (var ball in newBalls)
        {
            ball.transform.SetParent(ballContainer, false);
            balls.Add(ball); 
        }
        PositionBalls();
    }

    public void RemoveTopBalls(int count)
    {
        if (count > balls.Count || count <= 0) return;

        balls.RemoveRange(0, count);
        PositionBalls();
    }
    public void ReturnBallsToTop(List<BallController> returnedBalls)
    {
        foreach (var ball in returnedBalls)
        {
            ball.transform.SetParent(ballContainer, false);
        }
        balls.InsertRange(0, returnedBalls);
        PositionBalls();
    }
    public void AddBallForLevelSetup(BallController ball)
    {
        ball.transform.SetParent(ballContainer, false);
        balls.Add(ball); 
        PositionBalls();
    }
    public List<BallController> GetTopBallBlock()
    {
        if (IsEmpty()) return new List<BallController>();

        var block = new List<BallController>();
        BallColor topColor = GetTopBallColor();
        for (int i = 0; i < balls.Count; i++)
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
        return block;
    }

    public bool IsEmpty() => balls.Count == 0;
    public int GetEmptySlotCount() => tubeCapacity - balls.Count;

    public BallColor GetTopBallColor()
    {
        if (IsEmpty()) return (BallColor)(-1); 
        return balls[0].color; 
    }

    public bool IsSolved()
    {
        if (balls.Count < tubeCapacity) return false;

        BallColor firstColor = balls[0].color;
        return balls.All(ball => ball.color == firstColor);
    }
}