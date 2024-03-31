using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//This code move character on the level
public class move_character : MonoBehaviour
{
    //Speed of the character
    public float speed=20f;
    //Reference to the class FindShortestWay
    public FindShortestWay mainWays;
    //Reference to the main camera
    public Camera _camera;
    //Array of the points between which character should move
    private Vector2[] pointsToMove;
    //Copy of the reference
    private FindShortestWay copyDijkstra;
    //Bool to check is mouse on the wall or not
    static public bool IsMouseOnWall;
    //private bool mousePressed=false;

    // Start is called before the first frame update
    void Start()
    {
        copyDijkstra=mainWays;
    }

    // Update is called once per frame
    void Update()
    {
        //Check is left side of the mouse is pressed
        if (Input.GetMouseButtonDown(0) && copyDijkstra.distancesCalculated)
        {
            int pt1, pt2;

            //Get coords of the mouse
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //Find nearest point
            pt1=copyDijkstra.findNearestPoint(mousePosition);

            //Get coords of the character
            Vector3 bodyPos = transform.localPosition;
            //Find nearest point
            pt2=copyDijkstra.findNearestPoint(bodyPos);

            Debug.Log("Mouse: "+pt1);
            Debug.Log("Character: "+pt2);

            //Check is mouse on the wall or not
            if(IsMouseOnWall)
            {
                Debug.Log("Wall");
            }
            else
            {
                Debug.Log("Corridor");
            }
        }
    }
}
