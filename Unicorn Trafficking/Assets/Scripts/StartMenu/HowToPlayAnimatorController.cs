using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HowToPlayAnimatorController : MonoBehaviour
{
    public Animator animator;
    public GameObject balloons;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            animator.SetTrigger("Esc");
            Invoke("DisableHTP", 1);
            balloons.SetActive(true);
        }
    }


    public void FirstNext()
    {
        animator.SetTrigger("FirstNext");
    }
    public void SecondNext()
    {
        animator.SetTrigger("SecondNext");
    }
    public void ThirdNext()
    {
        animator.SetTrigger("ThirdNext");
        Invoke("DisableHTP", 1);
    }
    public void FirstBack()
    {
        animator.SetTrigger("FirstBack");
    }
    public void SecondBack()
    {
        animator.SetTrigger("SecondBack");
    }

    public void DisableHTP()
    {
        gameObject.SetActive(false);
    }

    public void HTPStart()
    {
        Invoke("WaitToStart", .5f);
    }

    void WaitToStart()
    {
        animator.SetTrigger("HTPStart");
    }


}
