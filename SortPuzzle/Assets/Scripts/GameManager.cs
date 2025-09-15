using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    private int currentLevelIndex = 0;
    private bool isLevelComplete = false;
    private bool isAnimating = false;

    [Header("Selection Logic")]
    private TubeController selectedTube;
    private List<BallController> heldBalls = new List<BallController>();
    [SerializeField] float heldBallYOffset = 2.0f;
    [SerializeField] Transform heldBallPosition;

    [Header("Managers")]
    [SerializeField]private LevelManager levelManager;
    [SerializeField]private UIManager uiManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
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
                heldBalls = selectedTube.GetTopBallBlock();
                heldBallPosition.position = tube.transform.position + new Vector3(0, heldBallYOffset, 0);

                StartCoroutine(AnimateAndRemoveBallsFromTube());
            }
        }
        else
        {
            if (tube == selectedTube)
            {
                selectedTube.ReturnBallsToTop(heldBalls);
                heldBalls.Clear();
                selectedTube = null;
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
                    StartCoroutine(AnimateAndPlaceBallsInTube(selectedTube, selectedTube.GetEmptySlotCount()));
                }
            }
        }
    }

    private IEnumerator AnimateAndRemoveBallsFromTube()
    {
        isAnimating = true;
        foreach (var ball in heldBalls)
        {
            ball.transform.SetParent(heldBallPosition, true);
        }
        selectedTube.RemoveTopBalls(heldBalls.Count);
        yield return new WaitForSeconds(0.2f);
        isAnimating = false;
    }

    private IEnumerator AnimateAndPlaceBallsInTube(TubeController destinationTube, int emptySlots)
    {
        isAnimating = true;

        int ballsToMoveCount = Mathf.Min(heldBalls.Count, emptySlots);
        var ballsToMove = heldBalls.Take(ballsToMoveCount).ToList();
        var ballsToReturn = heldBalls.Skip(ballsToMoveCount).ToList();

        if (ballsToMove.Count > 0)
        {
            destinationTube.AddBalls(ballsToMove);
        }

        if (ballsToReturn.Count > 0)
        {
            selectedTube.AddBalls(ballsToReturn);
        }

        heldBalls.Clear();
        selectedTube = null;

        yield return new WaitForSeconds(0.2f);
        isAnimating = false;
        CheckForWin();
    }

    void CheckForWin()
    {
        if (FindObjectsOfType<TubeController>().All(tube => tube.IsSolved()))
        {
            isLevelComplete = true;
            uiManager.ShowNextLevelButton();
        }
    }

    void LoadLevel(int index)
    {
        currentLevelIndex = index;
        levelManager.LoadLevel(currentLevelIndex);
        uiManager.UpdateLevelText(currentLevelIndex + 1);
        isLevelComplete = false;
    }

    public void LoadNextLevel()
    {
        int nextLevel = currentLevelIndex + 1;
        if (nextLevel >= 5)
        {
            nextLevel = 2;
        }
        LoadLevel(nextLevel);
        if (uiManager.nextLevelButton != null)
        {
            uiManager.nextLevelButton.gameObject.SetActive(false);
        }
    }

    public void ReloadLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}