using UnityEngine;
using TMPro;

public class ResultDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI resultText;

    // Isi di Inspector per-scene: "CompleteQuiz" (LevelBerhasil) atau "FailedQuiz" (LevelGagal*)
    [SerializeField] private string resultSfx;

    void Start()
    {
        resultText.text = $"Result: {SoalResultData.CorrectCount*20}/{SoalResultData.TotalSoal*20}";

        if (!string.IsNullOrEmpty(resultSfx))
            AudioManager.Play(resultSfx);
    }
}