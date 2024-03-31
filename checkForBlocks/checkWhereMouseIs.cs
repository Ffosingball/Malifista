using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class checkWhereMouseIs : MonoBehaviour
{
    //This code change variable move_character.IsMouseOnWall
    //to true if mouse is over the wall
    private void OnMouseEnter() 
    {
        move_character.IsMouseOnWall=true;
    }
}
