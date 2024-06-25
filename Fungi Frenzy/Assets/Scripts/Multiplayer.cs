using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Multiplayer : NetworkBehaviour
{
    // Start is called before the first frame update
    private GameControl gameControl=null;
    public static Multiplayer Instance;
    private bool gameControlAssigned;

    public NetworkVariable<int> CurrentStepCount = new(0);
    public NetworkVariable<int> CurrentTurn = new(0);


    public NetworkVariable<Vector3> dice1Position = new NetworkVariable<Vector3>(new Vector3(0,0,0)) ;
    public NetworkVariable<Vector3> dice2Position = new NetworkVariable<Vector3>(new Vector3(0,0,0));

    public NetworkVariable<Quaternion> dice1Rotation = new NetworkVariable<Quaternion>(new Quaternion(0,0,0,0));
    public NetworkVariable<Quaternion> dice2Rotation = new NetworkVariable<Quaternion>(new Quaternion(0,0,0,0));


    void Awake()
    {
        gameControlAssigned = false;
        DontDestroyOnLoad(this);
        CurrentStepCount.OnValueChanged += (int prevValue, int newValue) =>
        {
            gameControl.StepCount = newValue;
        };

        CurrentTurn.OnValueChanged += (int prevValue, int newValue) =>
        {
            gameControl.CurrentTurn = newValue;
        };

        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if(gameControlAssigned || (gameControl==null && GameControl.MainGameControl==null))
        {
            return;
        }
        gameControl=GameControl.MainGameControl;
        GameControl.ThisMultiplayer = this;
        gameControlAssigned = true;
        RequestSynchServerRpc();
    }

    private void FixedUpdate()
    {
        if(gameControl!=null && IsHost)
        {
            CurrentStepCount.Value = gameControl.StepCount;
            CurrentTurn.Value= gameControl.CurrentTurn;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestSynchServerRpc()
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
        SynchronizeClientRpc(powerUpPositions, powerUpKinds,occupied, playerPositions, GameControl.DeadPlayerList);
    }

    [ClientRpc]
    private void SynchronizeClientRpc(int[] powerupPositions, int[] powerupKinds, int[] occupied, int[] playerPositions, bool[] deadPlayerList)
    {
        if(IsHost || gameControl==null)
        {
            return;
        }
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
    public void TileClickedServerRpc(int position)
    {
        TileClickedClientRpc(position);
    }

    [ClientRpc]
    private void TileClickedClientRpc(int position)
    {
        gameControl.board[position].Click();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RollClickedServerRpc()
    {
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
    public void StartGameServerRpc(int playerCount)
    {
        StartGameClientRpc(playerCount);
    }

    [ClientRpc]
    public void StartGameClientRpc(int playerCount)
    {

        /*else if(playerCount==2)
        {
            GameControl.DeadPlayerList = new bool[4];
            for (int i = 0; i < 4; i++)
            {
                GameControl.DeadPlayerList[i] = i % 2 == 1;
            }
        }
        else if(playerCount==3)
        {
            GameControl.DeadPlayerList = new bool[4];
            for (int i = 0; i < 4; i++)
            {
                GameControl.DeadPlayerList[i] = i == 2;
            }
        }
        else*/
        {
            GameControl.DeadPlayerList = new bool[4];
            for (int i = 0; i < 4; i++)
            {
                GameControl.DeadPlayerList[i] = false;
            }
        }
        SceneManager.LoadScene("Game");
    }
}
