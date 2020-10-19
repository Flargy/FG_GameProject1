using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HighscoreTable : MonoBehaviour
{
    [SerializeField] private List<Transform> entries;
    [SerializeField] private float entrySpacing;
    [SerializeField] private GameObject inputWindow;
    [SerializeField] private InputField playerInput;
    [SerializeField] private Color firstPlace;
    [SerializeField] private Color secondPlace;
    [SerializeField] private Color thirdPlace;
    private List<HighscoreEntry> highscoreEntryList = new List<HighscoreEntry>();
    private Transform highscoreContainer;
    private bool placementFound = false;

    public void InitiateList()
    {
        string jsonString = PlayerPrefs.GetString("highscoreTable");
        Highscores highscores = JsonUtility.FromJson<Highscores>(jsonString);
        if (highscores == null)
        {
            return;
        }
        highscoreContainer = transform.Find("HighscoreContainer");
        for (int i = 0; i < highscores.highscoreEntryList.Count; i++)
        {
            HighscoreEntry currentEntry = new HighscoreEntry() {highScore = highscores.highscoreEntryList[i].highScore, name = highscores.highscoreEntryList[i].name};
            Transform entry = Instantiate(entries[i], highscoreContainer.transform);
            RectTransform entryRectTransform = entry.GetComponent<RectTransform>();
            entryRectTransform.anchoredPosition = new Vector2(0, -entrySpacing * i);
            
            int rank = i + 1;
            entry.Find("EntryBackground").gameObject.SetActive(rank % 2 == 1);
            switch (rank)
            {
                case 1:
                    entry.Find("PosTab").GetComponent<Text>().text = "1ST";
                    entry.Find("Trophy").GetComponent<Image>().color = firstPlace;
                    entry.Find("PosTab").GetComponent<Text>().color = Color.green;
                    entry.Find("ScoreTab").GetComponent<Text>().color = Color.green;
                    entry.Find("NameTab").GetComponent<Text>().color = Color.green;
                    break;
                case 2:
                    entry.Find("PosTab").GetComponent<Text>().text = "2ND";
                    entry.Find("Trophy").GetComponent<Image>().color = secondPlace;
                    break;
                case 3:
                    entry.Find("PosTab").GetComponent<Text>().text = "3RD";
                    entry.Find("Trophy").GetComponent<Image>().color = thirdPlace;
                    break;
                default:
                    entry.Find("PosTab").GetComponent<Text>().text = rank + "TH";
                    entry.Find("Trophy").gameObject.SetActive(false);
                    break;
                        
            }
            
            entry.Find("ScoreTab").GetComponent<Text>().text = currentEntry.highScore.ToString();
            entry.Find("NameTab").GetComponent<Text>().text = currentEntry.name;
            entries[i] = entry;
        }
    }

    public void ShowHighscore()
    {
        InitiateList();
        for (int i = 0; i < entries.Count; i++)
        {
            entries[i].gameObject.SetActive(true);
        }
    }

    public void EnableInput()
    {
        inputWindow.SetActive(true);
    }

    private void CalculateNewHighscore(HighscoreEntry highscoreEntry)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            HighscoreEntry currentEntry = new HighscoreEntry() {highScore = int.Parse(entries[i].Find("ScoreTab").GetComponent<Text>().text), name = entries[i].Find("NameTab").GetComponent<Text>().text};

            if (highscoreEntry.highScore > currentEntry.highScore && !placementFound)
            {
                currentEntry = highscoreEntry;
                placementFound = true;
            }
            entries[i].gameObject.SetActive(true);
            entries[i].Find("ScoreTab").GetComponent<Text>().text = currentEntry.highScore.ToString();
            entries[i].Find("NameTab").GetComponent<Text>().text = currentEntry.name;
        }
    }
    
    public void SubmitEntry()
    {
        if(playerInput.text == "") {return;}
        HighscoreEntry entry = new HighscoreEntry {highScore = PointHandeler.GetScore(), name = playerInput.text};
        CalculateNewHighscore(entry);
        for (int i = 0; i < entries.Count; i++)
        {
            HighscoreEntry currentEntry = new HighscoreEntry {
                highScore = int.Parse(entries[i].Find("ScoreTab").GetComponent<Text>().text), 
                name = entries[i].Find("NameTab").GetComponent<Text>().text};
            highscoreEntryList.Add(currentEntry);
        }
        Highscores highscores = new Highscores{highscoreEntryList = highscoreEntryList};
        string json = JsonUtility.ToJson(highscores);
        PlayerPrefs.SetString("highscoreTable", json);
        PlayerPrefs.Save();
        inputWindow.SetActive(false);
    }

    public void ResetEntries()
    {
        for(int i = 0; i<entries.Count; i++) 
        {
            HighscoreEntry currentEntry = new HighscoreEntry {
                highScore = int.Parse(entries[i].Find("ScoreTab").GetComponent<Text>().text), 
                name = entries[i].Find("NameTab").GetComponent<Text>().text};
            highscoreEntryList.Add(currentEntry);
        }
        Highscores highscores = new Highscores{highscoreEntryList = highscoreEntryList};
        string json = JsonUtility.ToJson(highscores);
        PlayerPrefs.SetString("highscoreTable", json);
        PlayerPrefs.Save();
    }

    private class Highscores
    {
        public List<HighscoreEntry> highscoreEntryList;
    }
    
    [System.Serializable]
    public class HighscoreEntry
    {
        public string name;
        public int highScore;
    }
}
