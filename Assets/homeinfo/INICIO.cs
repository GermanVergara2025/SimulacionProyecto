using UnityEngine;
using UnityEngine.SceneManagement;

public class INICIO : MonoBehaviour
{
    public GameObject QUESTIONS;
    public GameObject AJUSTES;

    public void StartGame()
    {
        SceneManager.LoadScene("Nivel 1");
    }

    public void OpenQuestion()
    {
        QUESTIONS.SetActive(true);
    }

    public void OpenConfig()
    {
        AJUSTES.SetActive(true);
    }

    public void ClosePanels()
    {
        QUESTIONS.SetActive(false);
        AJUSTES.SetActive(false);
    }
}

