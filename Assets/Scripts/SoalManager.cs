using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SoalManager : MonoBehaviour
{
    public SO_ChosenSoal chosenSoalCarrier;
    public ClockController clockController;

    [Header("UI Config Soal")]
    [SerializeField] private TextMeshProUGUI soalTxt;
    [SerializeField] private TextMeshProUGUI timerTxt;
    [SerializeField] private TextMeshProUGUI currentSoalTxt;
    [SerializeField] private Image gambarSoalImg;

    [Header("UI Opsi Jawaban")]
    [SerializeField] private Button opsiABtn;
    [SerializeField] private Button opsiBBtn;
    [SerializeField] private Button opsiCBtn;
    [SerializeField] private TextMeshProUGUI opsiATxt;
    [SerializeField] private TextMeshProUGUI opsiBTxt;
    [SerializeField] private TextMeshProUGUI opsiCTxt;

    [Header("UI Indikator Salah")]
    [SerializeField] private List<Image> wrongIndicatorImage = new List<Image>();
    [SerializeField] private Sprite defaultIndicator;
    [SerializeField] private Sprite wrongIndicator;

    [SerializeField] private GameObject endPanel;
    [SerializeField] private float timer;

    private SO_SoalData _currentSoalData;
    private List<Soal> _randomizeSO_SoalData = new List<Soal>();
    private Soal _currentSoal;
    private int _stepIndex;
    private int _wrongCount;
    private bool isSoalEnd;

    private void Start()
    {
        // 1. Ambil data asli
        var originalSoals = chosenSoalCarrier.ChosenSoal.Soal;

        // 2. Buat list baru agar tidak merusak SO asli
        List<Soal> shuffledSoals = new List<Soal>(originalSoals);

        // 3. Shuffle Soal
        for (int i = 0; i < shuffledSoals.Count; i++)
        {
            Soal temp = shuffledSoals[i];
            int randomIndex = Random.Range(i, shuffledSoals.Count);
            shuffledSoals[i] = shuffledSoals[randomIndex];
            shuffledSoals[randomIndex] = temp;
        }

        // 4. Shuffle Opsi Jawaban (Copy dulu list opsinya agar tidak merusak data SO)
        foreach (var soal in shuffledSoals)
        {
            // Buat copy list opsi agar tidak merusak data aslinya
            List<string> shuffledOpsi = new List<string>(soal.OpsiSoal);

            // Shuffle manual
            for (int i = 0; i < shuffledOpsi.Count; i++)
            {
                string temp = shuffledOpsi[i];
                int randomIndex = Random.Range(i, shuffledOpsi.Count);
                shuffledOpsi[i] = shuffledOpsi[randomIndex];
                shuffledOpsi[randomIndex] = temp;
            }

            // Sekarang update property objek soal dengan list yang sudah diacak
            soal.OpsiSoal = shuffledOpsi;
        }

        // 5. Gunakan hasil akhir
        _randomizeSO_SoalData = shuffledSoals;
        _currentSoal = _randomizeSO_SoalData[0];

        UpdateUI();
    }

    private void Update()
    {
        if (!isSoalEnd)
        {
            if (timer > 0)
            {
                timer -= Time.deltaTime;
                timerTxt.text = Mathf.FloorToInt(timer % 60).ToString() + "s";
            }
            else
            {
                timer = 0;
                timerTxt.text = "0";
                SceneManager.LoadScene("LevelGagalWaktu");
            }
        }

        if (_wrongCount >= 3)
        {
            SceneManager.LoadScene("LevelGagalSalah");
        }
    }

    public void MemilihJawaban(string value)
    {
        _stepIndex++;

        if (value != _currentSoal.JawabanSoal)
        {
            Debug.Log("JAWABAN SALAH");
            _wrongCount++;
            wrongIndicatorImage[_wrongCount - 1].sprite = wrongIndicator;
        }

        if (_stepIndex >= _randomizeSO_SoalData.Count)
        {
            isSoalEnd = true;

            // Save data
            int levelStage = PlayerPrefs.GetInt("LevelProgress", 1);
            levelStage++;

            if (levelStage > 3)
            {
                levelStage = 4;
            }

            PlayerPrefs.SetInt("LevelProgress", levelStage);
            PlayerPrefs.Save();

            SceneManager.LoadScene("LevelBerhasil");
            return;
        }

        //_currentSoal = _currentSoalData.Soal[_stepIndex];
        _currentSoal = _randomizeSO_SoalData[_stepIndex];
        UpdateUI();
    }

    private void UpdateUI()
    {
        //gambarSoalImg.sprite = _currentSoal.ImageSoal;

        soalTxt.text = _currentSoal.TextSoal;
        currentSoalTxt.text = $"{_stepIndex + 1}/{_randomizeSO_SoalData.Count.ToString()}";

        opsiATxt.text = _currentSoal.OpsiSoal[0];
        opsiBTxt.text = _currentSoal.OpsiSoal[1];
        opsiCTxt.text = _currentSoal.OpsiSoal[2];

        opsiABtn.GetComponent<ButtonsData>().holdAnswer = _currentSoal.OpsiSoal[0];
        opsiBBtn.GetComponent<ButtonsData>().holdAnswer = _currentSoal.OpsiSoal[1];
        opsiCBtn.GetComponent<ButtonsData>().holdAnswer = _currentSoal.OpsiSoal[2];

        opsiABtn.onClick.RemoveAllListeners();
        opsiABtn.onClick.AddListener(() => MemilihJawaban(opsiABtn.GetComponent<ButtonsData>().holdAnswer));

        opsiBBtn.onClick.RemoveAllListeners();
        opsiBBtn.onClick.AddListener(() => MemilihJawaban(opsiBBtn.GetComponent<ButtonsData>().holdAnswer));

        opsiCBtn.onClick.RemoveAllListeners();
        opsiCBtn.onClick.AddListener(() => MemilihJawaban(opsiCBtn.GetComponent<ButtonsData>().holdAnswer));

        if (clockController != null)
        {
            clockController.SetClockFromstring(_currentSoal.JawabanSoal);
            Debug.Log(_currentSoal.JawabanSoal);
        }
    }
}
