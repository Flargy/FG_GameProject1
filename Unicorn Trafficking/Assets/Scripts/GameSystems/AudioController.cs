using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    private static AudioController instance = null;

    public static AudioController Instance { get { return instance; } }

    public enum ClipName
    {
        Moving, Jumping, Landing, Shooting, BubbleBurst,
        WantNewBalloon, IsCloseToCrying, Crying, WrongBallon, GotBallon,
        HorseSound, HorseMoving, HitByBubble,
        SteppingInGum, GumSnapping
    }

    [SerializeField] private List<AudioClip> clips = new List<AudioClip>();
    [SerializeField] private AudioSource sourcePrefab = null;

    private List<AudioSource> sources = new List<AudioSource>();
    


    private void Awake()
    {
        sources = new List<AudioSource>();

        if(instance == null)
        {
            instance = this;
        }
    }

    public void GenerateAudio(AudioClip clip, Vector3 location, float strength)
    {
        AudioSource audio;
        if(sources.Count > 0)
        {
            audio = sources[0];
        }
        else
        {
            audio = Instantiate(sourcePrefab);
        }
        RemoveFromList(audio, clip.length);
        audio.gameObject.SetActive(true);
        audio.transform.position = location;
        audio.PlayOneShot(clip, strength);
    }
    
    public void GenerateAudio(ClipName type ,Vector3 location, float strength)
    {
        AudioClip clip = clips[(int)type];
        GenerateAudio(clip, location, strength);
    }


    public void GenerateAudioWithDelay(ClipName type, Vector3 position, float strength, float delayTime)
    {
        AudioClip clip = clips[(int)type];
        GenerateAudioWithDelay(clip, position, strength, delayTime);
    }
    
    public void GenerateAudioWithDelay(AudioClip clip, Vector3 position, float strength, float delayTime)
    {
        StartCoroutine(DelayedPlay(clip, position, strength, delayTime));
    }

    private IEnumerator DelayedPlay(AudioClip clip, Vector3 position, float strength, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        GenerateAudio(clip, position, strength);
    }
    
    // make a random pitch for audio

    private void RemoveFromList(AudioSource source, float duration)
    {
        if (sources.Contains(source))
        {
            sources.Remove(source);
        }
        StartCoroutine(AddToListAfterDelay(source, duration));
    }

    private void AddToList(AudioSource source)
    {
        source.gameObject.SetActive(false);
        sources.Add(source);
    }

    private IEnumerator AddToListAfterDelay(AudioSource source, float duration)
    {
        yield return new WaitForSeconds(duration);
        AddToList(source);
    }
}
