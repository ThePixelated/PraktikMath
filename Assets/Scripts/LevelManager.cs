using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public SO_ChosenSoal chosenSoalCarrier;
    public List<SO_SoalData> soalEasy = new List<SO_SoalData>();
    public List<SO_SoalData> soalMedium = new List<SO_SoalData>();
    public List<SO_SoalData> soalHard = new List<SO_SoalData>();

    public void ChoseDifficulty(string difficulty)
    {
        difficulty = difficulty.ToLower();

        switch(difficulty)
        {
            case "easy":
                chosenSoalCarrier.ChosenSoal = RandomizeSoal(soalEasy);
                break;
            case "medium":
                chosenSoalCarrier.ChosenSoal = RandomizeSoal(soalMedium);
                break;
            case "hard":
                chosenSoalCarrier.ChosenSoal = RandomizeSoal(soalHard);
                break;
        }
    }

    private SO_SoalData RandomizeSoal(List<SO_SoalData> targetSoalData)
    {
        SO_SoalData chosen = targetSoalData[Random.Range(0, targetSoalData.Count)];
        return chosen;
    }
}
