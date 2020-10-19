using UnityEngine;

public class ChewingGumDebuff : MonoBehaviour
{
    private IMovingActor movingActor;
    private Vector3 anchorPoint;

    public float snapDistance;
    public float maximumSlow;
    public float minimumSlow;
    private void Awake()
    {
        movingActor = GetComponent<IMovingActor>();
        anchorPoint = transform.position;
    }

    private void Update()
    {
        Vector3 anchorToTransform = transform.position - anchorPoint;
        var snapDistanceSq = snapDistance * snapDistance;
        float sqDistance = Vector3.Dot(anchorToTransform, anchorToTransform);
        if (sqDistance >= snapDistanceSq)
        {
            AudioController.Instance.GenerateAudio(AudioController.ClipName.GumSnapping, transform.position, 0.5f);
            movingActor.SetSpeedModifier(1.0f);
            Destroy(this);
            return;
        }

        float moveMultiplier = Mathfs.RemapClamped(0.0f, snapDistanceSq,
            1.0f-minimumSlow, 1.0f-maximumSlow,sqDistance);

        movingActor.SetSpeedModifier(moveMultiplier);
    }
}
