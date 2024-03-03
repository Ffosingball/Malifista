using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class generate_map : MonoBehaviour
{
    public int width=10, height=10, howMuchCells=5;
    private int[,] map;
    public GameObject corridor, wall;


    // Start is called before the first frame update
    void Start()
    {
        map=new int[width,height];
        generate_map_of_corridors();
    }



    void generate_map_of_corridors()
    {
        for(int i=0; i<howMuchCells; i++)
        {
            int y=height/2, x=width/2,direction=0;

            switch(UnityEngine.Random.Range(0,4))
            {
                case 0:
                    direction=0;
                    break;
                case 1:
                    direction=1;
                    break;
                case 2:
                    direction=2;
                    break;
                case 3:
                    direction=3;
                    break;
            }

            while(x>0 && x<width && y>0 && y<height)
            {
                int turnChance=UnityEngine.Random.Range(0,10);

                if(turnChance==1 || turnChance==2)
                {
                    direction=direction-1;
                }
                else if(turnChance==3 || turnChance==4)
                {
                    direction=direction+1;
                }

                move_square(out x, out y, out direction);
                map[x,y]=1;
            }
        }

        display_squares();
    }



    void move_square(out int x, out int y, out int direction)
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



    void display_squares()
    {
        for(int x=0; x++; x<width)
        {
            for(int y=0; y++; y<height)
            {
                if(map[x,y]==1)
                {
                    Instantiate(corridor, new Vector3(x,y,0f), Quaternion.Euler(0f,0f,0f));
                }
                else
                {
                    Instantiate(wall, new Vector3(x,y,0f), Quaternion.Euler(0f,0f,0f));
                }
            }
        }
    }
}
