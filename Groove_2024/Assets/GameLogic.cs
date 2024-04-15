using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEditor.Timeline.Actions;
using UnityEngine;

public enum BoardObject
{
    Empty, // Can become Alpha or Bravo
    Alpha_Static,
    Bravo_Static,
    Alpha_Active,
    Bravo_Active,
    Filled, // Forcibly-filled board piece. 'Cement'
    Sidewall // Edge of Boardwall. Resets at the end of each turn.
}

public class GameLogic : MonoBehaviour
{
    [SerializeField, Range(5, 20)]
    int BoardWidth_Maximum;
    [SerializeField, Range(5, 20)]
    int BoardHeight_Maximum;

    int BoardWidth;
    int BoardHeight;

    List<BoardObject> Board;

    // Start is called before the first frame update
    void Start()
    {
        Init_Board();

        
    }

    void Init_Board()
    {
        // Extend width of board by 2 to include the Sidewalls
        BoardWidth = BoardWidth_Maximum + 2;
        BoardHeight = BoardHeight_Maximum;

        Board = new List<BoardObject>();

        // Vertical
        for(int k = 0; k < BoardHeight; k++)
        {
            for(int j = 0; j < BoardWidth; j++)
            {
                // If the J value is 0 (left side) or BoardWidth - 1 (right side), add as Sidewall
                // Otherwise, add as Empty

                BoardObject tempBoardObject = BoardObject.Empty;
                if (j == 0 || j == BoardWidth - 1)
                    tempBoardObject = BoardObject.Sidewall;

                // print("[" + j + ", " + k + "]: " + tempBoardObject.ToString());

                Board.Add(tempBoardObject);
            }
        }

        Console_PrintBoard();
    }

    #region Gameplay Actions
    void RotateClockwise()
    {

    }

    void RotateCounterClockwise()
    {

    }

    void SoftDrop()
    {

    }

    void HardDrop()
    {

    }
    #endregion

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Overrides position in Board at [x,y] position with given BoardObject
    /// </summary>
    /// <param name="_x">X (Horizontal) Position. 0 = Left side of Row.</param>
    /// <param name="_y">Y (Vertical) Position. 0 = Bottom of Column.</param>
    /// <param name="_boardObject">Board Object to Override at [X,Y] position</param>
    /// <returns>Returns the board object that previously existed</returns>
    /// <example> BoardObject oldObject = SetBoardObjectAtPosition(1, 3, BoardObject.Alpha_Static)</example>
    BoardObject SetBoardObjectAtPosition(int _x, int _y, BoardObject _boardObject)
    {
        BoardObject oldBoardObject = GetBoardObjectAtPosition(_x, _y);

        Board[(BoardWidth * _y) + _x] = _boardObject;

        return oldBoardObject;
    }

    /// <summary>
    /// Returns the Board object at [x,y] position
    /// </summary>
    /// <param name="_x">X (Horizontal) Position. 0 = Left side of Row.</param>
    /// <param name="_y">Y (Vertical) Position. 0 = Bottom of Column.</param>
    /// <returns>Returns the Board object at [x,y] position</returns>
    BoardObject GetBoardObjectAtPosition(int _x, int _y)
    {
        BoardObject tempObject = BoardObject.Empty;

        tempObject = Board[(BoardWidth * _y) + _x];

        return tempObject;
    }

    void Console_PrintBoard()
    {
        for(int y = BoardHeight - 1; y >= 0; y--)
        {
            string textLine = "" + y + ": ";
            for(int x = 0; x < BoardWidth; x++)
            {
                // BoardWidth * k = vertical position
                // j = horizontal position
                BoardObject currBoardObject = Board[(BoardWidth * y) + x];

                if (currBoardObject == BoardObject.Empty)
                    textLine += "[ ]";
                else if (currBoardObject == BoardObject.Sidewall)
                    textLine += "[*]";
                else if (currBoardObject == BoardObject.Alpha_Active || currBoardObject == BoardObject.Alpha_Static)
                    textLine += "[X]";
                else if (currBoardObject == BoardObject.Bravo_Active || currBoardObject == BoardObject.Bravo_Static)
                    textLine += "[O]";
                else if (currBoardObject == BoardObject.Filled)
                    textLine += "[=]";
            }
            print(textLine);
        }
    }
}
