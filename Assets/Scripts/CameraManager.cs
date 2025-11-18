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

    [Header("Ajustes de seguimiento")]
    public Vector3 followOffset = new Vector3(0f, 2f, -6f);
    public float positionLerpSpeed = 5f;
    public float rotationLerpSpeed = 5f;

    private CameraMode currentMode = CameraMode.FirstPerson;

    private void Start()
    {
        // Al iniciar, dejamos la cámara pegada al anchor de primera persona
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
        // Offset detrás de la bola según su orientación
        Vector3 offsetWorld = ballTransform.right * followOffset.x
                            + Vector3.up * followOffset.y
                            + ballTransform.forward * followOffset.z;

        Vector3 targetPos = ballTransform.position + offsetWorld;
        Quaternion targetRot = Quaternion.LookRotation(ballTransform.position - targetPos, Vector3.up);

        transform.position = Vector3.Lerp(transform.position, targetPos, positionLerpSpeed * dt);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationLerpSpeed * dt);
    }

    public void SwitchToFirstPersonMode()
    {
        currentMode = CameraMode.FirstPerson;

        // Al cambiar, nos “pegamos” inmediatamente al anchor
        if (firstPersonAnchor != null)
        {
            transform.position = firstPersonAnchor.position;
            transform.rotation = firstPersonAnchor.rotation;
        }

        tutorialManager?.NotifyCameraFirstPerson();
    }

    public void SwitchToFollowBallMode()
    {
        currentMode = CameraMode.FollowBall;

        if (ballTransform != null)
        {
            // Hacemos un “snap” detrás de la bola para evitar acercamientos raros
            Vector3 offsetWorld = ballTransform.right * followOffset.x
                                + Vector3.up * followOffset.y
                                + ballTransform.forward * followOffset.z;

            Vector3 snapPos = ballTransform.position + offsetWorld;
            Quaternion snapRot = Quaternion.LookRotation(ballTransform.position - snapPos, Vector3.up);

            transform.position = snapPos;
            transform.rotation = snapRot;
        }

        tutorialManager?.NotifyCameraFollowBall();
    }

    public CameraMode GetCameraMode()
    {
        return currentMode;
    }
}
