using UnityEngine;
using UnityEngine.SceneManagement;

public class INICIO : MonoBehaviour
{
    public GameObject panelquestion;
    public GameObject panelajustes;

    public void StartGame()
    {
        SceneManager.LoadScene("Nivel 1");
    }

    public void OpenQuestion()
    {
        panelquestion.SetActive(true);
    }

    public void OpenConfig()
    {
        panelajustes.SetActive(true);
    }

    public void ClosePanels()
    {
        panelquestion.SetActive(false);
        panelajustes.SetActive(false);
    }
}

