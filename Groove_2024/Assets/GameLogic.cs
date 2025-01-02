using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum BoardObject
{
    Empty, // Can become Alpha or Bravo
    Alpha_Static = 1,
    Bravo_Static = 2,
    Alpha_Active = 3,
    Bravo_Active = 4,
    Filled, // Forcibly-filled board piece. 'Cement'
    Sidewall // Edge of Boardwall. Resets at the end of each turn.
}

public enum TileType
{
    TwoByTwo,
    ThreeWide,
    ThreeTall
}

public enum PathfindDirection
{
    Right,
    Up,
    Down,
    Left,
    None
}

public class GameLogic : MonoBehaviour
{
    [SerializeField, Range(5, 20)]
    int BoardWidth_Maximum;
    [SerializeField, Range(5, 20)]
    int BoardHeight_Maximum;

    const int HORIZ_LEFT_WALL_XPos_Playable = 1;
    const int HORIZ_LEFT_WALL_XPos_Sidewall = 0;
    int HORIZ_RIGHT_WALL_XPos_Playable;
    int HORIZ_RIGHT_WALL_XPos_Sidewall;

    int BoardWidth;
    int BoardHeight;
    Vector2Int TileBottomLeftPosition;

    List<BoardObject> Board;

    // Start is called before the first frame update
    void Start()
    {
        Init_Board();

        TEST_PresetBoard();

        Console_PrintBoard();

        // StartCoroutine(BeginPathfinding());
        BeginPathfinding();
    }

    void Init_Board()
    {
        // Extend width of board by 2 to include the Sidewalls
        BoardWidth = BoardWidth_Maximum + 2;
        BoardHeight = BoardHeight_Maximum;

        // Ex: 10 width pre-defined turns into 12 width including Sidewalls.
        // 10 width == 0 -> 11 for all spaces. 0 & 11 are Sidewall. 1 & 10 are Playable.
        HORIZ_RIGHT_WALL_XPos_Playable = BoardWidth_Maximum;
        HORIZ_RIGHT_WALL_XPos_Sidewall = HORIZ_RIGHT_WALL_XPos_Playable + 1;

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
    }

    void TEST_PresetBoard()
    {
        // X = Alpha
        // O = Bravo

        TileBottomLeftPosition = new Vector2Int(3, 3);

        // CreateNewBlockOfType(TileType.ThreeTall, TileBottomLeftPosition);
        for (int x = 0; x < BoardWidth; x++)
        {
            if (GetBoardObjectAtPosition(x, 2) != BoardObject.Sidewall)
            {
                SetBoardObjectAtPosition(x, 2, BoardObject.Alpha_Static);
            }

            if(GetBoardObjectAtPosition(x, 4) != BoardObject.Sidewall)
            {
                SetBoardObjectAtPosition(x, 4, BoardObject.Bravo_Static);
            }
        }

        SetBoardObjectAtPosition(1, 5, BoardObject.Bravo_Static);
        SetBoardObjectAtPosition(1, 3, BoardObject.Bravo_Static);

        SetBoardObjectAtPosition(1, 7, BoardObject.Alpha_Static);
        SetBoardObjectAtPosition(2, 7, BoardObject.Alpha_Static);
        SetBoardObjectAtPosition(4, 7, BoardObject.Alpha_Static);

        SetBoardObjectAtPosition(2, 3, BoardObject.Alpha_Static);

        SetBoardObjectAtPosition(5, 5, BoardObject.Bravo_Static);
        SetBoardObjectAtPosition(5, 6, BoardObject.Bravo_Static);
        SetBoardObjectAtPosition(5, 7, BoardObject.Bravo_Static);
        SetBoardObjectAtPosition(6, 7, BoardObject.Bravo_Static);
        SetBoardObjectAtPosition(7, 7, BoardObject.Bravo_Static);
        SetBoardObjectAtPosition(8, 7, BoardObject.Bravo_Static);
        SetBoardObjectAtPosition(8, 6, BoardObject.Bravo_Static);
        SetBoardObjectAtPosition(8, 5, BoardObject.Bravo_Static);

        // SetBoardObjectAtPosition(0, 2, BoardObject.Empty);
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

    #region Pathfinding Actions
    void BeginPathfinding()
    {
        bool alphaExists = true;
        bool bravoExists = true;

        AlphaPathfindList = new List<PathBoardObject>();
        BravoPathfindList = new List<PathBoardObject>();

        print("--------------------");

        #region Vertical Tests
        // Run horizontally to see if Static Alpha/Bravo pieces exist in at least each column
        for (int x = 0; x < BoardWidth; x++)
        {
            if(alphaExists)
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
                else if(x == 1)
                {
                    string test = "Alpha: ";
                    for(int num = 0; num < tempVertXPositions.Count; num++)
                    {
                        test += tempVertXPositions[num].ToString() + ", ";

                        PathBoardObject tempPathingBoardObject = new PathBoardObject(new Vector2Int(x, tempVertXPositions[num]), false, true, false, false);

                        // Adds the (1, yPos) vector position to the Pathfind list, which will run the coroutine down below
                        AlphaPathfindList.Add(tempPathingBoardObject);
                    }
                    print(test);
                }
            }

            if(bravoExists)
            {
                // Idea: Grab each column '1' Bravo position and add to BravoPathfindList?
                // Reset if !tempBravo?

                // Run through the column looking for Bravo_Static
                bool tempBravo = VerticalValidationCheck(x, BoardObject.Bravo_Static);
                
                if (!tempBravo)
                {
                    bravoExists = false;
                }
                else if( x == 1 )
                {
                    string test = "Bravo: ";
                    for(int num = 0; num < tempVertXPositions.Count; num++)
                    {
                        test += tempVertXPositions[num].ToString() + ", ";

                        PathBoardObject tempPathingBoardObject = new PathBoardObject(new Vector2Int(x, tempVertXPositions[num]), false, true, false, false);

                        // Adds the (1, yPos) vector position to the Pathfind list, which will run the coroutine down below
                        BravoPathfindList.Add(tempPathingBoardObject);
                    }
                    print(test);
                }
            }
        }
        #endregion

        print("Alpha Vertical Test: " + alphaExists);
        print("Bravo Vertical Test: " + bravoExists);
        print("--------------------");

        #region Horizontal Tests
        if (alphaExists)
        {
            for(int x = 0; x < AlphaPathfindList.Count; x++)
            {
                // StartCoroutine( PathfindLogic(BoardObject.Alpha_Static, AlphaPathfindList[x]) );
                PreloadPathfindBlock(BoardObject.Alpha_Static, AlphaPathfindList[x]);
            }
        }

        if(bravoExists)
        {
            for(int x = 0; x < BravoPathfindList.Count; x++)
            {
                // StartCoroutine( PathfindLogic(BoardObject.Bravo_Static, BravoPathfindList[x]) );
                PreloadPathfindBlock(BoardObject.Bravo_Static, BravoPathfindList[x]);
            }
        }


        #endregion
    }

    void PreloadPathfindBlock(BoardObject boardObjectType, PathBoardObject startBlock)
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

    List<PathBoardObject> AlphaPathfindList;
    List<PathBoardObject> BravoPathfindList;
    IEnumerator PathfindLogic(BoardObject boardObjectType, List<PathBoardObject> pathfindList)
    {
        ///
        /// Run a 'While True' loop through the logic system. Break out when a successful path is found, OR no possible paths exist.
        /// 

        print(boardObjectType + " @ " + pathfindList[pathfindList.Count - 1].Position + ": ");
        print("Left: " + pathfindList[pathfindList.Count - 1].LeftValid);
        print("Right: " + pathfindList[pathfindList.Count - 1].RightValid);
        print("Up: " + pathfindList[pathfindList.Count - 1].UpValid);
        print("Down: " + pathfindList[pathfindList.Count - 1].DownValid);
        print("Check Done\n-------------------------");

        bool shouldContinue = true;

        /// START WHILE TRUE
        while(shouldContinue)
        {
            List<PathBoardObject> validBoardObjects = new List<PathBoardObject>();

            PathBoardObject tempBlock = pathfindList[pathfindList.Count - 1];
            BoardObject evaluationBlock;
            Vector2Int nextPos = tempBlock.Position;

            // Create temporary positional list using pathfindList in order to compare through for already existing position
            List<Vector2Int> arrayPositionsList = new List<Vector2Int>();
            for(int i = 0; i < pathfindList.Count; i++)
            {
                arrayPositionsList.Add(pathfindList[i].Position);
            }

            ///
            /// Begin comparing all four directions (where appropriate)
            ///


            if (tempBlock.RightValid)
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
                    validBoardObjects.Add( new PathBoardObject(nextPos, false, true, true, true) );
                }
            }

            // Resets comparison
            nextPos = pathfindList[pathfindList.Count - 1].Position;


            if (tempBlock.DownValid)
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

            // Resets comparison
            nextPos = pathfindList[pathfindList.Count - 1].Position;


            if (tempBlock.UpValid)
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

            // Resets comparison
            nextPos = pathfindList[pathfindList.Count - 1].Position;


            if (tempBlock.LeftValid)
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

                if (tempBlock.DownValid)
                {
                    validBoardObjects.Add(new PathBoardObject(nextPos, true, true, false, true));
                }
            }

            print("Valid Positions remaining: " + validBoardObjects.Count);

            if(validBoardObjects.Count != 0)
            {
                if(validBoardObjects.Count > 1)
                {
                    // Duplicate thread FIRST, add position[1] in list, and begin new thread
                    print("Adding " + validBoardObjects[1].Position + " position to list, AND duplicating " + (validBoardObjects.Count - 1) + " PathfindingLists for thread");
                    List<PathBoardObject> firstNewThread = new List<PathBoardObject>();
                    firstNewThread = pathfindList;
                    firstNewThread.Add(validBoardObjects[1]);
                    StartCoroutine(PathfindLogic(boardObjectType, firstNewThread));

                    if(validBoardObjects.Count == 3)
                    {
                        print("Adding " + validBoardObjects[2].Position + " position to list, AND duplicating " + (validBoardObjects.Count - 1) + " PathfindingLists for thread");
                        List<PathBoardObject> secondNewThread = new List<PathBoardObject>();
                        secondNewThread = pathfindList;
                        secondNewThread.Add(validBoardObjects[2]);
                        StartCoroutine(PathfindLogic(boardObjectType, secondNewThread));
                    }
                }

                // Default for the first valid new block position
                print("Adding " + validBoardObjects[0].Position + " position to list. Continuing this thread.");
                pathfindList.Add(validBoardObjects[0]);

            }
            else
            {
                // Ends thread.
                shouldContinue = false;
            }
        }

        yield return false;

        /*
        #region While True Loop
        Vector2Int nextPos = pathfindList[pathfindList.Count - 1].Position;
        BoardObject bo_BlockType;

        // Previous position list to check against
        Vector2Int[] previousPositions = new Vector2Int[pathfindList.Count];
        for (int i = 0; i < pathfindList.Count; i++)
        {
            previousPositions[i] = pathfindList[i].Position;
        }
        int validPositions = 0;

        ///
        /// Right, Down, Up, Left
        /// 
        if (startBlock.RightValid)
        {
            // Evaluate based on the position to the right
            ++nextPos.x;

            bo_BlockType = GetBoardObjectAtPosition(nextPos);

            // Run check that the block being evaluated doesn't already exist in the list
            if (previousPositions.Contains(nextPos))
            {
                startBlock.RightValid = false;
            }

            if (bo_BlockType != boardObjectType && startBlock.RightValid)
            {
                startBlock.RightValid = false;
            }

            if (startBlock.RightValid) validPositions++;
        }

        nextPos = pathfindList[pathfindList.Count - 1].Position;


        if (startBlock.DownValid)
        {
            // Evaluate based on the position below
            --nextPos.y;

            bo_BlockType = GetBoardObjectAtPosition(nextPos);

            // Run check that the block being evaluated doesn't already exist in the list
            if (previousPositions.Contains(nextPos))
            {
                startBlock.DownValid = false;
            }

            // Run check if the next block is a valid comparison AND ensure we should keep running checks
            if (bo_BlockType != boardObjectType && startBlock.DownValid)
            {
                startBlock.DownValid = false;
            }

            if (startBlock.DownValid) validPositions++;
        }

        nextPos = pathfindList[pathfindList.Count - 1].Position;


        if (startBlock.UpValid)
        {
            // Evaluate based on the position above
            ++nextPos.y;

            bo_BlockType = GetBoardObjectAtPosition(nextPos);

            // Run check that the block being evaluated doesn't already exist in the list
            if (previousPositions.Contains(nextPos))
            {
                startBlock.UpValid = false;
            }

            // Run check if the next block is a valid comparison AND ensure we should keep running checks
            if (bo_BlockType != boardObjectType && startBlock.UpValid)
            {
                startBlock.UpValid = false;
            }

            if (startBlock.UpValid) validPositions++;
        }

        nextPos = pathfindList[pathfindList.Count - 1].Position;


        if (startBlock.LeftValid)
        {
            // Evaluate based on the position to the left
            --nextPos.x;

            bo_BlockType = GetBoardObjectAtPosition(nextPos);

            // Run check that the block being evaluated doesn't already exist in the list
            if (previousPositions.Contains(nextPos))
            {
                startBlock.LeftValid = false;
            }

            // Run check if the next block is a valid comparison AND ensure we should keep running checks
            if (bo_BlockType != boardObjectType && startBlock.LeftValid)
            {
                startBlock.LeftValid = false;
            }

            if (startBlock.DownValid) validPositions++;
        }

        print("Valid Positions remaining: " + validPositions);

        // If NO new valid positions exist, terminate this thread
        if (validPositions == 0)
        {
            print("ENDING THREAD - " + boardObjectType + " starting at " + startBlock);
            yield return false;
        }
        else
        {
            ///
            /// Compare remaining valid positions.
            /// If 2+ exist, Duplicate the pathfind list by 'Valid Positions - 1'
            /// Add each valid position to each currently existing pathfind list.
            /// Run new threads on any NEW pathfinding lists
            /// 

            if (validPositions > 1)
            {

            }
        }
        #endregion

        /// END WHILE TRUE

        */

        yield return true;
    }


    /// <summary>
    /// Used to determine if every column has at least one valid piece
    /// </summary>
    /// <param name="_boardObject">The piece to compare against for validity</param>
    /// <returns></returns>
    List<int> tempVertXPositions;
    bool VerticalValidationCheck(int _x, BoardObject _boardObject)
    {
        BoardObject tempObject;
        bool validColumn = false;
        tempVertXPositions = new List<int>();

        // Run vertically. If a static (or Sidewall) exists, continue
        for (int y = 0; y < BoardHeight; y++)
        {
            // If we haven't found a successful BoardObject yet, continue the check
            tempObject = GetBoardObjectAtPosition(_x, y);

            // If on the far sides of the board, AND is a Sidewall, keep searching
            if (_x == 0 || _x == BoardWidth - 1)
            {
                if (tempObject == BoardObject.Sidewall || tempObject == _boardObject)
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

                    if(_x != 1)
                    {
                        // Force exit to next column to check
                        y = BoardHeight;
                    }
                    else
                    {
                        // If the 1st column, get all valid vertical positions (not just the first one) to populate into Pathfinding Check.
                        tempVertXPositions.Add(y);
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
    #endregion

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Creates a new 'Active' block of random tiles, starting at the Bottom Left coordinate given.
    /// </summary>
    /// <param name="_type">Applies a block varying in height and width.</param>
    /// <param name="_position">The bottom left coordinate for the block to spawn</param>
    void CreateNewBlockOfType(TileType _type, Vector2Int _position)
    {
        Vector2Int boardPos = _position;
        int blockHeight = 2;
        int blockWidth = 2;
        if (_type == TileType.ThreeWide)
            blockWidth = 3;
        else if (_type == TileType.ThreeTall)
            blockHeight = 3;

        for(int y = 0; y < blockHeight; y++)
        {
            for(int x = 0; x < blockWidth; x++)
            {
                BoardObject randomBlock = DetermineRandomBlock(true);
                SetBoardObjectAtPosition(blockWidth + x, blockHeight + y, randomBlock);
                print("[" + (blockWidth + x) + "," + (blockHeight + y) + "]: " + randomBlock);
            }
        }
    }

    /// <summary>
    /// Get a randomly-given Alpha or Bravo type block.
    /// </summary>
    /// <param name="isActive">'True' returns the block as 'Active' state, rather than 'Static'</param>
    /// <returns></returns>
    BoardObject DetermineRandomBlock(bool isActive = true)
    {
        BoardObject boardObject = BoardObject.Alpha_Static;

        if (Random.Range(0, 1f) > 0.5f)
        {
            boardObject = BoardObject.Bravo_Static;
        }

        if(isActive)
        {
            // Converts Static type to Active type
            boardObject += 2;
        }

        return boardObject;
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
    BoardObject SetBoardObjectAtPosition(Vector2Int _position, BoardObject _boardObject)
    {
        return SetBoardObjectAtPosition(_position.x, _position.y, _boardObject);
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

    BoardObject GetBoardObjectAtPosition(Vector2Int v2_Position)
    {
        return GetBoardObjectAtPosition(v2_Position.x, v2_Position.y);
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
                    textLine += "[  ]";
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
