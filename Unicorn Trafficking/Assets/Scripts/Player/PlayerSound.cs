using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(AudioSource))]
public class PlayerSound : MonoBehaviour
{
    private AudioSource audioSource;
    private Rigidbody rBody;
    private NewPlayerMovement playerMovement;

    [SerializeField, Tooltip("Sound for the jump effect")] private AudioClip jumpSound;
    [SerializeField, Tooltip("Sound for the moonjump effect")] private AudioClip moonJumpSound;
    [SerializeField, Tooltip("Sound for the wheel spinning")] private AudioClip wheelSound;
    [SerializeField, Tooltip("Sound for landing")] private AudioClip landingSound;
    
    [Header("Jumping Stuff")]
    [SerializeField, Range(0.6f, 1.3f), Tooltip("Amount of pitch-shifting to make the jump sound more varied.")] private float minJumpPitch = 0.9f;
    [SerializeField, Range(0.6f, 1.3f), Tooltip("Amount of pitch-shifting to make the jump sound more varied.")] private float maxJumpPitch = 1.1f;
    [SerializeField, Range(0.0f, 1.3f), Tooltip("How loud the jump sound should be")] private float jumpVolume = 0.5f;
    [SerializeField, Range(0.0f, 1.3f), Tooltip("How loud the moon jump sound should be")] private float moonJumpVolume = 0.5f;
    [SerializeField, Range(0.1f, 1.3f), Tooltip("How loud the wheel should be when airborne")] private float airWheelVolume = 0.1f;
    
    [SerializeField, Range(0.1f, 1.3f), Tooltip("How loud the landing sound should be")] private float landingVolume = 0.8f;
    
    [Header("Wheel Noise Stuff")]
    [SerializeField, Range(0.6f, 1.3f), Tooltip("Shift the pitch of the wheel sound based on our speed")] private float minPitch = 0.75f;
    [SerializeField, Range(0.6f, 1.3f), Tooltip("Shift the pitch of the wheel sound based on our speed")] private float maxPitch = 0.9f;

    [SerializeField, Range(0.0f, 1.3f), Tooltip("Change the volume of the wheel sound based on our speed")] private float minVolume = 0.0f;
    [SerializeField, Range(0.1f, 1.3f), Tooltip("Change the volume of the wheel sound based on our speed")] private float maxVolume = 0.7f;

    [Header("Speed Stuff")]
    [SerializeField, Range(0.0f, 10.0f),Tooltip("Speed at which minPitch and minVolume are active")] private float minSpeed = 1.0f;
    [SerializeField, Range(0.1f, 10.0f), Tooltip("Speed at which maxPitch and maxVolume are active")] private float maxSpeed = 7.0f;

    private float dPitch;
    private float dVolume;

    private bool isPlayingJumpSound;
    
    private void Awake()
    {
        rBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        playerMovement = GetComponent<NewPlayerMovement>();
        audioSource.pitch = minPitch;
        audioSource.volume = minVolume;
    }

    private void OnEnable()
    {
        playerMovement.Jumped += PlayJumpSound;
        playerMovement.Landed += PlayLandingSound;
    }
    private void OnDisable()
    {
        playerMovement.Landed -= PlayLandingSound;
        playerMovement.Jumped -= PlayJumpSound;
    }
    
    private void Update()
    {
        // Don't play wheel sounds when not grounded, or busy playing the jump sound.
        var actualSpeed = rBody.velocity.magnitude;

        var clampedSpeed = Mathfs.Clamp(actualSpeed, minSpeed, maxSpeed);
        var t = Mathfs.InverseLerp(minSpeed, maxSpeed, clampedSpeed);

        if(!isPlayingJumpSound)
        {
            var volume = Mathfs.Lerp(minVolume, maxVolume, Mathfs.Smooth01(t));
            audioSource.volume = Mathfs.SmoothDamp(audioSource.volume, volume, ref dVolume, 1.0f);
        }
        else if(!playerMovement.IsGrounded())
        {
            audioSource.volume = Mathfs.SmoothDamp(audioSource.volume, airWheelVolume, ref dVolume, 0.1f);
        }
        
        var pitch = Mathfs.Lerp(minPitch, maxPitch, Mathfs.Smooth01(t));
        audioSource.pitch = Mathfs.SmoothDamp(audioSource.pitch, pitch, ref dPitch, dPitch < 0.0f ? 0.2f : 1.0f);
    }

    public void PlayJumpSound(bool wasMoonJump)
    {
        if (isPlayingJumpSound) return;
        isPlayingJumpSound = true;

        audioSource.Stop();
        
        var pitch = Random.Range(minJumpPitch, maxJumpPitch);
        audioSource.pitch = pitch;

        if (wasMoonJump)
        {
            audioSource.PlayOneShot(moonJumpSound, moonJumpVolume);
            StartCoroutine(JumpSoundEnd(moonJumpSound.length * pitch));
        }
        else
        {
            audioSource.PlayOneShot(jumpSound, jumpVolume);
            StartCoroutine(JumpSoundEnd(jumpSound.length * pitch));
        }
    }
    
    private void PlayLandingSound()
    {
        audioSource.Stop();
        AudioController.Instance.GenerateAudio(landingSound, transform.localPosition, landingVolume);

        audioSource.Stop();
        
        audioSource.clip = wheelSound;
        audioSource.pitch = maxPitch;
        audioSource.volume = airWheelVolume;
        
        audioSource.Play();
    }

    private IEnumerator JumpSoundEnd(float clipLength)
    {
        yield return new WaitForSeconds(clipLength);

        audioSource.Stop();
        
        audioSource.clip = wheelSound;
        audioSource.pitch = maxPitch;
        audioSource.volume = airWheelVolume;
        
        audioSource.Play();
        
        isPlayingJumpSound = false;
    }
}
