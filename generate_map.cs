using System.Collections;
using System.Collections.Generic;
using UnityEngine;


class Region
{
    int x, y, width, height, ownNumber;


    public Region(int semiSquare, int width, int height,int ownNumber)
    {
        int minX=0,maxX=0,minY=0,maxY=0;

        this.width=width;
        this.height=height;
        this.ownNumber=ownNumber;

        checkSemiSquare(semiSquare,ref minX,ref maxX, ref minY, ref maxY);

        x=UnityEngine.Random.Range(minX,maxX+1);
        y=UnityEngine.Random.Range(minY,maxY+1);
    }



    public Region(int x, int y,int width,int height,int ownNumber)
    {
        this.ownNumber=ownNumber;
        this.x=x;
        this.y=y;
        this.width=width;
        this.height=height;
    }



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



    public bool expand_region(ref int[,] region, int step)
    {
        bool finish=false;
        int changes=0;

        for(int l=(-1)*step; l<step+1;l++)
        {
            for(int h=(-1)*step; h<step+1; h++)
            {
                if(x+l>0 && x+l<width-1 && y+h>0 && y+h<width-1)
                {
                    if(region[x+l,y+h]==0)
                    {
                        region[x+l,y+h]=ownNumber;
                        changes++;
                    }
                }
            }
        }

        if(changes==0)
            finish=true;

        return finish;
    }
}






public class generate_map : MonoBehaviour
{
    public int width=10, height=10, howMuchCells=5;
    //This array tell where put wall(0) and corridor(1)
    private int[,] map, region;
    public GameObject corridor, wall, center;
    public GameObject[] regionSquaresObj=new GameObject[4];
    //This is array of all objects which will show me how corridors will be generated
    private GameObject[,] squares, regionSquares;


    // Start is called before the first frame update
    void Start()
    {
        //Initialize 2D arrays
        map=new int[width,height];
        region=new int[width,height];
        squares=new GameObject[width,height];
        regionSquares=new GameObject[width,height];

        generate_map_of_corridors();

        generate_regions_of_map();
    }


    //This function use cellular automata to generate map of the game
    //Basicly each square move at any direction and create corridor after himself 
    //and have 1/16 chance to turn right or 1/16 chance to turn left
    void generate_map_of_corridors()
    {
        //At the center always must be corridor because there will appear player
        map[width/2,height/2]=1;

        //Iterate all squares 
        for(int i=0; i<howMuchCells; i++)
        {
            //Set initial coords
            int y=height/2, x=width/2,direction=0;

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

        //Display map of corridors for developer if he needs it
        display_squares();
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


    //It go throug all Array and create two objects
    //Which represent walls and corridors
    void display_squares()
    {
        for(int x=0; x<width;x++)
        {
            for(int y=0; y<height; y++)
            {
                if(map[x,y]==1)
                {
                    //Create corridor
                    squares[x,y]=Instantiate(corridor, new Vector3(x,y,0f), Quaternion.Euler(0f,0f,0f));
                }
                else
                {
                    //Create wall
                    squares[x,y]=Instantiate(wall, new Vector3(x,y,0f), Quaternion.Euler(0f,0f,0f));
                }
            }
        }
    }


    //This function just delete current map of corridors
    void delete_map()
    {
        for(int x=0; x<width;x++)
        {
            for(int y=0; y<height; y++)
            {
                Destroy(squares[x,y]);
                map[x,y]=0;
            }
        }
    }


    void delete_regions_of_map()
    {
        for(int x=0; x<width;x++)
        {
            for(int y=0; y<height; y++)
            {
                Destroy(regionSquares[x,y]);
                region[x,y]=0;
            }
        }
    }


    //Function for button which will regenerate map of corridors
    public void regenerate_map()
    {
        delete_map();
        generate_map_of_corridors();
    }



    public void regenerate_regions_of_map()
    {
        delete_regions_of_map();
        generate_regions_of_map();
    }



    void generate_regions_of_map()
    {
        Region[] regions = new Region[4];

        regions[0]=new Region(width/2,height/2,width,height,1);

        int whichSemiSquare=UnityEngine.Random.Range(1,5);
        
        for(int i=1; i<4; i++)
        {
            whichSemiSquare--;
            if(whichSemiSquare==0)
                whichSemiSquare=4;
            regions[i]=new Region(whichSemiSquare,width,height,i+1);
        }

        bool con=true;
        int step=1;
        bool[] doneRegions=new bool[4];
        while(con)
        {
            for(int i=0; i<4; i++)
            {
                doneRegions[i]=regions[i].expand_region(ref region, step);
            }

            if(doneRegions[0] && doneRegions[1] && doneRegions[2] && doneRegions[3])
                con=false;
        }


        display_regions();
    }



    void display_regions()
    {
        for(int x=0; x<width;x++)
        {
            for(int y=0; y<height; y++)
            {
                if(region[x,y]==1)
                {
                    //Create corridor
                    regionSquares[x,y]=Instantiate(regionSquaresObj[0], new Vector3(x-width,y-height,0f), Quaternion.Euler(0f,0f,0f));
                }
                else if(region[x,y]==2)
                {
                    //Create wall
                    regionSquares[x,y]=Instantiate(regionSquaresObj[1], new Vector3(x-width,y-height,0f), Quaternion.Euler(0f,0f,0f));
                }
                else if(region[x,y]==3)
                {
                    //Create wall
                    regionSquares[x,y]=Instantiate(regionSquaresObj[2], new Vector3(x-width,y-height,0f), Quaternion.Euler(0f,0f,0f));
                }
                else if(region[x,y]==4)
                {
                    //Create wall
                    regionSquares[x,y]=Instantiate(regionSquaresObj[3], new Vector3(x-width,y-height,0f), Quaternion.Euler(0f,0f,0f));
                }
            }
        }
    }
}
