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


    public NetworkVariable<Vector3> dice1Position = new NetworkVariable<Vector3>(new Vector3(0,0,0)) ;
    public NetworkVariable<Vector3> dice2Position = new NetworkVariable<Vector3>(new Vector3(0,0,0));

    public NetworkVariable<Quaternion> dice1Rotation = new NetworkVariable<Quaternion>(new Quaternion(0,0,0,0));
    public NetworkVariable<Quaternion> dice2Rotation = new NetworkVariable<Quaternion>(new Quaternion(0,0,0,0));


    void Awake()
    {
        gameControlAssigned = false;
        DontDestroyOnLoad(this);

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
        RequestSynchServerRpc(false);
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
        int[] otherAttributes = new int[7];
        otherAttributes[0] = gameControl.CurrentTurn;
        otherAttributes[1] = gameControl.DiceRolling;
        otherAttributes[2] = gameControl.OccupyAmount;
        otherAttributes[3] = gameControl.GameOver ? 1 : 0;
        otherAttributes[4] = gameControl.StealingAndDonating;
        otherAttributes[5] = gameControl.StepCount;
        otherAttributes[6] = gameControl.SuperPowered;
        SynchronizeClientRpc(powerUpPositions, powerUpKinds,occupied, playerPositions, GameControl.DeadPlayerList, otherAttributes, endTurnCall);
    }

    [ClientRpc]
    private void SynchronizeClientRpc(int[] powerupPositions, int[] powerupKinds, int[] occupied, int[] playerPositions, bool[] deadPlayerList, int[] otherAttributes, bool endTurnCall)
    {
        if(IsHost || gameControl==null)
        {
            return;
        }
        if(endTurnCall)
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
    public void TileClickedServerRpc(int position)
    {
        RequestSynchServerRpc(false);
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

    [ServerRpc]
    public void WinnerDisplayServerRpc(int winner)
    {
        WinnerDisplayClientRpc(winner);
    }

    [ClientRpc]
    public void WinnerDisplayClientRpc(int winner)
    {
        gameControl.WinnerDisplayFunction(winner);
    }
}
