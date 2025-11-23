using UnityEngine;
using System.Collections;

public class GameManagerNivel1 : MonoBehaviour
{
    public int totalRondas = 5;
    public int rondaActual = 1;

    public int puntajeTotal = 0;
    private int puntajeRonda = 0;

    public PinManager pinManager;
    public BowlingBallController ballController;
    public UIManagerNivel1 uiManager;

    public float waitBeforeNextRound = 2f;

    private bool rondaTerminada = false;

    private Vector3 initialBallPosition;
    private Quaternion initialBallRotation;

    private int tiroActual = 1; // ← NUEVO: 1 o 2

    private void Start()
    {
        initialBallPosition = ballController.transform.position;
        initialBallRotation = ballController.transform.rotation;

        uiManager.UpdateRonda(rondaActual);
        uiManager.UpdatePuntaje(puntajeTotal);
        uiManager.ShowPuntajeRonda(0);
    }

    private void Update()
    {
        if (rondaTerminada) return;

        if (ballController.GetBallState() == BowlingBallController.BallState.Stopped)
        {
            rondaTerminada = true;
            StartCoroutine(FinDeTiro());
        }
    }

    private IEnumerator FinDeTiro()
    {
        int pinosCaidos = pinManager.GetFallenPins();
        puntajeRonda += pinosCaidos;
        puntajeTotal += pinosCaidos;

        uiManager.ShowPuntajeRonda(puntajeRonda);
        uiManager.UpdatePuntaje(puntajeTotal);

        yield return new WaitForSeconds(waitBeforeNextRound);

        // ----------- SI ES TIRO 1, PASA A TIRO 2 ----------
        if (tiroActual == 1)
        {
            tiroActual = 2;

            // NO se resetean pinos
            ballController.ResetBall(initialBallPosition, initialBallRotation);

            // permitir jugar nuevamente
            rondaTerminada = false;
            yield break;
        }

        // ----------- SI ES TIRO 2, TERMINA LA RONDA ----------
        tiroActual = 1; // reset para la siguiente ronda

        // Si ya es la última ronda
        if (rondaActual >= totalRondas)
        {
            uiManager.ShowFinal(puntajeTotal);
            yield break;
        }

        // Pasar a la siguiente ronda
        rondaActual++;
        uiManager.UpdateRonda(rondaActual);

        // Reset rondas y pinos
        puntajeRonda = 0;
        pinManager.ResetPins();
        ballController.ResetBall(initialBallPosition, initialBallRotation);

        rondaTerminada = false;
    }
}
