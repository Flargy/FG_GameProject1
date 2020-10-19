
using UnityEngine;

public class UnicornStats : MonoBehaviour
{
    public UnicornEnum.UnicornTypeEnum colorType;
    [SerializeField] private int weight = 1;
    [SerializeField] private Sprite displayImage;

    private Transform spawn;
    
    public void SetSpawn(Transform spawnLocation)
    {
        spawn = spawnLocation;
    }

    public Transform GetSpawn()
    {
        return spawn;
    }

    public void GetCaptured()
    {
        PlayerInventory.AddUnicorn(gameObject);
    }

    public void GetPooled()
    {
        UnicornPool.AddToList(colorType, gameObject);
    }

    public int GetWeight()
    {
        return weight;
    }

    public void IncreaseWeight(int newValue)
    {
        weight = newValue;
    }

    public Sprite GetSprite()
    {
        return displayImage;
    }
    
}
