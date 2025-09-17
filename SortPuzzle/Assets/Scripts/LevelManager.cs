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

    [Header("Prefabs")]
    [SerializeField] GameObject tubePrefab;
    [SerializeField] GameObject ballPrefab;

    [Header("Sprite Mappings")]
    [SerializeField] List<BallSpriteMapping> ballSpriteMappings;

    [Header("Scene References")]
    [SerializeField] Transform tubeHorizontalParent;
    [SerializeField] Transform tubeGridParent;

    private Dictionary<BallColor, Sprite> spriteMap;

    void Awake()
    {
        spriteMap = ballSpriteMappings.ToDictionary(mapping => mapping.color, mapping => mapping.sprite);
    }

    public void LoadLevel(int levelIndex)
    {
        foreach (Transform child in tubeHorizontalParent)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in tubeGridParent)
        {
            Destroy(child.gameObject);
        }

        if (levelIndex < 0 || levelIndex >= levels.Count)
        {
            Debug.LogError("Invalid level index requested: " + levelIndex);
            return;
        }

        LevelData levelData = levels[levelIndex];
        Transform parent = GetParent(levelData);

        foreach (var tubeData in levelData.tubes)
        {
            GameObject tubeObj = Instantiate(tubePrefab, parent);
            tubeObj.transform.DOScaleY(1f, 0.5f).SetEase(Ease.OutBounce);
            TubeController tubeController = tubeObj.GetComponent<TubeController>();

            if (tubeData.balls == null) continue;

            foreach (var ballColor in tubeData.balls)
            {
                GameObject ballObj = Instantiate(ballPrefab);
                BallController ballController = ballObj.GetComponent<BallController>();

                ballController.color = ballColor;

                ballController.SetSprite(spriteMap[ballColor]);

                tubeController.AddBallForLevelSetup(ballController);
            }
        }
    }
    Transform GetParent(LevelData levelData)
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