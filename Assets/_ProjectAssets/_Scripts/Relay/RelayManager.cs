using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayManager : MonoBehaviour
{
    public event EventHandler ClientRelayDataSet;
    public static RelayManager Instance { get; private set; }

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    public async Task<string> CreateRelay()
    {
		try
		{
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log("Relay join code: " + joinCode);
            RelayServerData data = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(data);
            return joinCode;
		}
		catch (RelayServiceException e)
		{
			Debug.LogWarning("Failed to create new relay");
			Debug.Log(e);
            return null;
		}
    }

	public async void JoinRelay(string joinCode)
	{
        try
        {
            Debug.Log("Joining relay with code: " + joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            RelayServerData data = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(data);
            ClientRelayDataSet?.Invoke(this, EventArgs.Empty);
        }
        catch (RelayServiceException e)
        {
            Debug.LogWarning("Failed to join relay");
            Debug.Log(e);
        }
    }
}
