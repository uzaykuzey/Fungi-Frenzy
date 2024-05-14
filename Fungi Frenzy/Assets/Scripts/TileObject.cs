using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileObject : MonoBehaviour
{
    public int occupiedBy; //0: no player, 1-4: players 1 is left top, goes clockwise
    public bool hasPlayerOn;
    public int powerUp; //0: no powerup, 1: food, 2: tile getter, 3: step donater, 4: step stealer
    [SerializeField] private SpriteRenderer food;
    public static readonly Color[] PlayerColors = new Color[] {Color.white, new Color(0.7133158f, 0.211819f, 0.990566f, 1), Color.red, Color.yellow, Color.cyan};
    public static readonly Color[] PowerUpColors = new Color[] { new Color(0,0,0,0), new Color(0.6226415f, 0.2161726f, 0.01566389f, 1), new Color(0.3f, 0.3f, 1, 1), Color.green, new Color(1, 0.5817609f, 0.9577771f, 1) };
    public bool moveable;
    public int boardPosition;
    public int signal;
    public float signalTime;
    public int signalEffecting;
    public bool previouslySignaled;
    [SerializeField] private GameControl gameControl;
    // Start is called before the first frame update
    void Start()
    {
        signalEffecting = 0;
        previouslySignaled = false;
        signal = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(signal!=0 && Time.time-signalTime>0.05)
        {
            List<TileObject> adjacents = gameControl.AdjacentTiles(boardPosition);
            foreach(TileObject tile in adjacents)
            {
                if(!tile.previouslySignaled)
                {
                    tile.signalEffecting = signalEffecting;
                    tile.signal = signal;
                    tile.signalTime = Time.time;
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
            GetComponent<SpriteRenderer>().color = signal == 1 ? PowerUpColors[3] : PowerUpColors[4];
        }
        else if (occupiedBy!=0&&gameControl.IsDead(occupiedBy-1))
        {
            this.GetComponent<SpriteRenderer>().color = Color.gray;
        }
        else
        {
            this.GetComponent<SpriteRenderer>().color = PlayerColors[occupiedBy];
        }
    }

    private void OnMouseDown()
    {
        if(gameControl.GameOver || gameControl.DiceRolling<=2)
        {
            return;
        }
        if(gameControl.StealingAndDonating!=0)
        {
            if(occupiedBy!=0&&occupiedBy-1!=gameControl.CurrentTurn%4&&!gameControl.IsDead(occupiedBy - 1))
            { 
                signalEffecting = occupiedBy;
                signal = gameControl.StealingAndDonating;
                gameControl.DonateAndSteal(occupiedBy - 1);
            }
        }
        else if(gameControl.OccupyAmount>0)
        {
            if(gameControl.CanBeOccupiedByPowerUp(boardPosition))
            {
                gameControl.OccupyLand(boardPosition);
            }
        }
        else if(moveable && !hasPlayerOn && gameControl.StepCount > 0)
        {
            gameControl.MovePlayer(boardPosition);
        }
    }
}
