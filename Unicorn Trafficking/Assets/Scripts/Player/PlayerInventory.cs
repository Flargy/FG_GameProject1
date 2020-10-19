using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private static PlayerInventory instance;
    
    [SerializeField] private int maxCarriedUnicorns = 3;
    [SerializeField] private List<GameObject> unicornList;

    [Header("Balloon Strings")] [SerializeField, Tooltip("Where do we draw the balloon string from?")]
    private GameObject clownHand;
    [SerializeField, Tooltip("Number of line segments to draw. Tune to be as low as you can while still looking ok.")]
    private int detail = 16;
    [SerializeField, Range(0.1f, 2.0f),
     Tooltip("Adjusts the lowest point that the string can possibly reach. Shorten to stop it from clipping through the ground.")]
    private float maximumSag = 0.5f;
    [SerializeField] private Material stringMaterial;
    [SerializeField, Range(0.01f, 0.3f)] private float stringThickness = 0.1f;
    [SerializeField] private Color stringColor = Color.white;
    
    private List<BalloonString> balloonStrings;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        unicornList = new List<GameObject>();
        balloonStrings = new List<BalloonString>();
    }

    public static List<GameObject> GetAllUnicorns() 
    {
        
        List<GameObject> returnList = instance.unicornList;
        instance.unicornList.Clear();
        return returnList;
    }

    public static int Count()
    {
        return instance?.unicornList?.Count ?? 0;
    }

    private void Update()
    {
        foreach (var balloonString in balloonStrings)
        {
            balloonString.Update();
        }
    }

    public static GameObject GetUnicorn(UnicornEnum.UnicornTypeEnum type)
    {
        GameObject correctUni = instance.unicornList.Find(x => x.GetComponent<UnicornStats>().colorType == type);
        if (correctUni == null) return null;
        
        instance.unicornList.Remove(correctUni);
        RemoveBalloonString(correctUni);

        return correctUni;
    }

    public static List<UnicornEnum.UnicornTypeEnum> ListOfUnicornTypes()
    {
        var retVal = new List<UnicornEnum.UnicornTypeEnum>();
        foreach (var unicorn in instance.unicornList)
        {
            var stats = unicorn.GetComponent<UnicornStats>();
            retVal.Add(stats.colorType);
        }
        return retVal;
    }
    
    public static void AddUnicorn(GameObject unicorn)
    {
        if (instance.unicornList.Count >= instance.maxCarriedUnicorns)
        {
            return;
        }

        instance.AddBalloonString(unicorn);
        instance.unicornList.Add(unicorn);
    }

    private void AddBalloonString(GameObject unicorn)
    {
        if(!unicorn.TryGetComponent<CapturedEnemy>(out var capturedEnemyComponent))
            Debug.LogWarning("Something went horribly wrong, this unicorn has no CapturedEnemy component...");
        
        var maxBubbleDistance = capturedEnemyComponent.MaxDistanceToPlayer;
        var balloonString = new BalloonString(clownHand, capturedEnemyComponent, maxBubbleDistance, maximumSag, stringThickness, stringMaterial, stringColor, detail);
        balloonStrings.Add(balloonString);
    }

    public static void DropAUnicorn()
    {
        int max = instance.unicornList.Count;
        if (max > 0)
        {
            int random = Random.Range(0, max - 1);
            GameObject droppedUnicorn = instance.unicornList[random];
            instance.unicornList.Remove(droppedUnicorn);
            RemoveBalloonString(droppedUnicorn);
            droppedUnicorn.GetComponent<CapturedEnemy>().Reset();
            
        }
    }

    private static void RemoveBalloonString(GameObject unicorn)
    {
        BalloonString balloonString = instance.balloonStrings.Find(x => x.TrackedUnicorn() == unicorn);
        instance.balloonStrings.Remove(balloonString);
        balloonString?.Dispose();
    }
    
}
