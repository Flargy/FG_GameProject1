using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UITimer : MonoBehaviour
{
    private Image image;
    private float timeLimit;
    private bool timerRunning = false;
    private Coroutine timerRoutine;
    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void StartTimer(float time)
    {
        timeLimit = time;
        if (timerRunning == true)
        {
            StopCoroutine(timerRoutine);
        }
        
        timerRoutine = StartCoroutine(Timer());

    }

    public void EndTimer()
    {
        if (timerRunning == true)
        {
            StopCoroutine(timerRoutine);
        }

        image.fillAmount = 0f;
    }

    private IEnumerator Timer()
    {
        timerRunning = true;
        image.fillAmount = 1f;
        float passedTime = 0f;

        while (passedTime <= timeLimit)
        {
            passedTime += Time.deltaTime;
            image.fillAmount = 1 - (passedTime / timeLimit);
            yield return null;
        }

        timerRunning = false;
    }
}
