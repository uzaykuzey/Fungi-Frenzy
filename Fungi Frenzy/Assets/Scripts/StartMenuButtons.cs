using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenuButtons : MonoBehaviour
{
    public Button[] buttons;
    // Start is called before the first frame update
    void Start()
    {
        FourPlayers();
        buttons[0].onClick.AddListener(PlayButton);
        buttons[1].onClick.AddListener(TwoPlayers);
        buttons[2].onClick.AddListener(ThreePlayers);
        buttons[3].onClick.AddListener(FourPlayers);
    }

    void PlayButton()
    {
        SceneManager.LoadScene("Game");
    }

    void TwoPlayers()
    {
        GameControl.deadPlayerList = new bool[4];
        for (int i = 0; i < 4; i++)
        {
            GameControl.deadPlayerList[i] = i % 2 == 1;
        }
        buttons[1].GetComponent<Image>().color = Color.white;
        buttons[2].GetComponent<Image>().color = Color.gray;
        buttons[3].GetComponent<Image>().color = Color.gray;
    }

    void ThreePlayers()
    {
        GameControl.deadPlayerList = new bool[4];
        for (int i = 0; i < 4; i++)
        {
            GameControl.deadPlayerList[i] = i==2;
        }
        buttons[2].GetComponent<Image>().color = Color.white;
        buttons[1].GetComponent<Image>().color = Color.gray;
        buttons[3].GetComponent<Image>().color = Color.gray;
    }

    void FourPlayers()
    {
        GameControl.deadPlayerList = new bool[4];
        for(int i = 0;i<4;i++)
        {
            GameControl.deadPlayerList[i] = false;
        }
        buttons[3].GetComponent<Image>().color = Color.white;
        buttons[2].GetComponent<Image>().color = Color.gray;
        buttons[1].GetComponent<Image>().color = Color.gray;
    }
}
