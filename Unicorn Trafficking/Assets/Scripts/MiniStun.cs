using UnityEngine;

public class MiniStun : MonoBehaviour
{
    public float duration = 1.0f;
    private NewPlayerMovement playerMovement;
    private void Start()
    {
        playerMovement = GetComponent<NewPlayerMovement>();
        playerMovement.enabled = false;
    }

    private void Update()
    {
        duration -= Time.deltaTime;
        if (duration > 0.0f) return;

        playerMovement.enabled = true;
        Destroy(this);
    }
}
