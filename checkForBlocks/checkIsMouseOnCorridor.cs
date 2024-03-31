using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class checkIsMouseOnCorridor : MonoBehaviour
{
    //This code change variable move_character.IsMouseOnWall
    //to false if mouse is over the corridor
    private void OnMouseEnter() 
    {
        move_character.IsMouseOnWall=false;
    }
}
