using System.Collections;
using UnityEngine;

public class DropGum : MonoBehaviour
{
    [SerializeField] private GameObject gum;
    [SerializeField] private LayerMask hitLayer;
    [SerializeField] private float cooldown = 4.0f;
    [SerializeField] private float dropDistance = 1.3f;

    private bool cooldownFinished = true;

    private RaycastHit hit;
    void Update()
    {
        if (cooldownFinished && Input.GetButtonDown("Fire2"))
        {
            if (Physics.Raycast(transform.position + Vector3.up * 0.7f - transform.forward * dropDistance ,  -Vector3.up , out hit, Mathf.Infinity, hitLayer))
            {
                Instantiate(gum, hit.point, Quaternion.identity);
                StartCoroutine(CooldownTime());
            }
        }
    }
    

    private IEnumerator CooldownTime()
    {
        cooldownFinished = false;
        yield return new WaitForSeconds(cooldown);
        cooldownFinished = true;
    }
}
