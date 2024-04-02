using Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PlayerCameraManager : NetworkBehaviour
{
    [SerializeField] private GameObject playerVirtualCamera;
    [SerializeField] private GameObject cameraFollowTarget;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner)
        {
            playerVirtualCamera.GetComponent<CinemachineVirtualCamera>().Follow = cameraFollowTarget.transform;
            playerVirtualCamera.SetActive(true);
        }
    }
}
