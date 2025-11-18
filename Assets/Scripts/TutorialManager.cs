using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controla las fases del nivel tutorial:
/// 1. Mover lateralmente.
/// 2. Ajustar dirección.
/// 3. Cargar fuerza.
/// 4. Lanzar la bola.
/// 5. Ver la cámara seguir a la bola.
/// 6. Observar impacto en los pines.
/// 7. Mostrar mensaje final y botón para ir al Nivel 1.
/// </summary>
public class TutorialManager : MonoBehaviour
{
    public enum TutorialStep
    {
        MoveLaterally,
        AdjustDirection,
        ChargeForce,
        LaunchBall,
        WatchCameraFollow,
        ObservePinsHit,
        TutorialComplete
    }

    [Header("Referencias")]
    public BowlingBallController ballController;
    public CameraManager cameraManager;
    public UIManagerTutorial uiManager;
    [Tooltip("Lista de pines del nivel tutorial.")]
    public List<PinController> pins = new List<PinController>();

    [Header("Configuración de pasos")]
    [Tooltip("Tiempo mínimo (segundos) que la cámara debe seguir a la bola para completar el paso de cámara.")]
    public float requiredCameraFollowTime = 2f;
    [Tooltip("Número mínimo de pines que deben ser golpeados para dar por completo el tutorial.")]
    public int requiredPinsHit = 1;

    private TutorialStep currentStep = TutorialStep.MoveLaterally;

    // Flags de progreso
    private bool hasMovedLaterally = false;
    private bool hasAdjustedDirection = false;
    private bool hasChargedForce = false;
    private bool hasLaunchedBall = false;
    private bool hasBallStopped = false;
    private float cameraFollowTimer = 0f;
    private int pinsHitCount = 0;

    private void Start()
    {
        SetStep(TutorialStep.MoveLaterally);
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        // Lógica de tiempo para la fase de cámara
        if (currentStep == TutorialStep.WatchCameraFollow &&
            cameraManager != null &&
            cameraManager.GetCameraMode() == CameraManager.CameraMode.FollowBall &&
            ballController != null &&
            ballController.GetBallState() == BowlingBallController.BallState.Launched)
        {
            cameraFollowTimer += dt;

            if (cameraFollowTimer >= requiredCameraFollowTime)
            {
                AdvanceToStep(TutorialStep.ObservePinsHit);
            }
        }

        // Si estamos en "ObservePinsHit" y ya se han golpeado suficientes pines, completamos el tutorial.
        if (currentStep == TutorialStep.ObservePinsHit && pinsHitCount >= requiredPinsHit)
        {
            AdvanceToStep(TutorialStep.TutorialComplete);
        }
    }

    /// <summary>
    /// Cambia el paso actual y muestra el texto correspondiente.
    /// </summary>
    private void SetStep(TutorialStep step)
    {
        currentStep = step;

        switch (currentStep)
        {
            case TutorialStep.MoveLaterally:
                uiManager?.ShowInstruction("Usa A y D (o flechas izquierda/derecha) para mover la bola lateralmente sobre la pista.");
                break;

            case TutorialStep.AdjustDirection:
                uiManager?.ShowInstruction("Usa Q y E para ajustar ligeramente la dirección del lanzamiento hacia la izquierda o la derecha.");
                break;

            case TutorialStep.ChargeForce:
                uiManager?.ShowInstruction("Mantén pulsada la tecla ESPACIO para cargar la fuerza del lanzamiento.");
                break;

            case TutorialStep.LaunchBall:
                uiManager?.ShowInstruction("Suelta la tecla ESPACIO para lanzar la bola con la fuerza cargada.");
                break;

            case TutorialStep.WatchCameraFollow:
                uiManager?.ShowInstruction("Observa cómo la cámara sigue la bola en tercera persona mientras avanza por la pista.");
                cameraFollowTimer = 0f;
                break;

            case TutorialStep.ObservePinsHit:
                uiManager?.ShowInstruction("Mira cómo los pines reaccionan al impacto de la bola.");
                break;

            case TutorialStep.TutorialComplete:
                uiManager?.HideInstruction();
                uiManager?.ShowCompletion("¡Excelente! Has completado el tutorial.\n\nPulsa el botón para continuar al Nivel 1.");
                break;
        }
    }

    private void AdvanceToStep(TutorialStep nextStep)
    {
        SetStep(nextStep);
    }

    // ------------------------------
    // MÉTODOS DE NOTIFICACIÓN PÚBLICOS
    // (se llaman desde otros scripts)
    // ------------------------------

    /// <summary>
    /// Llamado por BowlingBallController cuando el jugador se mueve lateralmente.
    /// </summary>
    public void NotifyBallMovedLaterally()
    {
        if (currentStep == TutorialStep.MoveLaterally && !hasMovedLaterally)
        {
            hasMovedLaterally = true;
            AdvanceToStep(TutorialStep.AdjustDirection);
        }
    }

    /// <summary>
    /// Llamado por BowlingBallController cuando el jugador ajusta la dirección (Q/E).
    /// </summary>
    public void NotifyDirectionAdjusted()
    {
        if (currentStep == TutorialStep.AdjustDirection && !hasAdjustedDirection)
        {
            hasAdjustedDirection = true;
            AdvanceToStep(TutorialStep.ChargeForce);
        }
    }

    /// <summary>
    /// Llamado por BowlingBallController cuando el jugador empieza a cargar fuerza.
    /// </summary>
    public void NotifyForceCharging(float currentForce)
    {
        if (currentStep == TutorialStep.ChargeForce && !hasChargedForce && currentForce > 0.1f)
        {
            hasChargedForce = true;
            AdvanceToStep(TutorialStep.LaunchBall);
        }
    }

    /// <summary>
    /// Llamado por BowlingBallController cuando el jugador suelta ESPACIO y lanza la bola.
    /// </summary>
    public void NotifyBallLaunched()
    {
        if (!hasLaunchedBall)
        {
            hasLaunchedBall = true;
            // Pasamos a la fase de cámara
            AdvanceToStep(TutorialStep.WatchCameraFollow);
        }
    }

    /// <summary>
    /// Llamado por BowlingBallController cuando la bola se ha detenido.
    /// </summary>
    public void NotifyBallStopped()
    {
        hasBallStopped = true;
        // Si quieres, podrías forzar el final del tutorial aunque no haya dado en los pines.
        // Aquí lo dejamos en manos de la fase ObservePinsHit / pinsHitCount.
    }

    /// <summary>
    /// Llamado por CameraManager cuando cambia a modo seguir bola.
    /// </summary>
    public void NotifyCameraFollowBall()
    {
        // Aquí solo confirmamos el cambio de cámara (ya controlado en Update).
    }

    /// <summary>
    /// Llamado por CameraManager cuando cambia a primera persona.
    /// </summary>
    public void NotifyCameraFirstPerson()
    {
        // Puedes usar esto para steps futuros si quieres otro turno.
    }

    /// <summary>
    /// Llamado por PinController cuando es golpeado por la bola.
    /// </summary>
    public void NotifyPinHit(PinController pin)
    {
        pinsHitCount++;

        // Si ya estamos en la fase de observar pines, y se cumple la cantidad requerida, completamos.
        if (currentStep == TutorialStep.ObservePinsHit && pinsHitCount >= requiredPinsHit)
        {
            AdvanceToStep(TutorialStep.TutorialComplete);
        }
        else
        {
            // Si todavía estábamos en la fase de cámara y se golpeó un pin,
            // podemos adelantar la fase también:
            if (currentStep == TutorialStep.WatchCameraFollow)
            {
                AdvanceToStep(TutorialStep.ObservePinsHit);
            }
        }
    }
}
