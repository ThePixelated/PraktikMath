using UnityEngine;
using UnityEngine.SceneManagement;

public class VideoSelectorButton : MonoBehaviour
{
    public string videoFileName; // isi di Inspector, contoh: "video1.mp4"
    public string sceneTarget = "VideoScene";

    public void OnButtonClicked()
    {
        string path = System.IO.Path.Combine(
            Application.streamingAssetsPath, videoFileName
        ).Replace("\\", "/"); // fix separator

        VideoSelectionData.SelectedVideoURL = path;
        SceneManager.LoadScene(sceneTarget);
    }
}