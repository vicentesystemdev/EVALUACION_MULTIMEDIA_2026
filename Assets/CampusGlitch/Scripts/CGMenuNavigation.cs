using UnityEngine;
using UnityEngine.InputSystem;

namespace CampusGlitch {
/// <summary>One selection step per tap, then controlled repeat for keyboard, d-pad and analog stick.</summary>
public sealed class CGMenuNavigation {
    Vector2Int previous;float repeatAt;
    public void Reset(){previous=Vector2Int.zero;repeatAt=0;}
    public int Read(int current,int count,bool grid){
        Vector2 axis=Vector2.zero;var keyboard=Keyboard.current;var pad=Gamepad.current;
        if(keyboard!=null){
            if(keyboard.upArrowKey.isPressed||keyboard.wKey.isPressed)axis.y=1;
            if(keyboard.downArrowKey.isPressed||keyboard.sKey.isPressed)axis.y=-1;
            if(keyboard.leftArrowKey.isPressed||keyboard.aKey.isPressed)axis.x=-1;
            if(keyboard.rightArrowKey.isPressed||keyboard.dKey.isPressed)axis.x=1;
        }
        if(pad!=null&&axis==Vector2.zero){axis=pad.dpad.ReadValue();if(axis.sqrMagnitude<.1f)axis=pad.leftStick.ReadValue();}
        Vector2Int direction=Vector2Int.zero;
        if(Mathf.Abs(axis.y)>=.55f&&Mathf.Abs(axis.y)>=Mathf.Abs(axis.x))direction.y=axis.y>0?-1:1;
        else if(grid&&Mathf.Abs(axis.x)>=.55f)direction.x=axis.x>0?1:-1;
        if(direction==Vector2Int.zero){previous=direction;return current;}
        bool first=direction!=previous;previous=direction;
        if(!first&&Time.unscaledTime<repeatAt)return current;
        repeatAt=Time.unscaledTime+(first?.32f:.13f);
        if(grid)return MoveOnMap(current,direction);
        return (current+direction.y+count)%count;
    }
    public static int MoveOnMap(int current,Vector2Int direction){
        current=Mathf.Clamp(current,0,9);
        if(current==9)return direction.y<0?7:direction.y>0?1:direction.x<0?6:8;
        int row=current/3,col=current%3;
        if(direction.x!=0)return row*3+(col+direction.x+3)%3;
        if(direction.y>0&&row==2&&col==1)return 9;
        return ((row+direction.y+3)%3)*3+col;
    }
}
}
