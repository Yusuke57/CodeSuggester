using UnityEngine;

public class TestScript : MonoBehaviour
{
    public float speed = 100;

    void Update () 
    {
        if(GameObject.Find("MainGame").GetComponent<MainGame>().gameStart)
        {
            if(Input.GetButton("Vertical"))
            {
                transform.position += transform.forward * speed;
            }
        }
    }
}