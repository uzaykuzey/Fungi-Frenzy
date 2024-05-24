using UnityEngine;

public class Dice : MonoBehaviour
{
    private Rigidbody dice;
    [SerializeField] private Transform[] sides;
    // Start is called before the first frame update
    void Start()
    {
        dice = GetComponent<Rigidbody>();
    }

    public int GetValue()
    {
        int minIndex = 0;
        for(int i = 1; i < sides.Length; i++)
        {
            if (sides[i].position.z < sides[minIndex].position.z)
            {
                minIndex = i;
            }
        }
        return sides[minIndex].name[0]-48;
    }

    private void OnCollisionStay(Collision collision)
    {
        string name = collision.gameObject.name;
        switch (name.ToLower())
        {
            case "left":
                dice.velocity += new Vector3(1, 0, 0);
                break;
            case "right":
                dice.velocity += new Vector3(-1, 0, 0);
                break;
            case "top":
                dice.velocity += new Vector3(0, -1, 0);
                break;
            case "bottom":
                dice.velocity += new Vector3(0, 1, 0);
                break;
        }

        if (name.ToLower().Contains("dice"))
        {
            dice.velocity += new Vector3(0, 0, (collision.transform.position.y < transform.position.y) ? 1 : -1);
        }
    }
}
