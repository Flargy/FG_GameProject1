using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnSystem : MonoBehaviour
{
    [SerializeField] private List<Transform> spawnLocations;
    [SerializeField] private List<GameObject> enemyPrefabs;
    private static SpawnSystem instance;
    private Dictionary<UnicornEnum.UnicornTypeEnum, GameObject> unicornDictionary = new Dictionary<UnicornEnum.UnicornTypeEnum, GameObject>();
    
    private List<GameObject> activeUnicorns = new List<GameObject>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        
        Array enums = Enum.GetValues(typeof(UnicornEnum.UnicornTypeEnum));

        for (int i = 0; i < enemyPrefabs.Count; i++)
        {
            instance.unicornDictionary.Add((UnicornEnum.UnicornTypeEnum)enums.GetValue(i), instance.enemyPrefabs[i]);
        }
    }

    public static List<GameObject> GetActiveUnicorns()
    {
        return instance.activeUnicorns;
    }

    public static Sprite SpawnNewUnicorn(UnicornEnum.UnicornTypeEnum type) 
    {
        Transform spawn = instance.spawnLocations[Random.Range(0, instance.spawnLocations.Count)];
        Vector3 location = spawn.position;
        GameObject spawned = UnicornPool.SpawnUnicorn(type);
        UnicornStats stats = spawned.GetComponent<UnicornStats>();
        spawned.transform.position = location;
        spawned.SetActive(true);
        stats.SetSpawn(spawn);
        instance.spawnLocations.Remove(spawn);
        instance.activeUnicorns.Add(spawned);
        ParticleSpawner.SpawnParticleEffect(ParticleSpawner.Particles.UnicornSpawn, location);

        return stats.GetSprite();

    }

    public static void AddSpawnPoint(Transform newSpawn, GameObject unicorn)
    {
        instance.spawnLocations.Add(newSpawn);
        instance.activeUnicorns.Remove(unicorn);
    }

    public static int GetRandomWeightIndex()
    {
        int totalWeight = 0;
        foreach (GameObject unicorn in instance.enemyPrefabs)
        {
            totalWeight += unicorn.GetComponent<UnicornStats>().GetWeight();
        }
        
        int returnIndex = 0;
        int randomNumber = Random.Range(0, totalWeight);

        foreach (GameObject unicorn in instance.enemyPrefabs)
        {
            int weight = unicorn.GetComponent<UnicornStats>().GetWeight();
            if (randomNumber < weight)
            {
                return returnIndex;
            }

            randomNumber -= weight;
            returnIndex++;
        }

        Debug.Assert(Mathf.Approximately(randomNumber, 0.0f));

        return returnIndex;

    }
    
}
