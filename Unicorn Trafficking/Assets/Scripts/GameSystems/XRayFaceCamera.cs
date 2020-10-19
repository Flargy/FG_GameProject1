using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XRayFaceCamera : MonoBehaviour
{
    [SerializeField] private GameObject seeThrough = null;
    [SerializeField] private LayerMask wallLayer = 0;
    [SerializeField] private Transform cam = null;
    [SerializeField] private float radius = 0.5f;
    [SerializeField] private float maxScale = 4.0f;
    [SerializeField] private float scaleTime = 1.0f;

    private List<Vector3> castPoints;
    private bool isLarge = false;
    private bool isSmall = true;
    private bool coroutineRunning = false;
    private Coroutine activeCoroutine;
    
    private void Awake()
    {
        if (cam == null)
        {
            cam = Camera.main.transform;
        }
        castPoints = new List<Vector3>();
        castPoints.Add(Vector3.up * 0.6f);
        castPoints.Add(Vector3.up * 0.6f + Vector3.right * radius);
        castPoints.Add(Vector3.up * 0.6f + Vector3.left * radius);
        castPoints.Add(Vector3.up * 0.6f + Vector3.up * radius);
        castPoints.Add(Vector3.up * 0.6f + Vector3.down * radius);
    }

    private void FixedUpdate()
    {
        
        for ( int i = 0; i < 5; i++)
        {
            if(Physics.Raycast(transform.TransformPoint( castPoints[i]), cam.position - transform.position, 20.0f, wallLayer, QueryTriggerInteraction.Collide))
            {
                if (isLarge == true)
                {
                    break;
                }
                if (coroutineRunning == true)
                {
                    StopCoroutine(activeCoroutine);
                }

                isLarge = true;
                isSmall = false;
                activeCoroutine = StartCoroutine(ScaleOverTime(maxScale));

            }
            else
            {
                if (isSmall == true)
                {
                    break;
                }
                else if (coroutineRunning == true)
                {
                    StopCoroutine(activeCoroutine);
                }
                isLarge = false;
                isSmall = true;
                activeCoroutine = StartCoroutine(ScaleOverTime(0.0f));
            }
        }
        
    }

    private IEnumerator ScaleOverTime(float newScale)
    {
        coroutineRunning = true;
        Vector3 oldScale = seeThrough.transform.localScale;
        Vector3 targetScale = new Vector3(newScale, newScale, newScale);

        float time = 0.0f;

        while (time < scaleTime)
        {
            time += Time.deltaTime;
            seeThrough.transform.localScale = Vector3.Slerp(oldScale, targetScale, time / scaleTime);
            yield return null;
        }

        coroutineRunning = false;
    }

    
}
