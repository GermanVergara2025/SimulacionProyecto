using UnityEngine;

/// <summary>
/// Controla la cámara para el nivel tutorial:
/// - Modo primera persona: anclada detrás de la bola (punto fijo).
/// - Modo seguimiento (tercera persona): sigue a la bola suavemente desde atrás y arriba.
/// </summary>
public class CameraManager : MonoBehaviour
{
    public enum CameraMode
    {
        FirstPerson,
        FollowBall
    }

    [Header("Referencias")]
    [Tooltip("Transform de la bola.")]
    public Transform ballTransform;
    [Tooltip("Punto de anclaje para primera persona (empty detrás de la bola).")]
    public Transform firstPersonAnchor;
    [Tooltip("Referencia al TutorialManager para notificar cambios de cámara.")]
    public TutorialManager tutorialManager;

    [Header("Ajustes de seguimiento")]
    [Tooltip("Offset relativo a la bola en modo seguimiento (tercera persona).")]
    public Vector3 followOffset = new Vector3(0f, 1.5f, -4f);
    [Tooltip("Velocidad de interpolación de posición.")]
    public float positionLerpSpeed = 5f;
    [Tooltip("Velocidad de interpolación de rotación.")]
    public float rotationLerpSpeed = 5f;

    private CameraMode currentMode = CameraMode.FirstPerson;

    private void LateUpdate()
    {
        float dt = Time.deltaTime;

        if (ballTransform == null) return;

        switch (currentMode)
        {
            case CameraMode.FirstPerson:
                UpdateFirstPerson(dt);
                break;

            case CameraMode.FollowBall:
                UpdateFollowBall(dt);
                break;
        }
    }

    private void UpdateFirstPerson(float dt)
    {
        if (firstPersonAnchor == null) return;

        Vector3 targetPos = firstPersonAnchor.position;
        Quaternion targetRot = firstPersonAnchor.rotation;

        transform.position = Vector3.Lerp(transform.position, targetPos, positionLerpSpeed * dt);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationLerpSpeed * dt);
    }

    private void UpdateFollowBall(float dt)
    {
        // Posición deseada en tercera persona: atrás y arriba de la bola según su orientación
        Vector3 offsetWorld = ballTransform.right * followOffset.x
                            + Vector3.up * followOffset.y
                            + ballTransform.forward * followOffset.z;

        Vector3 targetPos = ballTransform.position + offsetWorld;
        // Miramos hacia la bola
        Quaternion targetRot = Quaternion.LookRotation(ballTransform.position - targetPos, Vector3.up);

        transform.position = Vector3.Lerp(transform.position, targetPos, positionLerpSpeed * dt);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationLerpSpeed * dt);
    }

    public void SwitchToFirstPersonMode()
    {
        currentMode = CameraMode.FirstPerson;
        tutorialManager?.NotifyCameraFirstPerson();
    }

    public void SwitchToFollowBallMode()
    {
        currentMode = CameraMode.FollowBall;
        tutorialManager?.NotifyCameraFollowBall();
    }

    public CameraMode GetCameraMode()
    {
        return currentMode;
    }
}
