using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("SFX")]
    [SerializeField] private SoundEffectLibrary sfxLibrary;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource sfxRandomPitchSource;

    [Header("BGM")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioClip initialBgmClip; // assign di Inspector

    [Header("Volume Curve")]
    // dB di posisi slider paling kiri (tepat di atas 0). Makin negatif = makin
    // "terjun" pelannya. -30 s/d -40 biasanya enak buat musik. Bisa di-tweak di Inspector.
    [SerializeField] private float minVolumeDb = -30f;

    // State (dibaca oleh SettingsPanel)
    public float SfxVolume { get; private set; } = 1f;
    public float BgmVolume { get; private set; } = 1f;
    public bool SfxMuted { get; private set; }
    public bool BgmMuted { get; private set; }

    private float lastSfxVolume = 1f;
    private float lastBgmVolume = 1f;

    private const string PREF_SFX_VOL = "SfxVolume";
    private const string PREF_BGM_VOL = "BgmVolume";
    private const string PREF_SFX_MUTE = "SfxMuted";
    private const string PREF_BGM_MUTE = "BgmMuted";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadPrefs();
            ApplySfxVolume();
            ApplyBgmVolume();

            PlayBGM(initialBgmClip); // langsung main BGM begitu instance ini pertama kali dibuat
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ───────────────────────────────
    // SFX
    // ───────────────────────────────

    public static void Play(string soundName, bool randomPitch = false)
    {
        if (Instance == null) return;

        AudioClip clip = Instance.sfxLibrary.GetRandomClip(soundName);
        if (clip == null) return;

        if (randomPitch)
        {
            Instance.sfxRandomPitchSource.pitch = Random.Range(1f, 1.5f);
            Instance.sfxRandomPitchSource.PlayOneShot(clip);
        }
        else
        {
            Instance.sfxSource.PlayOneShot(clip);
        }
    }

    public void SetSfxVolume(float value)
    {
        if (value <= 0f)
        {
            if (!SfxMuted) lastSfxVolume = SfxVolume > 0f ? SfxVolume : lastSfxVolume;
            SfxMuted = true;
            SfxVolume = 0f;
        }
        else
        {
            SfxMuted = false;
            SfxVolume = value;
        }
        ApplySfxVolume();
        SavePrefs();
    }

    public void ToggleSfxMute()
    {
        if (!SfxMuted)
        {
            lastSfxVolume = SfxVolume > 0f ? SfxVolume : lastSfxVolume;
            SfxMuted = true;
        }
        else
        {
            SfxMuted = false;
            SfxVolume = lastSfxVolume > 0f ? lastSfxVolume : 1f;
        }
        ApplySfxVolume();
        SavePrefs();
    }

    private void ApplySfxVolume()
    {
        float actual = SfxMuted ? 0f : SliderToAmplitude(SfxVolume);
        sfxSource.volume = actual;
        sfxRandomPitchSource.volume = actual;
    }

    // ───────────────────────────────
    // BGM
    // ───────────────────────────────

    public void PlayBGM(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;
        if (bgmSource.clip == clip && bgmSource.isPlaying) return; // hindari restart clip yang sama

        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.Play();
        ApplyBgmVolume();
    }

    public void SetBgmVolume(float value)
    {
        if (value <= 0f)
        {
            if (!BgmMuted) lastBgmVolume = BgmVolume > 0f ? BgmVolume : lastBgmVolume;
            BgmMuted = true;
            BgmVolume = 0f;
        }
        else
        {
            BgmMuted = false;
            BgmVolume = value;
        }
        ApplyBgmVolume();
        SavePrefs();
    }

    public void ToggleBgmMute()
    {
        if (!BgmMuted)
        {
            lastBgmVolume = BgmVolume > 0f ? BgmVolume : lastBgmVolume;
            BgmMuted = true;
        }
        else
        {
            BgmMuted = false;
            BgmVolume = lastBgmVolume > 0f ? lastBgmVolume : 1f;
        }
        ApplyBgmVolume();
        SavePrefs();
    }

    private void ApplyBgmVolume()
    {
        bgmSource.volume = BgmMuted ? 0f : SliderToAmplitude(BgmVolume);
    }

    private Coroutine bgmFadeRoutine;

    // Hentikan sementara BGM (mis. saat video diputar). Tidak menyentuh
    // setting volume/mute user — posisi lagu tetap tersimpan.
    public void PauseBgm()
    {
        if (bgmFadeRoutine != null) { StopCoroutine(bgmFadeRoutine); bgmFadeRoutine = null; }
        if (bgmSource.isPlaying) bgmSource.Pause();
    }

    // Lanjutkan BGM dari titik terakhir. fadeDuration > 0 -> volume naik perlahan.
    public void ResumeBgm(float fadeDuration = 0f)
    {
        if (bgmSource.clip == null) return;
        if (!bgmSource.isPlaying) bgmSource.UnPause();

        float target = BgmMuted ? 0f : SliderToAmplitude(BgmVolume);

        if (bgmFadeRoutine != null) { StopCoroutine(bgmFadeRoutine); bgmFadeRoutine = null; }

        if (fadeDuration <= 0f)
        {
            bgmSource.volume = target;
            return;
        }

        bgmSource.volume = 0f;
        bgmFadeRoutine = StartCoroutine(FadeBgmVolume(target, fadeDuration));
    }

    private IEnumerator FadeBgmVolume(float target, float duration)
    {
        float start = bgmSource.volume;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }
        bgmSource.volume = target;
        bgmFadeRoutine = null;
    }

    // ───────────────────────────────
    // Volume curve
    // ───────────────────────────────

    // Ubah nilai slider (0..1 linear) jadi amplitudo yang terasa logaritmik di telinga.
    // slider 1.0 -> 0 dB (penuh), slider makin kecil -> turun secara dB, 0 -> senyap.
    private float SliderToAmplitude(float sliderValue)
    {
        if (sliderValue <= 0f) return 0f;
        float db = Mathf.Lerp(minVolumeDb, 0f, sliderValue);
        return Mathf.Pow(10f, db / 20f);
    }

    // ───────────────────────────────
    // Persistence (PlayerPrefs) — optional tapi disarankan
    // ───────────────────────────────

    private void SavePrefs()
    {
        PlayerPrefs.SetFloat(PREF_SFX_VOL, SfxMuted ? lastSfxVolume : SfxVolume);
        PlayerPrefs.SetFloat(PREF_BGM_VOL, BgmMuted ? lastBgmVolume : BgmVolume);
        PlayerPrefs.SetInt(PREF_SFX_MUTE, SfxMuted ? 1 : 0);
        PlayerPrefs.SetInt(PREF_BGM_MUTE, BgmMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadPrefs()
    {
        lastSfxVolume = PlayerPrefs.GetFloat(PREF_SFX_VOL, 1f);
        lastBgmVolume = PlayerPrefs.GetFloat(PREF_BGM_VOL, 1f);
        SfxMuted = PlayerPrefs.GetInt(PREF_SFX_MUTE, 0) == 1;
        BgmMuted = PlayerPrefs.GetInt(PREF_BGM_MUTE, 0) == 1;

        SfxVolume = SfxMuted ? 0f : lastSfxVolume;
        BgmVolume = BgmMuted ? 0f : lastBgmVolume;
    }
}