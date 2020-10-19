using UnityEditor;
using UnityEngine;

public class CamScript : MonoBehaviour
{
    [Header("Mouse or Keyboard Mode")]
    [SerializeField, Tooltip("Uncheck to use keyboard controls.")] private bool mouseControls = true;
    
    [Header("Initial Viewing Angle")]
    [SerializeField, Range(-180.0f, 180.0f)] private float HorizontalAngle = 45.0f;
    [SerializeField, Range(0.0f, 180.0f)] private float VerticalAngle = 35.0f;
    
    [Header("Mouse Tuning")]
    [SerializeField, Tooltip("Inverts the camera controls.")] private bool FlippedMouse = true;
    [SerializeField, Tooltip("Maximum speed that the camera can spin. Tune this before Mouse Sensitivity"), Range(100.0f, 500.0f)]
    private float RotateSpeed = 260.0f;
    [SerializeField, Tooltip("How long it takes for the camera to move when using Q/E"), Range(0.1f, 2.0f)]
        private float RotationDelay = 0.3f;
    [SerializeField, Tooltip("Mouse movement required to reach max rotate speed. Tune Rotate Speed first."), Range(0.5f,5.0f)]
    float MouseSensitivity = 3.0f;
    
    [Header("Field of View Tuning")]
    [SerializeField, Range(3.0f, 10.0f)] private float trackingSpeed = 6.0f;
    
    [Header("Field of View Tuning")]
    [SerializeField, Tooltip("Change the projection to appear more \"flat\"."), Range(1.0f, 150.0f)]
    private float Flatness = 70.0f;
    [SerializeField, Tooltip("Makes objects in the scene appear smaller or larger."), Range(1.0f, 40.0f)]
    private float Zoomness = 4.0f;

    [Header("Object to Track")]
    [SerializeField, Tooltip("Defaults to the player.")]
    private Transform FollowTarget = default;

    private Camera cam;
    private Vector3 currentFocalPoint = Vector3.zero;
    private Vector3 currentRotatedRelativePos = Vector3.zero;

    private float targetHorizAngle = 0.0f;
    private float currentHorizAngle = 0.0f;
    private float horizVel = 0.0f;
    
    private void Awake()
    {
        cam = GetComponent<Camera>();
        Startup();
    }

    private void Startup()
    {
        if (FollowTarget == null) FollowTarget = GameObject.FindWithTag("Player").transform;
        currentFocalPoint = FollowTarget.position;
        
        currentHorizAngle = HorizontalAngle;
        targetHorizAngle = HorizontalAngle;
    }

    private void Start()
    {
        Cursor.lockState = mouseControls ? CursorLockMode.Locked : CursorLockMode.None;
    }

    private void OnDisable()
    {
       Cursor.lockState = CursorLockMode.None;
    }

    private void LateUpdate()
    {
        currentHorizAngle = CalculateHorizontalAngle();
        
        float horizAngRad = currentHorizAngle * Mathfs.Deg2Rad;
        float vertAngRad = VerticalAngle * Mathfs.Deg2Rad;
        
        Vector3 desiredRelativePosition = new Vector3(Mathfs.Cos(horizAngRad),
            Mathfs.Sin(vertAngRad),
            Mathfs.Sin(horizAngRad)) * Flatness;

        var followTargetPosition = FollowTarget.position;
        var focalPointToFollowTarget = followTargetPosition - currentFocalPoint;
        var focalPointDelta = Vector3.Dot(focalPointToFollowTarget, focalPointToFollowTarget);

        float smoothingFactor = Mathfs.Clamp01(focalPointDelta) * trackingSpeed * Time.deltaTime;
        
        currentFocalPoint = Vector3.MoveTowards(currentFocalPoint, followTargetPosition, smoothingFactor);
        currentRotatedRelativePos = Vector3.Slerp(currentRotatedRelativePos, desiredRelativePosition, RotateSpeed * Time.deltaTime);

        transform.position = currentFocalPoint + currentRotatedRelativePos;
        transform.rotation = Quaternion.LookRotation(currentFocalPoint - transform.position, Vector3.up);

        // tie FoV to distance
        cam.fieldOfView = Mathf.Atan(Zoomness / Flatness) * Mathf.Rad2Deg * 2.0f;
    }

    #if UNITY_EDITOR
    
    private void OnValidate()
    {
        Startup();
    }

    private void OnDrawGizmosSelected()
    {
        using (new Handles.DrawingScope())
        {
            Handles.color = new Color(0.4f, 0.4f, 0.7f, 1.0f);
            Handles.SphereHandleCap(0, currentFocalPoint, Quaternion.identity, 0.2f, EventType.Repaint);
            
            Handles.color = new Color(0.7f, 0.7f, 1.0f, 1.0f);
            Handles.DrawDottedLine(currentFocalPoint, FollowTarget.position, 4.0f);
            
            Handles.color = new Color(0.1f, 0.1f, 0.5f, 1.0f);
            Handles.Label(currentFocalPoint, "Camera Focal Point");
        }
    }
    #endif

    private float CalculateHorizontalAngle()
    {
        return mouseControls ? MouseControls() : KeyboardControls();
    }
    
    private float MouseControls()
    {
        var mouseInput = Input.GetAxis("Mouse X");
        targetHorizAngle += (FlippedMouse ? -mouseInput : mouseInput) * MouseSensitivity;
        Mathfs.Repeat(targetHorizAngle, 360.0f);

        return Mathfs.SmoothDamp(currentHorizAngle, targetHorizAngle, ref horizVel, RotationDelay, RotateSpeed);
    }

    private float KeyboardControls()
    {
        if (Mathfs.Abs(currentHorizAngle - targetHorizAngle) < 5.0f)
        {
            //@TODO: Temporary hard-coded AWFULNESS. Clean this up when designers know how they want the camera to be.
            var requestedTurn = 0.0f;
            requestedTurn += Input.GetKeyDown(KeyCode.Q) ? -90.0f : 0.0f;
            requestedTurn += Input.GetKeyDown(KeyCode.E) ? 90.0f : 0.0f;

            targetHorizAngle += requestedTurn;
        }

        return Mathfs.SmoothDamp(currentHorizAngle, targetHorizAngle, ref horizVel, RotationDelay, RotateSpeed);
    }


}
