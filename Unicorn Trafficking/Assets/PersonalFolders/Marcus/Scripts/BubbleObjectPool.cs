using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleObjectPool : MonoBehaviour
{
    private static BubbleObjectPool instance;

    [SerializeField] private GameObject bubblePrefab;

    private List<GameObject> bubbleList = new List<GameObject>();
    
    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }

        GameObject firstBubble = Instantiate(instance.bubblePrefab);
        firstBubble.SetActive(false);
        instance.bubbleList.Add(firstBubble);
    }

    public static GameObject GetBubble()
    {
        GameObject fetchedBubble = null;
        if (instance.bubbleList.Count > 0)
        {
            fetchedBubble = instance.bubbleList[0];
        }
        else
        {
            fetchedBubble = Instantiate(instance.bubblePrefab);
            fetchedBubble.SetActive(false);
        }

        return fetchedBubble;
    }

    public static void ReturnBubble(GameObject bubble)
    {
        instance.bubbleList.Add(bubble);
        bubble.SetActive(false);
    }
}
