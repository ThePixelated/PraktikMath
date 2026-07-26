using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    public SceneManagement m_sceneManagement;
    public SO_ChosenSoal chosenSoalCarrier;
    public List<SO_SoalData> soalEasy = new List<SO_SoalData>();
    public List<SO_SoalData> soalMedium = new List<SO_SoalData>();
    public List<SO_SoalData> soalHard = new List<SO_SoalData>();

    public int levelState = 1;

    public GameObject btnSpriteMedium;
    public GameObject btnSpriteHard;
    //public Sprite[] lockedMedium;
    public Sprite[] unlockedMedium;
    //public Sprite[] lockedHard;
    public Sprite[] unlockedHard;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        levelState = PlayerPrefs.GetInt("LevelProgress", 1);
        //Debug.Log("test");
        InnitLevel();
    }

    private void InnitLevel()
    {
        if (levelState >= 2)
        {
            Button btnMedium = btnSpriteMedium.GetComponent<Button>();
            Image imageMedium = btnSpriteMedium.GetComponent<Image>();

            imageMedium.sprite = unlockedMedium[0];

            SpriteState st = btnMedium.spriteState;
            st.highlightedSprite = unlockedMedium[1];
            st.pressedSprite = unlockedMedium[2];
            st.selectedSprite = unlockedMedium[2];
            btnMedium.spriteState = st;

            //btnMedium.interactable = true;
            //Debug.Log("A");
        }

        if (levelState >= 3)
        {
            Button btnHard = btnSpriteHard.GetComponent<Button>();
            Image imageHard = btnSpriteHard.GetComponent<Image>();

            imageHard.sprite = unlockedHard[0];

            SpriteState st = btnHard.spriteState;
            st.highlightedSprite = unlockedHard[1];
            st.pressedSprite = unlockedHard[2];
            st.selectedSprite = unlockedHard[2];
            btnHard.spriteState = st;

            //btnMedium.interactable = true;
            //Debug.Log("B");
        }
    }

    public void ChoseDifficulty(string difficulty)
    {
        difficulty = difficulty.ToLower();

        switch(difficulty)
        {
            case "easy":
                chosenSoalCarrier.ChosenSoal = RandomizeSoal(soalEasy);
                m_sceneManagement.SceneLoader("Soal");
                break;
            case "medium":
                chosenSoalCarrier.ChosenSoal = RandomizeSoal(soalMedium);
                if (levelState >= 2)
                {
                    m_sceneManagement.SceneLoader("Soal");
                }
                break;
            case "hard":
                chosenSoalCarrier.ChosenSoal = RandomizeSoal(soalHard);
                if(levelState >= 3)
                {
                    m_sceneManagement.SceneLoader("Soal");
                }
                break;
        }
    }

    public void NextLevel()
    {
        levelState = PlayerPrefs.GetInt("LevelProgress", 1);

        switch (levelState)
        {
            case 2:
                chosenSoalCarrier.ChosenSoal = RandomizeSoal(soalMedium);
                m_sceneManagement.SceneLoader("Soal");
                break;
            case 3:
                chosenSoalCarrier.ChosenSoal = RandomizeSoal(soalHard);
                m_sceneManagement.SceneLoader("Soal");
                break;
            default:
                m_sceneManagement.SceneLoader("PilihLevel");
                break;
        }
    }

    private SO_SoalData RandomizeSoal(List<SO_SoalData> targetSoalData)
    {
        SO_SoalData chosen = targetSoalData[Random.Range(0, targetSoalData.Count)];
        return chosen;
    }
}
