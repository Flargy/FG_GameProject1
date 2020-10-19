using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PointHandeler : MonoBehaviour
{
    [SerializeField] private float timeLimitMinutes = 10f;
    [SerializeField,Tooltip("Order is pink, blue, yellow, red")] private List<Material> materialList;
    [SerializeField] private float secondsForDelivery = 50f;
    [Range(1, 20)]
    [SerializeField] private float timePercentageCutoff = 5;

    [SerializeField] private int timedComboSeconds = 10;
    [SerializeField] private float timedComboPercentageMultiplier = 5;
    [SerializeField] private int comboBonusPoints = 50;
    [SerializeField] private float multiplierLimit = 5f;
    [SerializeField] private UITimer comboTimeUIElement;
    [SerializeField] private Text comboDisplay;
    [SerializeField] private Text scoreDisplay;
    
    private static PointHandeler instance;

    private float deliveryBase;
    private int currentScore = 0;
    private Array enums;
    private float percentageMultiplier;
    private int comboBonus = 0;
    private float timeComboMultiplier = 1;
    private Coroutine comboRoutine;
    private bool routineRunning = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        instance.deliveryBase = instance.secondsForDelivery;
        instance.percentageMultiplier = 1 - (instance.timePercentageCutoff / 100);

        instance.enums = Enum.GetValues(typeof(UnicornEnum.UnicornTypeEnum));
    }

    public static int GetScore()
    {
        return instance.currentScore;
    }

    public static float GetTime()
    {
        return instance.secondsForDelivery;
    }

    private static void ChangeScore(int points)
    {
        instance.currentScore += (int)((points + instance.comboBonus * instance.comboBonusPoints) * instance.timeComboMultiplier);
       
        instance.scoreDisplay.text = instance.currentScore.ToString();
    }

    public static void FailedDelivery(int points)
    {
        instance.comboBonus = 0;
        UpdateCombo();
        instance.secondsForDelivery = instance.deliveryBase;
        ChangeScore(points);
    }

    private static void UpdateCombo()
    {
        
        instance.comboDisplay.text = instance.comboBonus.ToString();
        
    }

    public static void SuccessfulDelivery(PointCollection script, float delay, int points)
    {
        instance.secondsForDelivery *= instance.percentageMultiplier;
        ChangeScore(points);
        instance.comboBonus++;
        UpdateCombo();
        if (instance.routineRunning == true)
        {
            instance.StopCoroutine(instance.comboRoutine);
        }
        
        instance.comboTimeUIElement.EndTimer();
        

        instance.comboRoutine = instance.StartCoroutine(instance.TimedComboStart());
        instance.StartCoroutine(instance.NewTypeDelay(script, delay));
    }

    private IEnumerator NewTypeDelay(PointCollection target, float delay)
    {
        yield return new WaitForSeconds(delay);
        GetNewUnicorn(target);
    }

    public static void GetNewUnicorn(PointCollection target)
    {
        int value = SpawnSystem.GetRandomWeightIndex(); // here we randomize which unicorn it is that will spawn
        target.ChangeType((UnicornEnum.UnicornTypeEnum)instance.enums.GetValue(value), instance.materialList[value], instance.secondsForDelivery);
    }

    private IEnumerator TimedComboStart()
    {
        instance.comboTimeUIElement.StartTimer(instance.timedComboSeconds);
        instance.routineRunning = true;
        instance.timeComboMultiplier *= 1 + (instance.timedComboPercentageMultiplier / 100);
        Mathf.Clamp(instance.timeComboMultiplier, 1.0f, instance.multiplierLimit);
        yield return new WaitForSeconds(instance.timedComboSeconds);

        instance.timeComboMultiplier = 1;
        instance.routineRunning = false;

    }
    
}
