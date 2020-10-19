using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering.UI;

[RequireComponent( typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour
{    
    [SerializeField] private LayerMask collisionMask = 0;
    [SerializeField] private float delayToMaxSpeed = 1.0f;
    [SerializeField] public float maxSpeed = 6.0f;
    [SerializeField] private Transform cameraTransform;
    private Vector3 direction = Vector3.zero;


    private Vector3 velocity = Vector3.zero;
    private CapsuleCollider coll = null;


    private float horizontalDirection = 0.0f;
    private float verticalDirection = 0.0f;

    private Vector3 pointUp;
    private Vector3 pointDown;
    private PhysicsComponent otherPhysics;
    private RaycastHit capsuleRaycast;
    private float frictionCoefficient = 0.95f;

    private float gravity = 9.81f;
    private float skinWidth = 0.05f;

    private void Awake()
    {
        coll = GetComponent<CapsuleCollider>();
    }

    public void Update()
    {
        
        if (Grounded())
        {
            MovementInput();

            CameraDirectionChanges();
            ProjectToPlaneNormal();
            ControlDirection();
            GroundDistanceCheck();
            Accelerate(direction);
        }
        
        ApplyGravity();
        CollisionCheck(velocity * Time.deltaTime);

    }

    /// <summary>
    /// Checks for collision using <see cref="capsuleRaycast"/> and recursive calls.
    /// </summary>
    /// <param name="frameMovement"></param>
    private void CollisionCheck(Vector3 frameMovement)
    {
        Vector3 pos = transform.position;
        if(Grounded() == false)
        {
            transform.position += frameMovement;
            return;
        }
        
        Debug.DrawRay(transform.position, frameMovement.normalized, Color.red);

        pointUp = pos + (coll.center + Vector3.up * (coll.height / 2 - coll.radius));
        pointDown = pos + (coll.center + Vector3.down * (coll.height / 2 - coll.radius));
        if (Physics.CapsuleCast(pointUp, pointDown, coll.radius, frameMovement.normalized, out capsuleRaycast, Mathf.Infinity, collisionMask))
        {
            float angle = (Vector3.Angle(capsuleRaycast.normal, frameMovement.normalized) - 90) * Mathf.Deg2Rad;
            float snapDistanceFromHit = skinWidth / Mathf.Sin(angle);

            Vector3 snapMovementVector = frameMovement.normalized * (capsuleRaycast.distance - snapDistanceFromHit);
            snapMovementVector = Vector3.ClampMagnitude(snapMovementVector, frameMovement.magnitude);
            frameMovement -= snapMovementVector;

            Vector3 frameMovementNormalForce = HelpClass.NormalizeForce(frameMovement, capsuleRaycast.normal);
            frameMovement += frameMovementNormalForce;

            transform.position += snapMovementVector;

            if (frameMovementNormalForce.magnitude > 0.001f)
            {
                Vector3 velocityNormalForce = HelpClass.NormalizeForce(velocity, capsuleRaycast.normal);
                velocity += velocityNormalForce;

            }

            if (frameMovement.magnitude > 0.001f)
            {
                CollisionCheck(frameMovement);
            }
            
            return;
        }

        else
        {
            transform.position += frameMovement;
        }
    }

    /// <summary>
    /// Lowers the players speed by a set amount each update.
    /// </summary>
    private void Decelerate()
    {
        Vector3 pos = transform.position;
        
        pointUp = pos + (coll.center + Vector3.up * (coll.height / 2 - coll.radius));
        pointDown = pos + (coll.center + Vector3.down * (coll.height / 2 - coll.radius));
        Physics.CapsuleCast(pointUp, pointDown, coll.radius, velocity.normalized, out capsuleRaycast, maxSpeed, collisionMask);

        Vector3 velocityOnGround = Vector3.ProjectOnPlane(velocity, capsuleRaycast.normal);
        Vector3 decelerationVector = velocityOnGround * frictionCoefficient;

        if (decelerationVector.magnitude > velocityOnGround.magnitude)
        {
            velocity = Vector3.zero;
        }
        else
        {
            velocity -= decelerationVector;
        }


    }

    /// <summary>
    /// Applies the velocity of a moving object onto the player if the player is standing on it.
    /// </summary>
    /// <param name="collideObject"></param>
    /// <param name="normalForce"></param>
    private void InheritVelocity(Transform collideObject, ref Vector3 normalForce)
    {
        otherPhysics = collideObject.GetComponent<PhysicsComponent>();
        if (otherPhysics == null)
            return;
        normalForce = normalForce.normalized * (normalForce.magnitude + Vector3.Project(otherPhysics.GetVelocity(), normalForce.normalized).magnitude);
        Vector3 forceInDirection = Vector3.ProjectOnPlane(velocity - otherPhysics.GetVelocity(), normalForce.normalized);
        Vector3 friction = -forceInDirection.normalized * normalForce.magnitude;

        if (friction.magnitude > forceInDirection.magnitude)
            friction = friction.normalized * forceInDirection.magnitude;
        velocity += friction;
    }

    /// <summary>
    /// Applies a constant force of gravity on the player.
    /// </summary>
    private void ApplyGravity()
    {
        velocity += Vector3.down * (gravity * Time.deltaTime);

    }

    /// <summary>
    /// Gradually increases the players velocity.
    /// </summary>
    /// <param name="vectorDirection"></param>
    private void Accelerate(Vector3 vectorDirection)
    {
        velocity += vectorDirection.normalized * maxSpeed / delayToMaxSpeed * Time.deltaTime;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
       
    }

    /// <summary>
    /// Uses a <see cref="capsuleRaycast"/> to see if the player has made contact with the cround
    /// </summary>
    /// <returns></returns>
    public bool Grounded() // check versus the normal of the area hit to see if you should be sliding or not
    {
        Vector3 pos = transform.position;
        
        pointUp = pos + coll.center + Vector3.up * (coll.height / 2 - coll.radius);
        pointDown = pos + coll.center + Vector3.down * (coll.height / 2 - coll.radius);
        if (Physics.CapsuleCast(pointUp, pointDown, coll.radius, Vector3.down, out capsuleRaycast, (0.05f + skinWidth), collisionMask))
        {
            var foo = Vector3.Dot(capsuleRaycast.normal, Vector3.up);
            Debug.Log(foo);
                
            return true;
        }
        return false;
    }

    public void GroundDistanceCheck()
    {

        if (capsuleRaycast.collider != null)
        {
            if (capsuleRaycast.distance > 0.4f)
            {
                velocity += new Vector3(0, -capsuleRaycast.distance * 5, 0);
            }
        }

    }

    /// <summary>
    /// Returns the normal angle of the object below the player.
    /// </summary>
    /// <returns></returns>
    public Vector3 GroundedNormal()
    {
        Vector3 pos = transform.position;
        
        pointUp = pos + coll.center + Vector3.up * (coll.height / 2 - coll.radius);
        pointDown = pos + coll.center + Vector3.down * (coll.height / 2 - coll.radius);
        if (Physics.CapsuleCast(pointUp, pointDown, coll.radius, Vector3.down, out capsuleRaycast, (0.5f + skinWidth), collisionMask))
        {
            return capsuleRaycast.normal;
        }
        else
        {
            return Vector3.zero;
        }
    }



    /// <summary>
    /// Alters the direction input to match the cameras direction.
    /// </summary>
    private void CameraDirectionChanges()
    {
        direction = cameraTransform.transform.rotation * new Vector3(horizontalDirection, 0, verticalDirection).normalized;
    }

    /// <summary>
    /// Gives the movement variables values from input
    /// </summary>
    private void MovementInput()
    {
        verticalDirection = Input.GetAxisRaw("Vertical");
        horizontalDirection = Input.GetAxisRaw("Horizontal");

    }

    /// <summary>
    /// Updates the players direction to match the terrains normal.
    /// </summary>
    private void ProjectToPlaneNormal()
    {
        Vector3 pos = transform.position;
        float radius = coll.radius;
        
        RaycastHit collision;
        Vector3 point1 = pos + coll.center + Vector3.up * (coll.height / 2 - radius);
        Vector3 point2 = pos + coll.center + Vector3.down * (coll.height / 2 - radius);

        Physics.CapsuleCast(point1, point2, radius, Vector3.down, out collision, maxSpeed, collisionMask);

        direction = Vector3.ProjectOnPlane(direction, collision.normal).normalized;

    }

    /// <summary>
    /// Stops the player from sliding down hills
    /// </summary>
    private void ControlDirection()
    {
        Vector3 projectedDirection = Vector3.ProjectOnPlane(direction, capsuleRaycast.normal);

        float value = Vector3.Dot(Vector3.down, projectedDirection);

        if ( value > 0.6f)
        {
            return;
        }

        if (Vector3.Dot(projectedDirection, velocity) != 1)
        {
            velocity = projectedDirection.normalized * velocity.magnitude;
        }
    }

}
