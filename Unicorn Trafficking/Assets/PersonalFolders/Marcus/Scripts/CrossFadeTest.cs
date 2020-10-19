using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossFadeTest : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource ambianceSource;
    [SerializeField] private float musicEndDistance = 10f;
    [SerializeField] private float ambianceStartingDistance = 5f;
    [SerializeField] private float ambianceMaxVolumeDistance = 15f;
    [SerializeField, Range(0f, 1f)] private float maxAudioStrength = 1f;

    private float distanceToPlayer;

    void LateUpdate()
    {
        distanceToPlayer = Vector3.Distance(transform.position, player.position);

        musicSource.volume = 1 - distanceToPlayer / musicEndDistance;

        if (distanceToPlayer >= ambianceStartingDistance)
        {
            ambianceSource.volume = (distanceToPlayer - ambianceStartingDistance) / (ambianceMaxVolumeDistance - ambianceStartingDistance);
        }
        else
        {
            ambianceSource.volume = 0;
        }
    }
}
