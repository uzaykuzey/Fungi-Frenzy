using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameControl : MonoBehaviour
{
    // Start is called before the first frame update
    public static GameControl MainGameControl { get; set; }
    public static Multiplayer ThisMultiplayer { get; set; }
    public int OccupyAmount { get; private set; }
    public bool GameOver { get; private set; }
    public int CurrentTurn { get; set; }
    public int DiceRolling { get; set; } //0: dice can be thrown, 1: dice has been thrown, 2: the value first read, 3: wait for the turn to end
    public int StealingAndDonating { get; private set; }
    public int StepCount { get; set; }
    public bool LeaderboardActive { get; set; }
    public int SuperPowered { get; private set; }

    [SerializeField] private TileObject originalTile;
    [SerializeField] private Player originalPlayer;
    [SerializeField] private Text remainingStepCounter;
    [SerializeField] private Text remainingStepLabel;
    [SerializeField] private SpriteRenderer winnerDisplay;
    [SerializeField] private SpriteRenderer crown;
    [SerializeField] private SpriteRenderer crownJewels;
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private Leaderboard leaderboard;

    public TileObject[] board;
    public Player[] players;
    private int[] playerDebts;
    private int biggestTurnCounter;
    private int[] playerPositions;
    private int eatenPowerUps;
    private int toFill;
    private bool alreadyBoosted;
    public DiceRolling RollButton;

    public static bool[] DeadPlayerList;
    public static int SideLength;
    public static bool multiplayer;

    


    void Start()
    {
        MainGameControl = this;
        alreadyBoosted = false;
        StealingAndDonating = 0;
        DiceRolling = 3;
        GameOver = false;
        biggestTurnCounter = -1;
        CurrentTurn = -1;
        board = new TileObject[SideLength * SideLength];
        players = new Player[4];
        playerDebts=new int[4];
        playerPositions =new int[4];
        if(DeadPlayerList.Length!=4)
        {
            DeadPlayerList = new bool[4];
            for(int i=0;i<DeadPlayerList.Length;i++)
            {
                DeadPlayerList[i] = false;
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
        SuperPowered = 0;
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

    private bool SupersCanSpawn()
    {
        if(biggestTurnCounter<2 || leaderboard.CurrentLeading.Contains(CurrentTurn%4))
        {
            return false;
        }
        for(int i=0;i<board.Length;i++) 
        {
            if (board[i].powerUp==5 || board[i].powerUp==6)
            {
                return false;
            }
        }
        return true;
    }

    private int SuperBoost()
    {
        if(biggestTurnCounter<5 || alreadyBoosted)
        {
            return 0;
        }
        int[][] scores = ScoreCount();
        int biggest = scores[0][0] > scores[0][1] ? 0 : 1;
        int secondBiggest = 1-biggest;
        for(int i = 2; i < scores[0].Length;i++)
        {
            if (scores[0][i] > scores[0][biggest])
            {
                secondBiggest = biggest;
                biggest = i;
            }
        }
        int p = scores[0][biggest] - scores[0][secondBiggest];
        return  p>= 50.0/400.0 * board.Length ? 12: (p>= 37.0/400.0 * board.Length ? 7: 0);
    }

    void ReplenishPowerUps()
    {
        if(multiplayer && (ThisMultiplayer==null || !ThisMultiplayer.IsHost))
        {
            return;
        }
        int chanceFactor = board.Length;
        float sigmoidReciprocal = 1 + Mathf.Exp(-biggestTurnCounter);
        int probabilitySuper = SupersCanSpawn() ? ((int)(4.4 / (1 + Mathf.Exp(-biggestTurnCounter * 0.7054651f)) - 2.2) + SuperBoost()): 0;
        int probabilityStealing = (int) (30 / sigmoidReciprocal - 15) + probabilitySuper;
        int probabilityDonating = (int) (12 / sigmoidReciprocal - 6) + probabilityStealing;
        int probabilityClaiming = (int) (60 / sigmoidReciprocal - 30) + probabilityDonating;
        int counter = toFill;
        while (counter > 0 && chanceFactor>=0)
        {
            chanceFactor--;
            int i = Random.Range(0, board.Length);
            if (board[i].occupiedBy == 0)
            {
                int rand=Random.Range(1, 101);
                if(rand<=probabilitySuper)
                {
                    board[i].powerUp = Random.Range(0, 2)==0 ? 5: 6;
                    if(probabilitySuper>4)
                    {
                        alreadyBoosted = true;
                    }
                    probabilityStealing -= probabilitySuper;
                    probabilityDonating -= probabilitySuper;
                    probabilityClaiming -= probabilitySuper;
                    probabilitySuper = 0; 
                }
                else if (board[i].powerUp != 0)
                {
                    continue;
                }
                else if (rand <= probabilityStealing)
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
        if(multiplayer)
        {
            ThisMultiplayer.RequestSynchServerRpc();
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
        leaderboard.ColorUpdate();
        if(LeaderboardActive)
        {
            leaderboard.UpdateScores();
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
            SuperPowered = 0;
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
            if (DeadPlayerList[CurrentTurn % 4])
            {
                return;
            }
            ReplenishPowerUps();
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
            leaderboard.ColorUpdate();
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
        bool draw = false;
        for(int i=0;i<board.Length; i++)
        {
            if (board[i].occupiedBy==0)
            {
                return -1;
            }
            if (DeadPlayerList[board[i].occupiedBy-1])
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
        for(int i=1;i<playerPoints.Length;i++)
        {
            if (playerPoints[i] > playerPoints[maxIndex])
            {
                maxIndex = i;
                draw = false;
            }
            else if(playerPoints[i] == playerPoints[maxIndex])
            {
                draw = true;
            }
        }
        return draw ? -1 : maxIndex;
    }

    public bool CanMove(int position)
    {
        List<TileObject> tiles=AdjacentTiles(position);
        if(SuperPowered==2)
        {
            tiles.AddRange(Diagonals(position));
        }
        foreach(TileObject tile in tiles)
        {
            if(tile.hasPlayerOn && tile.occupiedBy==CurrentTurn%4+1)
            {
                return true;
            }
        }
        Stack<int> stack=new();
        stack.Push(position);
        bool[] visited=new bool[board.Length];
        while(stack.Count>0)
        {
            int current=stack.Pop();
            if (visited[current] || board[current].occupiedBy!=CurrentTurn%4+1)
            {
                continue;
            }
            if (board[current].hasPlayerOn)
            {
                return true;
            }
            visited[current]=true;
            tiles = AdjacentTiles(current);
            if (SuperPowered == 2)
            {
                tiles.AddRange(Diagonals(current));
            }
            foreach(TileObject tile in tiles)
            {
                stack.Push(tile.boardPosition);
            }
        }
        return false;
    }

    public void AutoMove(int position)
    {
        Vector2 destination = GetBoardCoordinate(position);
        Vector2 start = GetBoardCoordinate(playerPositions[CurrentTurn % 4]);
        if(destination.x-start.x == 0 || destination.y-start.y == 0)
        {
            Vector2 currentPos = start;
            int cost = 0;
            Vector2 direction=(destination-start).normalized;
            while(destination!=currentPos)
            {
                currentPos += direction;
                if (board[GetBoardPosition(currentPos)].occupiedBy == CurrentTurn % 4 + 1)
                {
                    return;
                }
                else if(board[GetBoardPosition(currentPos)].occupiedBy == 0 || DeadPlayerList[board[GetBoardPosition(currentPos)].occupiedBy-1])
                {
                    cost++;
                }
                else
                {
                    if(currentPos == destination)
                    {
                        cost--;
                    }
                    cost += 2;
                }
                if(board[GetBoardPosition(currentPos)].powerUp==1)
                {
                    cost -= 3;
                }
                else if(((board[GetBoardPosition(currentPos)].powerUp != 0 || cost >= StepCount) && currentPos!=destination) || board[GetBoardPosition(currentPos)].hasPlayerOn)
                {
                    return;
                }
            }
            if(cost<=StepCount)
            {
                currentPos = start;
                while (currentPos!=destination)
                {
                    currentPos += direction;
                    MovePlayer(GetBoardPosition(currentPos));
                }
            }
        }
    }

    public void MovePlayer(int pos)
    {
        if (board[pos].occupiedBy!=CurrentTurn%4+1 && board[pos].occupiedBy!=0 && !DeadPlayerList[board[pos].occupiedBy-1] && SuperPowered!=1)
        {
            StepCount--;
        }
        board[playerPositions[CurrentTurn % 4]].hasPlayerOn = false ;
        playerPositions[CurrentTurn % 4] = pos;
        board[pos].hasPlayerOn = true;
        StepCount--;
        switch (board[pos].powerUp)
        {
            case 1:
                StepCount += 3;
                board[pos].powerUp = 0;
                eatenPowerUps++;
                break;

            case 2:
                OccupyAmount = 3;
                board[pos].powerUp = 0;
                eatenPowerUps++;
                break;

            case 3:
                StealingAndDonating = 1;
                board[pos].powerUp = 0;
                StepCount += 3 + StealingAndDonating;
                eatenPowerUps++;
                break;

            case 4:
                StealingAndDonating = -1;
                board[pos].powerUp = 0;
                StepCount += 3 + StealingAndDonating;
                eatenPowerUps++;
                break;

            case 5:
                if (!leaderboard.CurrentLeading.Contains(CurrentTurn % 4))
                {
                    SuperPowered = 1;
                    board[pos].powerUp = 0;
                    StepCount++;
                    eatenPowerUps++;
                }
                break;

            case 6:
                if (!leaderboard.CurrentLeading.Contains(CurrentTurn % 4))
                {
                    SuperPowered = 2;
                    board[pos].powerUp = 0;
                    StepCount += 4;
                    eatenPowerUps++;
                    TileObject.AlphaPl = 0.5f;
                }
                break;

            default:
                break;
        }
        board[pos].occupiedBy=CurrentTurn%4+1;
        Fill(pos);
        leaderboard.ColorUpdate();
    }

    public void OccupyLand(int pos)
    {
        board[pos].occupiedBy = CurrentTurn % 4 + 1;
        OccupyAmount--;
        Fill(pos);
        leaderboard.ColorUpdate();
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
        if(multiplayer)
        {

        }
    }

    public void ClearSignals()
    {
        for (int i = 0; i < board.Length; i++)
        {
            board[i].previouslySignaled = false;
        }
    }

    public void DonateAndSteal(int playerNo)
    {
        ClearSignals();
        if(StealingAndDonating>0)
        {
            playerDebts[playerNo] += 4;
        }
        else
        {
            playerDebts[playerNo] -= 2;
        }
        StealingAndDonating = 0;
        leaderboard.ColorUpdate();
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
        if(SuperPowered==2)
        {
            adjacents = Diagonals(index);
            for (int i = 0; i < adjacents.Count; i++)
            {
                if (adjacents[i].occupiedBy == CurrentTurn % 4 + 1)
                {
                    return true;
                }
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

    //2D array, first chooses the player, then index 0 is the occupied tile count and index 1 is debt count multiplied by -1.
    public int[][] ScoreCount()
    {
        int[][] scores = new int[4][];
        for(int i=0;i< scores.Length;i++)
        {
            scores[i] = new int[2];
            scores[i][1] = -playerDebts[i];
            scores[i][0] = 0;
        }
        foreach(TileObject tile in board)
        {
            if(tile.occupiedBy==0)
            {
                continue;
            }
            scores[tile.occupiedBy - 1][0]++;
        }
        return scores;
    }

    public List<TileObject> Diagonals(int tileIndex)
    {
        List<TileObject> tiles = new();
        Vector2 v = GetBoardCoordinate(tileIndex);
        if (LegalPosition(v + new Vector2(1, 1)))
        {
            tiles.Add(board[GetBoardPosition(v + new Vector2(1, 1))]);
        }
        if (LegalPosition(v + new Vector2(1, -1)))
        {
            tiles.Add(board[GetBoardPosition(v + new Vector2(1, -1))]);
        }
        if (LegalPosition(v + new Vector2(-1, -1)))
        {
            tiles.Add(board[GetBoardPosition(v + new Vector2(-1, -1))]);
        }
        if (LegalPosition(v + new Vector2(-1, 1)))
        {
            tiles.Add(board[GetBoardPosition(v + new Vector2(-1, 1))]);
        }
        return tiles;
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

