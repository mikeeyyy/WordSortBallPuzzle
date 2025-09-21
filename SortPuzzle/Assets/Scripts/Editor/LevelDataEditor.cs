#if UNITY_EDITOR

using System.Collections.Generic;
using static Ball;
[UnityEditor.CustomEditor(typeof(LevelData))]

public class LevelDataEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        UnityEditor.EditorGUILayout.Space();
        UnityEditor.EditorGUILayout.LabelField("Level Validator", UnityEditor.EditorStyles.boldLabel);

        LevelData levelData = (LevelData)target;
        const int TUBE_CAPACITY = 4;

        if (levelData.tubes == null || levelData.tubes.Length == 0) return;

        if (!levelData.useGridLayout && levelData.tubes.Length > 4)
        {
            UnityEditor.EditorGUILayout.HelpBox(
                "Horizontal layout only supports a maximum of 4 tubes. This level has " + levelData.tubes.Length + ".",
                UnityEditor.MessageType.Error);
        }

        var ballCounts = new Dictionary<BallColor, int>();
        int emptyTubes = 0;

        foreach (var tube in levelData.tubes)
        {
            if (tube.balls == null || tube.balls.Length == 0)
            {
                emptyTubes++;
                continue;
            }
            foreach (var ball in tube.balls)
            {
                if (!ballCounts.ContainsKey(ball)) ballCounts[ball] = 0;
                ballCounts[ball]++;
            }
        }

        UnityEditor.EditorGUILayout.LabelField("Ball Counts:");

        bool isPossible = true;

        foreach (var pair in ballCounts)
        {
            if (pair.Value != TUBE_CAPACITY)
            {
                UnityEditor.EditorGUILayout.HelpBox($"{pair.Key}: {pair.Value} (Error: Should be {TUBE_CAPACITY})", UnityEditor.MessageType.Error);
                isPossible = false;
            }
        }
        if (isPossible)
        {
            if (ballCounts.Count > 0 && emptyTubes != 0)
            {
                UnityEditor.EditorGUILayout.HelpBox("All ball counts are correct. The level is possible.", UnityEditor.MessageType.Info);
            }
            if (levelData.recommendEmptyTubes && emptyTubes == 0)
            {
                UnityEditor.EditorGUILayout.HelpBox($"Zero empty tubes. (Warning: At least 1 or 2 are recommended for solvability)", UnityEditor.MessageType.Warning);
            }
        }
    }
}
#endif