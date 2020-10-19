using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HowToPlay : MonoBehaviour
{
    public Animator animator;
    public GameObject startMenu;
    
    void ReturnToMain()
    {
        startMenu.SetActive(true);
        gameObject.SetActive(false);
    }


    public void NextPressed()
    {
        animator.SetTrigger("NextPressed");
    }

    public void NextPressed2()
    {
        animator.SetTrigger("NextPressed2");
        Invoke("ReturnToMain", 1);
    }
}
