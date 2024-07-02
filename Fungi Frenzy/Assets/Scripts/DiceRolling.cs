using UnityEngine;
using UnityEngine.SceneManagement;

public class DiceRolling : MonoBehaviour
{
    [SerializeField] private Rigidbody dice1Rb;
    [SerializeField] private Rigidbody dice2Rb;
    [SerializeField] private GameControl gameControl;
    [SerializeField] private Sprite roll;
    [SerializeField] private Sprite back;
    private MeshRenderer dice1Renderer;
    private MeshRenderer dice2Renderer;
    private SpriteRenderer rollButton;
    private Dice dice1;
    private Dice dice2;
    private float lastTime;
    private float lastFromRoll;

    // Start is called before the first frame update
    void Start()
    {
        dice1Renderer = dice1Rb.GetComponent<MeshRenderer>();
        dice2Renderer = dice2Rb.GetComponent<MeshRenderer>();
        dice1=dice1Rb.GetComponent<Dice>();
        dice2=dice2Rb.GetComponent<Dice>();
        rollButton = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space)) 
        {
            OnMouseDown();
        }


    }

    private void FixedUpdate()
    {
        if(gameControl.DiceRolling>=2)
        {
            dice1Renderer.enabled = false;
            dice2Renderer.enabled = false;
        }
        rollButton.color = (gameControl.GameOver) ? Color.white : TileObject.PlayerColors[gameControl.CurrentTurn % 4 + 1];
        //rollButton.sprite = gameControl.GameOver ? back: roll;
        if(gameControl.DiceRolling==1 && Time.time - lastFromRoll > 8)
        {
            lastFromRoll = Time.time;
            if (Mathf.Abs(dice1.transform.position.z + 0.6785231f) > 0.0001)
            {
                RollDice(dice1Rb);
            }
            if (Mathf.Abs(dice2.transform.position.z + 0.6785231f) > 0.0001)
            {
                RollDice(dice2Rb);
            }
        }
        if(gameControl.DiceRolling==1 && Mathf.Abs(dice1.transform.position.z+0.6785231f)<0.0001 && Mathf.Abs(dice2.transform.position.z + 0.6785231f) < 0.0001 && dice1Rb.velocity.magnitude<0.1 && dice2Rb.velocity.magnitude<0.1)
        {
            lastTime = Time.time;
            gameControl.DiceRolling = 2;
            gameControl.DiceToStepCount(dice1.GetValue() + dice2.GetValue());
        }
        if (gameControl.DiceRolling==2 && Time.time-lastTime>1.2)
        {
            gameControl.DiceRolling=3;

        }
        if (GameControl.multiplayer && gameControl.DiceRolling < 3 && GameControl.ThisMultiplayer != null)
        {
            if (GameControl.ThisMultiplayer.IsHost)
            {
                Multiplayer.dice1Position.Value = dice1.transform.position;
                Multiplayer.dice1Rotation.Value = dice1.transform.rotation;
                Multiplayer.dice2Position.Value = dice2.transform.position;
                Multiplayer.dice2Rotation.Value = dice2.transform.rotation;
            }
            else
            {
                dice1.transform.position = Multiplayer.dice1Position.Value;
                dice1.transform.rotation = Multiplayer.dice1Rotation.Value;

                dice2.transform.position = Multiplayer.dice2Position.Value;
                dice2.transform.rotation = Multiplayer.dice2Rotation.Value;
            }
        }
    }

    private void OnMouseDown()
    {
        if(GameControl.multiplayer) 
        {
            GameControl.ThisMultiplayer.RollClickedServerRpc(GameControl.ThisMultiplayer.OwnerClientId);
        }
        else
        {
            Clicked();
        }
    }

    public void Clicked()
    {
        if (gameControl.DiceRolling != 0)
        {
            return;
        }
        /*if (gameControl.GameOver)
        {
            SceneManager.LoadScene("Start Menu");
            return;
        }*/
        dice1.transform.position = new Vector3(-5, 0, -3.1f);
        dice2.transform.position = new Vector3(5, 0, -3.1f);
        dice1Renderer.enabled = true;
        dice2Renderer.enabled = true;
        RollDice(dice1Rb);
        RollDice(dice2Rb);
        gameControl.DiceRolling = 1;
        lastFromRoll = Time.time;
    }

    private void RollDice(Rigidbody rb)
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.transform.rotation = Random.rotation;

        float randomTorque = 3f;
        Vector3 torque = new(
            Random.Range(-randomTorque, randomTorque),
            Random.Range(-randomTorque, randomTorque),
            Random.Range(-randomTorque, randomTorque)
        );

        float randomForce = 10f;
        Vector3 force = new(
            Random.Range(-randomForce, randomForce),
            Random.Range(-randomForce, randomForce),
            Random.Range(-randomForce/5, randomForce/5)
        );

        rb.AddTorque(torque, ForceMode.Impulse);
        rb.AddForce(force, ForceMode.Impulse);
    }

}
