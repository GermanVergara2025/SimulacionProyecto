using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManagerNivel1 : MonoBehaviour
{
    [Header("UI de juego")]
    public TextMeshProUGUI RondaText;
    public TextMeshProUGUI PuntajeText;
    public TextMeshProUGUI PuntajeRondaText;

    [Header("Panel Final")]
    public GameObject PanelFinal;
    public TextMeshProUGUI PuntajeFinalText;


    // -------- REINICIAR NIVEL ------------
    public void ReiniciarNivel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // -------- IR AL MENÚ ------------
    public void IrAlMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    // -------- SIGUIENTE NIVEL ------------
    public void IrAlSiguienteNivel()
    {
        int escenaActual = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(escenaActual + 1);
    }

    // -------- ACTUALIZAR UI --------
    public void UpdatePuntaje(int puntaje)
    {
        PuntajeText.text = "Puntaje: " + puntaje;
    }

    public void UpdateRonda(int ronda)
    {
        RondaText.text = "Ronda: " + ronda;
    }

    public void ShowPuntajeRonda(int pinos)
    {
        PuntajeRondaText.text = "Pinos Ronda: " + pinos;
    }

    public void ShowFinal(int puntajeFinal)
    {
        PanelFinal.SetActive(true);
        PuntajeFinalText.text = "Puntaje total: " + puntajeFinal;
    }
}