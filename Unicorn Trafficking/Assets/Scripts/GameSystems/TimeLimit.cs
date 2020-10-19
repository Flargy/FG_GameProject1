using UnityEngine;
using UnityEngine.UI;

public class TimeLimit : MonoBehaviour
{
    public delegate void StopAllFunctions();
    public static event StopAllFunctions Pause;
    
    [SerializeField] private float gameTimerInSeconds;
    [SerializeField] private Text uiText;
    [SerializeField] private GameObject highscoreTable;
    private bool endGameInitiated = false;

    
    
    private void Update()
    {
        SetTimer();
    }

    private void SetTimer()
    {
        if (gameTimerInSeconds > 1)
        {
            gameTimerInSeconds -= Time.deltaTime;

            int secondsFormat = Mathf.FloorToInt(gameTimerInSeconds % 60);
            int minutesFormat = Mathf.FloorToInt(gameTimerInSeconds / 60);

            uiText.text = string.Format("{0:00}:{1:00}", minutesFormat, secondsFormat);
        }
        
        if ((int)gameTimerInSeconds == 0 && !endGameInitiated)
        {
            EndGame();
            endGameInitiated = true;
        }
    }

    protected virtual void OnPuase()
    {
        if (Pause != null)
        {
            Pause();
        }
    }

    private void EndGame()
    {
        OnPuase();
        highscoreTable.SetActive(true);
        highscoreTable.GetComponent<HighscoreTable>().EnableInput();
        highscoreTable.GetComponent<HighscoreTable>().ShowHighscore();
        //TODO Run the code for when the timer reaches 0
    }
}
