using System;
using System.Collections.Generic;
using UnityEngine;

public class UnicornPool : MonoBehaviour
{
    private static UnicornPool instance;

    [SerializeField] private List<GameObject> unicornTypes;
    [SerializeField] private Dictionary<UnicornEnum.UnicornTypeEnum, List<GameObject>> unicornCollection = new Dictionary<UnicornEnum.UnicornTypeEnum, List<GameObject>>();
    [SerializeField] private int cachedUnicorns = 1;
    
    private Dictionary<UnicornEnum.UnicornTypeEnum, GameObject> unicornBackup = new Dictionary<UnicornEnum.UnicornTypeEnum, GameObject>();
    private Queue<GameObject> spawnQueue = new Queue<GameObject>();

    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        int index = 0;
        
        foreach (UnicornEnum.UnicornTypeEnum type in Enum.GetValues(typeof(UnicornEnum.UnicornTypeEnum)))
        {
            instance.unicornBackup.Add(type, instance.unicornTypes[index]);
            instance.unicornCollection.Add(type, new List<GameObject>());
            instance.unicornCollection.TryGetValue(type, out List<GameObject> value);
            
            for (int i = 0; i < cachedUnicorns; i++)
            {
                GameObject unicorn = Instantiate(unicornTypes[index]);
                value.Add(unicorn);
                unicorn.gameObject.SetActive(false);
            }

            index++;
        }
        
    }

    public static GameObject SpawnUnicorn(UnicornEnum.UnicornTypeEnum type)
    {
        instance.unicornCollection.TryGetValue(type, out List<GameObject> value);

        GameObject currentUnicorn;

        if (value.Count >= 1)
        {
            currentUnicorn = value[0];
        }
        else
        {
            instance.unicornBackup.TryGetValue(type, out GameObject unicorn);
            currentUnicorn = Instantiate(unicorn);
        }
        RemoveFromList(value, currentUnicorn);
        return currentUnicorn;
    }

    public static void RemoveFromList(List<GameObject> list, GameObject obj)
    {
        if (list.Contains(obj))
        {
            list.Remove(obj);
        }
    }

    public static void AddToList(UnicornEnum.UnicornTypeEnum type, GameObject obj)
    {
        instance.unicornCollection.TryGetValue(type, out List<GameObject> value);
        value.Add(obj);
        obj.gameObject.SetActive(false);
    }
    
    
    
   
}
