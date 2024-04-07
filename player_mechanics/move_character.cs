using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



//This code move character on the level
public class move_character : MonoBehaviour
{
    //Speed of the character
    public float speed=20f;
    //Reference to the class FindShortestWay
    public FindShortestWay mainWays;
    //Copy of the reference
    private FindShortestWay copyDijkstra;
    //Bool to check is mouse on the wall or not
    static public bool IsMouseOnWall;
    //canMoveByMouse - That checks if player can move character by mouse
    //move - If character should move or not
    //initialPoint - says is it first point in the list or not
    private bool canMoveByMouse=false, move=false, initialPoint=false;
    //Array of all points on the level
    private IntersectionPoint[] pointsArray;
    //currentPoint - current points to which character should move
    //iter - store which point in the List should be used
    private int currentPoint=-1, iter=-1;
    //List of points which should be passed by the character to 
    //reach the destination
    List<int> pointsToPass;
    //rigidBody of the character
    public Rigidbody2D rigidBody;
    //GameObject of the character
    public GameObject transformP;
    //Direction at which character should be moved
    private string direction;



    //Get rigidbody of the character
    void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
    }



    // Start is called before the first frame update
    void Start()
    {
        copyDijkstra=mainWays;
        pointsToPass = new List<int>();
    }

    // Update is called once per frame
    void Update()
    {
        //Check is left side of the mouse is pressed
        if (Input.GetMouseButtonDown(0) && copyDijkstra.distancesCalculated && canMoveByMouse)
        {
            pointsArray=copyDijkstra.returnArray();

            int pt1, pt2;

            //Get coords of the mouse
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //Find nearest point to the mouse
            pt1=copyDijkstra.findNearestPoint(mousePosition);

            //Get coords of the character
            Vector3 bodyPos = transform.localPosition;
            //Find nearest point to the character
            pt2=copyDijkstra.findNearestPoint(bodyPos);

            /*Debug.Log("Mouse: "+pt1);
            Debug.Log("Character: "+pt2);*/

            //Check is mouse on the wall or not
            if(IsMouseOnWall)
            {
                Debug.Log("Your mouse on the wall!");
            }
            else
            {
                //Find path of the character
                pointsToPass = copyDijkstra.dijkstra_alghorithm(pt2, pt1);
                //Change all variables for movement
                move=true;
                initialPoint=true;
                iter=-1;
                currentPoint=-1;
            }

            /*if(pointsToPass.Count!=0)
            {
                string stringOfPt="";
                foreach(var pt in pointsToPass)
                {
                    stringOfPt=stringOfPt+", "+pt;
                }
                Debug.Log(stringOfPt);
            }
            else
            {
                Debug.Log("Nothing");
            }*/
        }
    }



    void FixedUpdate()
    {
        //Check if move is true
        if(move)
        {
            //If current point undefined set to the first point in the List
            if(currentPoint==-1)
                currentPoint=pointsToPass[0];

            //Get position of the character
            Vector3 pos=transformP.transform.position;
            //Find distance between character and the point
            double distance=Math.Sqrt(Math.Pow(pos.y-pointsArray[currentPoint].y,2)+Math.Pow(pos.x-pointsArray[currentPoint].x,2));
            
            //Check if character at the point and not initial point
            if(distance>0.1 && !initialPoint)
            {
                //Get velocity of the character
                Vector2 direct = rigidBody.velocity;

                //Debug.Log(""+direction);
                //Switch direction and move the character
                switch(direction)
                {
                    case "left":
                        direct.x=-1*speed*Time.deltaTime;
                        rigidBody.velocity=direct;
                        break;
                    case "right":
                        direct.x=1*speed*Time.deltaTime;
                        rigidBody.velocity=direct;
                        break;
                    case "up":
                        direct.y=1*speed*Time.deltaTime;
                        rigidBody.velocity=direct;
                        break;
                    case "down":
                        direct.y=-1*speed*Time.deltaTime;
                        rigidBody.velocity=direct;
                        break;
                }
            }
            else
            {
                //Increase it for the next point
                iter++;

                //Set velocity to 0
                rigidBody.velocity=Vector2.zero;

                //Check was it the last point or not
                if(iter!=pointsToPass.Count)
                {
                    //Set new current point
                    currentPoint=pointsToPass[iter];
                    int previous;
                    //They needed to find at which direction character shoul be moved
                    int hor, vert;

                    //Check if it is the first point
                    if(initialPoint)
                    {
                        //Get another point as coord of the character
                        vert = (int)(pointsArray[currentPoint].y-pos.y);
                        hor = (int)(pointsArray[currentPoint].x-pos.x);
                    }
                    else
                    {
                        previous=pointsToPass[iter-1];

                        //Get another coords as previous point
                        vert = (int)(pointsArray[currentPoint].y-pointsArray[previous].y);
                        hor = (int)(pointsArray[currentPoint].x-pointsArray[previous].x);
                    }

                    /*Debug.Log("Vert: "+vert);
                    Debug.Log("Hor: "+hor);*/

                    //Set direction
                    if(vert<0 && Math.Abs(vert)>Math.Abs(hor))
                    {
                        direction="down";
                    }
                    else if(vert>0 && vert>Math.Abs(hor))
                    {
                        direction="up";
                    }
                    else if(hor>0)
                    {
                        direction="right";
                    }
                    else
                    {
                        direction="left";
                    }
                }
                else
                {
                    //Stop movement
                    move=false;
                    iter=-1;
                }

                initialPoint=false;
            }
        }
    }



    //This checkbox control can be character moved by mouse or not
    public void moveCharacterByMouse()
    {
        canMoveByMouse=canMoveByMouse?false:true;
    }
}
