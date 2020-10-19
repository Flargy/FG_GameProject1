using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    public GameObject howToPlay;
    public GameObject bnNext;
    
    public void StartGame()
    {
        Invoke("WaitForAnimation", 1);
    }

    public void Quit()
    {
        Application.Quit();
    }


    public void WaitForAnimation()
    {
        SceneManager.LoadScene(1);
    }


    public void HowToPlay()
    {
        Invoke("WaitForAbit", 0.5f);
    }

    void WaitForAbit()
    {
        howToPlay.SetActive(true);
        bnNext.SetActive(true);
        gameObject.SetActive(false);
    }

}
