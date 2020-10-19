using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PointCollection : MonoBehaviour
{
    [SerializeField] private UITimer uiElement;
    [SerializeField] private int positivePoint = 100;
    [SerializeField] private int negativePoint = -50;
    [SerializeField] private float successDelay = 2.0f;
    [SerializeField] private float failDelay = 8.0f;
    [SerializeField] private Image unicornUiImage = null;
    [SerializeField] private List<MeshRenderer> balloonList;
    [SerializeField] public Transform particlePoint;

    [Header("Volume Controls")]
    [SerializeField, Range(0.1f, 1.2f)] private float wantsNewBalloon = 0.5f;
    [SerializeField, Range(0.1f, 1.2f)] private float wrongBalloon = 0.5f;
    [SerializeField, Range(0.1f, 1.2f)] private float gotBalloon = 0.5f;
    [SerializeField, Range(0.1f, 1.2f)] private float nearCrying = 0.5f;
    [SerializeField, Range(0.1f, 1.2f)] private float crying = 0.5f;
    
    private UnicornEnum.UnicornTypeEnum type;
    private bool onCooldown = false;
    private float deliveryTime;
    private Coroutine delivery;
    private bool coroutineRunning = false;
    private bool successfulDelivery = false;
    
    private void Start()
    {
        PointHandeler.GetNewUnicorn(this);
        deliveryTime = PointHandeler.GetTime();
    }

    public void ChangeType(UnicornEnum.UnicornTypeEnum newType, Material newMat, float newTimer)
    {
        AudioController.Instance.GenerateAudio(AudioController.ClipName.WantNewBalloon, transform.position, wantsNewBalloon);
        type = newType;
        unicornUiImage.sprite = SpawnSystem.SpawnNewUnicorn(type);
        foreach (MeshRenderer rend in balloonList)
        {
            rend.material = newMat; 
        }
        onCooldown = false;
        deliveryTime = newTimer;
        if (coroutineRunning == true)
        {
            StopCoroutine(delivery);
        }
        
        
        successfulDelivery = false;
        delivery = StartCoroutine(DeliveryTimer());
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (onCooldown == true)
        {
            
            return;
        }
        if(other.CompareTag("Player"))
        {
            GameObject pointCorn = PlayerInventory.GetUnicorn(type);

            if (pointCorn != null)
            {
                ParticleSpawner.SpawnParticleEffect(ParticleSpawner.Particles.Delivery, particlePoint.position);
                AudioController.Instance.GenerateAudio(AudioController.ClipName.GotBallon, transform.position, gotBalloon);
                onCooldown = true;
                SpawnSystem.AddSpawnPoint(pointCorn.GetComponent<UnicornStats>().GetSpawn(), pointCorn);
                PointHandeler.SuccessfulDelivery(this, successDelay, positivePoint);
                pointCorn.GetComponent<UnicornStats>().GetPooled();
                successfulDelivery = true;
                if (coroutineRunning == true)
                {
                    StopCoroutine(delivery);
                    coroutineRunning = false;
                }
                uiElement.EndTimer();
            }
            else
            {
                AudioController.Instance.GenerateAudio(AudioController.ClipName.WrongBallon, transform.position, wrongBalloon);
            }

        }
    }

    private void ReactivateTimer()
    {
        deliveryTime = PointHandeler.GetTime();
        delivery = StartCoroutine(DeliveryTimer());
        
    }

    private IEnumerator DeliveryTimer()
    {
        coroutineRunning = true;
        bool soundHasPlayed = false;
        uiElement.StartTimer(deliveryTime);
        float timer = 0f;
        while (timer < deliveryTime && onCooldown == false)
        {
            timer += Time.deltaTime;
            if (!soundHasPlayed && timer / deliveryTime >= 0.7f)
            {
                if (Time.timeScale == 0)
                {
                    StopAllCoroutines();
                }
                AudioController.Instance.GenerateAudio(AudioController.ClipName.IsCloseToCrying, transform.position, nearCrying);
                soundHasPlayed = true;
            }
                
            yield return null;
            
        }
        
        PointHandeler.FailedDelivery(negativePoint);
        StartCoroutine(FailureDelay());
        AudioController.Instance.GenerateAudio(AudioController.ClipName.Crying, transform.position, crying);
        ParticleSpawner.SpawnParticleEffect(ParticleSpawner.Particles.Crying, particlePoint.position);
        coroutineRunning = false;

    }

    private IEnumerator FailureDelay()
    {
        onCooldown = true;
        yield return new WaitForSeconds(failDelay);
        onCooldown = false;
        ReactivateTimer();
    }
}
