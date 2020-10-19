using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopFunctions : MonoBehaviour
{
    [SerializeField] private List<MonoBehaviour> scripts;
    [SerializeField] private bool onlyStopCoroutines = false;
    
    void Start()
    {
        TimeLimit.Pause += OnEvent;
    }

    public void OnEvent()
    {
        foreach (MonoBehaviour mon in scripts)
        {
            if (onlyStopCoroutines)
            {
                mon.StopAllCoroutines();
                continue;
            }
            mon.enabled = false;
        }
    }
}
