using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] Button[] buttons;
    [SerializeField] Text codeShow;
    [SerializeField] InputField inputField;
    [SerializeField] SpriteRenderer[] players;
    [SerializeField] Sprite[] sprites;

    public static LobbyManager instance=null;
    public static Multiplayer multiplayerInstance=null;

    public bool[] playerChoices=new bool[4];

    private async void Start()
    {
        instance = this;
        playerChoices = new bool[4];
        for (int i = 0; i < playerChoices.Length; i++)
        {
            playerChoices[i] = false;
        }
        DontDestroyOnLoad(this);
        await UnityServices.InitializeAsync();
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch { }
        buttons[0].onClick.AddListener(CreateRelay);
        buttons[1].onClick.AddListener(JoinRelay);
        buttons[2].onClick.AddListener(() =>
        {
            if(multiplayerInstance != null && multiplayerInstance.IsHost)
            {
                multiplayerInstance.StartGameServerRpc(playerChoices);
            }
        });
        NetworkManager.Singleton.OnClientDisconnectCallback += (ulong client) =>
        {
            LoadStartMenuClientRpc();
        };
    }



    private void FixedUpdate()
    {
        if(SceneManager.GetActiveScene().name != "Lobby")
        {
            return;
        }
        for(int i= 0;i < 4; i++)
        {
            players[i].sprite = sprites[i + (playerChoices[i] ? 0:4)];
            /*if(spinning[i])
            {
                Vector3 v = players[i].transform.rotation.eulerAngles + new Vector3(0, 10, 0);
                players[i].transform.rotation = Quaternion.Euler(v.x, v.y, v.z);
            }*/
        }
    }

    private async void CreateRelay()
    {
        try
        {
            Allocation allocation=await RelayService.Instance.CreateAllocationAsync(3);
            codeShow.text = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            RelayServerData data = new(allocation, "dtls");
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
            codeShow.text=inputField.text;
        }
        catch(RelayServiceException e)
        {
            Debug.LogException(e);
        }
    }

    public void Click(int playerNo)
    {
        if(multiplayerInstance==null)
        {
            return;
        }
        multiplayerInstance.PlayerChoosingServerRpc( playerNo,multiplayerInstance.OwnerClientId);
    }

    [ClientRpc]
    public void LoadStartMenuClientRpc()
    {
        Multiplayer.ResetValues();
        GameControl.ThisMultiplayer = null;
        GameControl.MainGameControl = null;
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if(obj==this)
            {
                continue;
            }
            Destroy(obj);
        }
        Destroy(gameObject);
        SceneManager.LoadScene("Start Menu");
    }
}


