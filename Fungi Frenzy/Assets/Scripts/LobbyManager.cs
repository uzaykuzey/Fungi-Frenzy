using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    private int playerCount;
    [SerializeField] Button[] buttons;
    [SerializeField] Text codeShow;
    [SerializeField] InputField inputField;
    private async void Start()
    {
        DontDestroyOnLoad(this);
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        buttons[0].onClick.AddListener(CreateRelay);
        buttons[1].onClick.AddListener(JoinRelay);
        buttons[2].onClick.AddListener(() =>
        {
            if(Multiplayer.Instance != null && Multiplayer.Instance.IsHost)
            {
                Multiplayer.Instance.StartGameServerRpc(playerCount);
            }
        });
        
    }

    private async void CreateRelay()
    {
        try
        {
            Allocation allocation=await RelayService.Instance.CreateAllocationAsync(3);
            codeShow.text = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            RelayServerData data = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(data);
            NetworkManager.Singleton.StartHost();
        }
        catch(RelayServiceException e)
        {
            Debug.LogException(e);
        }
    }

    private async void JoinRelay()
    {
        try
        {
            JoinAllocation allocation=await RelayService.Instance.JoinAllocationAsync(inputField.text);
            RelayServerData data = new(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(data);
            NetworkManager.Singleton.StartClient();
        }
        catch(RelayServiceException e)
        {
            Debug.LogException(e);
        }
    }
}
