using System;


public static class GameEvents
{
    public static Action<TubeController> OnTubeSelected;

    public static Action<int> OnLoadLevel;

    public static Action OnReloadLevel;

    public static Action OnLoadNextLevel;

    public static Action OnLevelComplete;

    public static Action<TubeController> OnTubeCompleted;
}