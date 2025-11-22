using UnityEngine;

public class AJUSTES : MonoBehaviour
{
    [Header("Paneles de Configuración")]
    public GameObject Panelinicio;     
    public GameObject Panelajustes;   // Música
    public GameObject Panelajustes2;   // Créditos 1
    public GameObject Panelajustes3;  // Créditos 2

    [Header("Audio Global")]
    public AudioSource musicaFondo;   // Música principal opcional

   
    // -----------------------------
    //       CONTROL DE MÚSICA
    // -----------------------------

    public void MusicaON()
    {
        AudioListener.volume = 1f;
        if (musicaFondo != null)
            musicaFondo.Play();
    }

    public void MusicaOFF()
    {
        AudioListener.volume = 0f;
        if (musicaFondo != null)
            musicaFondo.Pause();
    }

    // ----------- BOTONES DE CERRAR -----------
    public void CerrarQ1()
    {
        Panelajustes.SetActive(false);
        Panelinicio.SetActive(true);
    }

    public void CerrarQ2()
    {
        Panelajustes2.SetActive(false);
        Panelinicio.SetActive(true);
    }

    public void CerrarQ3()
    {
        Panelajustes3.SetActive(false);
        Panelinicio.SetActive(true);
    }

    // ----------- SIGUIENTES -----------
    public void Q1_Siguiente()
    {
        Panelajustes.SetActive(false);
        Panelajustes2.SetActive(true);
    }

    public void Q2_Siguiente()
    {
        Panelajustes2.SetActive(false);
        Panelajustes3.SetActive(true);
    }

}
