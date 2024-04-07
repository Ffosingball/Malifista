using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;//It need to change text in UI object TextMeshPro



//Structure which contain x-coord, y-coord and id of the point 
public struct IntersectionPoint
{
    public float x;
    public float y;
    public int id;
}



//Point is a center of a corridor section
public class FindShortestWay : MonoBehaviour
{
    //Array of all points
    public IntersectionPoint[] pointsArray;
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



    //This function return pointsArray when called
    public IntersectionPoint[] returnArray()
    {
        return pointsArray;
    }



    //Add new point to the array until there are available space
    public void addCoords(float x, float y)
    {
        if(id<numberOfPoints && arrInitialized)
        {
            pointsArray[id].id=id;
            pointsArray[id].x=x;
            pointsArray[id].y=y;

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
                    point1.x=pointsArray[i].x;
                    point1.y=pointsArray[i].y;
                    point2.x=pointsArray[j].x;
                    point2.y=pointsArray[j].y;
                    //Debug.Log("Passed");

                    //Calculate distance between them
                    double distance=Math.Sqrt(Math.Pow(point1.y-point2.y,2)+Math.Pow(point1.x-point2.x,2));
                    Vector2 direction = (point2 - point1).normalized;

                    //Use raycastHit2D to check is there any wall intersect with this line
                    RaycastHit2D hit = Physics2D.Raycast(point1, direction, (float)distance, wallLayer);

                    //If line do not intersect add its real distance
                    //Also checks that line is not diagonal
                    if((pointsArray[i].x==pointsArray[j].x || pointsArray[j].y==pointsArray[i].y) && hit.collider==null)
                    {
                        distanceBetPoints[j,i]=(float)distance;
                        distanceBetPoints[i,j]=(float)distance;
                        l++;

                        //Debug.Log(i+"; "+j+"; "+Math.Round(distance,2));
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
        //Debug.Log("Num of lines "+l);
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
                    pointsShown[i] = Instantiate(point, new Vector3(pointsArray[i].x, pointsArray[i].y, -1f), Quaternion.Euler(0f,0f,0f));
                    
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
                            point1.x=pointsArray[i].x;
                            point1.y=pointsArray[i].y;
                            point2.x=pointsArray[j].x;
                            point2.y=pointsArray[j].y;

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
            distance=Math.Sqrt(Math.Pow(point.x-pointsArray[i].x,2)+Math.Pow(point.y-pointsArray[i].y,2));
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




    //This function use Dijkstra alghorithm to find the shortest
    //Way between any points and return List of points which
    //should be passed to came from pt1 (initial point) 
    //to the pt2 (destination point)
    public List<int> dijkstra_alghorithm(int pt1, int pt2)
    {
        //Create List of the List of the integers
        //Each List in the List corresponding to each point
        //And contain path from pt1 to its point
        List<List<int>> pointsToPass = new List<List<int>>();
        for(int i=0; i<pointsArray.Length; i++)
        {
            //Initialize all Lists
            pointsToPass.Add(new List<int>());
        }
        
        //Array which contain smallest total distance from pt1
        //To the current point
        float[] totalDistance = new float[pointsArray.Length];
        for(int i=0; i<totalDistance.Length; i++)
        {
            //Set maximum distance to each point
            totalDistance[i] = 99999;
        }
        
        //This needed for loop to start find the path
        totalDistance[pt1]=0;

        //This loop will continue until the path to the
        //destination point will not be found
        while(totalDistance[pt2]==99999)
        {
            for(int i=0; i<pointsArray.Length; i++)
            {
                //This needed so it will start find the path from the initial point
                int ti=pt1+i>=pointsArray.Length?pt1+i-pointsArray.Length:pt1+i;

                for(int j=0; j<pointsArray.Length; j++)
                {
                    //Check that the path exist between two points and if 
                    //distance for point ti was already calculated
                    if(distanceBetPoints[ti,j]!=99999 && totalDistance[ti]!=99999)
                    {                  
                        //Check if new distance is smaller than that which already exist
                        //for point j or if distance equal 99999
                        if(totalDistance[j]>totalDistance[ti]+distanceBetPoints[ti,j] || totalDistance[j]==99999)
                        {
                            //Set new distance
                            totalDistance[j]=totalDistance[ti]+distanceBetPoints[ti,j];

                            //Set new path
                            pointsToPass[j].Clear();
                            pointsToPass[j].AddRange(pointsToPass[ti]);
                            pointsToPass[j].Add(ti);

                            /*Debug.Log("ti: "+ti+"; j: "+j);
                            string stringOfPt="";
                            foreach(var pt in l)
                            {
                                stringOfPt=stringOfPt+", "+pt;
                            }
                            Debug.Log(" "+stringOfPt);*/

                            //Debug.Log("Added");
                        }
                    }
                }
            }
        }

        //Debug.Log("dist: "+totalDistance[pt2]);

        //Add destination point to the List
        pointsToPass[pt2].Add(pt2);

        /*int k=0;
        foreach(var l in pointsToPass)
        {
            string stringOfPt="";
            foreach(var pt in l)
            {
                stringOfPt=stringOfPt+", "+pt;
            }
            Debug.Log(k+":    "+stringOfPt);
            k++;
        }*/

        return pointsToPass[pt2];
    }
}