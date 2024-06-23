using System.Collections.Generic;
using UnityEngine;

public class TileObject : MonoBehaviour
{
    public int occupiedBy; //0: no player, 1-4: players 1 is left top, goes clockwise
    public bool hasPlayerOn;
    public int powerUp; //0: no powerup, 1: food, 2: tile getter, 3: step donater, 4: step stealer
    [SerializeField] private SpriteRenderer food;
    public static readonly Color[] PlayerColors = new Color[] {Color.white, new Color(0.7133158f, 0.211819f, 0.990566f, 1), Color.red, Color.yellow, Color.cyan};
    public static readonly Color[] PowerUpColors = new Color[] { new Color(0,0,0,0), new Color(0.6226415f, 0.2161726f, 0.01566389f, 1), new Color(0.3f, 0.3f, 1, 1), Color.green, new Color(1, 0.5817609f, 0.9577771f, 1) , Color.white, Color.black};
    public int boardPosition;
    public int signal;
    public float signalTime;
    public int signalEffecting;
    public float signalHue;
    public bool previouslySignaled;
    public float lastClicked;
    [SerializeField] private GameControl gameControl;
    private static float hue;
    private static float alphaPo;
    public static float AlphaPl { set; get; }
    private static float changePo;
    private static float changePl;
    private SpriteRenderer tileRenderer;
    private static Color diagonalPowerColor;
    private static float lastTimeFromSuperPowered;

    // Start is called before the first frame update
    void Start()
    {
        changePo = 0.3f;
        changePl = 0.1f;
        AlphaPl = 1;
        signalEffecting = 0;
        alphaPo = 0.8f;
        previouslySignaled = false;
        signal = 0;
        hue = 0;
        tileRenderer=GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (boardPosition == 1)
        {
            hue += Time.deltaTime * 0.2f;
            if (hue > 1f)
            {
                hue -= 1f;
            }
            PowerUpColors[5] = Color.HSVToRGB(hue, 1f, 1f);

            alphaPo += Time.deltaTime * changePo;
            AlphaPl += Time.deltaTime * changePl;
            if(alphaPo>1)
            {
                changePo = -0.5f;
            }
            else if(alphaPo<0)
            {
                changePo = 0.5f;
            }

            if(AlphaPl>1)
            {
                changePl = -0.2f;
            }
            else if(AlphaPl<0)
            {
                changePl = 0.2f;
            }


            float a = Mathf.Sin(alphaPo * Mathf.PI);
            PowerUpColors[6] = new Color(0,0,0,a*a);
            if(gameControl.SuperPowered==2)
            {
                Color c = PlayerColors[gameControl.CurrentTurn % 4 + 1];
                a = Mathf.Sin(AlphaPl * Mathf.PI);
                diagonalPowerColor = new Color(c.r,c.g,c.b,a*a*0.66666667f + 0.33333333f);
            }
        }

        if(hasPlayerOn && gameControl.CurrentTurn%4+1==occupiedBy && gameControl.SuperPowered == 1 && Time.time - lastTimeFromSuperPowered > 2.75f)
        {
            lastTimeFromSuperPowered = Time.time;
            signalEffecting = gameControl.CurrentTurn % 4 + 1;
            signal = 2;
            Color.RGBToHSV(PlayerColors[gameControl.CurrentTurn%4+1], out signalHue, out _, out _);
            gameControl.ClearSignals();
        }

        if (signal!=0 && Time.time-signalTime>0.05)
        {
            List<TileObject> adjacents = gameControl.AdjacentTiles(boardPosition);
            foreach(TileObject tile in adjacents)
            {
                if(!tile.previouslySignaled && (signal!=2 || tile.occupiedBy==signalEffecting))
                {
                    tile.signalEffecting = signalEffecting;
                    tile.signal = signal;
                    tile.signalTime = Time.time;
                    tile.signalHue = signalHue + 0.1f;
                    if(tile.signalHue>1)
                    {
                        tile.signalHue -= 1f;
                    }
                }
            }
            previouslySignaled = true;
            signal = 0;
        
        }
    }

    private void FixedUpdate()
    {
        if(food.enabled==false && powerUp!=0)
        {
            food.color = Color.white;
            food.enabled= true;
        }
        if(food.enabled==true && powerUp==0)
        {
            food.enabled = false;
        }

        if(food.enabled==true)
        {
            food.color = PowerUpColors[powerUp];
        }

        if(signal!=0 && signalEffecting==occupiedBy)
        {
            GetComponent<SpriteRenderer>().color = signal == 1 ? PowerUpColors[3] : signal==-1 ? PowerUpColors[4]: Color.HSVToRGB(signalHue,1,1);
        }
        else if (occupiedBy!=0&&GameControl.DeadPlayerList[occupiedBy-1])
        {
            tileRenderer.color = Color.gray;
        }
        else if(gameControl.SuperPowered==2 && occupiedBy -1==gameControl.CurrentTurn%4)
        {
            tileRenderer.color = diagonalPowerColor;
        }
        else
        {
            tileRenderer.color = PlayerColors[occupiedBy];
        }
    }

    private void OnMouseDown()
    {
        if(GameControl.multiplayer)
        {
            GameControl.ThisMultiplayer.TileClickedServerRpc(boardPosition);
        }
        else
        {
            Click();
        }
    }

    public void Click()
    {
        if (gameControl.GameOver || gameControl.DiceRolling <= 2)
        {
            return;
        }
        if (gameControl.StealingAndDonating != 0)
        {
            if (occupiedBy != 0 && occupiedBy - 1 != gameControl.CurrentTurn % 4 && !GameControl.DeadPlayerList[occupiedBy - 1])
            {
                signalEffecting = occupiedBy;
                signal = gameControl.StealingAndDonating;
                gameControl.DonateAndSteal(occupiedBy - 1);
            }
        }
        else if (gameControl.OccupyAmount > 0)
        {
            if (gameControl.CanBeOccupiedByPowerUp(boardPosition))
            {
                gameControl.OccupyLand(boardPosition);
            }
        }
        else if (gameControl.CanMove(boardPosition) && !hasPlayerOn && gameControl.StepCount > 0)
        {
            gameControl.MovePlayer(boardPosition);
        }
        else if (Time.time - lastClicked < 0.2)
        {
            gameControl.AutoMove(boardPosition);
        }
        lastClicked = Time.time;
    }
}
