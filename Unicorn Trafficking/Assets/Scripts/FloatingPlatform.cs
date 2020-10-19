using System;
using UnityEngine;

[SelectionBase]

public class FloatingPlatform : MonoBehaviour
{
    
    private enum MovementType
    {
        NotSmooth,
        QuiteSmooth,
        VerySmooth,
    }
    [Header("    Waypoint Stuff")]
    [SerializeField] private MovementType smoothing;

    [Serializable] public struct PlatformWaypoint
    {
        public Vector3 position;
        public float waitTime;
    }
    [SerializeField] public PlatformWaypoint[] waypoints;
    
    public enum LoopType
    {
        LoopBackToTheStart,
        MoveBackAndForth,
    }
    [Space(2), Header("    Movement Stuff")]
    [SerializeField] public LoopType movementStyle;

    [SerializeField, Range(0.1f, 5.0f),
     Tooltip("AVERAGE speed of the platform. Smoothing will make it slower at the start/end and faster in the middle.")]
    private float movementSpeed = 2.0f;
    
    [Space(2), Header("    Size and Appearance")]    
    [SerializeField, Tooltip("How broad the platform is.")] public Vector2 platformSize = Vector3.one;
    [SerializeField, Tooltip("How deep the platform is.")] public float platformHeight = 1.0f;
    [Space(2), SerializeField, Tooltip("A child object that represents the platform visually.")] public GameObject platformVisual;
    

    private Rigidbody rbody;
    private Collider coll;
    private GameObject player;
    
    
    private int incrementOrDecrement = 1;
    private int currentIndex = 0;
    private int nextIndex = 0;
    
    private float waitTimer = 0.0f;

    private float journeyDistance;
    private float fractionTimeElapsed;
    private float moveDuration;

    private bool playerIsOnPlatform;
    
    private void Awake()
    {
        rbody = GetComponent<Rigidbody>();
        coll = GetComponent<Collider>();
        player = GameObject.FindWithTag("Player"); // Sorry. So sorry.
        journeyDistance = Vector3.Distance(waypoints[0].position, waypoints[1].position);
        moveDuration = journeyDistance / movementSpeed;
    }

#if UNITY_EDITOR
    public void MakeWaypointsArraySafe()
    {
        if (waypoints != null && waypoints.Length >= 2) return;

        var position = transform.position;
        var forward = transform.forward;
        
        waypoints = new PlatformWaypoint[2];
        for (int i = 0; i < waypoints.Length; i++)
        {
            waypoints[i].position = position + (i * 3.0f * forward);
            waypoints[i].waitTime = 1.0f;
        }
    }
#endif


    private void FixedUpdate()
    {
        Vector3 playerPos = player.transform.position;
        Vector3 platformPosition = coll.ClosestPointOnBounds(playerPos);
        Vector3 tolerancePosition = (playerPos - platformPosition).normalized;
        float playerIsAbovePlatform = Vector3.Dot(transform.up, tolerancePosition);
        coll.enabled = playerIsAbovePlatform > -0.01f;

        if (WaitForTimer()) return;
        
        var currentDistance = Vector3.Distance(transform.position, waypoints[nextIndex].position);
        if (currentDistance < 0.01f)
        {
            SwitchTarget();
            return;
            
        }
        PerformMove();
    }
    
    private bool WaitForTimer()
    {
        waitTimer -= Time.deltaTime;
        return waitTimer > 0.0f;
    }

    private void SwitchTarget()
    {
        fractionTimeElapsed = 0.0f;
        currentIndex = nextIndex;
        // First, move us to the *precise* location of the waypoint.
        rbody.MovePosition(waypoints[currentIndex].position);
        
        // Set up the timer for the next move.
        waitTimer = waypoints[currentIndex].waitTime;
        
        // Establish where we're going next.
        int nextIndexCandidate = currentIndex + incrementOrDecrement;
        if (nextIndexCandidate < 0 || nextIndexCandidate > waypoints.Length - 1)
        {
            // NextIndexCandidate is out of bounds. Make the right choice:
            switch (movementStyle)
            {
                case LoopType.LoopBackToTheStart:
                    nextIndex = 0;
                    break;
                case LoopType.MoveBackAndForth:
                    incrementOrDecrement *= -1;
                    nextIndex = currentIndex + incrementOrDecrement;
                    break;
            }
        }
        else
        {
            // Otherwise, everything is fine.
            nextIndex = nextIndexCandidate;
        }

        journeyDistance = Vector3.Distance(waypoints[currentIndex].position, waypoints[nextIndex].position);
        moveDuration = journeyDistance / movementSpeed;
    }

    #if UNITY_EDITOR
    private void OnValidate()
    {
        var localCollider = GetComponent<BoxCollider>();
        
        var newSize = new Vector3(platformSize.x, platformHeight, platformSize.y);
        var newCenter = (platformHeight * 0.5f) * -transform.up;
        
        localCollider.size = newSize;
        localCollider.center = newCenter;
        
        if(platformVisual)
        {
            platformVisual.transform.localScale = newSize;
            platformVisual.transform.localPosition = newCenter;
        }
    }
    #endif

    private void OnCollisionEnter(Collision other)
    {
        if (other.collider.CompareTag("Player")) playerIsOnPlatform = true;
    }
    
    private void OnCollisionExit(Collision other)
    {
        if (other.collider.CompareTag("Player")) playerIsOnPlatform = false;
    }

    private void PerformMove()
    {
        Vector3 from = waypoints[currentIndex].position;
        Vector3 to = waypoints[nextIndex].position;
        fractionTimeElapsed += (Time.deltaTime / moveDuration);
        float t;
        switch (smoothing)
        {
            case MovementType.QuiteSmooth:
                t = Mathfs.Smooth01(fractionTimeElapsed);
                break;
            case MovementType.VerySmooth:
                t = Mathfs.Smoother01(fractionTimeElapsed);
                break;
            default: // MovementType.NotSmooth:
                t = fractionTimeElapsed;
                break;
        }

        rbody.MovePosition(Vector3.Lerp(from, to, t));
    }
}


