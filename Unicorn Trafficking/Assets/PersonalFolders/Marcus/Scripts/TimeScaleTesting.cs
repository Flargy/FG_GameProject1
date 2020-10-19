using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeScaleTesting : MonoBehaviour
{
    public delegate void ThisEvent();

    public MonoBehaviour script;

    public static event ThisEvent EventName;
    void Start()
    {
        Listen listen = new Listen();
        Debug.Log("i was started");
        listen.Register();
        //EventName += listen.OnEvent;
        OnEventName();
        StartCoroutine(Loop());

    }

    protected virtual void OnEventName()
    {
        if (EventName != null)
        {
            EventName();
        }
    }

    IEnumerator Loop()
    {
        Debug.Log("started coroutine");

        while (true)
        {
            
            Debug.Log("coroutine is running");
            yield return null;
        }
    }

    
}

public class Listen
{

    public void Register()
    {
        TimeScaleTesting.EventName += OnEvent;

    }
    
    public void OnEvent()
    {
        Debug.Log("I was triggered by an event");
    }
}
