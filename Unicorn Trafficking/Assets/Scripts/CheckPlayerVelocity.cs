using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPlayerVelocity : MonoBehaviour
{
    [SerializeField] public Rigidbody rb;
    [SerializeField] public Transform particlePoint;
    private bool used = true;

    private NewPlayerMovement playerMovement;
    
    private void Awake()
    {
        playerMovement = GetComponent<NewPlayerMovement>();
    }

    void Update()
    {
        if ((rb.velocity.magnitude > 1) && playerMovement.IsGrounded())
        {
            SpawnParticle();
        }
    }

    void SpawnParticle()
    {
        if (used == true)
        {
            ParticleSpawner.SpawnParticleEffect(ParticleSpawner.Particles.ClownWalk, particlePoint.position);
            StartCoroutine(WaitForIt());
        }
    }

    IEnumerator WaitForIt()
    {
        used = false;
        yield return new WaitForSeconds(0.1f);
        used = true;
    }

}
