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

    [Header("Selection Logic")]
    [SerializeField] Transform heldBallPosition;

    [Header("Scene References")]
    [SerializeField] Transform animationParent;
    [SerializeField] LevelManager levelManager;
    [SerializeField] UIManager uiManager;
 
    private List<TubeController> activeTubes = new List<TubeController>();

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
            GameEvents.OnReloadLevel += ReloadLevel;
            GameEvents.OnLoadNextLevel += LoadNextLevel;
            GameEvents.OnAnimationComplete += HandleAnimationComplete;
            GameEvents.OnAnimatePlaceBalls += (tubes, to, balls) => SetState(GameState.Animating);
            GameEvents.OnAnimateReturnBalls += (tubes, balls) => SetState(GameState.Animating);
        }
    }
    private void OnDestroy()
    {   
        GameEvents.OnReloadLevel -= ReloadLevel;
        GameEvents.OnLoadNextLevel -= LoadNextLevel;
        GameEvents.OnAnimationComplete -= HandleAnimationComplete;
        GameEvents.OnAnimatePlaceBalls -= (tubes, to, balls) => SetState(GameState.Animating);
        GameEvents.OnAnimateReturnBalls -= (tubes, balls) => SetState(GameState.Animating);
    }
    void Start()
    {
        LoadLevel(currentLevelIndex);
    }
    private void SetState(GameState newState)
    {
        _currentState = newState;
    }

    public void RegisterTube(TubeController tube)
    {
        if (!activeTubes.Contains(tube)) activeTubes.Add(tube);
    }

    private void HandleAnimationComplete()
    {
        CheckForWin();
        if (_currentState != GameState.LevelComplete)
        {
            SetState(GameState.Playing);
        }
    }

    void CheckForWin()
    {
        foreach (var tube in activeTubes)
        {
            tube.UpdateTubeState();
        }
        if (activeTubes.TrueForAll(tube => tube.IsSolved()))
        {
            SetState(GameState.LevelComplete);
            GameEvents.OnLevelComplete?.Invoke();
        }
    }

    private void ClearLevel()
    {
        DOTween.KillAll();
        activeTubes.Clear();
    }

    void LoadLevel(int index)
    {
        SetState(GameState.Loading);
        ClearLevel();
        currentLevelIndex = index;
        GameEvents.OnLoadLevel?.Invoke(currentLevelIndex);
        SetState(GameState.Playing);
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
    public bool IsPlaying() => _currentState == GameState.Playing;
}