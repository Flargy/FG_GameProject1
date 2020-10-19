using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CheckUnicornVelocity : MonoBehaviour
{
    [SerializeField] public NavMeshAgent agent;
    [SerializeField] public Transform particlePoint;
    private bool used = true;
    void Update()
    {
        if (agent.velocity.magnitude > 1)
        {
            SpawnParticle();
        }
    }

    void SpawnParticle()
    {
        if (used == true)
        {
            ParticleSpawner.SpawnParticleEffect(ParticleSpawner.Particles.UnicornRun, particlePoint.position);
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
