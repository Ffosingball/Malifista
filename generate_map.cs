using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



//Class which generate territory for its own region
//Basicly its pick the center point on the map
//After expand its territory while there is free space
//If it meets square which already occupied by other region it will not expand there
class Region
{
    //Own number is number of its region
    //x,y - coordinates of centers of this regions
    //width and height of the map
    int x, y, width, height, ownNumber;


    //Constructor for usual region
    public Region(int semiSquare, int width, int height,int ownNumber)
    {
        //Initialize internal boundaries for generatint center of the region
        //This is necessary in order to avoid generating all centers to near to each other
        //so all regions will have appropriate size
        int minX=0,maxX=0,minY=0,maxY=0;

        this.width=width;
        this.height=height;
        this.ownNumber=ownNumber;

        checkSemiSquare(semiSquare,ref minX,ref maxX, ref minY, ref maxY);

        //pick the central point inside internal boundaries
        x=UnityEngine.Random.Range(minX,maxX+1);
        y=UnityEngine.Random.Range(minY,maxY+1);
    }


    //Constructor for the starter region where player will appear
    public Region(int width,int height,int ownNumber)
    {
        this.ownNumber=ownNumber;
        this.width=width;
        this.height=height;

        //Center of this region will be in the center of the map
        this.x=width/2;
        this.y=height/2;
    }


    //This function give internal boundaries depending on the semisquare
    //Semisquares is like quadrants in cartesian coordinate plane
    void checkSemiSquare(int semiSquare,ref int minX,ref int maxX,ref int minY, ref int maxY)
    {
        switch(semiSquare)
        {
            case 1:
                minX=width/2+1;
                maxX=width-1;
                minY=height/2+1;
                maxY=height-1;
                break;
            case 2:
                minX=0;
                maxX=width/2-1;
                minY=height/2+1;
                maxY=height-1;
                break;
            case 3:
                minX=0;
                maxX=width/2-1;
                minY=height/2-1;
                maxY=0;
                break;
            case 4:
                minX=width/2+1;
                maxX=width-1;
                minY=height/2-1;
                maxY=0;
                break;
        }
    }



    //Function which expand region
    public bool expand_region(ref int[,] region, int step)
    {
        bool finish=false;
        int changes=0;

        //Expand region
        for(int l=(-1)*step; l<step+1;l++)
        {
            for(int h=(-1)*step; h<step+1; h++)
            {
                if(x+l>=0 && x+l<=width-1 && y+h>=0 && y+h<=height-1)
                {
                    if(region[x+l,y+h]==0)
                    {
                        region[x+l,y+h]=ownNumber;
                        changes++;
                    }
                }
            }
        }

        //If finish=false then there was some expansion in this step
        //If true there wasn`t any expansion
        //This need to stop while loop when all region cannot expand anymore
        if(changes==0)
            finish=true;

        return finish;
    }
}






public class generate_map : MonoBehaviour
{
    //How much squares will made of the level
    public float width=10, height=10;
    //This variables duplicate value from the original variables, so other classes can have access to them
    public static float widthS, heightS, widthCornerS, widthMainS;
    //How much iterates will be for cellular automata
    public int howMuchCells=5;
    //Array "map" tell where put wall(0) and corridor(1)
    //Array "region" tell which squares at which regions (from 1 to 4. 1 is basic region)  
    private int[,] map, region;
    //Reference to Canvas at which will be created Images which represents corridors and regions
    public Canvas canvas;
    //This is array of all objects which will show me how corridors or regions will be generated
    private GameObject[,] squaresShown;
    //Bool variables to show if any of toddlers is on or off
    public bool showsCorridors=false, showRegions=false;
    //Bool variables which tell is something regenerated or is any map is displayed
    private bool mapRegenerated=false, regionRegenerated=false, isShownMap=false;
    //It is for temporary level
    //First four is corridors and their number is number of the region-1
    //Another four is walls and yheir numbers is number of the region+3
    //Last two is for border between regions
    public GameObject[] blocksForGeneratingMap;
    //Width and height of blocks of which made the level
    public float widthCorner, widthMain;
    //It needs to mave maps, so it will be fully shown on the scren
    public int addToShowMapX=100,addToShowMapY=10;
    //Reference to loading screen and square of which map is made of
    public GameObject loadingScreen, square;
    //Reference to another class
    public FindShortestWay addPoints;


    // Start is called before the first frame update
    void Start()
    {
        loadingScreen.SetActive(false);
        //Initialize 2D arrays
        map=new int[(int)width,(int)height];
        region=new int[(int)width,(int)height];
        squaresShown=new GameObject[(int)width,(int)height];

        //Dublicate its value
        widthS=width;
        heightS=height;
        widthCornerS=widthCorner;
        widthMainS=widthMain;

        generate_map_of_corridors();

        generate_regions_of_map();

        //It is in courutine because it could take long time
        StartCoroutine(create_level());
    }



    void Update()
    {
        //Check is any of maps should be displayed
        if(showsCorridors)
        {
            if(isShownMap==false)
            {
                display_squares_UI();
                //Debug.Log("1");
            }
            else if(mapRegenerated)
            {
                delete_shown_objects();
                display_squares_UI();
                //Debug.Log("2");

                mapRegenerated=false;
            }
        }
        else if(showRegions)
        {
            if(isShownMap==false)
            {
                display_regions_UI();
                //Debug.Log("3");
            }
            else if(regionRegenerated)
            {
                delete_shown_objects();
                display_regions_UI();
                //Debug.Log("4");

                regionRegenerated=false;
            }
        }
        else if(isShownMap)
        {
            delete_shown_objects();
            //Debug.Log("5");

            isShownMap=false;
        }
    }







    //This function use cellular automata to generate map of the game
    //Basicly each square move at any direction and create corridor after himself 
    //and have 1/16 chance to turn right or 1/16 chance to turn left
    void generate_map_of_corridors()
    {
        //At the center always must be corridor because there will appear player
        map[(int)width/2,(int)height/2]=1;

        //Iterate all squares 
        for(int i=0; i<howMuchCells; i++)
        {
            //Set initial coords
            int y=(int)height/2, x=(int)width/2,direction=0;

            //Set initial direction
            switch(UnityEngine.Random.Range(0,4))
            {
                case 0:
                    direction=0;//Upward
                    break;
                case 1:
                    direction=1;//Right
                    break;
                case 2:
                    direction=2;//Downward
                    break;
                case 3:
                    direction=3;//Left
                    break;
            }

            //Iterate movement of a square
            while(x>0 && x<width-1 && y>0 && y<height-1)
            {
                //Generate random number for turning
                int turnChance=UnityEngine.Random.Range(0,16);

                if(turnChance==4)
                {
                    direction=direction-1;//Turn left
                }
                else if(turnChance==9)
                {
                    direction=direction+1;//Turn right
                }

                //Move square and add it to the array
                move_square(ref x, ref y, ref direction);
                map[x,y]=1;
            }
        }
    }


    //Just check at which direction squre should moved and move there
    void move_square(ref int x, ref int y, ref int direction)
    {
        switch(direction%4)
        {
            case 0:
                y++;
                break;
            case 1:
            case -3:
                x++;
                break;
            case 3:
            case -1:
                x--;
                break;
            case 2:
            case -2:
                y--;
                break;
        }
    }



    //It go throug all Array and create map of corridors in canvas
    void display_squares_UI()
    {
        //Get screen sizes
        Resolution screenSize = Screen.currentResolution;

        for(int x=0; x<width;x++)
        {
            for(int y=0; y<height; y++)
            {
                //Create new game object
                squaresShown[x,y] = Instantiate(square, new Vector3(0f,0f,0f), Quaternion.Euler(0f,0f,0f));
                squaresShown[x,y].transform.SetParent(canvas.transform); //Set this canvas as parent, so it will be shown on that canvas

                //Set coords and sizes of the object
                Transform squareTran = squaresShown[x,y].GetComponent<Transform>();
                //It will be on the bottom left corner of the screen
                squareTran.localPosition = new Vector3((x*4)-(screenSize.width/2)+addToShowMapX, (y*4)-(screenSize.height/2)+addToShowMapY, 0);
                squareTran.localScale = new Vector3(4, 4, 0f);

                SpriteRenderer squareRen = squaresShown[x,y].GetComponent<SpriteRenderer>();

                if(map[x,y]==1)
                {
                    //Set corridor
                    squareRen.color = Color.white;
                }
                else
                {
                    //Set wall
                    squareRen.color = Color.black;
                }
            }
            
        }

        isShownMap=true;
    }



    //This function just delete current map of corridors
    void delete_map()
    {
        for(int x=0; x<width;x++)
        {
            for(int y=0; y<height; y++)
            {
                map[x,y]=0;
            }
        }
    }



    //Function for button which will regenerate map of corridors
    public void regenerate_map()
    {
        delete_map();
        generate_map_of_corridors();

        mapRegenerated=true;
    }







    //This function just delete current map of region
    void delete_regions_of_map()
    {
        for(int x=0; x<width;x++)
        {
            for(int y=0; y<height; y++)
            {
                region[x,y]=0;
            }
        }
    }



    //Function for button which will regenerate map of regions
    public void regenerate_regions_of_map()
    {
        delete_regions_of_map();
        generate_regions_of_map();

        regionRegenerated=true;
    }



    //Function which generate regions for the map
    void generate_regions_of_map()
    {
        //Create array of regions
        Region[] regions = new Region[4];

        //Create starter region
        regions[0]=new Region((int)width,(int)height,1);

        //Randomly pick semiSquare at which center point will be picked up
        int whichSemiSquare=UnityEngine.Random.Range(1,5);
        
        //Create other 3 regions and semiSquares for them
        for(int i=1; i<4; i++)
        {
            whichSemiSquare--;
            if(whichSemiSquare==0)
                whichSemiSquare=4;
            regions[i]=new Region(whichSemiSquare,(int)width,(int)height,i+1);
        }


        bool con=true;//tell then to stop while loop
        int step=1;//step in expansion of the regions
        bool[] doneRegions=new bool[4];//array for bools from all regions
        //to check that all regions finished their expansion and stop loop

        while(con)
        {
            //All regions expand their territory
            for(int i=0; i<4; i++)
            {
                doneRegions[i]=regions[i].expand_region(ref region, step);
            }

            //Check if all regions finished their expansion
            if(doneRegions[0] && doneRegions[1] && doneRegions[2] && doneRegions[3])
                con=false;
            
            step++;
        }
    }



    //It go throug all array and create objects with 
    //different colors which represent different regions
    void display_regions_UI()
    {
        //Get screen sizes
        Resolution screenSize = Screen.currentResolution;

        for(int x=0; x<width;x++)
        {
            for(int y=0; y<height; y++)
            {
                //Create new object
                squaresShown[x,y] = Instantiate(square, new Vector3(0f,0f,0f), Quaternion.Euler(0f,0f,0f));
                squaresShown[x,y].transform.SetParent(canvas.transform); //Set this canvas as parent, so it will be shown on that canvas

                //Set coords and sizes of the object
                Transform squareTran = squaresShown[x,y].GetComponent<Transform>();
                //It will be on the bottom left corner of the screen
                squareTran.localPosition = new Vector3((x*4)-(screenSize.width/2)+addToShowMapX, (y*4)-(screenSize.height/2)+addToShowMapY, 0);
                squareTran.localScale = new Vector3(4, 4, 0);

                SpriteRenderer squareRen = squaresShown[x,y].GetComponent<SpriteRenderer>();

                if(region[x,y]==1)
                {
                    //Color of the region 1
                    squareRen.color = new Color(0.56f, 0.56f, 0.56f, 1f);
                }
                else if(region[x,y]==2)
                {
                    //Color of the region 2
                    squareRen.color = new Color(0.05f, 0.44f, 0.94f, 1f);
                }
                else if(region[x,y]==3)
                {
                    //Color of the region 3
                    squareRen.color = new Color(0.94f, 0.05f, 0.29f, 1f);
                }
                else if(region[x,y]==4)
                {
                    //Color of the region 4
                    squareRen.color = new Color(0.56f, 0.9f, 0.16f, 1f);
                }
            }
        }

        isShownMap=true;
    }






    //Function which create level using generated maps of corridors and regions
    private IEnumerator create_level()
    {
        int whichType=0;//Which type of square use from array of prephabs
        loadingScreen.SetActive(true);//Show loading screen
        yield return new WaitForSeconds(0.1f);//Coroutine requires do it

        //It count how many corridors will de on the level
        //It need to initialize array of all points 
        int n=0;
        for(int x=0; x<width;x++)
        {
            for(int y=0; y<height; y++)
            {
                //Check is it wall or corridor
                if(map[x,y]==1)
                {
                    n++;
                }
            }
        }

        //Initialize arrays in the FindShortestWay class
        addPoints.InitializeArrays(n);
        //Debug.Log("Number of poits: "+n);

        //Create main parts
        for(int x=0; x<width;x++)
        {
            for(int y=0; y<height; y++)
            {
                //Check is it wall or corridor
                if(map[x,y]==0)
                {
                    whichType=4;
                }

                check_region(ref whichType,x,y);

                //It creates parts of the level at required position and set required sizes
                GameObject temp = Instantiate(blocksForGeneratingMap[whichType], new Vector3((widthCorner+widthMain)*(x-(width/2)), (widthCorner+widthMain)*(y-(height/2)), 0f), Quaternion.Euler(0f,0f,0f));
                temp.transform.localScale=new Vector3(widthMain, widthMain, 1);
                
                //Check is it a corridor
                //If it true, it add its coordinates to the array of all points
                if(whichType<4)
                    addPoints.addCoords((widthCorner+widthMain)*(x-(width/2)),(widthCorner+widthMain)*(y-(height/2)));

                whichType=0;//Reset it
            }
        }

        //Create corner parts
        for(int x=0; x<width-1;x++)
        {
            for(int y=0; y<height-1; y++)
            {
                //Check is it at the border between regions
                if(region[x,y]!=region[x+1,y] || region[x,y]!=region[x,y+1] || region[x,y]!=region[x+1,y+1])
                {
                    whichType=8;

                    //Check is there wall anywhere
                    if(map[x,y]==0 || map[x,y+1]==0 || map[x+1,y]==0 || map[x+1,y+1]==0)
                    {
                        whichType++;
                    }
                }
                else
                {
                    //Check is there wall anywhere
                    if(map[x,y]==0 || map[x,y+1]==0 || map[x+1,y]==0 || map[x+1,y+1]==0)
                    {
                        whichType=4;
                    }

                    check_region(ref whichType,x,y);
                }

                //It creates parts of the level at required position and set required sizes
                GameObject temp = Instantiate(blocksForGeneratingMap[whichType], new Vector3((widthCorner+widthMain)*(x-(width/2))+(widthMain/2)+(widthCorner/2), (widthCorner+widthMain)*(y-(height/2))+(widthMain/2)+(widthCorner/2), 0f), Quaternion.Euler(0f,0f,0f));
                temp.transform.localScale=new Vector3(widthCorner, widthCorner, 1);

                whichType=0;//Reset it
            }
        }

        //Create middle horizontal parts
        for(int x=0; x<width;x++)
        {
            for(int y=0; y<height-1; y++)
            {
                //Check is it at the border between regions
                if(region[x,y]!=region[x,y+1])
                {
                    whichType=8;

                    //Check is there wall anywhere
                    if(map[x,y]==0 || map[x,y+1]==0)
                    {
                        whichType++;
                    }
                }
                else
                {
                    //Check is there wall anywhere
                    if(map[x,y]==0 || map[x,y+1]==0)
                    {
                        whichType=4;
                    }

                    check_region(ref whichType,x,y);
                }

                //It creates parts of the level at required position and set required sizes
                GameObject temp = Instantiate(blocksForGeneratingMap[whichType], new Vector3((widthCorner+widthMain)*(x-(width/2)), (widthCorner+widthMain)*(y-(height/2))+(widthMain/2)+(widthCorner/2), 0f), Quaternion.Euler(0f,0f,0f));
                temp.transform.localScale=new Vector3(widthMain, widthCorner, 1);

                whichType=0;//Reset it
            }
        }

        //Create middle vertical parts
        for(int x=0; x<width-1;x++)
        {
            for(int y=0; y<height; y++)
            {
                //Check is it at the border between regions
                if(region[x,y]!=region[x+1,y])
                {
                    whichType=8;

                    //Check is there wall anywhere
                    if(map[x,y]==0 || map[x+1,y]==0)
                    {
                        whichType++;
                    }
                }
                else
                {
                    //Check is there wall anywhere
                    if(map[x,y]==0 || map[x+1,y]==0)
                    {
                        whichType=4;
                    }

                    check_region(ref whichType,x,y);
                }

                //It creates parts of the level at required position and set required sizes
                GameObject temp = Instantiate(blocksForGeneratingMap[whichType], new Vector3((widthCorner+widthMain)*(x-(width/2))+(widthMain/2)+(widthCorner/2), (widthCorner+widthMain)*(y-(height/2)), 0f), Quaternion.Euler(0f,0f,0f));
                temp.transform.localScale=new Vector3(widthCorner, widthMain, 1);

                whichType=0;//Reset it
            }
        }

        //Call function which will calculate all possible
        //routes between points
        addPoints.findDistancesBetPoints();
        yield return new WaitForSeconds(0.1f);
        loadingScreen.SetActive(false);
    }


    //Function which set the right region
    void check_region(ref int whichType,int x, int y)
    {
        switch(region[x,y])
        {
            case 2:
                whichType++;
                break;
            case 3:
                whichType+=2;
                break;
            case 4:
                whichType+=3;
                break;
        }
    }






    //Function which delete all objects which was displayed 
    //to show map of corridors or regions
    void delete_shown_objects()
    {
        if(squaresShown[0,0]!=null)
        {
            for(int x=0; x<width;x++)
            {
                for(int y=0; y<height; y++)
                {
                    Destroy(squaresShown[x,y]);
                }
            }
        }
    }



    //Function for toddler which will show map of regions
    //Only one of two maps can be displayed
    public void show_map_of_regions_change()
    {
        if(showRegions)
        {
            showRegions=false;
            delete_shown_objects();
        }
        else
        {
            showRegions=true;

            if(showsCorridors)//Check if another toddler already on
            {
                showsCorridors=false;

                delete_shown_objects();
                display_regions_UI();
            }
        }
    }



    //Function for toddler which will show map of corridors
    //Only one of two maps can be displayed
    public void show_map_of_corridors_change()
    {
        if(showsCorridors)
        {
            showsCorridors=false;
            delete_shown_objects();
        }
        else
        {
            showsCorridors=true;

            if(showRegions)//Check if another toddler already on
            {
                showRegions=false;

                delete_shown_objects();
                display_squares_UI();
            }
        }
    }
}
