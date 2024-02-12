using Assets.Scripts;
using UnityEngine;

public class BarrierDetection : MonoBehaviour
{
    [SerializeField] ActionByDirection[] actionsByDirection;// = new ActionByDirection[2];

    public RaceState GetNextRaceState(TriggerState triggerState, Direction direction)
    {
        RaceState nextRaceState = RaceState.Unintialized;
        // Find the action based on the trigger

        bool fnd = false;
        for (int i = 0; i < actionsByDirection.Length && !fnd; i++)
        {
            if (actionsByDirection[i].IsMatch(triggerState, direction))
            {
                fnd = true;
                nextRaceState = actionsByDirection[i].RaceState;
            }

        }
        return nextRaceState;
    }
}
