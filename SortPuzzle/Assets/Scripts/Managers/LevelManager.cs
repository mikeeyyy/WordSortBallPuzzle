using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using static Ball;

[System.Serializable]
public class BallSpriteMapping
{
    public BallColor color;
    public Sprite sprite;
}

public class LevelManager : MonoBehaviour
{
    [Header("Level Data")]
    [SerializeField] List<LevelData> levels;

    [Header("Sprite Mappings")]
    [SerializeField] List<BallSpriteMapping> ballSpriteMappings;

    [Header("Scene References")]
    [SerializeField] Transform tubeHorizontalParent;
    [SerializeField] Transform tubeGridParent;

    private Dictionary<BallColor, Sprite> spriteMap;

    void Awake()
    {
        spriteMap = ballSpriteMappings.ToDictionary(mapping => mapping.color, mapping => mapping.sprite);
        GameEvents.OnLoadLevel += LoadLevel;
    }
    private void OnDestroy()
    {
        GameEvents.OnLoadLevel -= LoadLevel;
    }
    public void LoadLevel(int levelIndex)
    {
        ClearExistingLevel();

        if (levelIndex < 0 || levelIndex >= levels.Count)
        {
            Debug.LogError("Invalid level : " + levelIndex);
            return;
        }

        LevelData levelData = levels[levelIndex];
        Transform parent = GetParent(levelData);

        foreach (var tubeData in levelData.tubes)
        {
            GameObject tubeObj = ObjectPooler.Instance.SpawnFromPool("Tube", parent);
            TubeController tubeController = tubeObj.GetComponent<TubeController>();
            GameManager.Instance.RegisterTube(tubeController);

            var ballsToCreate = new List<BallController>();
            foreach (var ballColor in tubeData.balls)
            {
                GameObject ballObj = ObjectPooler.Instance.SpawnFromPool("Ball", null);
                BallController ballController = ballObj.GetComponent<BallController>();

                ballController.color = ballColor;
                ballController.SetSprite(spriteMap[ballColor]);
                ballsToCreate.Add(ballController);
            }
            tubeController.AddBalls(ballsToCreate);
        }
    }
    private void ClearExistingLevel()
    {
        List<TubeController> activeTubes = new List<TubeController>();
        activeTubes.AddRange(tubeHorizontalParent.GetComponentsInChildren<TubeController>());
        activeTubes.AddRange(tubeGridParent.GetComponentsInChildren<TubeController>());

        foreach (var tube in activeTubes)
        {
            tube.ClearBalls();
            ObjectPooler.Instance.ReturnToPool("Tube", tube.gameObject);
        }
    }
    private Transform GetParent(LevelData levelData)
    {
        GridLayoutGroup gridLayout = tubeGridParent.GetComponent<GridLayoutGroup>();
        HorizontalLayoutGroup horizontalLayout = tubeHorizontalParent.GetComponent<HorizontalLayoutGroup>();

        if (levelData.useGridLayout)
        {
            if (gridLayout != null) gridLayout.enabled = true;
            if (horizontalLayout != null) horizontalLayout.enabled = false;
            return tubeGridParent;
        }
        else
        {
            if (gridLayout != null) gridLayout.enabled = false;
            if (horizontalLayout != null) horizontalLayout.enabled = true;
            return tubeHorizontalParent;
        }
    }
}