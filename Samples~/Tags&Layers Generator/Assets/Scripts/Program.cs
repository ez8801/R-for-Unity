using UnityEngine;
using UnityEngine.SceneManagement;

public class Program : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
