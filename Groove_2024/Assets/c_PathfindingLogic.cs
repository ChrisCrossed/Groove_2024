using System.Collections.Generic;
using UnityEngine;

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

    public static List<int> StartPathfindingLogic(List<BoardObject> _board, int _boardWidth)
    {
        // Print("Here");
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

            if (LeftColumnStartPoints_Alpha.Count == 0 || RightColumnEndPoints_Alpha.Count == 0)
            {
                hasAlpha = false;
            }
        }

        if (hasBravo)
        {
            LeftColumnStartPoints_Bravo = GetLeftColumnValidStartPoints(BoardObject.Bravo_Static);
            RightColumnEndPoints_Bravo = GetRightColumnValidEndPoints(BoardObject.Bravo_Static);

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
        }

        // Store Bravo Array, and current best information for comparison
        FloodFillArrayObject floodFillArray_Bravo = new FloodFillArrayObject();

        if (hasBravo)
        {
            floodFillArray_Bravo = CycleAllReverseConnectionFloodFillArraysOfType(BoardObject.Bravo_Static, LeftColumnStartPoints_Bravo);

            hasBravo = floodFillArray_Bravo.SuccessfulPath;
        }

        // Decline the position that is higher up on the board
        if (hasAlpha && hasBravo)
        {
            hasAlpha = (floodFillArray_Alpha.CurrBestColumnPosition < floodFillArray_Bravo.CurrBestColumnPosition);
            hasBravo = !hasAlpha;
        }

        List<int> finalPathfind = new List<int>();
        if(hasAlpha)
        {
            finalPathfind = RecordFloodFillPath(floodFillArray_Alpha.FloodFillArray, floodFillArray_Alpha.CurrBestColumnPosition);
        }
        else if(hasBravo)
        {
            finalPathfind = RecordFloodFillPath(floodFillArray_Bravo.FloodFillArray, floodFillArray_Bravo.CurrBestColumnPosition);
        }

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

            // Print("Right Column: Positions " + currPos + " & " + (currPos - 1));

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

            // Print("Adding: " + currPos);
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
            if (MakeConnectionsBoard(_boardObjectType, columnValidStartPoint, out outArray, out validRightColumnExits))
            {
                if (validRightColumnExits.Count > 0)
                {
                    _outArray = outArray;
                    _validRightColumnExits = validRightColumnExits;

                    return true;
                }
            }
        }

        _outArray = null;
        _validRightColumnExits = null;

        // Print("ERROR - No Connection Board Found");

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
                if (BoardConnectionsArray[CurrentOneConnectors[0]] > 0)
                {
                    // Print("Already performed this position");
                    CurrentOneConnectors.RemoveAt(0);
                    continue;
                }

                startingPos = CurrentOneConnectors[0];
                CurrentOneConnectors.RemoveAt(0);
            }
            else
            {
                // Why did I apply this override? It's conflicting with successful pathing.
                // successfulEnd = false;
                connectionsFilled = true;

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

                    // continue;
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
        
        // Current Array Position
        int currPos = _leftColumnStartPos;
        
        while(currValue > 1)
        {
            int currArrayPos_ = currPath[currPath.Count - 1];
            int currScore_ = _boardReverseFloodFillArray[currArrayPos_];

            int currentBestNextPosition = currPos;

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
            }

            if(downPos > 0)
            {
                int value = _boardReverseFloodFillArray[downPos];

                if(value > 0)
                    downScore = value;
            }

            if (upPos < (BoardWidth * BoardHeight))
            {
                int value = _boardReverseFloodFillArray[upPos];

                if(value > 0)
                    upScore = _boardReverseFloodFillArray[upPos];
            }

            // Probably unnecessary, but...
            // Just a quick check that the leftPos is within the same row, and a positive value 
            if(leftPos % BoardWidth > 0 && leftPos > 0)
            {
                int value = _boardReverseFloodFillArray[leftPos];

                if (value > 0)
                    leftScore = value;
            }
            #endregion Compare & Assign Values

            #region Compare in preferred order for lowest score
            // Due to how I want to prioritize Right / Down / Up / Left order, that is the rule for tiebreakers below (<= instead of <)
            if (rightScore > 0 && rightScore < currScore_)
            {
                if (rightScore <= upScore && rightScore <= downScore && rightScore <= leftScore)
                {
                    currPath.Add(rightPos);
                    currValue = _boardReverseFloodFillArray[rightPos];

                    continue;
                }
            }

            if (downScore > 0 && downScore < currScore_)
            {
                if (downScore <= upScore && downScore <= leftScore && downScore < rightScore)
                {
                    currPath.Add(downPos);
                    currValue = _boardReverseFloodFillArray[downPos];
                    continue;
                }
            }

            if (upScore > 0 && upScore < currScore_)
            {
                if (upScore <= leftScore && upScore < rightScore && upScore < downScore)
                {
                    currPath.Add(upPos);
                    currValue = _boardReverseFloodFillArray[upPos];
                    continue;
                }
            }

            if(leftScore > 0 && leftScore < currScore_)
            {
                if (leftScore < upScore && leftScore < downScore && leftScore < rightScore)
                {
                    currPath.Add(leftPos);
                    currValue = _boardReverseFloodFillArray[leftPos];
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
}
