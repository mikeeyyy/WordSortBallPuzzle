using UnityEngine;
using static Ball;

[System.Serializable]
public class TubeData
{
    public BallColor[] balls;
}

[CreateAssetMenu(fileName = "Level_00", menuName = "Level/Create New Level")]
public class LevelData : ScriptableObject
{
    public bool useGridLayout = true;
    public TubeData[] tubes;
}