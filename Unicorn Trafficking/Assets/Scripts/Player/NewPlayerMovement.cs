using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
[SelectionBase]
public class NewPlayerMovement : MonoBehaviour, IMovingActor
{
    public delegate void JumpAction(bool isMoonJump);
    public event JumpAction Jumped;
    
    public delegate void LandedAction();
    public event LandedAction Landed;

    [SerializeField, Range(0f, 100f)] float maxSpeed = 6.0f;
    [SerializeField] private float rotationValue = 2.0f;
    [SerializeField, Range(0f, 10f)] private float jumpHeight = 1.5f;
    [SerializeField, Range(0f, 20f)] float moonJumpHeight = 8.0f;
    [SerializeField, Range(0f, 100f)] float acceleration = 10.0f;
    [SerializeField, Range(0f, 100f)] float airAcceleration = 3.0f;
    [SerializeField, Range(0f, 90f)] float maxSlopeAngle = 60.0f;
    [SerializeField, Range(0f, 0.3f)] float slowAmountPerUnicorn = 0.1f;
    
    [SerializeField] private Transform cameraTransform;
   
    private Rigidbody rBody = null;
    private Rigidbody groundBody;
    private Rigidbody previousGroundBody;
    
    private Vector3 lookDirection;
    private Vector3 faceDirection;
        
    private float currentMaxSpeed;
    private float slowMultiplier = 1.0f;

    Vector3 direction;
    Vector3 velocity;
    Vector3 groundVelocity;

    private Vector3 contactNormal;
    private Vector3 contactPosWorld;
    private Vector3 contactPosLocal;
    
    private bool jumpRequested;
    private float jumpVelocity;
    private float moonJumpVelocity;
    private bool moonJumpGround;
    
    private bool isGrounded;
    private float groundCheckDistance = 0.15f;

    float minCosToGround;

    private void Awake()
    {
        Vector3 forward = transform.forward;
        rBody = GetComponent<Rigidbody>();
        lookDirection = forward;
        faceDirection = forward;
        Initialize();
    }
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        Initialize();
    }
#endif
    private void Initialize()
    {
        minCosToGround = Mathf.Cos(maxSlopeAngle * Mathf.Deg2Rad);
        currentMaxSpeed = maxSpeed;

        jumpVelocity = Mathf.Sqrt(2.0f * Mathf.Abs(Physics.gravity.y) * jumpHeight);
        moonJumpVelocity = Mathf.Sqrt(2.0f * Mathf.Abs(Physics.gravity.y) * moonJumpHeight);
        if (cameraTransform == null && Camera.main)
        {
            cameraTransform = Camera.main.transform;
        }
    }
    private void Update()
    {
        MovementInput(); // Input should be polled in update
        FaceTowardsDirection();
    }

    private void FixedUpdate()
    {
        UpdateState();
        Move();
        Jump();
        rBody.velocity = velocity;
        ResetForNextFrame();
    }
    
    private void MovementInput()
    {
        Vector2 playerInput;
        playerInput.x = Input.GetAxisRaw("Horizontal");
        playerInput.y = Input.GetAxisRaw("Vertical");
        jumpRequested |= Input.GetButtonDown("Jump"); 
        playerInput = Vector2.ClampMagnitude(playerInput, 1.0f);

        Vector3 forward = IgnoreYComponent(cameraTransform.forward);
        Vector3 right = IgnoreYComponent(cameraTransform.right);
        direction = (forward * playerInput.y + right * playerInput.x) * currentMaxSpeed;
    }

    private Vector3 IgnoreYComponent(Vector3 input)
    {
        Vector3 vector = input;
        vector.y = 0.0f;
        vector.Normalize();
        return vector;
    }

    private void FaceTowardsDirection()
    {
        Vector3 position = transform.position;
        
        if (direction.magnitude > 0.0f)
        {
            lookDirection = new Vector3(direction.x, 0.0f, direction.z);
            if (Vector3.Dot(lookDirection, faceDirection) <= -0.95f)
            {
                faceDirection += transform.rotation * Vector3.left * 0.4f;
            }
            faceDirection += lookDirection * (Time.deltaTime * rotationValue);
            if (faceDirection.magnitude > 1.0f)
                faceDirection = faceDirection.normalized;
            transform.LookAt(position + faceDirection);
        }
        else
        {
            faceDirection += lookDirection * (Time.deltaTime * rotationValue);
            if (faceDirection.magnitude > 1.0f)
                faceDirection = faceDirection.normalized;
            transform.LookAt(position + faceDirection);
        }
    }
    
    private void ResetForNextFrame()
    {
        contactNormal = Vector3.zero;
        groundVelocity = Vector3.zero;
        previousGroundBody = groundBody;
        groundBody = null;
    }

    private void UpdateState()
    {
        velocity = rBody.velocity;
        if (isGrounded)
        {
            TrySnappingToGround();
        }

        contactNormal = Vector3.up;
        
        if (groundBody && groundBody.isKinematic)
            UpdatePlatformMotion();

        currentMaxSpeed = maxSpeed * slowMultiplier * (1.0f - slowAmountPerUnicorn * PlayerInventory.Count());
    }

    private void UpdatePlatformMotion()
    {
        if (groundBody == previousGroundBody)
        {
            Vector3 platformMotion = groundBody.transform.TransformPoint(contactPosLocal) - contactPosWorld;
            groundVelocity = platformMotion / Time.deltaTime;
        }
        contactPosWorld = rBody.position;
        contactPosLocal = groundBody.transform.InverseTransformPoint(contactPosWorld);
    }

    private void TrySnappingToGround()
    {
        var rayOrigin = transform.position;
        Debug.DrawLine(rayOrigin, rayOrigin + (Vector3.down) * groundCheckDistance);
        if (!Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, groundCheckDistance)) 
        {
            return; // Nothing to snap to!
        }
        if (hit.normal.y < minCosToGround) {
            return; // There's something below us, but it's too steep to consider as ground.
        }
        
        // We can snap! Return us to a grounded state.
        isGrounded = true;
        contactNormal = hit.normal;
        
        // Kill upwards velocity, but allow downwards velocity to remain.
        float speed = velocity.magnitude;
        float verticalVelocity = Vector3.Dot(velocity, hit.normal);		
        if (verticalVelocity > 0.0f)
        {
            velocity = (velocity - hit.normal * verticalVelocity).normalized * speed;
        }
    }
    
    private void Jump()
    {
        if(jumpRequested && isGrounded)
        {
            Jumped?.Invoke(moonJumpGround);
            groundCheckDistance = 0.0f;
            StartCoroutine(ResetGroundProbe());
            isGrounded = false;

            var jumpForce = moonJumpGround ? moonJumpVelocity : jumpVelocity;
            
            velocity += contactNormal * jumpForce;
            velocity.y = Mathfs.Clamp(velocity.y, 0.0f, jumpForce);
        }
        jumpRequested = false;
    }

    private IEnumerator ResetGroundProbe()
    {
        yield return new WaitForSeconds(0.2f);
        isGrounded = false;
        groundCheckDistance = 0.3f;
    }

    private Vector3 ProjectOnContactPlane(Vector3 vector)
    {
        return vector - contactNormal * Vector3.Dot(vector, contactNormal);
    }
    
    private void Move()
    {
        float maxSpeedChange = (isGrounded ? acceleration : airAcceleration) * Time.deltaTime;
        Vector3 BasisX = ProjectOnContactPlane(Vector3.right).normalized;
        Vector3 BasisZ = ProjectOnContactPlane(Vector3.forward).normalized;

        Vector3 relativeVelocity = velocity - groundVelocity;
        float oldVelX = Vector3.Dot(relativeVelocity, BasisX);
        float oldVelZ = Vector3.Dot(relativeVelocity, BasisZ);
        
        float newVelX = Mathf.MoveTowards(oldVelX, direction.x, maxSpeedChange);
        float newVelZ = Mathf.MoveTowards(oldVelZ, direction.z, maxSpeedChange);
        
        velocity += BasisX * (newVelX - oldVelX) + BasisZ * (newVelZ - oldVelZ);
    }
    
    private void OnCollisionEnter(Collision other)
    {
        DoCollision(other, true);
    }

    private void OnCollisionStay(Collision other)
    {
        DoCollision(other, false);
    }

    private void DoCollision(Collision other, bool wasEnter) 
    {
        for (int i = 0;
            i < other.contactCount;
            i++)
        {
            Vector3 normal = other.GetContact(i).normal;
            if (normal.y >= minCosToGround)
            {
                if(wasEnter && !isGrounded) Landed?.Invoke();
                isGrounded = true;
                contactNormal = normal;
                groundBody = other.rigidbody;
                moonJumpGround = other.collider.CompareTag("MoonJump");
            }
        }
    }

    public void SetSpeedModifier(float slowMultiplier)
    {
        this.slowMultiplier = slowMultiplier;
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }
}
