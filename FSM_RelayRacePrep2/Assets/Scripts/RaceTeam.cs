using Assets.Scripts;
using UnityEngine;
using TMPro;
using UnityEditor.Search;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System;
using Random = UnityEngine.Random;

/// <summary>
/// The various direction states that the Race Team Member can move on the track 
/// which are Forward, Reverse, or no movement (Stop)
/// </summary>
public enum Direction { Forward, Reverse, Stop };

/// <summary>
/// Each RaceState indicates a stage in the race.
/// Wait - waiting for the start of the race
/// Start - start the race
/// Accelerate - continuously increase the running speed
/// Steady - hold the running speed 
/// Decelerate - slow down as quickly as possible
/// ReverseDirection - go back to the other end of the track. This may involve changing runners.
/// Uninitialized - Essentially no state.
/// </summary>
public enum RaceState { Wait, Start, Accelerate, Steady, Decelerate, ReverseDirection, Stop, Unintialized }


/// <summary>
/// Class manages a N-Member relay race team, which is typically four members.
/// There will be one runner at a time for each team. 
/// When the player reaches the other end a new runner will take their place.
/// The race ends when the last player reaches the StartFinish line.
/// </summary>
public class RaceTeam : MonoBehaviour
{

    /// <summary>
    /// Let the designer pick the base speed.
    /// </summary>  
    [Range(2f, 100f)]
    [SerializeField] float initialSpeed = 2.0f;

    /// <summary>
    /// Let the designer pick the amount to increment (boost) 
    /// the runners speed by when accelerating.
    /// </summary>
    [SerializeField] float accelarationIncrement = .01f;

    /// <summary>
    /// Maximum speed that the runner can reach
    /// </summary>
    [SerializeField] float maxSpeed = 10;

    /// <summary>
    /// Direct reference to the child TextMeshPro Text item used to set the 
    /// number of the runner.
    /// </summary>
    [SerializeField] TMPro.TMP_Text runnerLabel;

    [SerializeField] TMP_Text timerLabel;

    [SerializeField] string TeamName;

    Stopwatch? timer;

    [SerializeField]
    [Range(1, 20)]
    int numberOfRunners;

    /// <summary>
    /// The current speed.
    /// </summary>
    float speed = 1.0f;

    int currentRunner = 0;

    /// <summary>
    /// The stage of the race. At the beginning of the race and at each end the runners are waiting.
    /// </summary>
    RaceState nextRaceState = RaceState.Wait;

    /// <summary>
    /// Which direction is the running moving: forward, reverse, or stop (no movement)
    /// This variable along with speed and accelerationIncrement is used to control the amount and
    /// direction of movement. The race starts in the forward direction.
    /// </summary>
    Direction direction = Direction.Forward;

    
    /// <summary>
    /// Runner Energy 
    /// </summary>
    [SerializeField]
    [Range(0f, 10f)]
    float MaxEnergy = 10f;

    float Energy = 0f;

    /// <summary>
    /// Energy burn
    /// </summary>
    [SerializeField]
    [Range(0f, 20f)]
    float EnergyBurn = 8f;


    float BurnRate = 0f;

    float DistTravel = 0f;

    private void Start()
    {
        GenerateTeam();
        speed = initialSpeed;
        Energy = MaxEnergy;
        BurnRate = EnergyBurn / speed;
    }
    /// <summary>
    /// The state machine is used to largely control the movement values based on the RaceState and Direction.
    /// </summary>
    void Update()
    {
        if(timer!= null)
        {
            var time = Math.Round(timer.Elapsed.TotalSeconds, 3);
            timerLabel.text = $"Time: {time}";
        }
        /*
        State Machine needs to change from 
        Wait to 
        Start with initial speed to 
        Accelerate with an increasing speed up to a max speed to
        Steady maintaing the same speed to
        Decelerate slowing down to (make sure that the speed is never lower than the initial speed)
        Reverse Direction

        In ReverseDirection if there are more runners on the team then 
        the next runner takes over and we see their jersey number on the cube.
        If the last runner gets to the ReverseDirection state then the race is over.

        Assume that the team has an even number of runners.

        Implement all of this within the switch below. 
        Your code does not need to go anywhere else.
        You do not need to change anything else in any of the scripts.
        */



        Vector3 movement = Vector3.zero;
        var m = Mag(direction);



        switch (nextRaceState)
        {
            case RaceState.Stop:
                timer.Stop();


                break;
            case RaceState.Start:
                movement = new Vector3(0, 0, speed * m);
                break;
            case RaceState.Accelerate:
                
                if(speed < maxSpeed)
                {
                    speed += accelarationIncrement;
                }
                movement = new Vector3(0, 0, speed * m);

                break;
            case RaceState.Steady:

                speed = maxSpeed;

                movement = new Vector3(0, 0, speed * m);
                break;
            case RaceState.Decelerate:

                if (speed > initialSpeed)
                {
                    speed -= accelarationIncrement;
                }

                movement = new Vector3(0, 0, speed * m);

                break;
            case RaceState.ReverseDirection:

                

                if (direction == Direction.Forward)
                {
                    direction = Direction.Reverse;
                }
                else if (direction == Direction.Reverse)
                {
                    direction = Direction.Forward;
                }

                if(numberOfRunners >= 0 && currentRunner < numberOfRunners)
                {
                    currentRunner++;
                    runnerLabel.text = $"{currentRunner}";

                    DistTravel = 0f;
                    Energy = MaxEnergy;
                }

                movement = Vector3.zero;
                if(currentRunner != numberOfRunners)
                {
                    nextRaceState = RaceState.Start;
                }
                else
                {
                    nextRaceState = RaceState.Stop;
                }

                break;
            default:
                break;

        }
        Debug.Log($"Current State:{nextRaceState}");
        if (movement != Vector3.zero) 
        {
            float FrameDist = DistPerFrame();
            DistTravel += FrameDist;

            BurnRate = EnergyBurn / speed;
            float BurnedEnergy = BurnRate * FrameDist;

            Energy -= BurnedEnergy;

            Debug.Log($"Current Energy:{Energy}");

            if (Energy <= 0)
            {
                speed = 0f;
                nextRaceState = RaceState.Stop;
                return;
            }



            this.transform.Translate(movement * Time.deltaTime);
        }
    }

    /// <summary>
    /// Receives the Start Race button click to start the race.
    /// </summary>
    public void StartRace()
    {
        timer = new Stopwatch();

        var raceteams = GetTeams();

        Parallel.ForEach(raceteams, (team) =>
        {
            team.nextRaceState = RaceState.Start;
        });

        timer.Start();
        nextRaceState = RaceState.Start;
        Debug.Log($"Current Dir: {direction}");
    }


    public int Mag(Direction dir)
    {
        switch(dir)
        {
            case Direction.Forward: return 1;
            case Direction.Reverse: return -1;
            case Direction.Stop: return 0;
            default: return 0;
        }
    }

    public float DistPerFrame() => speed * Time.deltaTime;


    public RaceTeam[] GetTeams() => (RaceTeam[])Resources.FindObjectsOfTypeAll(typeof(RaceTeam));

    public void GenerateTeam()
    {
        initialSpeed = Random.Range(1.8f, 2.2f);
        accelarationIncrement = Random.Range(.008f, .015f);
    }


    /// <summary>
    /// The runner will encounter game objects used as triggers. 
    /// The triggers provide the transition to the next Race State.
    /// The next Race State is used by Update.
    /// Both OnTriggerEnter and OnTriggerExit call a common method.
    /// </summary>
    /// <param name="other">The object that the runner has triggered.</param>
    private void OnTriggerEnter(Collider other)
    {
            bool changedState =
            GetNextRaceState(other, TriggerState.Enter);
            if (changedState)
            {
                Debug.Log($"{this.name} has entered {other.name} and the race state has changed to {nextRaceState}");
            }
     

    }

    /// <summary>
    /// See the above explanation.
    /// </summary>
    /// <param name="other">The object that the runner has triggered.</param>
    private void OnTriggerExit(Collider other)
    {

        bool changedState =
        GetNextRaceState(other, TriggerState.Exit);
        if (changedState)
        {
            Debug.Log($"{this.name} has exited {other.name} and the race state has changed to {nextRaceState}");
        }
    }


    /// <summary>
    /// If a valid race state has been found then change the nextRaceState, class variable,
    /// to the value of the local variable if the found race state is different than 
    /// the current race state.
    /// 
    /// If a race state was not found then return false indicating that there is no change. 
    /// True otherwise.
    /// </summary>
    /// <param name="other">The trigger object</param>
    /// <param name="triggerState">What kind of trigger - Enter, Exit</param>
    /// <returns>True if there is a change in the state.</returns>
    private bool GetNextRaceState(Collider other, TriggerState triggerState)
    {
        RaceState raceState = RaceState.Unintialized;
        if (other.CompareTag("Barrier"))
        {
            BarrierDetection barrierDetection = other.GetComponent<BarrierDetection>();
            if (barrierDetection != null)
            {
                raceState = barrierDetection.GetNextRaceState(triggerState, direction);
                Debug.Log($"Next Race State is: {raceState}");
            }
        }

        // Only change next race state if it is valid and different from the previous state.
        if (raceState != RaceState.Unintialized && raceState != nextRaceState)
        {
            nextRaceState = raceState;
            return true;
        }
        else
        {
            return false;
        }
    }


}

