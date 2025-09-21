using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using System.Linq;

public class AnimationController : MonoBehaviour
{
    public static AnimationController Instance { get; private set; }

    [Header("Animation Settings")]
    [SerializeField] float ballMoveDuration = 0.2f;
    [SerializeField] float animationStaggerDelay = 0.05f;
    [SerializeField] Ease moveEase = Ease.OutQuad;
    [SerializeField] float hoverHeight = 15f;
    [SerializeField] float hoverDuration = 0.7f;
    [SerializeField] float tubeTopYOffset;

    [Header("Scene References")]
    [SerializeField] Transform animationParent;
    [SerializeField] Transform heldBallPosition;

    private List<Tween> activeHoverTweens = new List<Tween>();

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        GameEvents.OnAnimateLiftBalls += AnimateLiftBalls;
        GameEvents.OnAnimatePlaceBalls += AnimatePlaceBalls;
        GameEvents.OnAnimateReturnBalls += AnimateReturnBalls;
    }

    private void OnDestroy()
    {
        GameEvents.OnAnimateLiftBalls -= AnimateLiftBalls;
        GameEvents.OnAnimatePlaceBalls -= AnimatePlaceBalls;
        GameEvents.OnAnimateReturnBalls -= AnimateReturnBalls;
    }

    private void AnimateLiftBalls(TubeController sourceTube, List<BallController> ballsToLift)
    {
        Debug.Log("AnimateLiftBalls");
        heldBallPosition.position = sourceTube.transform.position + new Vector3(0, tubeTopYOffset, 0);
        Sequence sequence = DOTween.Sequence();
        for (int i = ballsToLift.Count-1; i >=0; i--)
        {
            var ball = ballsToLift[i];
            ball.transform.SetParent(animationParent, true);
            Vector3 targetPos = heldBallPosition.position + new Vector3(0, i * (ball.GetComponent<RectTransform>().rect.height * 0.8f), 0);
            sequence.Insert(i * animationStaggerDelay, ball.transform.DOMove(targetPos, ballMoveDuration).SetEase(moveEase));
        }

        sequence.OnComplete(() =>
        {
            StartHoveringEffect(ballsToLift);
        });
    }

    private void AnimatePlaceBalls(TubeController sourceTube, TubeController destinationTube, List<BallController> balls)
    {
        Debug.Log("AnimatePlaceBalls");
        StopHoveringEffect();

        int ballsToMoveCount = Mathf.Min(balls.Count, destinationTube.GetEmptySlotCount());
        var ballsToMove = balls.Take(ballsToMoveCount).ToList();
        var ballsToReturn = balls.Skip(ballsToMoveCount).ToList();

        Sequence mainSequence = DOTween.Sequence();
        List<Vector3> finalPositions = destinationTube.GetWorldPositionsForSlots(ballsToMove.Count);

        for (int i = ballsToMove.Count-1; i >=-0 ; i--)
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
            List<Vector3> targetPositions = sourceTube.GetWorldPositionsForSlots(ballsToReturn.Count);
            for (int i = ballsToReturn.Count-1; i >=0; i--)
            {
                mainSequence.Insert(i * animationStaggerDelay, ballsToReturn[i].transform.DOMove(targetPositions[i], ballMoveDuration).SetEase(moveEase));
            }
        }

        mainSequence.OnComplete(() =>
        {
            destinationTube.AddBalls(ballsToMove);
            if (ballsToReturn.Count > 0)
            {
                sourceTube.AddBalls(ballsToReturn);
            }
            GameEvents.OnAnimationComplete?.Invoke();
        });
    }

    private void AnimateReturnBalls(TubeController sourceTube, List<BallController> ballsToReturn)
    {
        StopHoveringEffect();
        Sequence sequence = DOTween.Sequence();
        List<Vector3> targetPositions = sourceTube.GetWorldPositionsForSlots(ballsToReturn.Count);

        for (int i = ballsToReturn.Count-1; i >= 0; i--)
        {
            sequence.Insert(i * animationStaggerDelay, ballsToReturn[i].transform.DOMove(targetPositions[i], ballMoveDuration).SetEase(moveEase));
        }
        sequence.OnComplete(() =>
        {
            sourceTube.AddBalls(ballsToReturn);
            GameEvents.OnAnimationComplete?.Invoke();
        });
    }

    private void StartHoveringEffect(List<BallController> balls)
    {
        foreach (var ball in balls)
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
}