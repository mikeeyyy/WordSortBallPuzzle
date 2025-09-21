using System;
using System.Collections.Generic;


public static class GameEvents
{
    public static Action<TubeController> OnTubeSelected;
    public static Action<int> OnLoadLevel;
    public static Action OnReloadLevel;
    public static Action OnLoadNextLevel;
    public static Action OnLevelComplete;
    public static Action<TubeController> OnTubeCompleted;

    public static Action<TubeController, List<BallController>> OnAnimateLiftBalls;
    public static Action<TubeController, TubeController, List<BallController>> OnAnimatePlaceBalls;
    public static Action<TubeController, List<BallController>> OnAnimateReturnBalls;
    public static Action OnAnimationComplete;
}