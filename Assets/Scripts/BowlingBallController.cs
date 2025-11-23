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
        Idle,
        Aiming,
        Launched,
        Stopped
    }

    [Header("Referencias")]
    public TutorialManager tutorialManager;
    public CameraManager cameraManager;

    [Header("Propiedades de la bola")]
    public float ballRadius = 8.5f;

    [Header("Movimiento lateral")]
    public float lateralSpeed = 3f;
    public float laneMinX = -1.5f;
    public float laneMaxX = 1.5f;

    [Header("Dirección")]
    public float maxDirectionAngle = 10f;
    public float directionChangeSpeed = 45f;

    [Header("Fuerza del lanzamiento")]
    public float minLaunchForce = 5f;
    public float maxLaunchForce = 25f;
    public float forceChargeSpeed = 15f;

    [Header("Rozamiento y límites Z")]
    public float friction = 2f;
    public float stopSpeedThreshold = 0.2f;
    public float laneMinZ = 0f;
    public float laneMaxZ = 20f;

    [Header("Rebote con paredes")]
    public float wallBounceFactor = 0.3f;

    [Header("Modelo visual de la bola")]
    public Transform visualModel;

    private BallState state = BallState.Aiming;
    private Vector3 velocity;
    private float currentForce = 0f;
    private float currentDirectionAngle = 0f;
    private bool isChargingForce = false;

    public Vector3 CurrentVelocity => velocity;

    private void Start()
    {
        if (visualModel == null)
            visualModel = transform;

        transform.rotation = Quaternion.identity;
        state = BallState.Aiming;
    }

    private void Update()
    {
        float dt = Time.deltaTime;

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

    private void HandleAiming(float dt)
    {
        // Movimiento lateral
        float horizontal = Input.GetAxisRaw("Horizontal");
        if (Mathf.Abs(horizontal) > 0.01f)
        {
            Vector3 pos = transform.position;
            pos.x += horizontal * lateralSpeed * dt;
            pos.x = Mathf.Clamp(pos.x, laneMinX, laneMaxX);
            transform.position = pos;

            tutorialManager?.NotifyBallMovedLaterally();
        }

        // Ajuste dirección Q/E
        bool adjustLeft = Input.GetKey(KeyCode.Q);
        bool adjustRight = Input.GetKey(KeyCode.E);

        if (adjustLeft || adjustRight)
        {
            float dirSign = adjustRight ? 1f : -1f;
            currentDirectionAngle += dirSign * directionChangeSpeed * dt;
            currentDirectionAngle = Mathf.Clamp(currentDirectionAngle, -maxDirectionAngle, maxDirectionAngle);

            transform.rotation = Quaternion.Euler(0f, currentDirectionAngle, 0f);

            if (visualModel != null)
                visualModel.localRotation = Quaternion.identity;

            tutorialManager?.NotifyDirectionAdjusted();
        }

        // Carga de fuerza
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isChargingForce = true;
        }

        if (isChargingForce && Input.GetKey(KeyCode.Space))
        {
            currentForce += forceChargeSpeed * dt;
            currentForce = Mathf.Clamp(currentForce, 0f, maxLaunchForce);

            tutorialManager?.NotifyForceCharging(currentForce, maxLaunchForce);
        }

        // Lanzamiento
        if (isChargingForce && Input.GetKeyUp(KeyCode.Space))
        {
            isChargingForce = false;
            LaunchBall();
        }
    }

    private void LaunchBall()
    {
        float launchForce = Mathf.Clamp(currentForce, minLaunchForce, maxLaunchForce);

        Quaternion dirRotation = Quaternion.Euler(0f, currentDirectionAngle, 0f);
        Vector3 direction = dirRotation * Vector3.forward;

        velocity = direction * launchForce;
        state = BallState.Launched;

        AudioManagerTutorial.instance?.PlayBallLaunch();
        cameraManager?.SwitchToFollowBallMode();
        tutorialManager?.NotifyBallLaunched();

        if (tutorialManager != null && tutorialManager.uiManager != null)
            tutorialManager.uiManager.ResetPowerIndicator();

        currentForce = 0f;
    }

    private void HandleLaunched(float dt)
    {
        Vector3 pos = transform.position;
        pos += velocity * dt;

        // Límites laterales
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

        // Límite frontal
        if (pos.z > laneMaxZ)
        {
            pos.z = laneMaxZ;
            velocity = Vector3.zero;
        }

        // Rozamiento
        if (velocity.magnitude > 0f)
        {
            Vector3 frictionDir = -velocity.normalized;
            Vector3 frictionAccel = frictionDir * friction;

            Vector3 newVelocity = velocity + frictionAccel * dt;

            if (Vector3.Dot(velocity, newVelocity) <= 0f)
                newVelocity = Vector3.zero;

            velocity = newVelocity;
        }

        transform.position = pos;

        // Rotación visual
        if (velocity.magnitude > 0.001f && visualModel != null)
        {
            Vector3 moveDir = velocity.normalized;
            Vector3 rollAxis = Vector3.Cross(moveDir, Vector3.up);

            float speed = velocity.magnitude;
            float angularSpeedRad = speed / ballRadius;
            float angularSpeedDeg = angularSpeedRad * Mathf.Rad2Deg;

            visualModel.Rotate(rollAxis, -angularSpeedDeg * dt, Space.World);
        }

        // Bola se detiene
        if (velocity.magnitude < stopSpeedThreshold)
        {
            velocity = Vector3.zero;
            state = BallState.Stopped;

            cameraManager?.SwitchToFirstPersonMode();
            tutorialManager?.NotifyBallStopped();
        }
    }

    // ---------------------------------------------------------
    //  *** RESET COMPLETO PARA SISTEMA DE RONDAS ***
    // ---------------------------------------------------------
    public void ResetBall(Vector3 startPosition, Quaternion startRotation)
    {
        transform.position = startPosition;
        transform.rotation = startRotation;

        velocity = Vector3.zero;
        currentForce = 0f;
        currentDirectionAngle = 0f;

        if (visualModel != null)
            visualModel.localRotation = Quaternion.identity;

        state = BallState.Aiming;

        cameraManager?.SwitchToFirstPersonMode();

        Debug.Log("Bola reiniciada → Estado = AIMING");
    }

    public BallState GetBallState()
    {
        return state;
    }
}
