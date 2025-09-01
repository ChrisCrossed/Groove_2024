using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;

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

    public static void StartPathfindingLogic(List<BoardObject> _board, int _boardWidth)
    {
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

            if (LeftColumnStartPoints_Alpha == new List<int>() && RightColumnEndPoints_Alpha == new List<int>())
            {
                hasAlpha = false;
            }
        }

        if (hasBravo)
        {
            LeftColumnStartPoints_Bravo = GetLeftColumnValidStartPoints(BoardObject.Bravo_Static);
            RightColumnEndPoints_Bravo = GetRightColumnValidEndPoints(BoardObject.Bravo_Static);

            if (LeftColumnStartPoints_Bravo == new List<int>() && RightColumnEndPoints_Bravo == new List<int>())
            {
                hasBravo = false;
            }
        }
        #endregion Preload Left Column Start Position

        // Just kick out if no proper paths can exist
        if (!hasAlpha && !hasBravo)
            return;

        if(hasAlpha)
        {
            MakeConnectionsBoard(BoardObject.Alpha_Static, LeftColumnStartPoints_Alpha);
        }

        if(hasBravo)
        {
            MakeConnectionsBoard(BoardObject.Bravo_Static, LeftColumnStartPoints_Bravo);
        }
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

            // Position, and position to the left, are valid. Add to the list.
            results.Add(currPos);
        }

        return results;
    }

    static void MakeConnectionsBoard(BoardObject _boardObjectType, List<int> _columnValidStartPoints)
    {
        // Two-dimensional array to evaluate number of Board connections of same type
        int[] BoardConnectionsArray = new int[BoardWidth * BoardHeight];

        // Quick reference list of 'new' or 'initial' conncetors that currently have 1 branch.
        List<int> CurrentOneConnectors = new List<int>();

        for(int i = 0; i < _columnValidStartPoints.Count; i++)
        {
            // Preloads the left column with '1' connection (even though we know one to the right exists)
            BoardConnectionsArray[_columnValidStartPoints[i]] = 1;

            // Preloads the List with these same positions
            CurrentOneConnectors.Add(_columnValidStartPoints[i]);
        }

        bool connectionsFilled = false;

        while(!connectionsFilled)
        {
            bool foundNewConnection = false;
            int nextPos;

            // Take starting position AND remove from CurrentOneConnectors List
            int startingPos = CurrentOneConnectors[0];
            CurrentOneConnectors.RemoveAt(0);

            // Position to the RIGHT
            nextPos = startingPos + 1;

            // Check if InitialBoard position to the Right is same type AND empty (Because it wasn't checked yet)
            if (InitialBoard[nextPos] == _boardObjectType && BoardConnectionsArray[nextPos] == 0)
            {
                foundNewConnection = true;

                // If it is, put a '1' in BoardConnectionsArray spot
                BoardConnectionsArray[nextPos] = 1;

                // Add position to CurrentOneConnectors List
                CurrentOneConnectors.Add(nextPos);

                // If starting position is NOT left playable column, ADD 1 to starting position in BoardConnectionsArray spot
                if(startingPos % BoardWidth != 1)
                    BoardConnectionsArray[startingPos]++;
            }


            // If starting position is left playable column, do not check Up / Down / Left
            if (startingPos % BoardWidth != 1)
            {
                // Potential 'below' position
                nextPos = startingPos - BoardWidth;
                if(nextPos > 0)
                {
                    // Check if InitialBoard position Below is same type AND empty (ensure valid position)
                    if(InitialBoard[nextPos] == _boardObjectType && BoardConnectionsArray[nextPos] == 0)
                    {
                        foundNewConnection = true;

                        // If it is, put a '1' in BoardConnectionsArray spot
                        BoardConnectionsArray[nextPos] = 1;

                        // Add position to CurrentOneConnectors List
                        CurrentOneConnectors.Add(nextPos);

                        // Add 1 to starting position in BoardConnectionsArray spot
                        BoardConnectionsArray[startingPos]++;
                    }
                }

                // Potential 'above' position
                nextPos = startingPos + BoardWidth;
                if(nextPos < InitialBoard.Count)
                {
                    // Check if InitialBoard position Above is same type AND empty (ensure valid position)
                    if (InitialBoard[nextPos] == _boardObjectType && BoardConnectionsArray[nextPos] == 0)
                    {
                        foundNewConnection = true;

                        // If it is, put a '1' in BoardConnectionsArray spot
                        BoardConnectionsArray[nextPos] = 1;

                        // Add position to CurrentOneConnectors List
                        CurrentOneConnectors.Add(nextPos);

                        // Add 1 to starting position in BoardConnectionsArray spot
                        BoardConnectionsArray[startingPos]++;
                    }
                }

                // Potential 'left' position
                nextPos = startingPos - 1;

                // Needs to not be Ghost column
                if (nextPos % BoardWidth > 0)
                {
                    // Check if InitialBoard position Left is same type AND empty (ensure valid position)
                    if (InitialBoard[nextPos] == _boardObjectType && BoardConnectionsArray[nextPos] == 0)
                    {
                        foundNewConnection = true;

                        // Error catch to ensure left playable column doesn't get wrong data
                        if( nextPos % BoardWidth != 1 )
                        {
                            // If valid, put a '1' in BoardConnectionsArray spot
                            BoardConnectionsArray[nextPos] = 1;

                            // Add position to CurrentOneConnectors List
                            CurrentOneConnectors.Add(nextPos);
                        }

                        // Add 1 to starting position in BoardConnectionsArray spot
                        BoardConnectionsArray[startingPos]++;
                    }
                }
            }

            

            // If foundNewConnection = false, reduce all potential nearby connections by 1
            if(!foundNewConnection)
            {
                // Reset current position in BoardConnectionsArray spot to 0
                // Check each valid direction surrounding starting position of same type, and reduce them by 1
            }

            // If CurrentOneConnectors List is empty, set connectionsFilled to true
        }
    }
    #endregion Tests & Checks

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
