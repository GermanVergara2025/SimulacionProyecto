using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestiona la UI del tutorial:
/// - Muestra mensajes de instrucciones.
/// - Muestra el panel final con el botón "Ir al Nivel 1".
/// </summary>
public class UIManagerTutorial : MonoBehaviour
{
    [Header("Referencias de UI")]
    [Tooltip("Texto principal de instrucciones (TextMeshProUGUI).")]
    public TextMeshProUGUI instructionText;
    [Tooltip("Panel que contiene el texto de instrucciones.")]
    public GameObject instructionPanel;

    [Tooltip("Panel final de 'Tutorial completado'.")]
    public GameObject completionPanel;
    [Tooltip("Texto del panel de final del tutorial.")]
    public TextMeshProUGUI completionText;
    [Tooltip("Botón 'Ir al Nivel 1'.")]
    public Button goToLevel1Button;

    [Header("Nombre de la escena del Nivel 1")]
    public string level1SceneName = "Nivel1";

    private void Start()
    {
        if (instructionPanel != null) instructionPanel.SetActive(true);
        if (completionPanel != null) completionPanel.SetActive(false);

        if (goToLevel1Button != null)
        {
            goToLevel1Button.onClick.AddListener(OnGoToLevel1Clicked);
        }
    }

    public void ShowInstruction(string message)
    {
        if (instructionPanel != null) instructionPanel.SetActive(true);
        if (instructionText != null) instructionText.text = message;
    }

    public void HideInstruction()
    {
        if (instructionPanel != null) instructionPanel.SetActive(false);
    }

    public void ShowCompletion(string message)
    {
        if (completionPanel != null) completionPanel.SetActive(true);
        if (completionText != null) completionText.text = message;
    }

    private void OnGoToLevel1Clicked()
    {
        SceneManager.LoadScene(level1SceneName);
    }
}
