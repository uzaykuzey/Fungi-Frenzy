using UnityEngine;

public class Player : MonoBehaviour
{
    
    [SerializeField] private Sprite secret;
    [SerializeField] private GameControl gameControl;
    private static Sprite[] useSprites;
    private static readonly KeyCode[] code = new KeyCode[] { KeyCode.UpArrow, KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.B, KeyCode.A };
    private static int secretProgress;
    public int playerNo;
    private static bool canProgress;



    // Start is called before the first frame update
    void Start()
    {
        canProgress = true;
        useSprites =new Sprite[4];
        for(int i = 0; i < useSprites.Length; i++)
        {
            useSprites[i] = gameControl.GetSprite(i);
        }
        secretProgress = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(secretProgress==-1)
        {
            return;
        }
        if (canProgress&&Input.GetKeyDown(code[secretProgress]))
        {
            secretProgress++;
            canProgress = false;
        }
        else if(secretProgress!=0 && canProgress && Input.anyKeyDown)
        {
            secretProgress = 0;
        }
        if(!canProgress&&!Input.anyKeyDown)
        {
            canProgress = true;
        }
        if(secretProgress==code.Length)
        {
            useSprites[gameControl.CurrentTurn%4] = secret;
            secretProgress = -1;
        }
    }

    private void FixedUpdate()
    {
       GetComponent<SpriteRenderer>().sprite = useSprites[playerNo];
    }
}
