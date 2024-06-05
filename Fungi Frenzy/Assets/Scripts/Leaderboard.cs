using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Leaderboard : MonoBehaviour
{
    [SerializeField] SpriteRenderer[] sprites;
    [SerializeField] CanvasRenderer[] canvases;
    [SerializeField] GameControl gameControl;
    bool clicked;
    public List<int> CurrentLeading;
    // Start is called before the first frame update
    void Start()
    {
        clicked = false;
        Color color = new(1, 1, 1, 0);
        foreach (SpriteRenderer s in sprites)
        {
            s.enabled = false;
        }
        for (int i= 0;i<canvases.Length;i++)
        {
            canvases[i].SetColor(color);
        }
    }

    public void ColorUpdate()
    {
        int[][] scores = gameControl.ScoreCount();
        int most = 0;
        List<int> winners = new(){0};
        for (int i=1;i<scores.Length;i++)
        {
            if (scores[i][0]-scores[i][1] > scores[most][0] - scores[most][1])
            {
                most = i;
                winners = new(){i};
            }
            else if(scores[i][0] - scores[i][1] == scores[most][0] - scores[most][1])
            {
                winners.Add(i);
            }
        }
        CurrentLeading = winners;
        GetComponent<SpriteRenderer>().color = winners.Count==1 ? TileObject.PlayerColors[most + 1] : winners.Count==4 ? TileObject.PlayerColors[0] :Average(winners); 
    }

    private Color Average(List<int> winners)
    {
        Vector3 v=new(0,0,0);
        foreach(int i in winners)
        {
            v += new Vector3(TileObject.PlayerColors[i+1].r, TileObject.PlayerColors[i+1].g, TileObject.PlayerColors[i+1].b);
        }
        v /= winners.Count;
        return new Color(v.x, v.y, v.z, 1);
    }

    public void UpdateScores()
    {
        int[][] scores = gameControl.ScoreCount();
        for (int i = 0; i < canvases.Length; i++)
        {
            canvases[i].GetComponent<Text>().text = "Occupied: " + scores[i][0] + ", Debt: " + scores[i][1];
        }
    }

    private void OnMouseDown()
    {
        clicked = !clicked;
        gameControl.LeaderboardActive = clicked;
        Color color = new(1, 1, 1, (clicked ? 1 : 0));
        for(int i= 0; i < sprites.Length; i++) 
        {
            if(i!=0 && GameControl.DeadPlayerList[i - 1])
            {
                continue;
            }
            sprites[i].enabled = clicked;
        }
        int[][] scores = gameControl.ScoreCount();
        for(int i= 0;i<canvases.Length;i++)
        {
            if (GameControl.DeadPlayerList[i])
            {
                continue;
            }
            CanvasRenderer c = canvases[i];
            c.SetColor(color);
            c.GetComponent<Text>().text="Occupied: " + scores[i][0] + ", Debt: " + scores[i][1];
        }
    }
}
