using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using static Ball;

[CustomEditor(typeof(LevelData))]
public class LevelDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Level Validator", EditorStyles.boldLabel);

        LevelData levelData = (LevelData)target;
        const int TUBE_CAPACITY = 4;

        if (levelData.tubes == null || levelData.tubes.Length == 0) return;
        
        if (!levelData.useGridLayout && levelData.tubes.Length > 4)
        {
            EditorGUILayout.HelpBox(
                "Horizontal layout only supports a maximum of 4 tubes. This level has " + levelData.tubes.Length + ".",
                MessageType.Error);
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

        EditorGUILayout.LabelField("Ball Counts:");
        foreach (var pair in ballCounts)
        {
            if (pair.Value != TUBE_CAPACITY)
            {
                EditorGUILayout.HelpBox($"{pair.Key}: {pair.Value} (Warning: Should be {TUBE_CAPACITY}!!!!!!!!)", MessageType.Warning);
            }
        }

        if (emptyTubes < 2)
        {
            EditorGUILayout.HelpBox($"Only {emptyTubes} empty tubes. (Warning: At least 2 are recommended!!!!!!!!)", MessageType.Warning);
        }
    }
}