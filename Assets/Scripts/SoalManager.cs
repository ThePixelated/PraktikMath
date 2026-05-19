using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SoalManager : MonoBehaviour
{
    public SO_ChosenSoal chosenSoalCarrier;

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
    private Soal _currentSoal;
    private int _stepIndex;
    private int _wrongCount;
    private bool isSoalEnd;

    private void Start()
    {
        _currentSoalData = chosenSoalCarrier.ChosenSoal;
        _currentSoal = _currentSoalData.Soal[0];

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
                SceneManager.LoadScene("LevelGagal");
            }
        }

        if (_wrongCount >= 3)
        {
            SceneManager.LoadScene("LevelGagal");
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

        if (_stepIndex >= _currentSoalData.Soal.Count)
        {
            isSoalEnd = true;
            endPanel.SetActive(true);
            return;
        }

        _currentSoal = _currentSoalData.Soal[_stepIndex];
        UpdateUI();
    }

    private void UpdateUI()
    {
        //gambarSoalImg.sprite = _currentSoal.ImageSoal;

        soalTxt.text = _currentSoal.TextSoal;
        currentSoalTxt.text = $"{_stepIndex + 1}/{_currentSoalData.Soal.Count.ToString()}";

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
    }
}
