using UnityEngine;

/// <summary>
/// Control simple de un pin de bolos:
/// - Detecta colisión manual con la bola (esfera-esfera).
/// - Al ser golpeado, adquiere una velocidad inicial y cae.
/// - Se aplica una "gravedad" y fricción horizontales manuales.
/// </summary>
public class PinController : MonoBehaviour
{
    public enum PinState
    {
        Standing,   // De pie, esperando ser golpeado.
        Falling,    // En movimiento tras ser golpeado.
        Resting     // Ya cayó y está casi inmóvil.
    }

    [Header("Referencias")]
    public BowlingBallController ball;
    public TutorialManager tutorialManager;

    [Header("Colisión con la bola")]
    [Tooltip("Radio aproximado del pin para detección de colisión con la bola.")]
    public float pinRadius = 0.2f;

    [Header("Física simplificada del pin")]
    [Tooltip("Multiplicador para la velocidad inicial que recibe el pin al ser golpeado.")]
    public float hitForceMultiplier = 0.6f;
    [Tooltip("Velocidad de impulso vertical inicial al ser golpeado (para simular salto).")]
    public float verticalImpulse = 2f;
    [Tooltip("Gravedad aplicada al pin (unidades de velocidad/segundo^2).")]
    public float gravity = 9.81f;
    [Tooltip("Fricción horizontal para que se vaya frenando en el suelo.")]
    public float groundFriction = 3f;
    [Tooltip("Altura del suelo (Y).")]
    public float groundY = 0f;
    [Tooltip("Factor de rebote vertical al chocar con el suelo.")]
    public float bounceFactor = 0.2f;
    [Tooltip("Si la velocidad total baja de este valor, se considera reposo.")]
    public float restSpeedThreshold = 0.1f;

    // Estado interno
    private PinState state = PinState.Standing;
    private Vector3 velocity;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private bool alreadyHit = false;

    private void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        state = PinState.Standing;
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        switch (state)
        {
            case PinState.Standing:
                CheckCollisionWithBall();
                break;

            case PinState.Falling:
                SimulateFalling(dt);
                break;

            case PinState.Resting:
                // Nada, ya está quieto.
                break;
        }
    }

    /// <summary>
    /// Detecta colisión esfera-esfera: bola vs pin.
    /// </summary>
    private void CheckCollisionWithBall()
    {
        if (ball == null) return;

        Vector3 ballPos = ball.transform.position;
        Vector3 pinPos = transform.position;

        float combinedRadius = ball.ballRadius + pinRadius;
        float combinedRadiusSqr = combinedRadius * combinedRadius;

        Vector3 diff = pinPos - ballPos;
        float distSqr = diff.sqrMagnitude;

        if (distSqr <= combinedRadiusSqr)
        {
            // Impacto.
            OnHitByBall();
        }
    }

    /// <summary>
    /// Calcula la velocidad inicial del pin al ser golpeado por la bola.
    /// </summary>
    private void OnHitByBall()
    {
        if (alreadyHit) return;
        alreadyHit = true;

        // Dirección desde la bola hacia el pin
        Vector3 direction = (transform.position - ball.transform.position).normalized;
        float ballSpeed = ball.CurrentVelocity.magnitude;

        // Velocidad inicial = parte horizontal por impacto de la bola + impulso vertical
        Vector3 horizontalVelocity = direction * ballSpeed * hitForceMultiplier;
        velocity = horizontalVelocity + Vector3.up * verticalImpulse;

        state = PinState.Falling;

        // Notificamos al tutorial que un pin fue impactado.
        tutorialManager?.NotifyPinHit(this);
    }

    /// <summary>
    /// Simula la caída del pin con gravedad y fricción.
    /// </summary>
    private void SimulateFalling(float dt)
    {
        // Aplicamos gravedad (en -Y).
        velocity += Vector3.down * gravity * dt;

        // Integramos posición: p = p + v * dt
        Vector3 pos = transform.position;
        pos += velocity * dt;

        // Colisión con el suelo
        if (pos.y <= groundY)
        {
            pos.y = groundY;

            // Rebote vertical amortiguado
            if (velocity.y < 0f)
                velocity.y = -velocity.y * bounceFactor;

            // Pequeña fricción horizontal cuando está en el suelo
            Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
            if (horizontalVelocity.magnitude > 0f)
            {
                Vector3 frictionDir = -horizontalVelocity.normalized;
                Vector3 frictionAccel = frictionDir * groundFriction;

                Vector3 newHorizontal = horizontalVelocity + frictionAccel * dt;
                // Si cambiamos de sentido, lo dejamos en 0
                if (Vector3.Dot(horizontalVelocity, newHorizontal) <= 0f)
                {
                    newHorizontal = Vector3.zero;
                }

                velocity = new Vector3(newHorizontal.x, velocity.y, newHorizontal.z);
            }
        }

        // Aplicamos la posición
        transform.position = pos;

        // Rotación simple: inclinamos el pin según la dirección del movimiento horizontal
        Vector3 flatVelocity = new Vector3(velocity.x, 0f, velocity.z);
        if (flatVelocity.magnitude > 0.01f)
        {
            // Hacemos que el pin se "acueste" en la dirección de la velocidad.
            Quaternion targetRot = Quaternion.LookRotation(flatVelocity.normalized) * Quaternion.Euler(90f, 0f, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 5f * dt);
        }

        // Si la velocidad es casi cero, lo consideramos en reposo.
        if (velocity.magnitude < restSpeedThreshold && Mathf.Abs(pos.y - groundY) < 0.01f)
        {
            velocity = Vector3.zero;
            state = PinState.Resting;
        }
    }

    /// <summary>
    /// Permite resetear el pin a su posición original (por si quieres reiniciar el tutorial).
    /// </summary>
    public void ResetPin()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        velocity = Vector3.zero;
        state = PinState.Standing;
        alreadyHit = false;
    }

    public PinState GetState()
    {
        return state;
    }
}
