using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;//It need to change text in UI object TextMeshPro



//Class which contain x-coord, y-coord and id of the point 
class IntersectionPoint
{
    private float x, y;
    private int id;


    //Constructor of the class
    public IntersectionPoint(float x, float y, int id)
    {
        this.x=x;
        this.y=y;
        this.id=id;
    }


    public float X
    {
        get
        {
            return x; // Return this field
        }
    }


    public float Y
    {
        get
        {
            return y; // Return this field
        }
    }


    public int ID
    {
        get
        {
            return id; // Return this field
        }
    }
}



//Point is a center of a corridor section
public class FindShortestWay : MonoBehaviour
{
    //Array of all points
    private IntersectionPoint[] pointsArray;
    //Array of distances between from all points to all points
    private float[,] distanceBetPoints;
    //Store id of the new point and number of points
    private int id=0, numberOfPoints;
    //Some boolean variables to check if some procedures done
    private bool arrInitialized=false, showPoints=false, showLines=false;
    //[HideInInspector] public, this makes this variable visible in
    //other classes but hide it in the unity
    [HideInInspector] public bool distancesCalculated=false;
    //Reference to the layer at which all walls are, so raycast can work correctly
    public LayerMask wallLayer;
    //Objects which will be used to show points and routes between them
    public GameObject line, point;
    //Array of all objects which will be shown on the map
    //Only if turn on some toddlers
    private GameObject[] pointsShown, linesShown;


    //Procedure which will initialize all arrays
    public void InitializeArrays(int n)
    {
        if(arrInitialized==false)
        {
            pointsArray=new IntersectionPoint[n];
            distanceBetPoints=new float[n,n];
            pointsShown=new GameObject[n];
            numberOfPoints=n;

            arrInitialized=true;
        }
        else
        {
            Debug.Log("Arrays already initialized!");
        }
    }


    //Add new point to the array until there are available space
    public void addCoords(float x, float y)
    {
        if(id<numberOfPoints && arrInitialized)
        {
            IntersectionPoint p = new IntersectionPoint(x,y,id);

            pointsArray[id]=p;

            id++;
            //Debug.Log("Added point!");
        }
        else
        {
            Debug.Log("Imposible to add more points!");
        }
    }


    //This procedure will find distances between all points
    //If draw a straight line between two points, and any wall
    //will intersect with this line, this route cannot exists
    //so distance will be 99999
    public void findDistancesBetPoints()
    {
        int l=0;//number of all posible routes

        for(int i=0; i<numberOfPoints; i++)
        {
            for(int j=i; j<numberOfPoints; j++)
            {
                if(j==i)
                {
                    //Distance from point to the same point is 0, so
                    distanceBetPoints[i,j]=99999;
                }
                else
                {
                    //Add coords of two points to vectors
                    Vector2 point1, point2;
                    point1.x=pointsArray[i].X;
                    point1.y=pointsArray[i].Y;
                    point2.x=pointsArray[j].X;
                    point2.y=pointsArray[j].Y;
                    //Debug.Log("Passed");

                    //Calculate distance between them
                    double distance=Math.Sqrt(Math.Pow(point1.y-point2.y,2)+Math.Pow(point1.x-point2.x,2));
                    Vector2 direction = (point2 - point1).normalized;

                    //Use raycastHit2D to check is there any wall intersect with this line
                    RaycastHit2D hit = Physics2D.Raycast(point1, direction, (float)distance, wallLayer);

                    //If line do not intersect add its real distance
                    //Also checks that line is not diagonal
                    if((pointsArray[i].X==pointsArray[j].X || pointsArray[j].Y==pointsArray[i].Y) && hit.collider==null)
                    {
                        distanceBetPoints[j,i]=(float)distance;
                        distanceBetPoints[i,j]=(float)distance;
                        l++;
                    }
                    else
                    {
                        //otherwise route does not exist between 2 points
                        distanceBetPoints[i,j]=99999;
                        distanceBetPoints[j,i]=99999;
                    }
                }
            }
        }

        distancesCalculated=true;
        linesShown = new GameObject[l];//Initialize array
    }



    //This procedure will show all points
    //or hide them, if toddler will be switched on or off
    public void showAllPoints()
    {
        if(showPoints)
        {
            for(int i=0; i<numberOfPoints; i++)
            {
                //Destroy objects
                Destroy(pointsShown[i]);
            }

            showPoints=false;
        }
        else
        {
            //Check if points was added to the array
            if(arrInitialized)
            {
                for(int i=0; i<numberOfPoints; i++)
                {
                    //Create points on the level
                    pointsShown[i] = Instantiate(point, new Vector3(pointsArray[i].X, pointsArray[i].Y, -1f), Quaternion.Euler(0f,0f,0f));
                    
                    //Find child gameObject of the created gameObject by its name
                    GameObject childCanva = pointsShown[i].transform.Find("Canvas").gameObject;
                    //And again
                    GameObject textNeeded = childCanva.transform.Find("Text").gameObject;
                    
                    //Get this component from found gameObject
                    TextMeshProUGUI textComponent = textNeeded.GetComponent<TextMeshProUGUI>();
                    //Add ID of the point above it
                    textComponent.text=""+i;
                }
            }
            else
            {
                Debug.Log("No point has been added yet!");
            }

            showPoints=true;
        }
    }



    //This procedure will show all possible routes between points
    //or hide them, if toddler will be switched on or off
    public void showAllRoutes()
    {
        if(showLines)
        {
            for(int i=0; i<linesShown.Length; i++)
            {
                //Destroy objects
                Destroy(linesShown[i]);
            }

            showLines=false;
        }
        else
        {
            int c=0;

            //Check that all distances are calculated
            if(distancesCalculated)
            {
                for(int i=0; i<numberOfPoints; i++)
                {
                    for(int j=i; j<numberOfPoints; j++)
                    {
                        //Check if route exist between these 2 points
                        if(distanceBetPoints[i,j]!=99999)
                        {
                            //Create vectors for calculations
                            Vector2 point1,point2;
                            point1.x=pointsArray[i].X;
                            point1.y=pointsArray[i].Y;
                            point2.x=pointsArray[j].X;
                            point2.y=pointsArray[j].Y;

                            //Calculate angle of the line
                            Vector2 direction = point1 - point2;
                            float angle = (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg)-90;
                            angle=angle<-180?angle+360:angle;

                            //Create line at the particular coordinates, rotate it and change its scale
                            linesShown[c] = Instantiate(line, new Vector3((point1.x+point2.x)/2, (point1.y+point2.y)/2, -1f), Quaternion.Euler(0f,0f,angle));
                            linesShown[c].transform.localScale=new Vector3(1, distanceBetPoints[i,j], 1);

                            c++;
                        }
                    }
                }
                
                showLines=true;
            }
            else
            {
                Debug.Log("Distances between points have not been calculated yet!");
            }
        }
    }



    //This function takes coordinates in the level
    //and find the nearest point to this coords
    public int findNearestPoint(Vector3 point)
    {
        double distance, min=999999;
        int id=-1;

        for(int i=0; i<pointsArray.Length; i++)
        {
            //Calculate distance between point and coords
            distance=Math.Sqrt(Math.Pow(point.x-pointsArray[i].X,2)+Math.Pow(point.y-pointsArray[i].Y,2));
            //Check is it bigger or smaller
            if(distance<min)
            {
                min=distance;
                id=i;
            }
        }

        //Return ID of the nearest point
        return id;
    }
}