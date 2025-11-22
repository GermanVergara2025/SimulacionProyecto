using UnityEngine;
using UnityEngine.SceneManagement;

public class QUESTIONS : MonoBehaviour
{
    [Header("Referencias a Paneles")]
    public GameObject Panelinicio;     // Pantalla principal (la misma donde está INICIO)
    public GameObject Panelquestion;
    public GameObject Panelquestion2;
    public GameObject Panelquestion3;

    // ----------- BOTONES DE CERRAR -----------
    public void CerrarQ1()
    {
        Panelquestion.SetActive(false);
        Panelinicio.SetActive(true);
    }

    public void CerrarQ2()
    {
        Panelquestion2.SetActive(false);
        Panelinicio.SetActive(true);
    }

    public void CerrarQ3()
    {
        Panelquestion3.SetActive(false);
        Panelinicio.SetActive(true);
    }

    // ----------- SIGUIENTES -----------
    public void Q1_Siguiente()
    {
        Panelquestion.SetActive(false);
        Panelquestion2.SetActive(true);
    }

    public void Q2_Siguiente()
    {
        Panelquestion2.SetActive(false);
        Panelquestion3.SetActive(true);
    }

    // ----------- ÚLTIMO PANEL: COMENZAR JUEGO -----------
    public void ComenzarJuego()
    {
        SceneManager.LoadScene("Nivel 1");
    }
}
