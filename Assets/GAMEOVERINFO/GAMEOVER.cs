using UnityEngine;
using UnityEngine.SceneManagement;

public class GAMEOVER : MonoBehaviour
{
    public void Retry()
    {
        SceneManager.LoadScene("Nivel 1");
    }

    public void GoHome()
    {
        SceneManager.LoadScene("Home");
    }
}
