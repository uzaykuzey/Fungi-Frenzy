using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Multiplayer : NetworkBehaviour
{
    // Start is called before the first frame update
    private static GameControl gameControl=null;
    private static LobbyManager lobbyManager =null;
    private NetworkVariable<bool> gameControlAssigned=new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<bool> lobbyAssigned = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);



    public static NetworkVariable<Vector3> dice1Position = new NetworkVariable<Vector3>(new Vector3(0,0,0)) ;
    public static NetworkVariable<Vector3> dice2Position = new NetworkVariable<Vector3>(new Vector3(0,0,0));

    public static NetworkVariable<Quaternion> dice1Rotation = new NetworkVariable<Quaternion>(new Quaternion(0,0,0,0));
    public static NetworkVariable<Quaternion> dice2Rotation = new NetworkVariable<Quaternion>(new Quaternion(0,0,0,0));


    public static Dictionary<ulong, int> playerChoicesDictionary;

    void Awake()
    {
        lobbyAssigned.Value = false;
        gameControlAssigned.Value = false;
        playerChoicesDictionary = new Dictionary<ulong, int>();
        DontDestroyOnLoad(this);
    }

    public static void ResetValues()
    {
        gameControl = null;
        lobbyManager = null;
        dice1Position = new NetworkVariable<Vector3>(new Vector3(0, 0, 0));
        dice2Position = new NetworkVariable<Vector3>(new Vector3(0, 0, 0));
        dice1Rotation = new NetworkVariable<Quaternion>(new Quaternion(0, 0, 0, 0));
        dice2Rotation = new NetworkVariable<Quaternion>(new Quaternion(0, 0, 0, 0));
}

    

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner)
        {
            return;
        }
        if (!gameControlAssigned.Value && GameControl.MainGameControl != null && GameControl.ThisMultiplayer == null && gameControl == null)
        {
            gameControl = GameControl.MainGameControl;
            GameControl.ThisMultiplayer = LobbyManager.multiplayerInstance;
            gameControlAssigned.Value = true;
            RequestSynchServerRpc(false);
        }
        if (!lobbyAssigned.Value && lobbyManager == null && LobbyManager.multiplayerInstance == null && LobbyManager.instance != null)
        {
            lobbyManager = LobbyManager.instance;
            LobbyManager.multiplayerInstance = this;
            lobbyAssigned.Value = true;
            SynchronizeLobbyServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestSynchServerRpc(bool endTurnCall)
    {
        int[] occupied = new int[gameControl.board.Length];
        int count = 0;
        for(int i=0;i<gameControl.board.Length;i++)
        {
            if (gameControl.board[i].powerUp!=0)
            {
                count++;
            }
            occupied[i] = gameControl.board[i].occupiedBy;
        }
        int[] powerUpPositions=new int[count];
        int[] powerUpKinds = new int[count];
        int[] playerPositions = new int[4];
        int j = 0;
        int k = 0;
        for(int i=0;i< gameControl.board.Length; i++)
        {
            if (gameControl.board[i].powerUp != 0)
            {
                powerUpPositions[j] = i;
                powerUpKinds[j] = gameControl.board[i].powerUp;
                j++;
            }
            if (gameControl.board[i].hasPlayerOn)
            {
                playerPositions[k] = i;
                k++;
            }
        }
        int[] otherAttributes = new int[8];
        otherAttributes[0] = gameControl.CurrentTurn;
        otherAttributes[1] = gameControl.DiceRolling;
        otherAttributes[2] = gameControl.OccupyAmount;
        otherAttributes[3] = gameControl.GameOver ? 1 : 0;
        otherAttributes[4] = gameControl.StealingAndDonating;
        otherAttributes[5] = gameControl.StepCount;
        otherAttributes[6] = gameControl.SuperPowered;
        otherAttributes[7] = GameControl.SideLength;
        SynchronizeGameClientRpc(powerUpPositions, powerUpKinds, occupied, playerPositions, GameControl.DeadPlayerList, otherAttributes, endTurnCall);
    }

    [ClientRpc]
    private void SynchronizeGameClientRpc(int[] powerupPositions, int[] powerupKinds, int[] occupied, int[] playerPositions, bool[] deadPlayerList, int[] otherAttributes, bool endTurnCall)
    {
        if(IsHost || gameControl==null)
        {
            return;
        }
        if (GameControl.SideLength != otherAttributes[7])
        {
            GameControl.SideLength = otherAttributes[7];
            GameControl.MainGameControl = null;
            GameControl.ThisMultiplayer = null;
            gameControlAssigned.Value = false;
            gameControl = null;
            SceneManager.LoadScene("Game");
            return;
        }
        if (endTurnCall)
        {
            gameControl.remainingStepCounter.text = "0";
        }
        gameControl.CurrentTurn = otherAttributes[0];
        gameControl.DiceRolling= otherAttributes[1];
        gameControl.OccupyAmount= otherAttributes[2];
        gameControl.GameOver = otherAttributes[3] == 1;
        gameControl.StealingAndDonating= otherAttributes[4];
        gameControl.StepCount= otherAttributes[5];
        gameControl.SuperPowered = otherAttributes[6];
        
        for(int i=0;i<gameControl.board.Length;i++)
        {
            gameControl.board[i].powerUp = 0;
            gameControl.board[i].occupiedBy = occupied[i];
            gameControl.board[i].hasPlayerOn = false;
        }
        for(int i=0;i<powerupPositions.Length;i++)
        {
            gameControl.board[powerupPositions[i]].powerUp = powerupKinds[i];
        }
        foreach(int i in playerPositions)
        {
            gameControl.board[i].hasPlayerOn = true;
            gameControl.playerPositions[gameControl.board[i].occupiedBy - 1] = i;
        }
        GameControl.DeadPlayerList = deadPlayerList;
    }

    [ServerRpc(RequireOwnership = false)]
    public void TileClickedServerRpc(int position, ulong playerId)
    {
        playerChoicesDictionary.TryGetValue(playerId, out int value);
        if (value != gameControl.CurrentTurn%4)
        {
            return;
        }
        RequestSynchServerRpc(false);
        TileClickedClientRpc(position);
    }

    [ClientRpc]
    private void TileClickedClientRpc(int position)
    {
        gameControl.board[position].Click();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RollClickedServerRpc(ulong playerId)
    {
        playerChoicesDictionary.TryGetValue(playerId, out int value);
        if (value != gameControl.CurrentTurn % 4)
        {
            return;
        }
        RequestSynchServerRpc(false);
        RollClickedClientRpc();
    }

    [ClientRpc]
    private void RollClickedClientRpc()
    {
        gameControl.RollButton.Clicked();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SecretServerRpc()
    {
        SecretClientRpc();
    }

    [ClientRpc]
    public void SecretClientRpc()
    {
        gameControl.players[0].ActivateSecret();
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartGameServerRpc(bool[] playerChoices)
    {
        StartGameClientRpc(playerChoices);
    }

    [ClientRpc]
    public void StartGameClientRpc(bool[] playerChoices)
    {
        int noOfPlayers = 0;
        for(int i=0; i<playerChoices.Length;i++)
        {
            if (playerChoices[i])
            {
                noOfPlayers++;
            }
        }
        if(noOfPlayers<2)
        {
            return;
        }
        for(int i=0;i<playerChoices.Length;i++)
        {
            GameControl.DeadPlayerList[i]= !(playerChoices[i]);
        }
        SceneManager.LoadScene("Game");
    }

    [ServerRpc(RequireOwnership = false)]
    public void WinnerDisplayServerRpc(int winner)
    {
        WinnerDisplayClientRpc(winner);
    }

    [ClientRpc]
    public void WinnerDisplayClientRpc(int winner)
    {
        gameControl.WinnerDisplayFunction(winner);
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerChoosingServerRpc(int playerNo, ulong playerId)
    {
        if (playerChoicesDictionary.ContainsValue(playerNo))
        {
            SynchronizeLobbyServerRpc();
            return;
        }
        if (playerChoicesDictionary.ContainsKey(playerId))
        {
            playerChoicesDictionary.TryGetValue(playerId, out int value);
            lobbyManager.playerChoices[value] = false;
            playerChoicesDictionary.Remove(playerId);
        }
        playerChoicesDictionary.Add(playerId, playerNo);
        lobbyManager.playerChoices[playerNo] = true;
        SynchronizeLobbyServerRpc();
        return;
    }

    [ClientRpc]
    private void SynchronizeLobbyClientRpc(bool[] playerChoices)
    {
        for(int i=0;i<playerChoices.Length;i++)
        {
            lobbyManager.playerChoices[i] = playerChoices[i];
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SynchronizeLobbyServerRpc()
    {
        SynchronizeLobbyClientRpc(lobbyManager.playerChoices);
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerLeftServerRpc(ulong playerId)
    {
        if(SceneManager.GetActiveScene().name=="Lobby")
        {
            if (playerChoicesDictionary.ContainsKey(playerId))
            {
                playerChoicesDictionary.TryGetValue(playerId, out int value);
                lobbyManager.playerChoices[value] = false;
                playerChoicesDictionary.Remove(playerId);
            }
            SynchronizeLobbyServerRpc();
            return;
        }
        lobbyManager.LoadStartMenuClientRpc();
    }
}
