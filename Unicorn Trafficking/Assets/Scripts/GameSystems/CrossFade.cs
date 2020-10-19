using UnityEngine;

public class CrossFade : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource ambianceSource;
    [SerializeField] private float musicEndDistance = 10f;
    [SerializeField] private float ambianceStartingDistance = 5f;
    [SerializeField] private float ambianceMaxVolumeDistance = 15f;
    [SerializeField, Range(0f, 1f)] private float maxMusicStrength = 1f;
    [SerializeField, Range(0f, 1f)] private float maxAmbianceStrength = 1f;


    private float distanceToPlayer;

    void LateUpdate()
    {
        distanceToPlayer = Vector3.Distance(transform.position, player.position);

        musicSource.volume = (1 - (distanceToPlayer / musicEndDistance)) * maxMusicStrength;

        if (distanceToPlayer >= ambianceStartingDistance)
        {
            ambianceSource.volume = ((distanceToPlayer - ambianceStartingDistance) / (ambianceMaxVolumeDistance - ambianceStartingDistance)) * maxAmbianceStrength;
        }
        else
        {
            ambianceSource.volume = 0;
        }
    }
}
