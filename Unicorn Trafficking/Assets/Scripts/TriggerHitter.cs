
using System;
using System.Collections.Generic;
using UnityEngine;

public class TriggerHitter : MonoBehaviour
{
    private delegate void ReactToTrigger(GameObject ourGO, ITriggerZone other);
    private static readonly Dictionary<Type,ReactToTrigger> Reactions =
        new Dictionary<Type,ReactToTrigger>
    {
        {typeof(ChewingGum), ChewingGumReaction}
    };
    
    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        var triggerZone = other.gameObject.GetComponent<ITriggerZone>();
        if (triggerZone == null) return;
        
        if (Reactions.TryGetValue(triggerZone.GetType(), out ReactToTrigger Reaction))
        {
            Reaction(gameObject, triggerZone);
            triggerZone.Trigger(gameObject);
        }
    }

    static void ChewingGumReaction(GameObject ourGO, ITriggerZone other)
    {
        Debug.Log("Chewing Gum Ohnoes!");
        var chewingGum = (ChewingGum) other;
        var debuff = ourGO.AddComponent<ChewingGumDebuff>();
        debuff.snapDistance = chewingGum.snapDistance;
        debuff.maximumSlow = chewingGum.maximumSlow;
        debuff.minimumSlow = chewingGum.minimumSlow;
    }
}