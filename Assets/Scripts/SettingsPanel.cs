using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    [Header("SFX")]
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Button sfxMuteButton;
    [SerializeField] private Image sfxIcon;
    [SerializeField] private Sprite sfxOnSprite;
    [SerializeField] private Sprite sfxOffSprite;

    [Header("BGM")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Button bgmMuteButton;
    [SerializeField] private Image bgmIcon;
    [SerializeField] private Sprite bgmOnSprite;
    [SerializeField] private Sprite bgmOffSprite;

    private void OnEnable()
    {
        RefreshUI();

        sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
        bgmSlider.onValueChanged.AddListener(OnBgmSliderChanged);
        sfxMuteButton.onClick.AddListener(OnSfxMuteClicked);
        bgmMuteButton.onClick.AddListener(OnBgmMuteClicked);
    }

    private void OnDisable()
    {
        sfxSlider.onValueChanged.RemoveListener(OnSfxSliderChanged);
        bgmSlider.onValueChanged.RemoveListener(OnBgmSliderChanged);
        sfxMuteButton.onClick.RemoveListener(OnSfxMuteClicked);
        bgmMuteButton.onClick.RemoveListener(OnBgmMuteClicked);
    }

    private void RefreshUI()
    {
        var am = AudioManager.Instance;

        sfxSlider.SetValueWithoutNotify(am.SfxMuted ? 0f : am.SfxVolume);
        bgmSlider.SetValueWithoutNotify(am.BgmMuted ? 0f : am.BgmVolume);

        sfxIcon.sprite = am.SfxMuted ? sfxOffSprite : sfxOnSprite;
        bgmIcon.sprite = am.BgmMuted ? bgmOffSprite : bgmOnSprite;
    }

    private void OnSfxSliderChanged(float value)
    {
        AudioManager.Instance.SetSfxVolume(value);
        sfxIcon.sprite = AudioManager.Instance.SfxMuted ? sfxOffSprite : sfxOnSprite;
    }

    private void OnBgmSliderChanged(float value)
    {
        AudioManager.Instance.SetBgmVolume(value);
        bgmIcon.sprite = AudioManager.Instance.BgmMuted ? bgmOffSprite : bgmOnSprite;
    }

    private void OnSfxMuteClicked()
    {
        AudioManager.Instance.ToggleSfxMute();
        RefreshUI();
    }

    private void OnBgmMuteClicked()
    {
        AudioManager.Instance.ToggleBgmMute();
        RefreshUI();
    }

    public void PlayAudio(string targetAudio)
    {
        AudioManager.Play(targetAudio);
    }
}