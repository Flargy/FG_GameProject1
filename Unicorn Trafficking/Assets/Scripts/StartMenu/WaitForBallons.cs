using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitForBallons : MonoBehaviour
{
    public GameObject audioSource;
    public GameObject ballonsLoop;
    public GameObject directionalLight;
    
    // Start is called before the first frame update
    void Start()
    {
        Invoke("ActivateBallons", 6);
    }


    void ActivateBallons()
    {
        ballonsLoop.SetActive(true);
        directionalLight.SetActive(true);
        audioSource.SetActive(true);
    }



}
