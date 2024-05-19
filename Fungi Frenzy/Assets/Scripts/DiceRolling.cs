using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceRolling : MonoBehaviour
{
    [SerializeField] private Rigidbody dice1Rb;
    [SerializeField] private Rigidbody dice2Rb;
    [SerializeField] private GameControl gameControl;
    private MeshRenderer dice1Renderer;
    private MeshRenderer dice2Renderer;
    private SpriteRenderer rollButton;
    private Dice dice1;
    private Dice dice2;
    private float lastTime;
    private bool shiftProgression;
    private bool shifted;
    private float lastFromRoll;

    // Start is called before the first frame update
    void Start()
    {
        shiftProgression = false;
        shifted = false;
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
        if(!shiftProgression && Input.GetKeyDown(KeyCode.LeftShift))
        {
            shiftProgression = true;
            shifted = !shifted;
        }
        if(shiftProgression && !Input.GetKeyDown(KeyCode.LeftShift))
        {
            shiftProgression = false;
        }
    }

    private void FixedUpdate()
    {
        if(gameControl.DiceRolling==1 && Time.time - lastFromRoll > 6)
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
            if(!shifted)
            {
                gameControl.DiceRolling = 2;
                gameControl.DiceToStepCount(dice1.GetValue() + dice2.GetValue());
            }
            else
            {
                gameControl.DiceRolling = 0;
            }
        }
        if (gameControl.DiceRolling==2 && Time.time-lastTime>1.2)
        {
            gameControl.DiceRolling=3;
            dice1Renderer.enabled = false;
            dice2Renderer.enabled=false;
        }
        rollButton.color= shifted? Color.white:TileObject.PlayerColors[gameControl.CurrentTurn % 4 + 1];
    }

    private void OnMouseDown()
    {
        if(gameControl.DiceRolling!=0 || gameControl.GameOver || gameControl.LeaderboardActive)
        {
            return;
        }
        dice1.transform.position = new Vector3(-5, 0, -3.1f);
        dice2.transform.position = new Vector3(5, 0, -3.1f);
        dice1Renderer.enabled=true;
        dice2Renderer.enabled=true;
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
