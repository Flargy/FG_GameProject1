using System;
using UnityEngine;

[SelectionBase, RequireComponent(typeof(Rigidbody))]
public class RigidbodyMovement : MonoBehaviour, IMovingActor
{
    [SerializeField] private float maxSpeed = 6.0f;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private LayerMask collisionMask = 0;
    [SerializeField] private float rotationValue = 2.0f;
    [SerializeField] private float jumpHeight = 2f;

    private float horizontalDirection = 0.0f;
    private float verticalDirection = 0.0f;
    private Vector3 direction = Vector3.zero;
    private Rigidbody rBody = null;
    private CapsuleCollider coll = null;
    private Vector3 lookDirection;
    private Vector3 faceDirection;
    private float jumpVelocity;

    private float currentMaxSpeed;
    
    private void Awake()
    {
        Vector3 forward = transform.forward;
        rBody = GetComponent<Rigidbody>();
        coll = GetComponent<CapsuleCollider>();
        lookDirection = forward;
        faceDirection = forward;
        currentMaxSpeed = maxSpeed;
        jumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(Physics.gravity.y) * jumpHeight);
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void Update()
    {
        Jump();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        MovementInput();
        CameraDirectionChanges();
        FaceTowardsDirection();
        ProjectToPlaneNormal();
        Move();
         
        
    }

    private void Jump()
    {
        if (Input.GetKeyDown(KeyCode.E) && Grounded() == true)
        {
            rBody.velocity += Vector3.up * (jumpVelocity - 0.3f); //Add value here for jumpheight
        }
    }
    
    private void CameraDirectionChanges()
    {
        direction = cameraTransform.rotation * new Vector3(horizontalDirection, 0, verticalDirection).normalized;
        direction.y = 0;
    }
    
    private void MovementInput()
    {
        verticalDirection = Input.GetAxisRaw("Vertical");
        horizontalDirection = Input.GetAxisRaw("Horizontal");

    }

    private bool Grounded()
    {
        if (!Physics.Raycast(transform.position, Vector3.down, coll.height / 2 + 0.3f , collisionMask))
        {
            return false;
        }

        return true;
    }

    private void ApplyGravity()
    {
        direction = direction + Physics.gravity * Time.deltaTime;
    }

    private void Move()
    {
        direction *= currentMaxSpeed;
        direction.y = rBody.velocity.y;
        if (Grounded() == false)
        {
            ApplyGravity();
        }
        rBody.velocity = direction;

    }
    
    private void FaceTowardsDirection()
    {
        Vector3 position = transform.position;
        
        if (direction.magnitude > 0)
        {
            lookDirection = new Vector3(direction.x, 0, direction.z);
            if (Vector3.Dot(lookDirection, faceDirection) <= -0.95f)
            {
                faceDirection += transform.rotation * Vector3.left * 0.4f;
            }
            faceDirection += lookDirection * (Time.deltaTime * rotationValue);
            if (faceDirection.magnitude > 1)
                faceDirection = faceDirection.normalized;
            transform.LookAt(position + faceDirection);

        }
        else
        {
            faceDirection += lookDirection * (Time.deltaTime * rotationValue);
            if (faceDirection.magnitude > 1)
                faceDirection = faceDirection.normalized;
            transform.LookAt(position + faceDirection);
        }
        
    }
    
    
    private void ProjectToPlaneNormal()
    {
        Vector3 pos = transform.position;
        float radius = coll.radius;
        
        RaycastHit collision;
        Vector3 point1 = pos + coll.center + Vector3.up * (coll.height / 2 - radius);
        Vector3 point2 = pos + coll.center + Vector3.down * (coll.height / 2 - radius);

        Physics.CapsuleCast(point1, point2, radius, Vector3.down, out collision, currentMaxSpeed, collisionMask);

        direction = Vector3.ProjectOnPlane(direction, collision.normal).normalized;

    }

    public void SetSpeedModifier(float slowMultiplier)
    {
        currentMaxSpeed = slowMultiplier * maxSpeed;
    }
}
