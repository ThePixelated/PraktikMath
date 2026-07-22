using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

public class VideoPlayerController : MonoBehaviour
{
    [Header("Video")]
    public VideoPlayer videoPlayer;
    public RawImage videoDisplay;

    [Header("Controls UI")]
    public CanvasGroup controlsGroup;       // ControlsContainer punya CanvasGroup
    public Image gradientOverlay;           // gradient di atas video

    [Header("Play/Pause/Replay")]
    public Button playPauseButton;
    public Image playPauseIcon;
    public Sprite playSprite;
    public Sprite pauseSprite;
    public Sprite replaySprite;

    [Header("Seek Slider")]
    public Slider seekSlider;

    [Header("Timestamp")]
    public TextMeshProUGUI timestampText;

    [Header("Volume")]
    public Button muteButton;
    public Image muteIcon;
    public Sprite soundSprite;
    public Sprite muteSprite;
    public Slider volumeSlider;

    [Header("Hover Settings")]
    public float showDelay = 0.2f;
    public float hideDelay = 1.5f;
    public float fadeDuration = 0.3f;

    [Header("BGM")]
    public float bgmFadeDuration = 1f;      // durasi BGM naik perlahan saat video pause/selesai

    [Header("Autoplay")]
    public float autoPlayDelay = 4f;        // auto-play kalau user diam (tanpa hover/klik) sekian detik

    // State
    private bool isMuted = false;
    private bool isDraggingSeek = false;
    private bool isVideoEnded = false;
    private bool isHovering = false;

    private Coroutine fadeCoroutine;
    private Coroutine hoverCoroutine;
    private Coroutine autoPlayCoroutine;

    private bool firstFrameShown = false;
    private bool hasStartedPlayback = false;
    private bool autoPlayCancelled = false;

    private float lastVolume = 1f;

    void Start()
    {
        if (!string.IsNullOrEmpty(VideoSelectionData.SelectedVideoURL))
        {
            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = VideoSelectionData.SelectedVideoURL;
        }
        else
        {
            Debug.LogWarning("Tidak ada video URL yang dipilih!");
        }

        // Init VideoPlayer
        videoPlayer.loopPointReached += OnVideoEnd;
        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.Prepare();

        // Slider events
        seekSlider.onValueChanged.AddListener(OnSeekSliderChanged);

        // Volume slider
        volumeSlider.value = 1f;
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

        // Buttons
        playPauseButton.onClick.AddListener(OnPlayPauseReplayClicked);
        muteButton.onClick.AddListener(OnMuteClicked);

        // Hide controls initially
        controlsGroup.alpha = 0f;
        controlsGroup.interactable = false;
        controlsGroup.blocksRaycasts = false;
        if (gradientOverlay) gradientOverlay.color = new Color(0, 0, 0, 0);
    }

    void OnDestroy()
    {
        // Balik ke scene sebelumnya -> nyalakan lagi BGM dari titik terakhir.
        if (AudioManager.Instance != null) AudioManager.Instance.ResumeBgm();
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        var rt = new RenderTexture((int)vp.width, (int)vp.height, 0);
        vp.targetTexture = rt;
        videoDisplay.texture = rt;

        // Auto-set aspect ratio sesuai resolusi video
        var fitter = videoDisplay.GetComponent<AspectRatioFitter>();
        if (fitter != null)
        {
            fitter.aspectRatio = (float)vp.width / vp.height;
        }

        // Render frame pertama sebagai thumbnail (tanpa suara), lalu pause lagi.
        vp.SetDirectAudioMute(0, true);
        vp.frameReady += OnFirstFrameReady;
        vp.sendFrameReadyEvents = true;
        vp.Play();
    }

    void OnFirstFrameReady(VideoPlayer vp, long frameIdx)
    {
        if (firstFrameShown) return;
        firstFrameShown = true;

        vp.Pause();
        vp.sendFrameReadyEvents = false;
        vp.frameReady -= OnFirstFrameReady;
        vp.SetDirectAudioMute(0, isMuted); // balikin ke state mute user

        playPauseIcon.sprite = playSprite;

        // Mulai hitung mundur auto-play kalau user diam.
        StartAutoPlay();
    }

    // ───────────────────────────────────────────
    // AUTOPLAY
    // ───────────────────────────────────────────

    void StartAutoPlay()
    {
        if (autoPlayCancelled || hasStartedPlayback) return;
        if (autoPlayCoroutine != null) StopCoroutine(autoPlayCoroutine);
        autoPlayCoroutine = StartCoroutine(AutoPlayCountdown());
    }

    IEnumerator AutoPlayCountdown()
    {
        yield return new WaitForSeconds(autoPlayDelay);
        if (!autoPlayCancelled && !hasStartedPlayback && !isVideoEnded)
            PlayVideoInternal();
    }

    void CancelAutoPlay()
    {
        autoPlayCancelled = true;
        if (autoPlayCoroutine != null) { StopCoroutine(autoPlayCoroutine); autoPlayCoroutine = null; }
    }

    // ───────────────────────────────────────────
    // PLAY / PAUSE INTERNAL (+ kontrol BGM)
    // ───────────────────────────────────────────

    void PlayVideoInternal()
    {
        videoPlayer.Play();
        playPauseIcon.sprite = pauseSprite;
        hasStartedPlayback = true;
        if (AudioManager.Instance != null) AudioManager.Instance.PauseBgm();
    }

    void PauseVideoInternal()
    {
        videoPlayer.Pause();
        playPauseIcon.sprite = playSprite;
        if (AudioManager.Instance != null) AudioManager.Instance.ResumeBgm(bgmFadeDuration);
    }

    void Update()
    {
        if (videoPlayer.isPrepared && !isDraggingSeek && !isVideoEnded)
        {
            // Update seek slider tanpa trigger event
            float progress = videoPlayer.frameCount > 0
                ? (float)videoPlayer.frame / videoPlayer.frameCount
                : 0f;

            seekSlider.SetValueWithoutNotify(progress);
            UpdateTimestamp();
        }
    }

    // ───────────────────────────────────────────
    // PLAY / PAUSE / REPLAY
    // ───────────────────────────────────────────

    void OnPlayPauseReplayClicked()
    {
        CancelAutoPlay();

        if (isVideoEnded)
        {
            ReplayVideo();
            return;
        }

        if (videoPlayer.isPlaying)
            PauseVideoInternal();
        else
            PlayVideoInternal();
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        isVideoEnded = true;
        playPauseIcon.sprite = replaySprite;

        // Video selesai -> BGM balik naik perlahan.
        if (AudioManager.Instance != null) AudioManager.Instance.ResumeBgm(bgmFadeDuration);
    }

    void ReplayVideo()
    {
        isVideoEnded = false;
        videoPlayer.frame = 0;
        PlayVideoInternal();
    }

    // ───────────────────────────────────────────
    // SEEK SLIDER
    // ───────────────────────────────────────────

    // Dipanggil dari EventTrigger: PointerDown di seekSlider
    public void OnSeekBeginDrag()
    {
        CancelAutoPlay();
        isDraggingSeek = true;
    }

    // Dipanggil dari EventTrigger: PointerUp di seekSlider
    public void OnSeekEndDrag()
    {
        isDraggingSeek = false;
        SeekToSliderValue();
    }

    void OnSeekSliderChanged(float value)
    {
        if (isDraggingSeek)
        {
            SeekToSliderValue();
        }
    }

    // Dipanggil juga dari EventTrigger: PointerClick di seekSlider (untuk click langsung)
    public void OnSeekClick()
    {
        CancelAutoPlay();
        SeekToSliderValue();
    }

    void SeekToSliderValue()
    {
        if (!videoPlayer.isPrepared) return;
        long targetFrame = (long)(seekSlider.value * videoPlayer.frameCount);
        videoPlayer.frame = targetFrame;

        if (isVideoEnded && seekSlider.value < 1f)
        {
            isVideoEnded = false;
            playPauseIcon.sprite = videoPlayer.isPlaying ? pauseSprite : playSprite;
        }

        UpdateTimestamp();
    }

    // ───────────────────────────────────────────
    // TIMESTAMP
    // ───────────────────────────────────────────

    void UpdateTimestamp()
    {
        double current = videoPlayer.time;
        double total = videoPlayer.frameCount / videoPlayer.frameRate;
        timestampText.text = $"{FormatTime(current)} / {FormatTime(total)}";
    }

    string FormatTime(double seconds)
    {
        int m = Mathf.FloorToInt((float)seconds / 60);
        int s = Mathf.FloorToInt((float)seconds % 60);
        return $"{m}:{s:D2}";
    }

    // ───────────────────────────────────────────
    // VOLUME & MUTE
    // ───────────────────────────────────────────

    void OnMuteClicked()
    {
        isMuted = !isMuted;

        if (isMuted)
        {
            lastVolume = volumeSlider.value;        // simpan posisi sebelumnya
            volumeSlider.SetValueWithoutNotify(0f); // geser slider ke 0 tanpa trigger event
            videoPlayer.SetDirectAudioVolume(0, 0f);
        }
        else
        {
            float restoreValue = lastVolume > 0f ? lastVolume : 1f; // fallback ke 1 kalau lastVolume 0
            volumeSlider.SetValueWithoutNotify(restoreValue);
            videoPlayer.SetDirectAudioVolume(0, restoreValue);
        }

        videoPlayer.SetDirectAudioMute(0, isMuted);
        muteIcon.sprite = isMuted ? muteSprite : soundSprite;
        volumeSlider.interactable = !isMuted;
    }

    void OnVolumeChanged(float value)
    {
        if (isMuted) return; // lagi mute, ignore drag

        videoPlayer.SetDirectAudioVolume(0, value);
        muteIcon.sprite = value == 0f ? muteSprite : soundSprite;
    }

    // ───────────────────────────────────────────
    // HOVER (dipanggil dari EventTrigger di VideoPlayerPanel)
    // ───────────────────────────────────────────

    public void OnPointerEnter()
    {
        CancelAutoPlay(); // user udah berinteraksi -> batalkan auto-play
        isHovering = true;
        if (hoverCoroutine != null) StopCoroutine(hoverCoroutine);
        hoverCoroutine = StartCoroutine(ShowControlsDelayed());
    }

    public void OnPointerExit()
    {
        isHovering = false;
        if (hoverCoroutine != null) StopCoroutine(hoverCoroutine);
        hoverCoroutine = StartCoroutine(HideControlsDelayed());
    }

    IEnumerator ShowControlsDelayed()
    {
        yield return new WaitForSeconds(showDelay);
        if (isHovering) StartFade(1f);
    }

    IEnumerator HideControlsDelayed()
    {
        yield return new WaitForSeconds(hideDelay);
        if (!isHovering) StartFade(0f);
    }

    void StartFade(float targetAlpha)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeControls(targetAlpha));
    }

    IEnumerator FadeControls(float target)
    {
        float start = controlsGroup.alpha;
        float elapsed = 0f;

        // Set interactable sebelum fade masuk
        if (target > 0f)
        {
            controlsGroup.interactable = true;
            controlsGroup.blocksRaycasts = true;
        }

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            float alpha = Mathf.Lerp(start, target, t);
            controlsGroup.alpha = alpha;
            if (gradientOverlay)
                gradientOverlay.color = new Color(1, 1, 1, alpha);
            yield return null;
        }

        controlsGroup.alpha = target;

        if (target == 0f)
        {
            controlsGroup.interactable = false;
            controlsGroup.blocksRaycasts = false;
        }
    }
}

public static class VideoSelectionData
{
    public static string SelectedVideoURL; // ganti dari VideoClip ke string URL
}