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
        TwentyxTwenty();
        buttons[0].onClick.AddListener(PlayButton);
        buttons[1].onClick.AddListener(TwoPlayers);
        buttons[2].onClick.AddListener(ThreePlayers);
        buttons[3].onClick.AddListener(FourPlayers);
        buttons[4].onClick.AddListener(FifteenxFifteen);
        buttons[5].onClick.AddListener(TwentyxTwenty);
        buttons[6].onClick.AddListener(TwentyFivexTwentyFive);
    }

    void PlayButton()
    {
        SceneManager.LoadScene("Game");
    }

    void TwoPlayers()
    {
        GameControl.DeadPlayerList = new bool[4];
        for (int i = 0; i < 4; i++)
        {
            GameControl.DeadPlayerList[i] = i % 2 == 1;
        }
        buttons[1].GetComponent<Image>().color = Color.white;
        buttons[2].GetComponent<Image>().color = Color.gray;
        buttons[3].GetComponent<Image>().color = Color.gray;
    }

    void ThreePlayers()
    {
        GameControl.DeadPlayerList = new bool[4];
        for (int i = 0; i < 4; i++)
        {
            GameControl.DeadPlayerList[i] = i==2;
        }
        buttons[2].GetComponent<Image>().color = Color.white;
        buttons[1].GetComponent<Image>().color = Color.gray;
        buttons[3].GetComponent<Image>().color = Color.gray;
    }

    void FourPlayers()
    {
        GameControl.DeadPlayerList = new bool[4];
        for(int i = 0;i<4;i++)
        {
            GameControl.DeadPlayerList[i] = false;
        }
        buttons[3].GetComponent<Image>().color = Color.white;
        buttons[2].GetComponent<Image>().color = Color.gray;
        buttons[1].GetComponent<Image>().color = Color.gray;
    }

    void TwentyxTwenty()
    {
        GameControl.SideLength = 20;
        buttons[5].GetComponent<Image>().color=Color.white;
        buttons[4].GetComponent<Image>().color = Color.gray;
        buttons[6].GetComponent<Image>().color = Color.gray;
    }

    void FifteenxFifteen()
    {
        GameControl.SideLength = 15;
        buttons[4].GetComponent<Image>().color = Color.white;
        buttons[5].GetComponent<Image>().color = Color.gray;
        buttons[6].GetComponent<Image>().color = Color.gray;
    }

    void TwentyFivexTwentyFive()
    {
        GameControl.SideLength = 25;
        buttons[6].GetComponent<Image>().color = Color.white;
        buttons[5].GetComponent<Image>().color = Color.gray;
        buttons[4].GetComponent<Image>().color = Color.gray;
    }
}
