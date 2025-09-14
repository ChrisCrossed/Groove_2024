using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;
using System.Linq;

public struct FloodFillArrayObject
{
    // Store Array, and current best information for comparison
    private int[] floodFillArray;
    private int currBestColumnValue;
    private int currBestColumnPosition;
    private bool successfulPath;

    public FloodFillArrayObject(int currBestColumnValue_ = 999, int currBestColumnPosition_ = 0)
    {
        floodFillArray = new int[0];
        currBestColumnValue = currBestColumnValue_;
        currBestColumnPosition = currBestColumnPosition_;
        successfulPath = false;
    }

    public int[] FloodFillArray
    {
        get => floodFillArray;
        set => floodFillArray = value;
    }

    public int CurrBestColumnValue
    {
        get => currBestColumnValue;
        set => currBestColumnValue = value;
    }

    public int CurrBestColumnPosition
    {
        get => currBestColumnPosition;
        set => currBestColumnPosition = value;
    }

    public bool SuccessfulPath
    {
        get => successfulPath;
        set => successfulPath = value;
    }
}

public static class c_PathfindingLogic
{
    static List<BoardObject> InitialBoard;
    static int BoardWidth;
    static int BoardHeight;

    static List<PathBoardObject> AlphaPathfindList;
    static List<PathBoardObject> BravoPathfindList;

    static int CurrentAlpha = 99;
    static int CurrentBravo = 99;

    static int AlphaThreads = 0;
    static int BravoThreads = 0;

    static bool FoundScoreline;

    static List<PathBoardObject> SuccessfulPathfindList_Alpha;
    static List<PathBoardObject> SuccessfulPathfindList_Bravo;

    static List<int> tempVertXPositions;
    static int numEachColumn_Alpha;
    static int numEachColumn_Bravo;

    static int RepeatScorelineEvalLength;

    public static List<int> StartPathfindingLogic(List<BoardObject> _board, int _boardWidth)
    {
        Print("Here");
        BoardWidth = _boardWidth;
        BoardHeight = _board.Count / _boardWidth;

        InitialBoard = _board;

        #region Vertical Tests
        bool hasAlpha = VerticalTest(BoardObject.Alpha_Static);
        bool hasBravo = VerticalTest(BoardObject.Bravo_Static);
        #endregion Vertical Tests

        #region Preload Left Column Start Positions
        List<int> LeftColumnStartPoints_Alpha = new List<int>();
        List<int> RightColumnEndPoints_Alpha = new List<int>();

        List<int> LeftColumnStartPoints_Bravo = new List<int>();
        List<int> RightColumnEndPoints_Bravo = new List<int>();

        if (hasAlpha)
        {
            LeftColumnStartPoints_Alpha = GetLeftColumnValidStartPoints(BoardObject.Alpha_Static);
            RightColumnEndPoints_Alpha = GetRightColumnValidEndPoints(BoardObject.Alpha_Static);

            Print("Alpha (Left)");
            for (int i = 0; i < LeftColumnStartPoints_Alpha.Count; i++)
            {
                Print(LeftColumnStartPoints_Alpha[i].ToString());
            }
            Print("---\nAlpha (Right)");
            for (int i = 0; i < RightColumnEndPoints_Alpha.Count; i++)
            {
                Print(RightColumnEndPoints_Alpha[i].ToString());
            }

            if (LeftColumnStartPoints_Alpha.Count == 0 || RightColumnEndPoints_Alpha.Count == 0)
            {
                hasAlpha = false;
            }
        }

        if (hasBravo)
        {
            LeftColumnStartPoints_Bravo = GetLeftColumnValidStartPoints(BoardObject.Bravo_Static);
            RightColumnEndPoints_Bravo = GetRightColumnValidEndPoints(BoardObject.Bravo_Static);

            Print("Bravo (Left)");
            for (int i = 0; i < LeftColumnStartPoints_Bravo.Count; i++)
            {
                Print(LeftColumnStartPoints_Bravo[i].ToString());
            }
            Print("---\nBravo (Right)");
            for (int i = 0; i < RightColumnEndPoints_Bravo.Count; i++)
            {
                Print(RightColumnEndPoints_Bravo[i].ToString());
            }

            if (LeftColumnStartPoints_Bravo.Count == 0 || RightColumnEndPoints_Bravo.Count == 0)
            {
                hasBravo = false;
            }
        }
        #endregion Preload Left Column Start Position

        // Just kick out if no proper paths can exist
        if (!hasAlpha && !hasBravo)
            return null;


        // Store Alpha Array, and current best information for comparison
        FloodFillArrayObject floodFillArray_Alpha = new FloodFillArrayObject();

        if (hasAlpha)
        {
            floodFillArray_Alpha = CycleAllReverseConnectionFloodFillArraysOfType(BoardObject.Alpha_Static, LeftColumnStartPoints_Alpha);

            hasAlpha = floodFillArray_Alpha.SuccessfulPath;

            Print("Alpha Values:");
            Print("Length: " + floodFillArray_Alpha.FloodFillArray.Length.ToString());
            Print("Best Column Score: " + floodFillArray_Alpha.CurrBestColumnValue);
            Print("Best Column Array Position: " + floodFillArray_Alpha.CurrBestColumnPosition);
            Print("Successful: " + floodFillArray_Alpha.SuccessfulPath);
            Print("---");
        }

        // Store Bravo Array, and current best information for comparison
        FloodFillArrayObject floodFillArray_Bravo = new FloodFillArrayObject();

        if (hasBravo)
        {
            floodFillArray_Bravo = CycleAllReverseConnectionFloodFillArraysOfType(BoardObject.Bravo_Static, LeftColumnStartPoints_Bravo);

            hasBravo = floodFillArray_Bravo.SuccessfulPath;

            Print("Bravo Values:");
            Print("Length: " + floodFillArray_Bravo.FloodFillArray.Length.ToString());
            Print("Best Column Score: " + floodFillArray_Bravo.CurrBestColumnValue);
            Print("Best Column Position: " + floodFillArray_Bravo.CurrBestColumnPosition);
            Print("Successful: " + floodFillArray_Bravo.SuccessfulPath);
            Print("---");
        }

        // Print("PRE Check: Alpha - " + hasAlpha + ", Bravo - " + hasBravo);

        // Decline the position that is higher up on the board
        if (hasAlpha && hasBravo)
        {
            hasAlpha = (floodFillArray_Alpha.CurrBestColumnPosition < floodFillArray_Bravo.CurrBestColumnPosition);
            hasBravo = !hasAlpha;

            //Print("Alpha: " + hasAlpha);
            //Print("Bravo: " + hasBravo);
        }

        // Print("Post Check: Alpha - " + hasAlpha + ", Bravo - " + hasBravo);

        List<int> finalPathfind = new List<int>();
        if(hasAlpha)
        {
            //PrintCurrentList(floodFillArray_Alpha.FloodFillArray);
            finalPathfind = RecordFloodFillPath(floodFillArray_Alpha.FloodFillArray, floodFillArray_Alpha.CurrBestColumnPosition);
        }
        else if(hasBravo)
        {
            finalPathfind = RecordFloodFillPath(floodFillArray_Bravo.FloodFillArray, floodFillArray_Bravo.CurrBestColumnPosition);
        }

        Print("Length: " + finalPathfind.Count.ToString());
        return finalPathfind;
    }

    #region Tests & Checks
    static bool VerticalTest(BoardObject _boardObject)
    {
        // Skip Ghost Columns
        for(int x = 1; x < BoardWidth - 1; x++)
        {
            for (int y = 0; y < BoardHeight; y++)
            {
                BoardObject foundObject = InitialBoard[(y * BoardWidth) + x];

                if (foundObject == _boardObject)
                {
                    y = BoardHeight;

                    continue;
                }

                if( y == (BoardHeight - 1) )
                {
                    return false;
                }
            }
        }

        return true;
    }

    static List<int> GetLeftColumnValidStartPoints(BoardObject boardObject)
    {
        List<int> results = new List<int>();

        for(int y = 0; y < BoardHeight; y++)
        {
            int currPos = (y * BoardWidth) + 1;

            // Move on if not this block type
            if (InitialBoard[currPos] != boardObject)
                continue;

            // Move on if the block to the right is not the same block type
            if (InitialBoard[currPos + 1] != boardObject)
                continue;

            Print("Pos: " + currPos + ", " + (currPos + 1));

            // Position, and position to the right, are valid. Add to list.
            results.Add(currPos);
        }

        return results;
    }

    static List<int> GetRightColumnValidEndPoints(BoardObject boardObject)
    {
        List<int> results = new List<int>();

        for (int y = 0; y < BoardHeight; y++)
        {
            // -2 from board width due to 'Width' starting at 1
            int currPos = (y * BoardWidth) + (BoardWidth - 2);

            Print("Right Column: Positions " + currPos + " & " + (currPos - 1));

            // Move on if not this block type
            if (InitialBoard[currPos] != boardObject)
            {
                continue;
            }

            // Move on if the block to its left is not the same block type
            if (InitialBoard[currPos - 1] != boardObject)
            {
                continue;
            }

            Print("Adding: " + currPos);
            // Position, and position to the left, are valid. Add to the list.
            results.Add(currPos);
        }

        return results;
    }

    static bool FindBestConnectionBoard(BoardObject _boardObjectType, List<int> _columnValidStartPoints, out int[] _outArray, out List<int> _validRightColumnExits)
    {
        int[] outArray = new int[0];
        List<int> validRightColumnExits = new List<int>();


        foreach (int columnValidStartPoint in _columnValidStartPoints)
        {
            int[] tempBest_OutArray = new int[0];
            List<int> tempBest_ValidRightColumnExits = new List<int>();

            if (MakeConnectionsBoard(_boardObjectType, columnValidStartPoint, out tempBest_OutArray, out tempBest_ValidRightColumnExits))
            {
                if (tempBest_ValidRightColumnExits.Count > 0)
                {
                    _outArray = tempBest_OutArray;
                    _validRightColumnExits = tempBest_ValidRightColumnExits;

                    return true;
                }
            }
        }

        _outArray = null;
        _validRightColumnExits = null;

        Print("ERROR - No Connection Board Found");

        return false;
    }

    static bool MakeConnectionsBoard(BoardObject _boardObjectType, int _columnValidStartPoint, out int[] _outArray, out List<int> _validRightColumnExits)
    {
        // Run new logic loop based on *one* Valid Column Start Point (Left side)
        // Instead of filling with numbers based on how many connections there are, just apply a True bool
        // Return a bool if successful, and 'out' an array, and valid Right Column Exit positions

        bool successfulEnd = false;
        _outArray = new int[0];
        _validRightColumnExits = new List<int>();

        // Two-dimensional array to evaluate number of Board connections of same type
        int[] BoardConnectionsArray = new int[BoardWidth * BoardHeight];

        // Quick reference list of 'new' or 'initial' conncetors that currently have 1 branch.
        List<int> CurrentOneConnectors = new List<int>();

        /*
        Print("Incoming Valid Column Starts: " + _columnValidStartPoints.Count);

        for(int i = 0; i < _columnValidStartPoints.Count; i++)
        {
            // Preloads the List with these same positions
            CurrentOneConnectors.Add(_columnValidStartPoints[i]);
        }
        */


        Print("Incoming Valid Column Start: " + _columnValidStartPoint);

        // Preloads the List with these same positions
        CurrentOneConnectors.Add(_columnValidStartPoint);

        bool connectionsFilled = false;

        while(!connectionsFilled)
        {
            int nextPos;
            int startingPos = -1;

            // Take starting position AND remove from CurrentOneConnectors List
            if (CurrentOneConnectors != null && CurrentOneConnectors.Count > 0)
            {
                startingPos = CurrentOneConnectors[0];
                CurrentOneConnectors.RemoveAt(0);
            }
            else
            {
                if (CurrentOneConnectors == null)
                    Print("Is Null");

                if (CurrentOneConnectors.Count <= 0)
                    Print("Count: " + CurrentOneConnectors.Count);

                successfulEnd = false;
                connectionsFilled = true;

                Print("CRASH");

                continue;
            }

            // Reset connections value for evaluation
            int numConnections = 0;

            #region Evaluate all four possible connections, and determine if valid
            
            // Position to the RIGHT
            nextPos = startingPos + 1;

            if (InitialBoard[nextPos] == _boardObjectType)
            {
                // If position to the right happens to be the right-most playable column, there's a valid path
                if (nextPos % BoardWidth == BoardWidth - 2)
                {
                    successfulEnd = true;

                    if(!_validRightColumnExits.Contains(nextPos))
                        _validRightColumnExits.Add(nextPos);

                    continue;
                }

                // Add the position to the right for future valid checks
                if (BoardConnectionsArray[nextPos] == 0)
                    CurrentOneConnectors.Add(nextPos);

                // Increment THIS Board Connection Array position
                numConnections++;
            }

            // Position BELOW
            nextPos = startingPos - BoardWidth;

            if (nextPos > 0)
            {
                // Check if InitialBoard position Below is same type AND empty (ensure valid position)
                if (InitialBoard[nextPos] == _boardObjectType)
                {
                    // Add the position to the right for future valid checks
                    if (BoardConnectionsArray[nextPos] == 0)
                        CurrentOneConnectors.Add(nextPos);

                    // Increment THIS Board Connection Array position
                    numConnections++;
                }
            }

            // Position ABOVE
            nextPos = startingPos + BoardWidth;

            if (nextPos < InitialBoard.Count)
            {
                // Check if InitialBoard position Above is same type AND empty (ensure valid position)
                if (InitialBoard[nextPos] == _boardObjectType)
                {
                    // Add the position to the right for future valid checks
                    if (BoardConnectionsArray[nextPos] == 0)
                        CurrentOneConnectors.Add(nextPos);

                    // Increment THIS Board Connection Array position
                    numConnections++;
                }
            }

            // Position LEFT
            nextPos = startingPos - 1;

            if (nextPos % BoardWidth > 0)
            {
                // Check if InitialBoard position Left is same type AND empty (ensure valid position)
                if (InitialBoard[nextPos] == _boardObjectType)
                {
                    if (nextPos % BoardWidth != 1)
                    {
                        // Add the position to the right for future valid checks
                        if (BoardConnectionsArray[nextPos] == 0)
                            CurrentOneConnectors.Add(nextPos);
                    }

                    // Increment THIS Board Connection Array position
                    numConnections++;
                }
            }

            BoardConnectionsArray[startingPos] = numConnections;

            #endregion

            // If CurrentOneConnectors List is empty, set connectionsFilled to true
            if (CurrentOneConnectors.Count == 0)
                connectionsFilled = true;
        }

        if(successfulEnd)
        {
            // PrintCurrentArray(BoardConnectionsArray);
            _outArray = BoardConnectionsArray;
        }

        return successfulEnd;
    }

    /// <summary>
    /// A wrapper to run through all Reverse-Connection Flood Fill Arrays as the process is performed more than once.
    /// </summary>
    /// <param name="_boardObjectType"></param>
    /// <param name="leftColumnStartPoints"></param>
    static FloodFillArrayObject CycleAllReverseConnectionFloodFillArraysOfType(BoardObject _boardObjectType, List<int> leftColumnStartPoints)
    {
        // Store Array, and current best information for comparison
        FloodFillArrayObject tempFloodFillArrayObject = new FloodFillArrayObject();
        tempFloodFillArrayObject.FloodFillArray = new int[0];
        tempFloodFillArrayObject.CurrBestColumnValue = 999;
        tempFloodFillArrayObject.CurrBestColumnPosition = 0;
        tempFloodFillArrayObject.SuccessfulPath = false;

        int[] connectionArray = new int[BoardWidth * BoardHeight];
        List<int[]> connectionArrayList = new List<int[]>();
        List<int> exitList;

        // Change to run a For Loop through each 'leftColumnStartPoints', which populate new List<int[]> positions
        if (FindBestConnectionBoard(_boardObjectType, leftColumnStartPoints, out connectionArray, out exitList))
        {
            // Run through all alphaExitList positions, and find the lowest left-side column value.
            for (int i = 0; i < exitList.Count; i++)
            {
                // Get 'flower'-style positional floodfill starting from right-hand column
                int[] tempArray = ReverseConnectionsFloodFill(connectionArray, exitList[i]);

                // Get left-side column value and compare against previous best
                for (int j = 0; j < BoardHeight; j++)
                {
                    // Array position to check assigned value
                    int evalPos = (j * BoardWidth) + 1;
                    int valueAtArrayPosition = tempArray[evalPos];

                    // If the value of the array position is positive, and less than the previous best, store this as new best.
                    if (valueAtArrayPosition > 0 && valueAtArrayPosition < tempFloodFillArrayObject.CurrBestColumnValue)
                    {
                        tempFloodFillArrayObject.CurrBestColumnValue = valueAtArrayPosition;
                        tempFloodFillArrayObject.FloodFillArray = tempArray;
                        tempFloodFillArrayObject.CurrBestColumnPosition = evalPos;
                    }

                    // Continue since the value has been evaluated
                    continue;
                }
            }
        }

        if(tempFloodFillArrayObject.CurrBestColumnValue < 999)
            tempFloodFillArrayObject.SuccessfulPath = true;

        return tempFloodFillArrayObject;
    }

    /// <summary>
    /// Takes an Array of BoardObjects that were previously given values of all same-type adjacent connections 
    /// Creates a new 2D Array based on _connectionsArray, starting at the given [x,y] position on the right-hand column,
    /// by Flood-Filling from right-to-left, assigning incrementing values to each *new* connection.
    /// This 2D Array is intended to finally be evaluated later for the optimal path.
    /// </summary>
    /// <param name="_connectionsArray"></param> The Board, as int[], where each populated position is the number of surrounded same-type blocks. 
    /// <param name="_rightColumnExitPos"></param> The given Array position of the Board to begin pathfinding from the right-side column.
    /// <param name="_maxLength"></param> The highest value this path should attempt to go. Anything longer will forcibly break out.
    /// <returns>int</returns> A new Array of value-based pathfinding, with the starting position being the lowest, and the left-column being the highest.
    static int[] ReverseConnectionsFloodFill(int[] _connectionsArray, int _rightColumnExitPos)
    {
        int[] floodFillArray = new int[BoardWidth * BoardHeight];

        List<int> nextBatchList = new List<int>();
        List<int> currentBatchList = new List<int>();

        int counter = 1;

        bool reachedStart = false;

        currentBatchList.Add(_rightColumnExitPos);
        floodFillArray[_rightColumnExitPos] = 1;

        while(!reachedStart)
        {
            // Add 1 to counter
            counter++;

            for(int i = 0; i < currentBatchList.Count; i++)
            {
                // Check Position LEFT
                int nextPos = currentBatchList[i] - 1;

                int evalPos = nextPos % BoardWidth;
                if (evalPos > 0)
                {
                    // Normal position
                    if (_connectionsArray[nextPos] > 0 && floodFillArray[nextPos] == 0)
                    {
                        floodFillArray[nextPos] = counter;

                        nextBatchList.Add(nextPos);

                        if (evalPos == 1)
                        {
                            reachedStart = true;

                            continue;
                        }
                    }
                }

                // Check Position ABOVE
                nextPos = currentBatchList[i] + BoardWidth;
                if(nextPos < InitialBoard.Count)
                {
                    if (_connectionsArray[nextPos] > 0 && floodFillArray[nextPos] == 0)
                    {
                        floodFillArray[nextPos] = counter;

                        nextBatchList.Add(nextPos);
                    }
                }

                // Check Position BELOW
                nextPos = currentBatchList[i] - BoardWidth;
                if(nextPos > 0)
                {
                    if (_connectionsArray[nextPos] > 0 && floodFillArray[nextPos] == 0)
                    {
                        floodFillArray[nextPos] = counter;

                        nextBatchList.Add(nextPos);
                    }
                }

                // Check Position RIGHT
                nextPos = currentBatchList[i] + 1;
                if(nextPos % BoardWidth < BoardWidth - 1)
                {
                    if (_connectionsArray[nextPos] > 0 && floodFillArray[nextPos] == 0)
                    {
                        floodFillArray[nextPos] = counter;

                        nextBatchList.Add(nextPos);
                    }
                }
            }

            if (!reachedStart)
            {
                currentBatchList = nextBatchList;
                nextBatchList = new List<int>();
            }

        }

        // PrintCurrentArray(floodFillArray);
        return floodFillArray;
    }
    
    static List<int> RecordFloodFillPath(int[] _boardReverseFloodFillArray, int _leftColumnStartPos)
    {
        List<int> currPath = new List<int>();
        currPath.Add(_leftColumnStartPos);

        // Current Score
        int currValue = _boardReverseFloodFillArray[_leftColumnStartPos];
        // Print("Starting Score: " + currValue);

        // Current Array Position
        int currPos = _leftColumnStartPos;
        // Print("Starting Column Pos: " + currPos);
        // Print("START Flood Fill Loop");
        
        while(currValue > 2)
        {
            int currArrayPos_ = currPath[currPath.Count - 1];
            int currScore_ = _boardReverseFloodFillArray[currArrayPos_];

            int currentBestNextPosition = currPos;

            /*
            Print("Next Loop: ");
            Print("currArrayPos: " + currArrayPos_);
            Print("currScore: " + currScore_);
            Print("Loop Starting Array Pos: " +  currentBestNextPosition);
            */

            // Starting with the current array position score, check each direction for:
            // If it is > 0, AND
            // If it is lower than the previous directional check.
            // If it fits both, then it's the current new direction.

            #region Init
            int rightScore = 999;
            int downScore = 999;
            int upScore = 999;
            int leftScore = 999;
            
            int rightPos = currArrayPos_ + 1;
            int downPos = currArrayPos_ - BoardWidth;
            int upPos = currArrayPos_ + BoardWidth;
            int leftPos = currArrayPos_ - 1;

            /*
            Print("Comparison Array Positions: " + rightPos + ", " + downPos + ", " + upPos + ", " + leftPos);

            string arrayScoresOutput = "Comparison Array Scores: ";
            if (rightPos < BoardWidth * BoardHeight)
                arrayScoresOutput += _boardReverseFloodFillArray[rightPos];
            else
                arrayScoresOutput += "-1";

            arrayScoresOutput += ", ";

            if (downPos > 0)
                arrayScoresOutput += _boardReverseFloodFillArray[downPos];
            else
                arrayScoresOutput += "-1";
            arrayScoresOutput += ", ";


            if (upPos < BoardHeight * BoardWidth)
                arrayScoresOutput += _boardReverseFloodFillArray[upPos];
            else
                arrayScoresOutput += "-1";

            arrayScoresOutput += ", ";

            if (leftPos > 0)
                arrayScoresOutput += _boardReverseFloodFillArray[leftPos];
            else
                arrayScoresOutput += "-1";
                
            Print(arrayScoresOutput);
            */
            #endregion Init

            #region Compare & Assign Values
            if (rightPos % BoardWidth < BoardWidth)
            {
                int value = _boardReverseFloodFillArray[rightPos];

                if(value > 0)
                {
                    rightScore = value;

                    if(value == 1)
                    {
                        currPath.Add(rightPos);
                        currValue = 1;
                        continue;
                    }
                }

                //Print("Right Score: " + value);
            }

            if(downPos > 0)
            {
                int value = _boardReverseFloodFillArray[downPos];

                if(value > 0)
                    downScore = value;

                //Print("Down Score: " + value);
            }

            if (upPos < (BoardWidth * BoardHeight))
            {
                int value = _boardReverseFloodFillArray[upPos];

                if(value > 0)
                    upScore = _boardReverseFloodFillArray[upPos];

                //Print("Up Score: " + value);
            }

            // Probably unnecessary, but...
            // Just a quick check that the leftPos is within the same row, and a positive value 
            if(leftPos % BoardWidth > 0 && leftPos > 0)
            {
                int value = _boardReverseFloodFillArray[leftPos];

                if (value > 0)
                    leftScore = value;

                //Print("Left Score: " + value);
            }
            #endregion Compare & Assign Values

            #region Compare in preferred order for lowest score
            if (rightScore > 0 && rightScore < currScore_)
            {
                if (rightScore < upScore && rightScore < downScore && rightScore < leftScore)
                {
                    currPath.Add(rightPos);
                    currValue = _boardReverseFloodFillArray[rightPos];

                    // Print("Adding RIGHT: " + rightPos);
                    continue;
                }
            }

            if (downScore > 0 && downScore < currScore_)
            {
                if (downScore < upScore && downScore < leftScore && downScore < rightScore)
                {
                    currPath.Add(downPos);
                    currValue = _boardReverseFloodFillArray[downPos];
                    // Print("Adding DOWN: " + downPos);
                    continue;
                }
            }

            if (upScore > 0 && upScore < currScore_)
            {
                if (upScore < leftScore && upScore < rightScore && upScore < downScore)
                {
                    currPath.Add(upPos);
                    currValue = _boardReverseFloodFillArray[upPos];
                    // Print("Adding UP: " + upPos);
                    continue;
                }
            }

            if(leftScore > 0 && leftScore < currScore_)
            {
                if (leftScore < upScore && leftScore < downScore && leftScore < rightScore)
                {
                    currPath.Add(leftPos);
                    currValue = _boardReverseFloodFillArray[leftPos];
                    // Print("Adding LEFT: " + leftPos);
                    continue;
                }
            }

            #endregion Compare in preferred order for lowest score
        }

        return currPath;
    }
    #endregion Tests & Checks

    static void Print(string _output)
    {
        GameObject gameLogic = GameObject.Find("GameLogic");
        GameLogic gameLogicScr = gameLogic.GetComponent<GameLogic>();

        gameLogicScr.PF_OutputTest(_output);
    }
    static void PrintCurrentArray(int[] _boardConnectionsArray)
    {
        GameObject gameLogic = GameObject.Find("GameLogic");
        GameLogic gameLogicScr = gameLogic.GetComponent<GameLogic>();

        gameLogicScr.PF_OutputTest("-----");
        gameLogicScr.PF_OutputTest("SOLUTION BELOW");
        gameLogicScr.PF_OutputTest("-----");

        for (int y = BoardHeight - 1; y > -1; y--)
        {
            string output = "";

            for (int x = 0; x < BoardWidth; x++)
            {
                output += "[";
                output += _boardConnectionsArray[(y * BoardWidth) + x];
                output += "]";
            }

            // gameLogicScr.PF_OutputTest(output);
            Print(output);
        }

        gameLogicScr.PF_OutputTest("-----");
        gameLogicScr.PF_OutputTest("SOLUTION ABOVE");
        gameLogicScr.PF_OutputTest("-----");
    }

    static void PrintCurrentList(List<int> _boardConnectionsList)
    {
        int[] tempArray = new int[_boardConnectionsList.Count];

        foreach(int i in _boardConnectionsList)
            tempArray[i] = _boardConnectionsList[i];

        PrintCurrentArray(tempArray);
    }

    #region Old Pathfinding

    static IEnumerator HardDropPathfindLoop()
    {
        bool continuePathfindLoop = true;

        // print("HARD DROP PATHFIND LOOP");

        // Reset the RepeatScorelineEvalLength
        RepeatScorelineEvalLength = 999;

        // SetGamePlayingState(false);

        while (continuePathfindLoop)
        {
            // HardDrop();

            // *IF* I choose to implement mid-field Ghost Blocks, this won't
            // work prior to Pathfinding, since the mid-field Ghost Blocks
            // won't allow scoring before being cleared.
            // ResetGhostBlocks();

            // Current longest Alpha / Bravo length.
            int maxLinePossibility = (BoardHeight - 2) / 2;
            maxLinePossibility *= (BoardWidth - 4);
            maxLinePossibility += 2;
            maxLinePossibility += ((BoardHeight - 2) / 2) - 1;

            if (RepeatScorelineEvalLength < maxLinePossibility)
            {
                maxLinePossibility = RepeatScorelineEvalLength;
                maxLinePossibility += 2;
                // print("RUNNING SHORTER PATHING: " + maxLinePossibility);
            }

            BeginPathfinding(maxLinePossibility);

            continuePathfindLoop = FoundScoreline;

            yield return new WaitForSecondsRealtime(0.25f);
        }

        /*
        BlockSize nextBlockSize = NextBlockListSize[0];
        List<BoardObject> nextBlock = GetNextBlock(true);
        PlaceNewSquircleGroupOfType(nextBlockSize, nextBlock);
        */

        // SetGamePlayingState(true);

        /*
        if (BugTestConsoleOutput)
        {
            print("-----------");
            print("-----------");
            print("-----------");
            Console_PrintBoard();
        }
        */

        yield return true;
    }

    public static void BeginPathfinding( int repeatScorelineEvalLength )
    {
        bool alphaExists = true;
        bool bravoExists = true;

        AlphaPathfindList = new List<PathBoardObject>();
        BravoPathfindList = new List<PathBoardObject>();

        CurrentAlpha = repeatScorelineEvalLength;
        CurrentBravo = repeatScorelineEvalLength;

        // Number of currently running Alpha / Bravo threads.
        AlphaThreads = 0;
        BravoThreads = 0;

        // Pre-load before running next phase
        FoundScoreline = false;

        // Most efficient Alpha / Bravo lists
        SuccessfulPathfindList_Alpha = new List<PathBoardObject>();
        SuccessfulPathfindList_Bravo = new List<PathBoardObject>();



        // Run horizontally to see if Static Alpha/Bravo pieces exist in at least each column
        // TODO: THIS WILL NOT WORK Going forward. 'x < BoardWidth - 1' does not resolve properly for the right wall,
        // because HardDrop needs to reset ghost blocks so the VertValidationCheck can properly evaluate the right wall.
        /// for (int x = 0; x < BoardWidth; x++)
        for (int x = 0; x < BoardWidth - 1; x++)
        {
            if (alphaExists)
            {
                // Idea: Grab each column '1' Alpha position and add to AlphaPathfindList?
                // Reset if !tempAlpha?

                // Run through the column looking for Alpha_Static
                bool tempAlpha = VerticalValidationCheck(x, BoardObject.Alpha_Static);

                // Didn't find an appropriate piece. Don't continue searching for Static Alpha pieces.
                if (!tempAlpha)
                {
                    // Sets to False without kicking out of loop to check for Bravo
                    alphaExists = false;
                }
                // Only want to apply the following data if in the left playable column, AND we found a tempAlpha
                else if (x == 1)
                {
                    string test = "Alpha: ";
                    for (int num = 0; num < tempVertXPositions.Count; num++)
                    {
                        test += tempVertXPositions[num].ToString() + ", ";

                        PathBoardObject tempPathingBoardObject = new PathBoardObject(new Vector2Int(x, tempVertXPositions[num]), false, true, false, false);

                        // Adds the (1, yPos) vector position to the Pathfind list, which will run the coroutine down below
                        AlphaPathfindList.Add(tempPathingBoardObject);
                    }

                    /*
                    if (BugTestConsoleOutput)
                        print(test);
                    */
                }
            }

            if (bravoExists)
            {
                // Idea: Grab each column '1' Bravo position and add to BravoPathfindList?
                // Reset if !tempBravo?

                // Run through the column looking for Bravo_Static
                bool tempBravo = VerticalValidationCheck(x, BoardObject.Bravo_Static);

                if (!tempBravo)
                {
                    bravoExists = false;
                }
                else if (x == 1)
                {
                    string test = "Bravo: ";
                    for (int num = 0; num < tempVertXPositions.Count; num++)
                    {
                        test += tempVertXPositions[num].ToString() + ", ";

                        PathBoardObject tempPathingBoardObject = new PathBoardObject(new Vector2Int(x, tempVertXPositions[num]), false, true, false, false);

                        // Adds the (1, yPos) vector position to the Pathfind list, which will run the coroutine down below
                        BravoPathfindList.Add(tempPathingBoardObject);
                    }

                    /*
                    if (BugTestConsoleOutput)
                        print(test);
                    */
                }
            }
        }

        /*
        if (BugTestConsoleOutput)
        {
            print("--------------------");
            print("Alpha Vertical Test: " + alphaExists);
            print("Bravo Vertical Test: " + bravoExists);
            print("--------------------");
        }
        */


        // This *MUST* be run before moving to the PreloadPathfindBlock section
        if (alphaExists)
        {
            for (int i = 0; i < AlphaPathfindList.Count; i++)
                ThreadCounter(BoardObject.Alpha_Static, true);
        }
        if (bravoExists)
        {
            for (int j = 0; j < BravoPathfindList.Count; j++)
                ThreadCounter(BoardObject.Bravo_Static, true);
        }


        if (alphaExists)
        {
            for (int x = 0; x < AlphaPathfindList.Count; x++)
            {
                PreloadPathfindBlock(BoardObject.Alpha_Static, AlphaPathfindList[x]);
            }
        }

        if (bravoExists)
        {
            for (int x = 0; x < BravoPathfindList.Count; x++)
            {
                PreloadPathfindBlock(BoardObject.Bravo_Static, BravoPathfindList[x]);
            }
        }

        if (!alphaExists && !bravoExists)
        {
            // SetGamePlayingState(true);
        }
    }

    /*
    public static void PreloadPathfindBlock(BoardObject boardObjectType, PathBoardObject startBlock)
    {
        // Using start position & boardObjectType, preload a new List and begin the loop process
        List<PathBoardObject> pathfindList = new List<PathBoardObject>();

        PathBoardObject compareBlock = new PathBoardObject();
        compareBlock = startBlock;

        // If checking to the left && left position is Left Valid Column, don't check it.
        if (startBlock.LeftValid && startBlock.Position.x - 1 <= HORIZ_LEFT_WALL_XPos_Playable)
            compareBlock.LeftValid = false;

        if (startBlock.UpValid && startBlock.Position.y >= BoardHeight)
            compareBlock.DownValid = false;

        if (startBlock.DownValid && startBlock.Position.y == 0)
            compareBlock.DownValid = false;

        if (startBlock.Position.x == HORIZ_LEFT_WALL_XPos_Playable)
        {
            compareBlock.DownValid = false;
            compareBlock.UpValid = false;
        }

        pathfindList.Add(compareBlock);

        StartCoroutine(PathfindLogic(boardObjectType, pathfindList));
    }
    */

    /*
    static IEnumerator PathfindLogic(BoardObject boardObjectType, List<PathBoardObject> pathfindList)
    {
        ///
        /// Run a 'While True' loop through the logic system. Break out when a successful path is found, OR no possible paths exist.
        /// 

        bool shouldContinue = true;

        /// START WHILE TRUE
        while (shouldContinue)
        {
            // In case a shorter path has already been found, this path is not good enough. End.
            if (pathfindList.Count > CheckBestPathfindList(boardObjectType))
            {
                if (BugTestConsoleOutput)
                    print("Path isn't short enough. Closing it off.");

                ThreadCounter(boardObjectType, false);
                shouldContinue = false;
                continue;
            }

            List<PathBoardObject> validBoardObjects = new List<PathBoardObject>();

            PathBoardObject tempBlock = pathfindList[pathfindList.Count - 1];
            BoardObject evaluationBlock;
            Vector2Int nextPos = tempBlock.Position;

            // Create temporary positional list using pathfindList in order to compare through for already existing position
            List<Vector2Int> arrayPositionsList = new List<Vector2Int>();
            for (int i = 0; i < pathfindList.Count; i++)
            {
                arrayPositionsList.Add(pathfindList[i].Position);
            }

            ///
            /// Begin comparing all four directions (where appropriate)
            ///



            if (tempBlock.RightValid)
            {
                if (nextPos.x < HORIZ_RIGHT_WALL_XPos_Sidewall)
                {
                    // Evaluate based on the position to the right
                    ++nextPos.x;

                    evaluationBlock = GetBoardObjectAtPosition(nextPos);

                    // Compares this block to the one passed into the function
                    if (evaluationBlock != boardObjectType)
                    {
                        tempBlock.RightValid = false;
                    }

                    // Run check that the block being evaluated doesn't already exist in the list, AND ensures the 'Right Valid' value wasn't changed above
                    // (This is run second under the understanding that .Contains() is expensive, and should not be run if necessary)
                    if (arrayPositionsList.Contains(nextPos) && tempBlock.RightValid)
                    {
                        tempBlock.RightValid = false;
                    }

                    if (tempBlock.RightValid)
                    {
                        validBoardObjects.Add(new PathBoardObject(nextPos, false, true, true, true));

                        // If this block to the right is valid AND is along the right-hand side of the board, SUCCESS
                        if (nextPos.x == HORIZ_RIGHT_WALL_XPos_Playable)
                        {
                            // Ensure that, when all Threads in the Thread Counter have finished, we progress to ScoreLineLogic
                            FoundScoreline = true;

                            // pathfindList
                            validBoardObjects = pathfindList;
                            validBoardObjects.Add(new PathBoardObject(nextPos, false, false, false, false));

                            SaveSuccessfulPathing(boardObjectType, validBoardObjects);
                            ThreadCounter(boardObjectType, false);
                            shouldContinue = false;
                            continue;
                        }
                    }
                }
                else tempBlock.RightValid = false;
            }

            // Resets comparison
            nextPos = pathfindList[pathfindList.Count - 1].Position;


            if (tempBlock.DownValid)
            {
                if (nextPos.y > 0)
                {
                    // Evaluate based on the position below
                    --nextPos.y;

                    evaluationBlock = GetBoardObjectAtPosition(nextPos);

                    // Compares this block to the one passed into the function
                    if (evaluationBlock != boardObjectType)
                    {
                        tempBlock.DownValid = false;
                    }

                    // Run check that the block being evaluated doesn't already exist in the list, AND ensures the 'Down Valid' value wasn't changed above
                    // (This is run second under the understanding that .Contains() is expensive, and should not be run if necessary)
                    if (arrayPositionsList.Contains(nextPos) && tempBlock.DownValid)
                    {
                        tempBlock.DownValid = false;
                    }

                    if (tempBlock.DownValid)
                    {
                        validBoardObjects.Add(new PathBoardObject(nextPos, true, true, false, true));
                    }
                }
                else tempBlock.DownValid = false;
            }

            // Resets comparison
            nextPos = pathfindList[pathfindList.Count - 1].Position;

            if (tempBlock.UpValid)
            {
                if (nextPos.y < BoardHeight)
                {
                    // Evaluate based on the position above
                    ++nextPos.y;

                    evaluationBlock = GetBoardObjectAtPosition(nextPos);

                    // Compares this block to the one passed into the function
                    if (evaluationBlock != boardObjectType)
                    {
                        tempBlock.UpValid = false;
                    }

                    // Run check that the block being evaluated doesn't already exist in the list, AND ensures the 'Up Valid' value wasn't changed above
                    // (This is run second under the understanding that .Contains() is expensive, and should not be run if necessary)
                    if (arrayPositionsList.Contains(nextPos) && tempBlock.UpValid)
                    {
                        tempBlock.UpValid = false;
                    }

                    if (tempBlock.UpValid)
                    {
                        validBoardObjects.Add(new PathBoardObject(nextPos, true, true, true, false));
                    }
                }
                else tempBlock.UpValid = false;
            }

            // Resets comparison
            nextPos = pathfindList[pathfindList.Count - 1].Position;


            if (tempBlock.LeftValid)
            {
                if (nextPos.x > 0)
                {
                    // Evaluate based on the position to the left
                    --nextPos.x;

                    evaluationBlock = GetBoardObjectAtPosition(nextPos);

                    // Compares this block to the one passed into the function
                    if (evaluationBlock != boardObjectType)
                    {
                        tempBlock.LeftValid = false;
                    }

                    // Run check that the block being evaluated doesn't already exist in the list, AND ensures the 'Left Valid' value wasn't changed above
                    // (This is run second under the understanding that .Contains() is expensive, and should not be run if necessary)
                    if (arrayPositionsList.Contains(nextPos) && tempBlock.LeftValid)
                    {
                        tempBlock.LeftValid = false;
                    }

                    if (tempBlock.LeftValid)
                    {
                        validBoardObjects.Add(new PathBoardObject(nextPos, true, false, true, true));
                    }
                }
                else tempBlock.LeftValid = false;
            }

            if (BugTestConsoleOutput)
            {
                print("Valid Positions remaining: " + validBoardObjects.Count);

                if (validBoardObjects.Count > 0)
                {
                    print("Valid Positions: ");
                    foreach (PathBoardObject boardObject in validBoardObjects) { print(boardObject.Position); }
                }
            }

            if (validBoardObjects.Count != 0)
            {
                if (validBoardObjects.Count > 1)
                {
                    ThreadCounter(boardObjectType, true);

                    // Duplicate thread FIRST, add position[1] in list, and begin new thread
                    if (BugTestConsoleOutput)
                        print("Adding " + validBoardObjects[1].Position + " position to list, AND duplicating " + (validBoardObjects.Count - 1) + " PathfindingLists for thread");

                    List<PathBoardObject> firstNewThread = new List<PathBoardObject>();

                    // This was necessary because the 'original' thread was still being accessed (with the direction change IT had), while ALSO adding the new direction.
                    // This resolves that.
                    for (int i = 0; i < pathfindList.Count; i++)
                        firstNewThread.Add(pathfindList[i]);

                    firstNewThread.Add(validBoardObjects[1]);

                    if (BugTestConsoleOutput)
                    {
                        print("THREAD '1'");
                        PrintAllPositionsInList(firstNewThread);
                    }

                    StartCoroutine(PathfindLogic(boardObjectType, firstNewThread));

                    if (validBoardObjects.Count == 3)
                    {
                        ThreadCounter(boardObjectType, true);

                        if (BugTestConsoleOutput)
                            print("Adding " + validBoardObjects[2].Position + " position to list, AND duplicating " + (validBoardObjects.Count - 1) + " PathfindingLists for thread");

                        List<PathBoardObject> secondNewThread = new List<PathBoardObject>();

                        // This was necessary because the 'original' thread was still being accessed (with the direction change IT had), while ALSO adding the new direction.
                        // This resolves that.
                        for (int i = 0; i < pathfindList.Count; i++)
                            secondNewThread.Add(pathfindList[i]);

                        secondNewThread.Add(validBoardObjects[2]);

                        if (BugTestConsoleOutput)
                        {
                            PrintAllPositionsInList(secondNewThread);
                        }

                        StartCoroutine(PathfindLogic(boardObjectType, secondNewThread));
                    }
                }

                // Default for the first valid new block position
                if (BugTestConsoleOutput)
                {
                    print("Adding " + validBoardObjects[0].Position + " position to list. Continuing this thread. Length: " + pathfindList.Count);
                }

                pathfindList.Add(validBoardObjects[0]);

                if (BugTestConsoleOutput)
                {
                    PrintAllPositionsInList(pathfindList);
                }

            }
            else
            {
                ThreadCounter(boardObjectType, false);

                // Ends thread.
                shouldContinue = false;
            }
        }

        yield return false;
    }
    */

    /*
    static bool VerticalValidationCheck(int _x, BoardObject _boardObject)
    {
        BoardObject tempObject;
        bool validColumn = false;
        tempVertXPositions = new List<int>();

        numEachColumn_Alpha = 0;
        numEachColumn_Bravo = 0;

        // Run vertically. If a static (or Sidewall) exists, continue
        for (int y = 0; y < BoardHeight; y++)
        {
            // If we haven't found a successful BoardObject yet, continue the check
            tempObject = GetBoardObjectAtPosition(_x, y);

            // If on the far sides of the board, AND is a Sidewall, keep searching
            if (_x == 0 || _x == BoardWidth - 1)
            {
                if (tempObject == BoardObject.Ghost || tempObject == _boardObject)
                {
                    validColumn = true;

                    // Force exit to next column to check
                    y = BoardHeight;
                }
            }
            // All other normal board positions. Check accordingly.
            else
            {
                if (tempObject == _boardObject)
                {
                    validColumn = true;

                    if (_x != 1)
                    {
                        // Force exit to next column to check
                        y = BoardHeight;

                        if (_boardObject == BoardObject.Alpha_Static)
                            numEachColumn_Alpha++;
                    }
                    else
                    {
                        // If the 1st column, get all valid vertical positions (not just the first one) to populate into Pathfinding Check.
                        tempVertXPositions.Add(y);

                        // TODO: Only add 1 to each Alpha / Bravo count for the sake of future evaluation



                        // NOTE: During pathfinding, compare blocks y+1 & y-1 in x == 1 coordinate when x+1 is NOT valid (Basically, check to see if L shape start happens, and remove the possibility)
                        // 
                        // [X] [_] [_] <- Remove from Pathfinding
                        // [X] [X] [X]
                        // [X] [_] [_] <- Remove from Pathfinding
                    }
                }
            }
        }

        return validColumn;
    }
    */

    /*
    static void SaveSuccessfulPathing(BoardObject boardObjectType, List<PathBoardObject> pathfindList)
    {
        if (BugTestConsoleOutput)
            print("Saving Successful Pathing: " + boardObjectType.ToString());

        if (boardObjectType == BoardObject.Alpha_Static)
        {
            if (pathfindList.Count < CurrentAlpha)
            {
                SuccessfulPathfindList_Alpha = pathfindList;
                CurrentAlpha = SuccessfulPathfindList_Alpha.Count;
            }
        }
        else if (boardObjectType == BoardObject.Bravo_Static)
        {
            if (pathfindList.Count < CurrentBravo)
            {
                SuccessfulPathfindList_Bravo = pathfindList;
                CurrentBravo = SuccessfulPathfindList_Bravo.Count;
            }
        }

        if (BugTestConsoleOutput)
        {
            string output = "Saved: " + boardObjectType.ToString() + ": ";
            for (int i = 0; i < pathfindList.Count; i++)
            {
                output += pathfindList[i].Position.ToString() + ", ";
            }
            print(output);
        }
    }
    */

    /*
    static int CheckBestPathfindList(BoardObject boardObjectType)
    {
        int returnNum = 99;

        if (boardObjectType == BoardObject.Alpha_Static)
            returnNum = CurrentAlpha;
        else if (boardObjectType == BoardObject.Bravo_Static)
            returnNum = CurrentBravo;

        return returnNum;
    }
    */

    /*
    static void ThreadCounter(BoardObject boardObjectType, bool increment)
    {
        if (boardObjectType == BoardObject.Alpha_Static)
        {
            if (increment)
                AlphaThreads++;
            else AlphaThreads--;

            if (BugTestConsoleOutput)
                print("Thread Counter: " + boardObjectType.ToString() + " has " + AlphaThreads + " remaining");
        }
        else if (boardObjectType == BoardObject.Bravo_Static)
        {
            if (increment)
                BravoThreads++;
            else BravoThreads--;

            if (BugTestConsoleOutput)
                print("Thread Counter: " + boardObjectType.ToString() + " has " + BravoThreads + " remaining");
        }

        if (AlphaThreads == 0 && BravoThreads == 0)
        {
            // FoundScoreLine is enabled when 1+ successful lines have been found.
            if (FoundScoreline)
            {
                ScoreLineLogic();

                SetGamePlayingState(true);
            }
        }
    }
    */

    /*
    static void ScoreLineLogic()
    {
        // Still determining if I want to score the longer of 2+ lines, or the one closer to the bottom.
        // Logic exists for the 2+ lines, but gonna prioritize the closest to the bottom for now.

        int alphaLine_YPos = -1;
        int bravoLine_YPos = -1;

        if (SuccessfulPathfindList_Alpha.Count > 0)
            alphaLine_YPos = AlphaPathfindList[AlphaPathfindList.Count - 1].Position.y;

        if (SuccessfulPathfindList_Bravo.Count > 0)
            bravoLine_YPos = SuccessfulPathfindList_Bravo[SuccessfulPathfindList_Bravo.Count - 1].Position.y;

        List<PathBoardObject> ChosenPathfindList = SuccessfulPathfindList_Alpha;

        // If a scoreline for each type exist, pick the one closer to the bottom.
        if (bravoLine_YPos != -1)
        {
            if (bravoLine_YPos < alphaLine_YPos || alphaLine_YPos == -1)
                ChosenPathfindList = SuccessfulPathfindList_Bravo;
        }

        for (int i = 0; i < ChosenPathfindList.Count; i++)
        {
            Vector2Int _pos = ChosenPathfindList[i].Position;

            if (BugTestConsoleOutput)
                print("Clearing: " + _pos);

            SetBoardObjectAtPosition(_pos, BoardObject.Empty);
            BoardLogicScript.DestroySquircleAtGridPos(_pos);
        }

        // In case a second evaluation is run, this count is used to limit Scoreline Check
        // Hopefully stops infinite loops
        RepeatScorelineEvalLength = ChosenPathfindList.Count;
    }
    */

    #endregion Old Pathfinding

    #region TEMP OVERRIDE
    static void ThreadCounter(BoardObject boardObject, bool value)
    {
        // TODO: FIX
    }

    static void PreloadPathfindBlock(BoardObject boardObject, PathBoardObject startBlock)
    {
        // TODO: FIX
    }


    static bool VerticalValidationCheck(int _x, BoardObject _boardObject)
    {
        // TODO: FIX

        return false;
    }
    #endregion
}
