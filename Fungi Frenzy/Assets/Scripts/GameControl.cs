using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class GameControl : MonoBehaviour
{
    // Start is called before the first frame update
    public int OccupyAmount { get; private set; }
    public bool GameOver { get; private set; }
    public int CurrentTurn { get; private set; }
    public int DiceRolling { get; set; } //0: dice can be thrown, 1: dice has been thrown, 2: the value first read, 3: wait for the turn to end
    public int StealingAndDonating { get; private set; }
    public int StepCount { get; private set; }

    [SerializeField] private int SideLength;
    [SerializeField] private bool[] deadPlayerList;
    [SerializeField] private TileObject originalTile;
    [SerializeField] private Player originalPlayer;
    [SerializeField] private int defaultStepNo;
    [SerializeField] private Text remainingStepCounter;
    [SerializeField] private Text remainingStepLabel;
    [SerializeField] private SpriteRenderer winnerDisplay;
    [SerializeField] private SpriteRenderer crown;
    [SerializeField] private SpriteRenderer crownJewels;
    [SerializeField] private Sprite[] sprites;

    private TileObject[] board;
    private Player[] players;
    private int[] playerDebts;
    private int biggestTurnCounter;
    private int[] playerPositions;
    private int eatenPowerUps;
    private int toFill;


    void Start()
    {
        StealingAndDonating = 0;
        DiceRolling = 3;
        GameOver = false;
        biggestTurnCounter = -1;
        CurrentTurn = -1;
        board = new TileObject[SideLength * SideLength];
        players = new Player[4];
        playerDebts=new int[4];
        playerPositions =new int[4];
        if(deadPlayerList.Length!=4)
        {
            deadPlayerList = new bool[4];
            for(int i=0;i<deadPlayerList.Length;i++)
            {
                deadPlayerList[i] = false;
            }
        }
        StepCount = 0;
        float cubeSize= 11.59895f/SideLength;
        toFill = 0;
        eatenPowerUps = (int)(1.4f*SideLength)+1;
        OccupyAmount = 0;
        winnerDisplay.enabled = false;
        crown.enabled = false;
        crownJewels.enabled = false;
        CreateBoard(cubeSize);
    }

    void CreateBoard(float cubeSize)
    {
        Vector3 scale = new(cubeSize, cubeSize, 1);
        float cubeSizeLength = cubeSize*0.857f;
        int playerTerritories = (int) Mathf.Ceil((SideLength*6.0f)/25.0f);
        Vector3 position= new(-4.992f+ cubeSizeLength/2, 4.964f - cubeSizeLength / 2, 0);
        for (int i=0;i < SideLength * SideLength; i++)
        {
            GameObject clonedTile = Instantiate(originalTile.gameObject);
            clonedTile.transform.localScale=scale;
            clonedTile.transform.position = position+new Vector3((i%SideLength)*cubeSizeLength, -(i/SideLength)*cubeSizeLength,0);
            board[i]=clonedTile.GetComponent<TileObject>();
            board[i].occupiedBy=0;
            board[i].hasPlayerOn = false;
            board[i].powerUp = 0;
            board[i].moveable = false;
            board[i].boardPosition = i;
            if(i/SideLength < playerTerritories && i%SideLength < playerTerritories)
            {
                board[i].occupiedBy=1;
            }
            else if(i/SideLength < playerTerritories && i%SideLength>SideLength- playerTerritories-1)
            {
                board[i].occupiedBy = 2;
            }
            else if(i / SideLength > SideLength - playerTerritories-1 && i % SideLength > SideLength - playerTerritories-1)
            {
                board[i].occupiedBy = 3;
            }
            else if(i / SideLength > SideLength - playerTerritories-1 && i % SideLength < playerTerritories)
            {
                board[i].occupiedBy = 4;
            }
        }
        for(int i=0;i<players.Length;i++)
        {
            GameObject clonedPlayer = Instantiate(originalPlayer.gameObject);
            clonedPlayer.transform.localScale = scale;
            players[i] = clonedPlayer.GetComponent<Player>();
            players[i].playerNo = i;
            playerDebts[i] = 0;
        }
        playerPositions[0] = GetBoardPosition(new Vector2(0,0));
        playerPositions[1] = GetBoardPosition(new Vector2(SideLength - 1, 0));
        playerPositions[3] = GetBoardPosition(new Vector2(0, SideLength - 1));
        playerPositions[2] = GetBoardPosition(new Vector2(SideLength - 1, SideLength - 1));
        for(int i=0;i<playerPositions.Length;i++)
        {
            board[playerPositions[i]].hasPlayerOn=true;
        }
    }

    int GetBoardPosition(Vector2 v)
    {
        if(!LegalPosition(v))
        {
            print("Not a legal position!!!");
        }
        return (int)(v.x + v.y * SideLength);
    }

    bool LegalPosition(Vector2 v)
    {
        return !(v.x >= SideLength || v.x < 0 || v.y >= SideLength || v.y < 0);
    }

    Vector2 GetBoardCoordinate(int index)
    {
        return new Vector2(index%SideLength, index/SideLength);
    }

    void ReplenishPowerUps()
    {
        int chanceFactor = board.Length;
        int probabilityStealing = (int) (30 / (1 + Mathf.Exp(-biggestTurnCounter)) - 15);
        int probabilityDonating = (int) (12 / (1 + Mathf.Exp(-biggestTurnCounter)) - 6) + probabilityStealing;
        int probabilityClaiming = (int) (60 / (1 + Mathf.Exp(-biggestTurnCounter)) - 30) + probabilityDonating;
        //print("Food: " + (100-probabilityClaiming-probabilityDonating-probabilityStealing) + " Claiming: " + (probabilityClaiming-probabilityDonating) + " Stealing: " + probabilityStealing + " Donating: " + (probabilityDonating-probabilityStealing));
        int counter = toFill;
        
        while (counter > 0 && chanceFactor>=0)
        {
            chanceFactor--;
            int i = UnityEngine.Random.Range(0, SideLength*SideLength);
            if (board[i].occupiedBy == 0 && board[i].powerUp == 0)
            {
                int rand=UnityEngine.Random.Range(1, 101);
                if (rand <= probabilityStealing)
                {
                    board[i].powerUp = 4;
                }
                else if(rand<=probabilityDonating)
                {
                    board[i].powerUp = 3;
                }
                else if(rand<=probabilityClaiming)
                {
                    board[i].powerUp = 2;
                }
                else
                {
                    board[i].powerUp = 1;
                }
            }
            else
            {
                continue;
            }
            counter--;
            eatenPowerUps--;
        }
    }

    public void DiceToStepCount(int value)
    {
        if (value + playerDebts[CurrentTurn % 4] <= 0)
        {
            StepCount = 0;
            remainingStepCounter.text = value + "" +playerDebts[CurrentTurn % 4] + "";
            playerDebts[CurrentTurn % 4] += value;
        }
        else
        {
            StepCount = value + playerDebts[CurrentTurn % 4];
            int debt = playerDebts[CurrentTurn % 4];
            remainingStepCounter.text = value + (debt > 0 ? "+" + debt: (debt == 0 ? "": debt+""));
            playerDebts[CurrentTurn % 4] = 0;
        }
    }

    void Update()
    {
        if(GameOver)
        {
            return;
        }
        if(StepCount <= 0 && OccupyAmount<=0 && DiceRolling==3 && StealingAndDonating<=0)
        {
            remainingStepCounter.text = "0";
            if (CurrentTurn>0)
            {
                playerDebts[CurrentTurn % 4] += StepCount;
            }
            CurrentTurn++;
            if (CurrentTurn % 4 == 0)
            {
                biggestTurnCounter++;
                int winner = GetWinner();
                eatenPowerUps--;
                toFill = eatenPowerUps / 4;
                if (winner != -1)
                {
                    remainingStepCounter.text = "Player " + (winner + 1);
                    remainingStepLabel.text = "Winner is: ";
                    remainingStepCounter.fontSize = 40;
                    winnerDisplay.gameObject.GetComponent<Player>().playerNo = winner;
                    winnerDisplay.enabled = true;
                    crown.enabled = true;
                    crownJewels.enabled = true;
                    GameOver = true;
                    if(winner==1 || winner==2)
                    {
                        winnerDisplay.transform.position = new Vector3(6.97f, -1.96f, 0);
                    }
                    else
                    {
                        winnerDisplay.transform.position = new Vector3(7.08f, -1.96f, 0);
                    }
                }
            }
            if (deadPlayerList[CurrentTurn % 4])
            {
                return;
            }
            ReplenishPowerUps();
            MarkMoveables();
            if(IsKilled())
            {
                return;
            }
            /*if(IsKilled())
            {
                deadPlayerList[CurrentTurn % 4] = true;
                StepCount = 0;
                OccupyAmount = 0;
                DiceRolling = 3;
            }*/
            DiceRolling = 0;
            return;
        }

    }

    public Sprite GetSprite(int i)
    {
        return sprites[i];
    }

    public int GetWinner()
    {
        int[] playerPoints=new int[4];
        for(int i=0;i<board.Length; i++)
        {
            if (board[i].occupiedBy==0)
            {
                return -1;
            }
            if (deadPlayerList[board[i].occupiedBy-1])
            {
                continue;
            }
            playerPoints[board[i].occupiedBy - 1]++;
        }
        for(int i=0;i<playerPoints.Length;i++)
        {
            playerPoints[i] += playerDebts[i];
        }
        int maxIndex = 0;
        for(int i=0;i<playerPoints.Length;i++)
        {
            if (playerPoints[i] >= playerPoints[maxIndex])
            {
                maxIndex = i;
            }
        }
        return maxIndex;
    }

    public void MovePlayer(int pos)
    {
        if (board[pos].occupiedBy!=CurrentTurn%4+1 && board[pos].occupiedBy!=0 && !deadPlayerList[board[pos].occupiedBy-1])
        {
            StepCount--;
        }
        board[playerPositions[CurrentTurn % 4]].hasPlayerOn = false ;
        playerPositions[CurrentTurn % 4] = pos;
        board[pos].hasPlayerOn = true;
        StepCount--;
        if (board[pos].powerUp==1)  
        {
            StepCount += 3;
            board[pos].powerUp = 0;
            eatenPowerUps++;
        }
        else if (board[pos].powerUp==2)
        {
            OccupyAmount = 3;
            board[pos].powerUp = 0;
            eatenPowerUps++;
        }
        else if (board[pos].powerUp==3)
        {
            StealingAndDonating = 1;
            board[pos].powerUp = 0;
            StepCount += 3 + StealingAndDonating;
            eatenPowerUps++;
        }
        else if (board[pos].powerUp==4)
        {
            StealingAndDonating = -1;
            board[pos].powerUp = 0;
            StepCount += 3 + StealingAndDonating;
            eatenPowerUps++;
        }
        board[pos].occupiedBy=CurrentTurn%4+1;
        Fill(pos);
        MarkMoveables();
    }

    public void OccupyLand(int pos)
    {
        board[pos].occupiedBy = CurrentTurn % 4 + 1;
        OccupyAmount--;
        Fill(pos);
        MarkMoveables();
    }

    void FillSingle(int position)
    {
        Stack<int> stack = new();
        stack.Push(position);
        while (stack.Count > 0)
        {
            int index = stack.Pop();
            if (board[index].occupiedBy - 1 == CurrentTurn % 4)
            {
                continue;
            }
            board[index].occupiedBy = CurrentTurn % 4 + 1;
            List<TileObject> adjacent = AdjacentTiles(index);
            foreach (TileObject tile in adjacent)
            {
                stack.Push(tile.boardPosition);
            }
        }
    }

    bool ShouldFillSingle(int position)
    {
        Stack<int> stack = new();
        stack.Push(position);
        bool[] visited = new bool[board.Length];
        while (stack.Count > 0)
        {
            int index = stack.Pop();
            if (board[index].occupiedBy - 1 == CurrentTurn % 4 || visited[index])
            {
                continue;
            }
            visited[index] = true;
            List<TileObject> adjacent = AdjacentTiles(index);
            if (board[index].hasPlayerOn || adjacent.Count < 4)
            {
                return false;
            }
            foreach (TileObject tile in adjacent)
            {
                stack.Push(tile.boardPosition);
            }
        }
        return true;
    }

    void MarkMoveables()
    {
        for(int i=0;i<board.Length;i++)
        {
            board[i].moveable = false;
        }
        int pos = playerPositions[CurrentTurn % 4];
        board[pos].occupiedBy = CurrentTurn%4 + 1;
        IterativeMarking(pos);
        List<TileObject> tileObjects = AdjacentTiles(pos);
        foreach(TileObject tile in tileObjects)
        {
            if(!tile.hasPlayerOn)
            {
                tile.moveable = true;
            }
        }
    }


    void RecursiveMarking(int position)
    {
        int index= position;
        if (board[index].moveable || board[index].occupiedBy-1!=CurrentTurn%4)
        {
            return;
        }
        board[index].moveable= true;
        List<TileObject> adjacent = AdjacentTiles(index);
        foreach(TileObject tile in adjacent)
        {
            RecursiveMarking(tile.boardPosition);
        }
    }

    void IterativeMarking(int start)
    {
        Stack<int> stack=new();
        stack.Push(start);
        while (stack.Count > 0)
        {
            int index=stack.Pop();
            if(board[index].moveable || board[index].occupiedBy - 1 != CurrentTurn % 4)
            {
                continue;
            }
            board[index].moveable = true;
            List<TileObject> adjacent = AdjacentTiles(index);
            foreach (TileObject tile in adjacent)
            {
                stack.Push(tile.boardPosition);
            }
        }
    }

    void Fill(int position)
    {
        List<TileObject> adjacents=AdjacentTiles(position);
        foreach(TileObject tile in adjacents)
        {
            if(ShouldFillSingle(tile.boardPosition))
            {
                FillSingle(tile.boardPosition);
            }
        }
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < players.Length; i++)
        {
            players[i].transform.position = board[playerPositions[i]].transform.position + new Vector3(0, 0, board[0].transform.position.z - 1);
            board[playerPositions[i]].occupiedBy = i + 1;
        }
        if(!GameOver && DiceRolling==3)
        {
            remainingStepCounter.text = (StepCount<=0 ? 0: StepCount) + "";
        }
    }

    public void DonateAndSteal(int playerNo)
    {
        for(int i=0;i<board.Length;i++)
        {
            board[i].previouslySignaled = false;
        }
        if(StealingAndDonating>0)
        {
            playerDebts[playerNo] += 4;
        }
        else
        {
            playerDebts[playerNo] -= 2;
        }
        StealingAndDonating = 0;
    }

    public bool CanBeOccupiedByPowerUp(int index)
    {
        if (board[index].occupiedBy==CurrentTurn%4+1 || board[index].hasPlayerOn)
        {
            return false;
        }
        List<TileObject> adjacents=AdjacentTiles(index);
        for(int i=0;i<adjacents.Count;i++)
        {
            if (adjacents[i].occupiedBy==CurrentTurn%4+1)
            {
                return true;
            }
        }
        return false;
    }

    private bool IsKilled()
    {
        List<TileObject> tiles = AdjacentTiles(playerPositions[CurrentTurn % 4]);
        for(int i=0;i< tiles.Count;i++)
        {
            if (!tiles[i].hasPlayerOn)
            {
                return false;
            }
        }
        return true;
    }

    public bool IsDead(int playerNo)
    {
        return deadPlayerList[playerNo % 4];
    }

    public List<TileObject> AdjacentTiles(int tileIndex)
    {
        List<TileObject> tiles = new();
        Vector2 v = GetBoardCoordinate(tileIndex);
        if(LegalPosition(v+new Vector2(1,0)))
        {
            tiles.Add(board[GetBoardPosition(v + new Vector2(1, 0))]);
        }
        if(LegalPosition(v + new Vector2(0, 1)))
        {
            tiles.Add(board[GetBoardPosition(v + new Vector2(0, 1))]);
        }
        if (LegalPosition(v + new Vector2(-1, 0)))
        {
            tiles.Add(board[GetBoardPosition(v + new Vector2(-1, 0))]);
        }
        if (LegalPosition(v + new Vector2(0, -1)))
        {
            tiles.Add(board[GetBoardPosition(v + new Vector2(0, -1))]);
        }
        return tiles;
    }
}

