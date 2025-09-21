using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance { get; private set; }

    private TubeController selectedTube;
    private List<BallController> heldBalls = new List<BallController>();

    private bool IsSelectionLocked => heldBalls.Count > 0;
    private TubeController _nextTubeToSelect = null;
    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        GameEvents.OnTubeSelected += HandleTubeSelection;
        GameEvents.OnAnimationComplete += OnMoveComplete;
    }

    private void OnDestroy()
    {
        GameEvents.OnTubeSelected -= HandleTubeSelection;
        GameEvents.OnAnimationComplete -= OnMoveComplete;
    }

    private void HandleTubeSelection(TubeController tube)
    {
        if (!GameManager.Instance.IsPlaying())
        {
            return; 
        }
        if (!IsSelectionLocked)
        {
            if (!tube.IsEmpty())
            {
                selectedTube = tube;
                heldBalls = selectedTube.RemoveTopBalls();
                Debug.Log("HandleTubeSelection");
                GameEvents.OnAnimateLiftBalls?.Invoke(selectedTube, heldBalls);
            }
        }
        else
        {
            if (tube == selectedTube)
            {
                GameEvents.OnAnimateReturnBalls?.Invoke(selectedTube, heldBalls);
            }
            else
            {
                int emptySlots = tube.GetEmptySlotCount();
                bool canPlace = tube.IsEmpty() || tube.GetTopBallColor() == heldBalls[0].color;

                if (emptySlots > 0 && canPlace)
                {
                    GameEvents.OnAnimatePlaceBalls?.Invoke(selectedTube, tube, heldBalls);
                }
                else
                {
                    _nextTubeToSelect = tube;
                    GameEvents.OnAnimateReturnBalls?.Invoke(selectedTube, heldBalls);
                }
            }
        }
    }

    private void OnMoveComplete()
    {
        heldBalls.Clear();
        selectedTube = null;
        if (_nextTubeToSelect != null)
        {
            var tubeToSelect = _nextTubeToSelect;
            _nextTubeToSelect = null;

            if (!tubeToSelect.IsEmpty())
            {
                selectedTube = tubeToSelect;
                heldBalls = selectedTube.RemoveTopBalls();
                GameEvents.OnAnimateLiftBalls?.Invoke(selectedTube, heldBalls);
            }
        }
    }
}
