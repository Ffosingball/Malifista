using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player_movement : MonoBehaviour
{
    //Speed of movement
    public float speed=10f;
    //For the camera
    public GameObject _camera_;
    //Rigidbody of the player
    public Rigidbody2D rb;


    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        //This code move player if he press buttons
        if(Input.GetKey(KeyCode.A))//Move left
        {
            Vector2 direction = rb.velocity;//Get current velocity of the player
            direction.x=-1*speed*Time.deltaTime;//Change only movement by x 
            rb.velocity=direction;//Set new velocity
        }
        else if(Input.GetKey(KeyCode.D))//Move right
        {
            Vector2 direction = rb.velocity;
            direction.x=1*speed*Time.deltaTime;
            rb.velocity=direction;
        }
        else//If player do not pres any button there must not be any movement
        {
            Vector2 direction = rb.velocity;
            direction.x=0;
            rb.velocity=direction;
        }

        if(Input.GetKey(KeyCode.W))//Move upwards
        {
            Vector2 direction = rb.velocity;
            direction.y=1*speed*Time.deltaTime;
            rb.velocity=direction;
        }
        else if(Input.GetKey(KeyCode.S))//Move downwards
        {
            Vector2 direction = rb.velocity;
            direction.y=-1*speed*Time.deltaTime;
            rb.velocity=direction;
        }
        else
        {
            Vector2 direction = rb.velocity;
            direction.y=0;
            rb.velocity=direction;
        }
    }


    void Update()
    {
        //Change position of the camera to the position of the player
        //So camera always will follow the player
        Vector3 curPos=transform.position;
        curPos.z=-2f;
        _camera_.transform.position=curPos;
    }
}
