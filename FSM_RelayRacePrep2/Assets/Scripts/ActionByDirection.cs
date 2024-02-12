using UnityEngine;

namespace Assets.Scripts
{
    public enum TriggerState { Enter, Exit, None};
    [CreateAssetMenu]
    public class ActionByDirection : ScriptableObject
    {
        public Direction Direction = Direction.Forward;
        public RaceState RaceState  = RaceState.Wait;
        public TriggerState TriggerState = TriggerState.Enter;

        /// <summary>
        /// Match on TriggerState and Direction to find 
        /// the matching ActionByDirection.
        /// </summary>
        /// <param name="match"></param>
        /// <returns>True if there is a match on the RaceState and TriggerState values.</returns>
        public bool IsMatch( TriggerState triggerState, Direction direction)
        {

            return triggerState == this.TriggerState && direction == this.Direction;    
        }

        public override string ToString() => $"Direction {Direction}, RaceState {RaceState}, TriggerState {TriggerState}";
    }
}
