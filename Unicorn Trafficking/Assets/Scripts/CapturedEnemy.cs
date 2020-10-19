using System;
using EnemyBehaviours;
using UnityEngine;

public class CapturedEnemy : MonoBehaviour
{
    [SerializeField] private GameObject bubble;
    [SerializeField] private GameObject colliderObject;
    [SerializeField] public Transform stringConnectionPoint;
    [SerializeField] private float hoverUpDuration;
    [SerializeField] private float altitudePosition;
    [SerializeField] private float bobbingFrequency;
    [SerializeField] private float bobbingMagnitude;
    [SerializeField] private float timeUntilBobbing;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float minDistanceToPlayer;
    [SerializeField] public float MaxDistanceToPlayer;
    [SerializeField, Range(0.5f, 1.0f)] private float bubbleScaleFactor = 0.9f;
    private float movementTimer;
    private float timeSinceHovered;
    private float turnToSeconds = 100;
    private float yPosRelativeToPlayer;
    private GameObject player;
    private Rigidbody body;

    [NonSerialized] public bool Isbubbled = false;
    [NonSerialized] public bool IsCaptured = false;

    private void Start()
    {
        player = GameObject.FindWithTag("Player");
        body = GetComponent<Rigidbody>();
        yPosRelativeToPlayer = player.transform.position.y;
    }

    private void Update()
    {
        EncapsulateAndHoverEnemy();
        FollowPlayer();
    }

    public void EncapsulateAndHoverEnemy()
    {
        if (Isbubbled)
        {
            movementTimer += Time.deltaTime / (hoverUpDuration*turnToSeconds);
            transform.position = new Vector3(transform.position.x,Mathf.Lerp(transform.position.y, yPosRelativeToPlayer+altitudePosition, movementTimer),transform.position.z);
            HoverUpAndDown();
        }
    }
    
    private void HoverUpAndDown()
    {
        if (movementTimer > timeUntilBobbing/turnToSeconds)
        {
            timeSinceHovered += Time.deltaTime;
            if (IsCaptured)
                yPosRelativeToPlayer = player.transform.position.y;
            transform.position = new Vector3(transform.position.x,  yPosRelativeToPlayer+altitudePosition, transform.position.z) + transform.up * Mathf.Sin(timeSinceHovered * bobbingFrequency) * bobbingMagnitude/10;
        }
    }

    private void FollowPlayer()
    {
        if (IsCaptured)
        {
            Vector3 direction = Vector3.zero;

            if (Vector3.Distance(player.transform.position, transform.position) > minDistanceToPlayer)
            {
                direction.x = player.transform.position.x - transform.position.x;
                direction.z = player.transform.position.z - transform.position.z;
                body.AddForce(direction.normalized * movementSpeed, ForceMode.Acceleration);
            }
            
            if (Vector3.Distance(player.transform.position, transform.position) > MaxDistanceToPlayer)
            {
                Vector3 pos = transform.position;
                pos.x = player.transform.position.x;
                pos.z = player.transform.position.z;
                transform.position = pos;
            }

            // playerPos = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);
           // transform.position = Vector3.MoveTowards(transform.position, playerPos, movementSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Isbubbled)
        {
            if (other.CompareTag("Player"))
            {
                IsCaptured = true;
                colliderObject.SetActive(false);
                GetComponent<UnicornStats>().GetCaptured();
            }
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        //body.AddForce(new Vector3(-other.transform.position.x,transform.position.y, -other.transform.position.z)*10);
    }

    public void BubbleHit()
    {
        AudioController.Instance.GenerateAudio(AudioController.ClipName.HitByBubble, transform.position, 0.5f);
        if(TryGetComponent<IEnemyAttack>(out var enemyAttack))
        {
            enemyAttack.Disable();
        }
        bubble.SetActive(true);
        colliderObject.SetActive(true);
        Isbubbled = true;
        transform.localScale = Vector3.one * bubbleScaleFactor;
        //body.useGravity = false;
        GetComponent<EnemyBehaviour>().enabled = false;
    }

    public void Reset()
    {
        IsCaptured = false;
        bubble.SetActive(false);
        colliderObject.SetActive(false);
        Isbubbled = false;
        transform.localScale = Vector3.one;
        //body.useGravity = true;
        GetComponent<EnemyBehaviour>().enabled = true;
        movementTimer = 0f;
    }

    public void OnEnable()
    {
        Reset();
    }
}
