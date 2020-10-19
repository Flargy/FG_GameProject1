using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSpawner : MonoBehaviour
{
    private static ParticleSpawner instance = null;
    
    public enum Particles { UnicornSpawn, UnicornRun, Delivery, Crying, ClownWalk, RocketJump}

    [SerializeField] private Dictionary<Particles, List<ParticleSystem>> particleCollection = new Dictionary<Particles, List<ParticleSystem>>();
    [SerializeField] private List<ParticleSystem> particles = new List<ParticleSystem>();

    private Dictionary<Particles, ParticleSystem> particleTypes = new Dictionary<Particles, ParticleSystem>();

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        instance.particleTypes = new Dictionary<Particles, ParticleSystem>();
        instance.particleCollection = new Dictionary<Particles, List<ParticleSystem>>();
        int index = 0;

        
        foreach (Particles type in Enum.GetValues(typeof(Particles)))
        {
            instance.particleTypes.Add(type, instance.particles[index]);
            instance.particleCollection.Add(type, new List<ParticleSystem>());
            instance.particleCollection.TryGetValue(type, out List<ParticleSystem> value);

            index++;
        }
        
    }


    public static void SpawnParticleEffect(Particles type, Vector3 location)
    {
        instance.particleCollection.TryGetValue(type, out List<ParticleSystem> value);

        ParticleSystem currentParticle;
        if (value.Count >= 1)
        {
            currentParticle = value[0];
            currentParticle.transform.position = location;
            currentParticle.gameObject.SetActive(true);
        }
        else
        {

            instance.particleTypes.TryGetValue(type, out ParticleSystem particleSys);
            currentParticle = Instantiate(particleSys, location, Quaternion.identity);
        }
        RemoveFromList(value, currentParticle);
    }

    private static void RemoveFromList(List<ParticleSystem> list, ParticleSystem obj)
    {
        if (list.Contains(obj))
        {
            list.Remove(obj);
        }
        instance.StartCoroutine(instance.ParticleDelay(list, obj, obj.main.duration));
    }

    

    private IEnumerator ParticleDelay(List<ParticleSystem> list, ParticleSystem obj, float duration)
    {
        yield return new WaitForSeconds(duration);
        list.Add(obj);
        obj.gameObject.SetActive(false);
    }

    
}
