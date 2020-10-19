using UnityEngine;

public class BubbleGun : MonoBehaviour
{
    [SerializeField, Range(0.1f, 1.2f)] private float shootVolume = 0.5f;
    [SerializeField] private float coolDown;
    [SerializeField] private GameObject bubble;
    [SerializeField] private GameObject firePointForward;
    private float timeSinceShot;
    
    void Start()
    { 
        if (bubble == null) { return; }
    }
    
    // Update is called once per frame
    void Update()
    {
        Shoot();
    }
    
    private void Shoot()
    {
        timeSinceShot += Time.deltaTime;
        if (Input.GetButtonDown("Fire1") && timeSinceShot > coolDown)
        {
            AudioController.Instance.GenerateAudio(AudioController.ClipName.Shooting, transform.position, shootVolume);
            Instantiate(bubble,firePointForward.transform.position, firePointForward.transform.rotation); 
            timeSinceShot = 0;
        }
    }
}
