using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

public class TeleportMenu : MonoBehaviour
{
    [SerializeField] Transform teleportPoint;
    [SerializeField] float deltaPosition;
    private TeleportationProvider m_LocalPlayerTeleportProvider;

    private void Start()
    {
        m_LocalPlayerTeleportProvider = FindFirstObjectByType<TeleportationProvider>();
    }

    public void Teleport()
    {
        TeleportRequest teleportRequest = new()
        {
            destinationPosition = teleportPoint.position +
            new Vector3(
                Random.Range(-deltaPosition, deltaPosition),
                0,
                Random.Range(-deltaPosition, deltaPosition)
            ),
            destinationRotation = teleportPoint.rotation,
            matchOrientation = MatchOrientation.TargetUpAndForward
        };
        m_LocalPlayerTeleportProvider.QueueTeleportRequest(teleportRequest);
    }
}
