using UnityEngine;

/// <summary>
/// Controla la bola de bolos SIN usar físicas de Unity.
/// - Movimiento lateral antes del lanzamiento.
/// - Ajuste de dirección (giro leve).
/// - Carga de fuerza y lanzamiento.
/// - Integración de la velocidad con rozamiento.
/// - Colisiones simples con los límites de la pista.
/// </summary>
public class BowlingBallController : MonoBehaviour
{
    public enum BallState
    {
        Idle,       // Aún no se puede controlar (por si lo necesitas).
        Aiming,     // El jugador puede mover lateral, ajustar dirección y cargar fuerza.
        Launched,   // La bola va rodando por la pista.
        Stopped     // La bola ya se detuvo (fin del turno).
    }

    [Header("Referencias")]
    public TutorialManager tutorialManager;
    public CameraManager cameraManager;

    [Header("Propiedades de la bola")]
    [Tooltip("Radio aproximado de la bola, para colisiones con los pines.")]
    public float ballRadius = 8.5f;


    [Header("Ajustes de movimiento lateral (antes de lanzar)")]
    [Tooltip("Velocidad de movimiento lateral (eje X) mientras apuntas.")]
    public float lateralSpeed = 3f;
    [Tooltip("Límite mínimo en X de la pista.")]
    public float laneMinX = -1.5f;
    [Tooltip("Límite máximo en X de la pista.")]
    public float laneMaxX = 1.5f;

    [Header("Ajustes de dirección")]
    [Tooltip("Ángulo máximo que puede girar la bola a izquierda/derecha (en grados).")]
    public float maxDirectionAngle = 10f;
    [Tooltip("Velocidad de cambio de ángulo al usar Q/E.")]
    public float directionChangeSpeed = 45f; // grados/segundo

    [Header("Ajustes de fuerza de lanzamiento")]
    [Tooltip("Fuerza mínima que se aplicará al lanzar.")]
    public float minLaunchForce = 5f;
    [Tooltip("Fuerza máxima que se aplicará al lanzar.")]
    public float maxLaunchForce = 25f;
    [Tooltip("Velocidad a la que crece la fuerza al mantener ESPACIO.")]
    public float forceChargeSpeed = 15f;

    [Header("Ajustes de rozamiento y límites Z")]
    [Tooltip("Magnitud del rozamiento que desacelera la bola (unidad: velocidad/segundo).")]
    public float friction = 2f;
    [Tooltip("Cuando la velocidad sea menor que este valor, se considera detenida.")]
    public float stopSpeedThreshold = 0.2f;
    [Tooltip("Límite mínimo en Z de la pista (normalmente donde empieza la bola).")]
    public float laneMinZ = 0f;
    [Tooltip("Límite máximo en Z de la pista (donde están los pines).")]
    public float laneMaxZ = 20f;

    [Header("Rebote con paredes laterales (opcional)")]
    [Tooltip("Factor de rebote en X al chocar con los límites laterales (0 = se detiene, 1 = rebote perfecto).")]
    public float wallBounceFactor = 0.3f;

[Header("Modelo visual de la bola")]
[Tooltip("Transform del modelo visual que se rota (por ejemplo, BallVisual). Si se deja vacío, se usa el propio transform.")]
public Transform visualModel;

    // Estado interno
    private BallState state = BallState.Aiming;
    private Vector3 velocity;          // Velocidad actual de la bola.
    private float currentForce = 0f;   // Fuerza cargada.
    private float currentDirectionAngle = 0f; // Ángulo de la dirección (+ derecha, - izquierda).
    private bool isChargingForce = false;

    // Permite a otros scripts (pines) leer la velocidad de la bola
    public Vector3 CurrentVelocity => velocity;

    private void Start()
    {
        // Aseguramos orientación inicial hacia adelante (eje Z)
        transform.rotation = Quaternion.identity;
        state = BallState.Aiming;
    if (visualModel == null)
        visualModel = transform;

    transform.rotation = Quaternion.identity;
    state = BallState.Aiming;
    }

  private void Update()
{
    float dt = Time.deltaTime;

    Debug.Log("Estado bola: " + state);

    switch (state)
    {
        case BallState.Aiming:
            HandleAiming(dt);
            break;

        case BallState.Launched:
            HandleLaunched(dt);
            break;

        case BallState.Stopped:
            break;
    }
}


    /// <summary>
    /// Lógica mientras el jugador está preparando el lanzamiento (Aiming).
    /// </summary>
    private void HandleAiming(float dt)
    {
        // 1) Movimiento lateral (A/D o flechas horizontales)
        float horizontal = Input.GetAxisRaw("Horizontal"); // -1, 0, 1
        if (Mathf.Abs(horizontal) > 0.01f)
        {
            Vector3 pos = transform.position;
            pos.x += horizontal * lateralSpeed * dt;
            pos.x = Mathf.Clamp(pos.x, laneMinX, laneMaxX);
            transform.position = pos;

            // Notificamos al tutorial que el jugador ya se movió lateralmente.
            tutorialManager?.NotifyBallMovedLaterally();
        }

        // 2) Ajuste de dirección (Q/E)
        bool adjustLeft = Input.GetKey(KeyCode.Q);
        bool adjustRight = Input.GetKey(KeyCode.E);

        if (adjustLeft || adjustRight)
        {
            float dirSign = adjustRight ? 1f : -1f;
            currentDirectionAngle += dirSign * directionChangeSpeed * Time.deltaTime;
            currentDirectionAngle = Mathf.Clamp(currentDirectionAngle, -maxDirectionAngle, maxDirectionAngle);

            // Aplicamos la rotación a la bola para visualizar la dirección
           visualModel.rotation = Quaternion.Euler(0f, currentDirectionAngle, 0f);


            // Notificamos al tutorial que ya ajustó la dirección
            tutorialManager?.NotifyDirectionAdjusted();
        }

        // 3) Carga de fuerza (mantener ESPACIO)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isChargingForce = true;
        }

if (isChargingForce && Input.GetKey(KeyCode.Space))
{
    currentForce += forceChargeSpeed * dt;
    currentForce = Mathf.Clamp(currentForce, 0f, maxLaunchForce);

    // Notificamos al tutorial y le pasamos también la fuerza máxima
    tutorialManager?.NotifyForceCharging(currentForce, maxLaunchForce);
}


        // 4) Lanzamiento (soltar ESPACIO)
        if (isChargingForce && Input.GetKeyUp(KeyCode.Space))
        {
            isChargingForce = false;
            LaunchBall();
        }
    }

    /// <summary>
    /// Calcula la velocidad inicial y pasa al estado Launched.
    /// </summary>
    private void LaunchBall()
    {
        // Si apenas cargó un poco, aseguramos una fuerza mínima.
        float launchForce = Mathf.Clamp(currentForce, minLaunchForce, maxLaunchForce);

        // Dirección: eje Z adelante, rotado por currentDirectionAngle en Y.
        Quaternion dirRotation = Quaternion.Euler(0f, currentDirectionAngle, 0f);
        Vector3 direction = dirRotation * Vector3.forward;

        velocity = direction * launchForce;
        state = BallState.Launched;

        // Cambiamos la cámara a modo seguimiento.
        cameraManager?.SwitchToFollowBallMode();

        // Notificamos al tutorial que la bola fue lanzada.
        tutorialManager?.NotifyBallLaunched();

// Reseteamos indicador de potencia
if (tutorialManager != null && tutorialManager.uiManager != null)
{
    tutorialManager.uiManager.ResetPowerIndicator();
}


        // Reseteamos la fuerza para un futuro turno (si quieres).
        currentForce = 0f;
    }

    /// <summary>
    /// Lógica física de la bola tras el lanzamiento.
    /// </summary>
    private void HandleLaunched(float dt)
    {
        // Integración de posición: p = p + v * dt
        Vector3 pos = transform.position;
        pos += velocity * dt;

        // Colisión con límites laterales (X)
        if (pos.x < laneMinX)
        {
            pos.x = laneMinX;
            velocity.x = -velocity.x * wallBounceFactor;
        }
        else if (pos.x > laneMaxX)
        {
            pos.x = laneMaxX;
            velocity.x = -velocity.x * wallBounceFactor;
        }

        // Límite frontal (Z): si supera el final de la pista la detenemos
        if (pos.z > laneMaxZ)
        {
            pos.z = laneMaxZ;
            velocity = Vector3.zero;
        }

        // Aplicamos rozamiento (en dirección opuesta a la velocidad).
        if (velocity.magnitude > 0f)
        {
            Vector3 frictionDir = -velocity.normalized; // dirección opuesta a v
            Vector3 frictionAccel = frictionDir * friction; // a = -k * v_normalizada (simple)

            // v_new = v + a * dt
            Vector3 newVelocity = velocity + frictionAccel * dt;

            // Si el producto escalar indica que hemos pasado por cero, dejamos v en cero.
            if (Vector3.Dot(velocity, newVelocity) <= 0f)
            {
                newVelocity = Vector3.zero;
            }

            velocity = newVelocity;
        }

        // Aplicamos la posición resultante
        transform.position = pos;

// --- Rotación visual de la bola para simular rodadura ---
if (velocity.magnitude > 0.001f)
{
    // Dirección de movimiento en el plano XZ
    Vector3 moveDir = velocity.normalized;

    // Eje de giro: perpendicular al movimiento y al eje Y (suelo plano)
    Vector3 rollAxis = Vector3.Cross(moveDir, Vector3.up);

    // v = w * r  =>  w = v / r
    float speed = velocity.magnitude;                // velocidad lineal
    float angularSpeedRad = speed / ballRadius;      // radianes/segundo
    float angularSpeedDeg = angularSpeedRad * Mathf.Rad2Deg; // grados/segundo

    // Aplicamos rotación alrededor del eje de giro (en el mundo)
    // Usamos valor POSITIVO; si gira al revés, cambia el signo.
    // Rotamos SOLO el modelo visual, no el root
if (visualModel != null)
{
    // Cambiamos el signo para que gire en el sentido correcto
    visualModel.Rotate(rollAxis, -angularSpeedDeg * dt, Space.World);
}

}


        // Si la velocidad es muy baja, consideramos que la bola se detuvo.
        if (velocity.magnitude < stopSpeedThreshold)
        {
            velocity = Vector3.zero;
            state = BallState.Stopped;

            // Notificamos al tutorial y regresamos la cámara al modo primera persona.
            cameraManager?.SwitchToFirstPersonMode();
            tutorialManager?.NotifyBallStopped();
        }
    }

    /// <summary>
    /// Permite al TutorialManager reiniciar la bola para otro intento (opcional).
    /// </summary>
    public void ResetBall(Vector3 startPosition, Quaternion startRotation)
    {
        transform.position = startPosition;
        transform.rotation = startRotation;
        velocity = Vector3.zero;
        currentForce = 0f;
        currentDirectionAngle = 0f;
        state = BallState.Aiming;
    }

    public BallState GetBallState()
    {
        return state;
    }
}
