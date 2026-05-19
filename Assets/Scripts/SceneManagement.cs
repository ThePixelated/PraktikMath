using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagement : MonoBehaviour
{
    public void SceneLoader(string targetScene)
    {
        SceneManager.LoadScene(targetScene);
    }
}
