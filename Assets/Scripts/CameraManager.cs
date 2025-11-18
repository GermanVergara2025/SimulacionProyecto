using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public enum CameraMode
    {
        FirstPerson,
        FollowBall
    }

    [Header("Referencias")]
    public Transform ballTransform;
    public Transform firstPersonAnchor;
    public TutorialManager tutorialManager;

    [Header("Ajustes de seguimiento (world space)")]
    [Tooltip("Offset global respecto a la bola en modo seguimiento (tercera persona).")]
    public Vector3 followOffset = new Vector3(0f, 3f, -8f);
    public float positionLerpSpeed = 5f;
    public float rotationLerpSpeed = 5f;

    private CameraMode currentMode = CameraMode.FirstPerson;

    private void Start()
    {
        // Empezamos en primera persona bien colocados
        if (firstPersonAnchor != null)
        {
            transform.position = firstPersonAnchor.position;
            transform.rotation = firstPersonAnchor.rotation;
            currentMode = CameraMode.FirstPerson;
        }
    }

    private void LateUpdate()
    {
        if (ballTransform == null) return;

        float dt = Time.deltaTime;

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
        // Offset en coordenadas MUNDIALES, no dependiendo de la rotación de la bola
        Vector3 targetPos = ballTransform.position + followOffset;

        // Miramos hacia la bola
        Quaternion targetRot = Quaternion.LookRotation(ballTransform.position - targetPos, Vector3.up);

        transform.position = Vector3.Lerp(transform.position, targetPos, positionLerpSpeed * dt);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationLerpSpeed * dt);
    }

    public void SwitchToFirstPersonMode()
    {
        currentMode = CameraMode.FirstPerson;

        if (firstPersonAnchor != null)
        {
            transform.position = firstPersonAnchor.position;
            transform.rotation = firstPersonAnchor.rotation;
        }

        tutorialManager?.NotifyCameraFirstPerson();
    }

public void SwitchToFollowBallMode()
{
    // Solo cambiamos el modo, NO movemos la cámara de golpe.
    currentMode = CameraMode.FollowBall;

    if (ballTransform != null)
    {
        // Tomamos el offset ACTUAL entre la cámara y la bola.
        // Así la cámara se queda donde está y solo seguirá a la bola
        // manteniendo esa distancia.
        followOffset = transform.position - ballTransform.position;
    }

    tutorialManager?.NotifyCameraFollowBall();
}


    public CameraMode GetCameraMode()
    {
        return currentMode;
    }
}
