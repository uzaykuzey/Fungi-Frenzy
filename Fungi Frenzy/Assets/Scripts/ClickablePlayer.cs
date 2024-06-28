using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickablePlayer : MonoBehaviour
{
    [SerializeField] LobbyManager lobbyManager;
    [SerializeField] int playerNo;

    private void OnMouseDown()
    {
        lobbyManager.Click(playerNo);
    }
}