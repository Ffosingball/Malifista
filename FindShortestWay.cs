using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



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
    private bool arrInitialized=false, distancesCalculated=false, showPoints=false, showLines=false;
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


    //This function will find distances between all points
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
                    if(hit.collider==null)
                    {
                        distanceBetPoints[j,i]=(float)distance;
                        distanceBetPoints[i,j]=(float)distance;
                        l++;
                    }
                    else
                    {
                        //otherwise
                        distanceBetPoints[i,j]=99999;
                        distanceBetPoints[j,i]=99999;
                    }
                }
            }
        }

        distancesCalculated=true;
        linesShown = new GameObject[l];//Initialize array
    }



    //This function will show all points
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
            if(arrInitialized)
            {
                for(int i=0; i<numberOfPoints; i++)
                {
                    pointsShown[i] = Instantiate(point, new Vector3(pointsArray[i].X, pointsArray[i].Y, -1f), Quaternion.Euler(0f,0f,0f));
                }
            }
            else
            {
                Debug.Log("No point has been added yet!");
            }

            showPoints=true;
        }
    }



    public void showAllRoutes()
    {
        if(showLines)
        {
            for(int i=0; i<linesShown.Length; i++)
            {
                Destroy(linesShown[i]);

            }

            showLines=false;
        }
        else
        {
            int c=0;

            if(distancesCalculated)
            {
                for(int i=0; i<numberOfPoints; i++)
                {
                    for(int j=i; j<numberOfPoints; j++)
                    {
                        if(distanceBetPoints[i,j]!=99999)
                        {
                            Vector2 point1,point2;
                            point1.x=pointsArray[i].X;
                            point1.y=pointsArray[i].Y;
                            point2.x=pointsArray[j].X;
                            point2.y=pointsArray[j].Y;

                            Vector2 direction = point1 - point2;
                            float angle = (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg)-90;
                            angle=angle<-180?angle+360:angle;

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
}