using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndStartAnimation : MonoBehaviour
{
    public float destroyTimer;

    private void Start()
    {
        Destroy(gameObject, destroyTimer);
    }


}
