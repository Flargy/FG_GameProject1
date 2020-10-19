using UnityEngine;

public class BubbleProjectile : MonoBehaviour
{
        [SerializeField] private float bulletSpeed;
        [SerializeField] private float collisionRadius;
        [SerializeField] private int bubbleLife;
        [SerializeField] private LayerMask layer;
    
        void Update()
        {
            Move();
            CollisionCheck();
        }
    
        private void Move()
        {
            transform.position = transform.position + transform.forward * bulletSpeed * Time.deltaTime;
            Destroy(gameObject, bubbleLife);
        }
    
        private void CollisionCheck()
        {
            RaycastHit hit;
    
            if (Physics.SphereCast(transform.position, collisionRadius, transform.forward, out hit,0.3f,layer))
            {
                Debug.Log("hit");
                switch (hit.collider.tag)
                {
                    case "Enemy":
                        hit.collider.GetComponent<CapturedEnemy>().BubbleHit();
                        Destroy(gameObject);
                        break;
                    case "Finish":
                        break;
                    default:
                        AudioController.Instance.GenerateAudio(AudioController.ClipName.BubbleBurst, transform.position, 0.5f);
                        Destroy(gameObject);
                        break;
                }
            }
        }
}
