using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player_movement : MonoBehaviour
{
    //Speed of movement
    public float speed=10f, scrollSpeed = 0.2f, changeInSpeed=1f;
    //Whole mainCamera
    public GameObject _camera_;
    //Camera of the mainCamera
    public Camera camera_;
    //Rigidbody of the player
    public Rigidbody2D rb;
    //Maximal and minimal boundaries of the resizement of the mainCamera
    public float maxResizeCamera=100f, minResizeCamera=5f;
    //First one is variable which increase or decrease speed of the movement
    //Of the camera depending on its Orthographic Size
    //So when player will resize the camera it will feel naturally speed
    //Coeficient of the width to the height 
    private float speedChangeOfSizeOfCamera, kOfborderForCamera;
    //Minimal speed at the minimal resize of the camera
    //Maximal speed at the maximal resize of the camera
    public float minSpeedChange=4f, maxSpeedChange=120f;


    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        //Set it to minimal, as size of the camera also minimal
        speedChangeOfSizeOfCamera=minSpeedChange;

        //Calculate the coeficient
        Resolution screenSize = Screen.currentResolution;
        kOfborderForCamera=screenSize.width/screenSize.height;
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        //This code move player if he press buttons
        if(Input.GetKey(KeyCode.A))//Move left
        {
            //Check so player will not fly outside the level
            if((camera_.orthographicSize*(-1)*kOfborderForCamera)+_camera_.transform.position.x>=(-1)*((generate_map.widthS/2)+1)*(generate_map.widthMainS+generate_map.widthCornerS))
                _camera_.transform.Translate(new Vector3(-1,0,0)*speed*Time.deltaTime*speedChangeOfSizeOfCamera);
        }
        else if(Input.GetKey(KeyCode.D))//Move right
        {
            //Check so player will not fly outside the level
            if((camera_.orthographicSize*kOfborderForCamera)+_camera_.transform.position.x<=(generate_map.widthS/2)*(generate_map.widthMainS+generate_map.widthCornerS))
                _camera_.transform.Translate(new Vector3(1,0,0)*speed*Time.deltaTime * speedChangeOfSizeOfCamera);
        }

        if(Input.GetKey(KeyCode.W))//Move upwards
        {
            //Check so player will not fly outside the level
            if(camera_.orthographicSize+_camera_.transform.position.y<=(generate_map.heightS/2)*(generate_map.widthMainS+generate_map.widthCornerS))
                _camera_.transform.Translate(new Vector3(0,1,0)*speed*Time.deltaTime * speedChangeOfSizeOfCamera);
        }
        else if(Input.GetKey(KeyCode.S))//Move downwards
        {
            //Check so player will not fly outside the level
            if((camera_.orthographicSize*(-1))+_camera_.transform.position.y>=(-1)*((generate_map.heightS/2)+1)*(generate_map.widthMainS+generate_map.widthCornerS))
                _camera_.transform.Translate(new Vector3(0,-1,0)*speed*Time.deltaTime * speedChangeOfSizeOfCamera);
        }
    }


    void Update()
    {
        //Get number 1, -1 or 0 which tell was mouse scrollwheel
        //used by player or not and tell at which direction
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        //Get current size of the camera
        float resizeO = camera_.orthographicSize;

        //Check is size of the camera reaches limits or not
        resizeO=resizeO+(scrollInput*scrollSpeed);

        //Check is size of the camera reaches limits or not
        //If it is out of limits it will set maximal or minimal allowed size of the camera
        if(resizeO<minResizeCamera)
        {
            resizeO=minResizeCamera;
            speedChangeOfSizeOfCamera = minSpeedChange;
        }
        else if(resizeO>maxResizeCamera)
        {
            resizeO=maxResizeCamera;
            speedChangeOfSizeOfCamera = maxSpeedChange;
        }
        else
        {
            speedChangeOfSizeOfCamera = speedChangeOfSizeOfCamera + (scrollInput * changeInSpeed);
        }

        //Change size of the camera
        camera_.orthographicSize=resizeO;
    }
}
