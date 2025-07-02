using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public enum BoardObject
{
    Empty = 0, // Can become Alpha or Bravo
    Alpha_Static = 1,
    Bravo_Static = 2,
    Alpha_Active = 11,
    Bravo_Active = 12,
    Filled = 20, // Forcibly-filled board piece. 'Cement'
    Filled_Alpha = 21, // Forcibly kept as 'Alpha'
    Filled_Bravo = 22, // Forcibly kept as 'Bravo'
    Ghost = 30 // Edge of Boardwall. Resets at the end of each turn.
}

public enum BlockSize
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
    [SerializeField]
    bool BugTestConsoleOutput = false;

    [SerializeField, Range(6, 20)]
    int BoardWidth_Maximum;
    [SerializeField, Range(6, 20)]
    int BoardHeight_Maximum;

    [SerializeField]
    bool BlockObject_Active_TwoByTwo;
    [SerializeField]
    bool BlockObject_Active_ThreeWide;
    [SerializeField]
    bool BlockObject_Active_ThreeTall;
    Vector2Int TileBottomLeftPosition;
    BlockSize CurrBlockSize;

    const int HORIZ_LEFT_WALL_XPos_Playable = 1;
    const int HORIZ_LEFT_WALL_XPos_Sidewall = 0;
    int HORIZ_RIGHT_WALL_XPos_Playable;
    int HORIZ_RIGHT_WALL_XPos_Sidewall;

    int BoardWidth;
    int BoardHeight;

    List<BoardObject> Board;

    GameObject GO_BoardArray;
    c_BoardLogic BoardLogicScript;

    #region Initialization
    // Start is called before the first frame update
    void Start()
    {
        Init_Random();

        Init_Board();

        SetValidActiveBlockTypes(BlockObject_Active_ThreeWide, BlockObject_Active_ThreeTall, BlockObject_Active_TwoByTwo);

        PopulateNextFourBlocksList();

        SetGamePlayingState(true);

        

        BlockSize nextBlockSize = NextBlockListSize[0];
        List<BoardObject> nextBlock = GetNextBlock(true);
        PlaceNewSquircleGroupOfType(nextBlockSize, nextBlock);

        HardDropPathfindLoop();

        if(BugTestConsoleOutput)
        {
            Console_PrintBoard();
        }
    }

    int PreviousRandomSeed;
    void Init_Random()
    {
        // Getting an initial seed with 6 digits. Could make 8 digits later if desired. Arbitrary.
        PreviousRandomSeed = UnityEngine.Random.Range(100000, 999999);
        SetRandomSeed(PreviousRandomSeed);
    }

    void Init_Board()
    {
        GO_BoardArray = GameObject.Find("BoardArray").gameObject;
        BoardLogicScript = GO_BoardArray.GetComponent<c_BoardLogic>();

        ClearGhostBlockList();

        NextBlockList = new List<List<BoardObject>>();

        NextBlockListSize = new List<BlockSize>();

        // Sets whether 3 wide and 3 tall Active blocks are allowed.
        // Technically calls it's own values, but is safe.
        SetValidActiveBlockTypes(BlockObject_Active_ThreeWide, BlockObject_Active_ThreeTall, BlockObject_Active_TwoByTwo);

        // Extend width of board by 2 to include the Sidewalls
        if (BoardWidth_Maximum % 2 == 1)
            BoardWidth_Maximum += 1;

        BoardWidth = BoardWidth_Maximum + 2;
        BoardHeight = BoardHeight_Maximum;

        // Ex: 10 width pre-defined turns into 12 width including Sidewalls.
        // 10 width == 0 -> 11 for all spaces. 0 & 11 are Sidewall. 1 & 10 are Playable.
        HORIZ_RIGHT_WALL_XPos_Playable = BoardWidth - 2;
        HORIZ_RIGHT_WALL_XPos_Sidewall = HORIZ_RIGHT_WALL_XPos_Playable + 1;

        Board = new List<BoardObject>();

        // Horizontal
        for(int y = 0; y < BoardHeight; y++)
        {
            for(int x = 0; x < BoardWidth; x++)
            {
                // If the K value is 0 (left side) or BoardWidth - 1 (right side), add as Sidewall
                // Otherwise, add as Empty

                BoardObject tempBoardObject = BoardObject.Empty;

                // Needs to be created into the Board before manipulated into being a Ghost Block (If applicable)
                Board.Add(tempBoardObject);

                if (x == HORIZ_LEFT_WALL_XPos_Sidewall || x == HORIZ_RIGHT_WALL_XPos_Sidewall)
                {
                    SetGhostBlock(x, y);
                }
            }
        }

        BoardLogicScript.Init_BoardLogic();

        // Set Default SoftDrop Timer Thread
        SetSoftDropWaitTime(5.0f);
        // StartCoroutine(SoftDropTimer());
    }

    void TEST_PresetBoard()
    {
        // X = Alpha
        // O = Bravo

        SetBoardObjectAtPosition(1, 1, BoardObject.Alpha_Active);
        BoardLogicScript.AddSquircleToBoard(new Vector2Int(1, 1), BoardObject.Alpha_Active);

        for(int y = 0; y < BoardHeight - 3; y++)
        {
            for (int x = 3; x < BoardWidth - 3; x++)
            {
                if(y % 3 == 1)
                {
                    if (GetBoardObjectAtPosition(x, y) != BoardObject.Ghost)
                    {
                        SetBoardObjectAtPosition(x, y, BoardObject.Alpha_Active);
                        BoardLogicScript.AddSquircleToBoard(new Vector2Int(x, y), BoardObject.Alpha_Active);
                    }
                }
                else
                {
                    if (GetBoardObjectAtPosition(x, y) != BoardObject.Ghost)
                    {
                        SetBoardObjectAtPosition(x, y, BoardObject.Bravo_Active);
                        BoardLogicScript.AddSquircleToBoard(new Vector2Int(x, y), BoardObject.Bravo_Active);
                    }
                }
            }
        }

        SetBoardObjectAtPosition(1, 0, BoardObject.Bravo_Active);
        BoardLogicScript.AddSquircleToBoard(new Vector2Int(1, 0), BoardObject.Bravo_Active);
        SetBoardObjectAtPosition(2, 0, BoardObject.Bravo_Active);
        BoardLogicScript.AddSquircleToBoard(new Vector2Int(2, 0), BoardObject.Bravo_Active);

        SetBoardObjectAtPosition(2, 1, BoardObject.Alpha_Active);
        BoardLogicScript.AddSquircleToBoard(new Vector2Int(2, 1), BoardObject.Alpha_Active);

        SetBoardObjectAtPosition(1, 2, BoardObject.Bravo_Active);
        BoardLogicScript.AddSquircleToBoard(new Vector2Int(1, 2), BoardObject.Bravo_Active);
        SetBoardObjectAtPosition(2, 2, BoardObject.Bravo_Active);
        BoardLogicScript.AddSquircleToBoard(new Vector2Int(2, 2), BoardObject.Bravo_Active);
        SetBoardObjectAtPosition(1, 3, BoardObject.Bravo_Active);
        BoardLogicScript.AddSquircleToBoard(new Vector2Int(1, 3), BoardObject.Bravo_Active);
        SetBoardObjectAtPosition(2, 3, BoardObject.Bravo_Active);
        BoardLogicScript.AddSquircleToBoard(new Vector2Int(2, 3), BoardObject.Bravo_Active);

        SetBoardObjectAtPosition(2, 4, BoardObject.Alpha_Active);
        BoardLogicScript.AddSquircleToBoard(new Vector2Int(2, 4), BoardObject.Alpha_Active);
        SetBoardObjectAtPosition(2, 5, BoardObject.Alpha_Active);
        BoardLogicScript.AddSquircleToBoard(new Vector2Int(2, 5), BoardObject.Alpha_Active);
        SetBoardObjectAtPosition(2, 6, BoardObject.Alpha_Active);
        BoardLogicScript.AddSquircleToBoard(new Vector2Int(2, 6), BoardObject.Alpha_Active);
        SetBoardObjectAtPosition(2, 7, BoardObject.Alpha_Active);
        BoardLogicScript.AddSquircleToBoard(new Vector2Int(2, 7), BoardObject.Alpha_Active);

        SetBoardObjectAtPosition(13, 6, BoardObject.Bravo_Active);
        BoardLogicScript.AddSquircleToBoard(new Vector2Int(13, 6), BoardObject.Bravo_Active);
        SetBoardObjectAtPosition(13, 5, BoardObject.Bravo_Active);
        BoardLogicScript.AddSquircleToBoard(new Vector2Int(13, 5), BoardObject.Bravo_Active);

        SetBoardObjectAtPosition(14, 0, BoardObject.Alpha_Active);
        BoardLogicScript.AddSquircleToBoard(new Vector2Int(14, 0), BoardObject.Alpha_Active);

        for (int y = 1; y < 7; y++)
        {
            SetBoardObjectAtPosition(14, y, BoardObject.Bravo_Active);
            BoardLogicScript.AddSquircleToBoard(new Vector2Int(14, y), BoardObject.Bravo_Active);
        }

        SetBoardObjectAtPosition(1, 4, BoardObject.Bravo_Active);
        BoardLogicScript.AddSquircleToBoard(new Vector2Int(1, 4), BoardObject.Bravo_Active);
        SetBoardObjectAtPosition(1, 5, BoardObject.Bravo_Active);
        BoardLogicScript.AddSquircleToBoard(new Vector2Int(1, 5), BoardObject.Bravo_Active);
        SetBoardObjectAtPosition(1, 6, BoardObject.Bravo_Active);
        BoardLogicScript.AddSquircleToBoard(new Vector2Int(1, 6), BoardObject.Bravo_Active);
        SetBoardObjectAtPosition(1, 7, BoardObject.Bravo_Active);
        BoardLogicScript.AddSquircleToBoard(new Vector2Int(1, 7), BoardObject.Bravo_Active);

        SetBoardObjectAtPosition(13, 0, BoardObject.Bravo_Active);
        BoardLogicScript.AddSquircleToBoard(new Vector2Int(13, 0), BoardObject.Bravo_Active);

        SetBoardObjectAtPosition(13, 1, BoardObject.Alpha_Active);
        BoardLogicScript.AddSquircleToBoard(new Vector2Int(13, 1), BoardObject.Alpha_Active);
        SetBoardObjectAtPosition(13, 2, BoardObject.Alpha_Active);
        BoardLogicScript.AddSquircleToBoard(new Vector2Int(13, 2), BoardObject.Alpha_Active);
        SetBoardObjectAtPosition(13, 3, BoardObject.Alpha_Active);
        BoardLogicScript.AddSquircleToBoard(new Vector2Int(13, 3), BoardObject.Alpha_Active);
        SetBoardObjectAtPosition(13, 4, BoardObject.Alpha_Active);
        BoardLogicScript.AddSquircleToBoard(new Vector2Int(13, 4), BoardObject.Alpha_Active);
        /*
        */


        /*
        for(int y = 0; y < BoardHeight - 3; y++)
        {
            // CreateNewBlockOfType(TileType.ThreeTall, TileBottomLeftPosition);
            for (int x = 0; x < BoardWidth; x++)
            {
                BoardObject boardObject = new BoardObject();
                boardObject = BoardObject.Alpha_Active;

                if (y % 2 == 0)
                {
                    boardObject = BoardObject.Bravo_Active;
                }
                else
                {
                    if(x % 2 == 0)
                    {
                        boardObject = BoardObject.Bravo_Active;
                    }
                }

                if (GetBoardObjectAtPosition(x, y) != BoardObject.Ghost)
                {
                    SetBoardObjectAtPosition(x, y, boardObject);
                    BoardLogicScript.AddSquircleToBoard(new Vector2Int(x, y), boardObject);
                }
            }
        }
        */
    }


    void SetRandomSeed(string seed_)
    {
        seed_.ToUpper();

        if(int.TryParse(seed_, out int newSeed_))
        {
            SetRandomSeed(newSeed_);
        }
    }

    void SetRandomSeed(int seed_)
    {
        UnityEngine.Random.InitState(seed_);
    }

    #endregion Initialization

    #region Block Placement
    List<BoardObject> GetNextBlock(bool RemoveFromList = false)
    {
        // NextBlockList
        // NextBlockListSize

        // get/store the series of blocks in position 0 of the list
        // If 'RemoveFromList' is true, clear position 0 from BOTH Lists & run the function to populate the list
        // Return the list 

        List<BoardObject> nextBlocks = new List<BoardObject>();
        for(int i = 0; i < NextBlockList[0].Count; i++)
        {
            nextBlocks.Add(NextBlockList[0][i]);
            
            if(BugTestConsoleOutput)
            {
                print("Block: " + NextBlockList[0][i].ToString());

            }
        }

        if(RemoveFromList)
        {
            NextBlockList.RemoveAt(0);
            NextBlockListSize.RemoveAt(0);
            PopulateNextFourBlocksList();
        }

        return nextBlocks;
    }

    void SetValidActiveBlockTypes(bool threeWide_, bool threeTall_, bool twoByTwo_ = true)
    {
        BlockObject_Active_TwoByTwo = twoByTwo_;
        BlockObject_Active_ThreeWide = threeWide_;
        BlockObject_Active_ThreeTall = threeTall_;

        if(!threeWide_ && !threeTall_)
        {
            BlockObject_Active_TwoByTwo = true;
        }
    }

    /// <summary>
    /// Creates a new 'Active' block of random tiles, starting at the Bottom Left coordinate given.
    /// </summary>
    /// <param name="_size">Applies a block varying in height and width.</param>
    /// <param name="_position">The bottom left coordinate for the block to spawn</param>
    void PlaceNewSquircleGroupOfType(BlockSize _size, List<BoardObject> _blockArray)
    {
        // Find position to begin placing blocks
        Vector2Int boardPos = new Vector2Int();
        int blockHeight = 2;
        int blockWidth = 2;
        int blockCounter = 0;

        boardPos.x = (int)(HORIZ_RIGHT_WALL_XPos_Sidewall / 2f);
        boardPos.y = (int)(BoardHeight - 2f);

        if (_size == BlockSize.ThreeWide)
        {
            boardPos.x--;
            blockWidth = 3;
        }
        else if (_size == BlockSize.ThreeTall)
        {
            boardPos.y--;
            blockHeight = 3;
        }

        // Left to right, bottom to top, place each block from the Block Array.
        for( int y = boardPos.y; y < boardPos.y + blockHeight; y++ )
        {
            for( int x = boardPos.x; x < boardPos.x + blockWidth; x++ )
            {
                BoardObject tempBlock = GetBoardObjectAtPosition(x, y);

                if (tempBlock == BoardObject.Empty)
                {
                    SetBoardObjectAtPosition( x, y, _blockArray[blockCounter] );

                    if(BugTestConsoleOutput)
                    {
                        print("Placing " + _blockArray[blockCounter] + " at position: " + x + ", " + y);
                    }
                    
                    BoardLogicScript.AddSquircleToBoard(new Vector2Int(x, y), _blockArray[blockCounter]);

                    blockCounter++;
                }
                else
                {
                    print("GAME OVER");
                }
            }
        }

        TileBottomLeftPosition = boardPos;
        CurrBlockSize = _size;
    }

    List<List<BoardObject>> NextBlockList;
    List<BlockSize> NextBlockListSize;
    void PopulateNextFourBlocksList()
    {
        for(int i = NextBlockListSize.Count; i < 4; i++)
        {
            // PUSH a new block size to the end of the Lists
            List<BlockSize> _blockTypes = new List<BlockSize>();

            if (BlockObject_Active_TwoByTwo)
                _blockTypes.Add(BlockSize.TwoByTwo);

            if (BlockObject_Active_ThreeWide)
                _blockTypes.Add(BlockSize.ThreeWide);

            if (BlockObject_Active_ThreeTall)
                _blockTypes.Add(BlockSize.ThreeTall);

            int randBlockSize = UnityEngine.Random.Range(0, _blockTypes.Count);

            NextBlockListSize.Add(_blockTypes[randBlockSize]);

            NextBlockList.Add(new List<BoardObject>());

            // PUSH the new block type added to the List
            int numBlocks = 4;
            if (NextBlockListSize[NextBlockListSize.Count - 1] == BlockSize.ThreeWide || NextBlockListSize[NextBlockListSize.Count - 1] == BlockSize.ThreeTall)
                numBlocks = 6;

            for(int j = 0; j < numBlocks; j++)
            {
                BoardObject randomBlock = DetermineRandomIndividualBlock(true);

                NextBlockList[NextBlockList.Count - 1].Add(randomBlock);
            }
        }
    }


    /// <summary>
    /// Get a randomly-given Alpha or Bravo type block.
    /// </summary>
    /// <param name="isActive">'True' returns the block as 'Active' state, rather than 'Static'</param>
    /// <returns></returns>
    BoardObject DetermineRandomIndividualBlock(bool isActive = true)
    {
        BoardObject boardObject = BoardObject.Alpha_Static;

        if (UnityEngine.Random.Range(0, 1f) > 0.5f)
        {
            boardObject = BoardObject.Bravo_Static;
        }

        if (isActive)
        {
            // Converts Static type to Active type
            if (boardObject == BoardObject.Alpha_Static)
                boardObject = BoardObject.Alpha_Active;
            else
                boardObject = BoardObject.Bravo_Active;
        }

        return boardObject;
    }


    #endregion Block Placement

    #region Pathfinding Logic

    IEnumerator HardDropPathfindLoop()
    {
        bool continuePathfindLoop = true;

        print("HARD DROP PATHFIND LOOP");

        SetGamePlayingState(false);

        while (continuePathfindLoop)
        {
            HardDrop();

            // *IF* I choose to implement mid-field Ghost Blocks, this won't
            // work prior to Pathfinding, since the mid-field Ghost Blocks
            // won't allow scoring before being cleared.
            ResetGhostBlocks();

            BeginPathfinding();

            continuePathfindLoop = FoundScoreline;

            yield return new WaitForSecondsRealtime(0.25f);
        }

        BlockSize nextBlockSize = NextBlockListSize[0];
        List<BoardObject> nextBlock = GetNextBlock(true);
        PlaceNewSquircleGroupOfType(nextBlockSize, nextBlock);

        SetGamePlayingState(true);

        if (BugTestConsoleOutput)
        {
            print("-----------");
            print("-----------");
            print("-----------");
            Console_PrintBoard();
        }

        yield return true;
    }

    void BeginPathfinding()
    {
        bool alphaExists = true;
        bool bravoExists = true;

        AlphaPathfindList = new List<PathBoardObject>();
        BravoPathfindList = new List<PathBoardObject>();

        // Current longest Alpha / Bravo length. Set to 999 so all discovered lines are shorter.
        CurrentAlpha = 999;
        CurrentBravo = 999;

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

                    if (BugTestConsoleOutput)
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

                    if (BugTestConsoleOutput)
                        print(test);
                }
            }
        }

        if(BugTestConsoleOutput)
        {
            print("--------------------");
            print("Alpha Vertical Test: " + alphaExists);
            print("Bravo Vertical Test: " + bravoExists);
            print("--------------------");
        }
        

        // This *MUST* be run before moving to the PreloadPathfindBlock section
        if(alphaExists)
        {
            for (int i = 0; i < AlphaPathfindList.Count; i++)
                ThreadCounter(BoardObject.Alpha_Static, true);
        }
        if(bravoExists)
        {
            for (int j = 0; j < BravoPathfindList.Count; j++)
                ThreadCounter(BoardObject.Bravo_Static, true);
        }
        

        if (alphaExists)
        {
            for(int x = 0; x < AlphaPathfindList.Count; x++)
            {
                PreloadPathfindBlock(BoardObject.Alpha_Static, AlphaPathfindList[x]);
            }
        }

        if(bravoExists)
        {
            for(int x = 0; x < BravoPathfindList.Count; x++)
            {
                PreloadPathfindBlock(BoardObject.Bravo_Static, BravoPathfindList[x]);
            }
        }

        if(!alphaExists && !bravoExists)
        {
            SetGamePlayingState(true);
        }
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

        bool shouldContinue = true;

        /// START WHILE TRUE
        while(shouldContinue)
        {
            // In case a shorter path has already been found, this path is not good enough. End.
            if(pathfindList.Count > CheckBestPathfindList(boardObjectType))
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
            for(int i = 0; i < pathfindList.Count; i++)
            {
                arrayPositionsList.Add(pathfindList[i].Position);
            }

            ///
            /// Begin comparing all four directions (where appropriate)
            ///

            

            if (tempBlock.RightValid)
            {
                if(nextPos.x < HORIZ_RIGHT_WALL_XPos_Sidewall)
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
                if(nextPos.y > 0)
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
                if(nextPos.x > 0)
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

                if(validBoardObjects.Count > 0)
                {
                    print("Valid Positions: ");
                    foreach (PathBoardObject boardObject in validBoardObjects) { print(boardObject.Position); }
                }
            }
            
            if(validBoardObjects.Count != 0)
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
                    for(int i = 0; i < pathfindList.Count; i++)
                        firstNewThread.Add(pathfindList[i]);

                    firstNewThread.Add(validBoardObjects[1]);

                    if (BugTestConsoleOutput)
                    {
                        print("THREAD '1'");
                        PrintAllPositionsInList(firstNewThread);
                    }
                    
                    StartCoroutine(PathfindLogic(boardObjectType, firstNewThread));

                    if(validBoardObjects.Count == 3)
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

    List<PathBoardObject> SuccessfulPathfindList_Alpha;
    List<PathBoardObject> SuccessfulPathfindList_Bravo;
    void SaveSuccessfulPathing(BoardObject boardObjectType, List<PathBoardObject> pathfindList)
    {
        if (BugTestConsoleOutput)
            print("Saving Successful Pathing: " + boardObjectType.ToString());

        if(boardObjectType == BoardObject.Alpha_Static)
        {
            if(pathfindList.Count < CurrentAlpha)
            {
                SuccessfulPathfindList_Alpha = pathfindList;
                CurrentAlpha = SuccessfulPathfindList_Alpha.Count;
            }
        }
        else if(boardObjectType == BoardObject.Bravo_Static)
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

    int CurrentAlpha = 99;
    int CurrentBravo = 99;
    int CheckBestPathfindList(BoardObject boardObjectType)
    {
        int returnNum = 99;

        if (boardObjectType == BoardObject.Alpha_Static)
            returnNum = CurrentAlpha;
        else if (boardObjectType == BoardObject.Bravo_Static)
            returnNum = CurrentBravo;

        return returnNum;
    }

    int AlphaThreads = 0;
    int BravoThreads = 0;
    void ThreadCounter(BoardObject boardObjectType, bool increment)
    {
        if ( boardObjectType == BoardObject.Alpha_Static )
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

    bool FoundScoreline;
    void ScoreLineLogic()
    {
        // Still determining if I want to score the longer of 2+ lines, or the one closer to the bottom.
        // Logic exists for the 2+ lines, but gonna prioritize the closest to the bottom for now.

        int alphaLine_YPos = -1;
        int bravoLine_YPos = -1;

        if(SuccessfulPathfindList_Alpha.Count > 0)
            alphaLine_YPos = AlphaPathfindList[AlphaPathfindList.Count - 1].Position.y;

        if(SuccessfulPathfindList_Bravo.Count > 0)
            bravoLine_YPos = SuccessfulPathfindList_Bravo[SuccessfulPathfindList_Bravo.Count - 1].Position.y;

        List<PathBoardObject> ChosenPathfindList = SuccessfulPathfindList_Alpha;
        
        // If a scoreline for each type exist, pick the one closer to the bottom.
        if (bravoLine_YPos != -1)
        {
            if(bravoLine_YPos < alphaLine_YPos || alphaLine_YPos == -1)
            ChosenPathfindList = SuccessfulPathfindList_Bravo;
        }

        for(int i = 0; i < ChosenPathfindList.Count; i++)
        {
            Vector2Int _pos = ChosenPathfindList[i].Position;

            if (BugTestConsoleOutput)
                print("Clearing: " + _pos);

            SetBoardObjectAtPosition(_pos, BoardObject.Empty);
            BoardLogicScript.DestroySquircleAtGridPos(_pos);
        }
    }

    #endregion Pathfinding Logic
    // Update is called once per frame
    void Update()
    {
        if(IsGamePlaying)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                RotateCounterClockwise();

                if (BugTestConsoleOutput)
                {
                    Console_PrintBoard();
                }
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                RotateClockwise();

                if (BugTestConsoleOutput)
                {
                    Console_PrintBoard();
                }
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                ShiftLeft();

                if (BugTestConsoleOutput)
                {
                    Console_PrintBoard();
                }
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                ShiftRight();

                if (BugTestConsoleOutput)
                {
                    Console_PrintBoard();
                }
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                SoftDrop();

                if (BugTestConsoleOutput)
                {
                    Console_PrintBoard();
                }
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartCoroutine( HardDropPathfindLoop() );
            }

            /// 
            /// TESTING
            ///

            if(Input.GetKeyDown(KeyCode.K))
            {
                ChangeBoardSize(BoardWidth + 2);
                // BoardLogicScript.ReconstructBackdropArray();
            }

            if(Input.GetKeyDown(KeyCode.L))
            {
                ChangeBoardSize(BoardWidth - 2);
                // BoardLogicScript.ReconstructBackdropArray();
            }

            if(Input.GetKeyDown(KeyCode.M))
            {
                TEST_PresetBoard();

                // StartCoroutine(HardDropPathfindLoop());
            }

            if(Input.GetKeyDown(KeyCode.O))
            {
                BlockSizeFlip = !BlockSizeFlip;

                SetValidActiveBlockTypes(BlockSizeFlip, BlockSizeFlip, TwoByTwoFlip);
            }

            if(Input.GetKeyDown(KeyCode.P))
            {
                TwoByTwoFlip = !TwoByTwoFlip;

                SetValidActiveBlockTypes(BlockSizeFlip, BlockSizeFlip, TwoByTwoFlip);
            }
        }
    }
    bool BlockSizeFlip;
    bool TwoByTwoFlip;

    #region Timer / Gameplay Pause
    double SoftDropWaitTime;
    public void SetSoftDropWaitTime(double _waitTime)
    {
        SoftDropWaitTime = _waitTime;
    }

    bool IsGamePlaying;
    void SetGamePlayingState(bool _isGamePlaying)
    {
        IsGamePlaying = _isGamePlaying;
    }

    double LastSoftDropTimeActivated;
    IEnumerator SoftDropTimer()
    {
        while(IsGamePlaying)
        {
            print( (SoftDropWaitTime + LastSoftDropTimeActivated) - Time.fixedTimeAsDouble );
            if(Time.time > SoftDropWaitTime + LastSoftDropTimeActivated)
            {
                SoftDrop();
            }
            yield return new WaitForEndOfFrame();
        }

        yield return false;
    }

    #endregion Timer / Gameplay Pause

    #region Block Manipulation

    void RotateClockwise()
    {
        // Store bottom left of active block list
        BoardObject tempBlock = GetBoardObjectAtPosition(TileBottomLeftPosition);

        BoardLogicScript.RotateSquirclesAtBottomLeftPos_Clockwise(TileBottomLeftPosition, CurrBlockSize);

        int width = 2;
        if (CurrBlockSize == BlockSize.ThreeWide)
            width = 3;

        int height = 2;
        if (CurrBlockSize == BlockSize.ThreeTall)
        {
            height = 3;
        }

        for (int x = 0; x < width - 1; x++)
        {
            BoardObject shiftBlock = GetBoardObjectAtPosition(x + TileBottomLeftPosition.x + 1, TileBottomLeftPosition.y);
            SetBoardObjectAtPosition(x + TileBottomLeftPosition.x, TileBottomLeftPosition.y, shiftBlock);
        }

        for (int y = 0; y < height - 1; y++)
        {
            BoardObject shiftBlock = GetBoardObjectAtPosition(TileBottomLeftPosition.x + width - 1, TileBottomLeftPosition.y + y + 1);
            SetBoardObjectAtPosition(TileBottomLeftPosition.x + width - 1, TileBottomLeftPosition.y + y, shiftBlock);
        }

        for (int x = width - 1; x > 0; x--)
        {
            BoardObject shiftBlock = GetBoardObjectAtPosition(TileBottomLeftPosition.x + x - 1, TileBottomLeftPosition.y + height - 1);
            SetBoardObjectAtPosition(TileBottomLeftPosition.x + x, TileBottomLeftPosition.y + height - 1, shiftBlock);
        }

        for (int y = height - 1; y > 0; y--)
        {
            BoardObject prevBlock = GetBoardObjectAtPosition(TileBottomLeftPosition.x, TileBottomLeftPosition.y + y - 1);
            SetBoardObjectAtPosition(TileBottomLeftPosition.x, TileBottomLeftPosition.y + y, prevBlock);
        }

        SetBoardObjectAtPosition(TileBottomLeftPosition.x, TileBottomLeftPosition.y + 1, tempBlock);
    }

    void RotateCounterClockwise()
    {
        // Store bottom left of active block list
        BoardObject tempBlock = GetBoardObjectAtPosition(TileBottomLeftPosition);

        BoardLogicScript.RotateSquirclesAtBottomLeftPos_CounterClockwise(TileBottomLeftPosition, CurrBlockSize);

        int width = 2;
        if (CurrBlockSize == BlockSize.ThreeWide)
            width = 3;

        int height = 2;
        if (CurrBlockSize == BlockSize.ThreeTall)
        {
            height = 3;
        }


        // Blocks on Left Side shift Down
        for (int y = 0; y < height - 1; y++)
        {
            // Grid Array Manipulation
            BoardObject shiftBlock = GetBoardObjectAtPosition(TileBottomLeftPosition.x, TileBottomLeftPosition.y + y + 1);
            SetBoardObjectAtPosition(TileBottomLeftPosition.x, TileBottomLeftPosition.y + y, shiftBlock);
        }

        // Blocks on Top shift Left
        for (int x = 0; x < width - 1; x++)
        {
            // Grid Array Manipulation
            BoardObject shiftBlock = GetBoardObjectAtPosition(TileBottomLeftPosition.x + x + 1, TileBottomLeftPosition.y + height - 1);
            SetBoardObjectAtPosition(TileBottomLeftPosition.x + x, TileBottomLeftPosition.y + height - 1, shiftBlock);
        }

        // Blocks on Right Side shift Up
        for (int y = height - 1; y > 0; y--)
        {
            // Grid Array Manipulation
            BoardObject prevBlock = GetBoardObjectAtPosition(TileBottomLeftPosition.x + width - 1, TileBottomLeftPosition.y + y - 1);
            SetBoardObjectAtPosition(TileBottomLeftPosition.x + width - 1, TileBottomLeftPosition.y + y, prevBlock);
        }

        // Blocks on Bottom Side shift Right
        for (int x = width - 1; x > 0; x--)
        {
            // Grid Array Manipulation
            BoardObject shiftBlock = GetBoardObjectAtPosition(TileBottomLeftPosition.x + x - 1, TileBottomLeftPosition.y);
            SetBoardObjectAtPosition(TileBottomLeftPosition.x + x, TileBottomLeftPosition.y, shiftBlock);
        }

        // Grid Array Manipulation
        SetBoardObjectAtPosition(TileBottomLeftPosition.x + 1, TileBottomLeftPosition.y, tempBlock);
    }

    void ShiftLeft()
    {
        int width = 2;
        if (CurrBlockSize == BlockSize.ThreeWide)
            width = 3;

        int height = 2;
        if (CurrBlockSize == BlockSize.ThreeTall)
            height = 3;

        // Ensure left-bound positions are valid
        if (! (TileBottomLeftPosition.x - 1 >= HORIZ_LEFT_WALL_XPos_Sidewall) )
            return;

        // Check left bounds. If positions to its left are open, continue
        for (int y = 0; y < height; y++)
        {
            BoardObject blockCheck = GetBoardObjectAtPosition(TileBottomLeftPosition.x - 1, TileBottomLeftPosition.y + y);

            if (!(blockCheck == BoardObject.Empty || blockCheck == BoardObject.Ghost))
                return;
        }

        // Begin shifting blocks left
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int gridPos = new Vector2Int(TileBottomLeftPosition.x + x, TileBottomLeftPosition.y + y);

                BoardObject blockToShift = GetBoardObjectAtPosition(gridPos);

                SetBoardObjectAtPosition(TileBottomLeftPosition.x + x - 1, TileBottomLeftPosition.y + y, blockToShift);

                // Board Logic Squircle Object Manipulation
                BoardLogicScript.MoveSquircleAtPosTowardDirection(gridPos, PathfindDirection.Left);
            }
        }

        // Convert all right-side positions to Empty
        for (int y = 0; y < height; y++)
        {
            SetBoardObjectAtPosition(TileBottomLeftPosition.x + width - 1, TileBottomLeftPosition.y + y, BoardObject.Empty);
        }

        // Set new TileBottomLeftPosition
        TileBottomLeftPosition = new Vector2Int(TileBottomLeftPosition.x - 1, TileBottomLeftPosition.y);
    }

    void ShiftRight()
    {
        int width = 2;
        if (CurrBlockSize == BlockSize.ThreeWide)
            width = 3;

        int height = 2;
        if (CurrBlockSize == BlockSize.ThreeTall)
            height = 3;

        // Ensure right-bound positions are valid
        if (!(TileBottomLeftPosition.x + width < HORIZ_RIGHT_WALL_XPos_Sidewall + 1))
            return;

        // Check right bounds. If positions to its right are open, continue
        for (int y = 0; y < height; y++)
        {
            BoardObject blockCheck = GetBoardObjectAtPosition(TileBottomLeftPosition.x + width, TileBottomLeftPosition.y + y);

            if (!(blockCheck == BoardObject.Empty || blockCheck == BoardObject.Ghost))
                return;
        }

        // Begin shifting blocks right
        for (int x = width - 1; x >= 0; x--)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int gridPos = new Vector2Int(TileBottomLeftPosition.x + x, TileBottomLeftPosition.y + y);

                BoardObject blockToShift = GetBoardObjectAtPosition(gridPos);

                SetBoardObjectAtPosition(TileBottomLeftPosition.x + x + 1, TileBottomLeftPosition.y + y, blockToShift);

                // Board Logic Squircle Object Manipulation
                BoardLogicScript.MoveSquircleAtPosTowardDirection(gridPos, PathfindDirection.Right);
            }
        }

        // Convert all right-side positions to Empty
        for (int y = 0; y < height; y++)
        {
            SetBoardObjectAtPosition(TileBottomLeftPosition.x, TileBottomLeftPosition.y + y, BoardObject.Empty);
        }

        // Set new TileBottomLeftPosition
        TileBottomLeftPosition = new Vector2Int(TileBottomLeftPosition.x + 1, TileBottomLeftPosition.y);
    }

    void SoftDrop()
    {
        // Starting from the active Bottom Left corner,
        // 
        int height = 2;
        if (CurrBlockSize == BlockSize.ThreeTall)
            height = 3;

        int width = 2;
        if (CurrBlockSize == BlockSize.ThreeWide)
            width = 3;

        // We don't immediately go to HardDrop if y = 1 because we want to allow rotation on the bottom row before HardDrop
        if( TileBottomLeftPosition.y == 0 )
        {
            HardDrop();
            return;
        }

        for (int x = 0; x < width; x++)
        {
            BoardObject blockCheck = GetBoardObjectAtPosition(TileBottomLeftPosition.x + x, TileBottomLeftPosition.y - 1);

            if (! (blockCheck == BoardObject.Empty || blockCheck == BoardObject.Ghost) )
            {
                HardDrop();
                return;
            }
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2Int gridPos = new Vector2Int(TileBottomLeftPosition.x + x, TileBottomLeftPosition.y + y);

                // Game Logic Array Manipulation
                BoardObject blockToShift = GetBoardObjectAtPosition(gridPos);

                SetBoardObjectAtPosition(TileBottomLeftPosition.x + x, TileBottomLeftPosition.y + y - 1, blockToShift);

                // Board Logic Squircle Object Manipulation
                BoardLogicScript.MoveSquircleAtPosTowardDirection(gridPos, PathfindDirection.Down);
            }
        }

        for( int x = 0; x < width; x++)
        {
            // Already Did Board Logic Squircle Object Manipulation, so not needed below
            SetBoardObjectAtPosition(TileBottomLeftPosition.x + x, TileBottomLeftPosition.y + height - 1, BoardObject.Empty);
        }

        TileBottomLeftPosition.y -= 1;

        LastSoftDropTimeActivated = Time.time;
        Console_PrintBoard();
    }

    void HardDrop()
    {
        print("HARD DROP");

        AllBlocksStatic();

        // Go from left side to right, bottom to top
        for (int x = 0; x < BoardWidth; x++)
        {
            for (int y = 1; y < BoardHeight; y++)
            {
                BoardObject thisBlock = GetBoardObjectAtPosition(x, y);

                if (thisBlock == BoardObject.Alpha_Static || thisBlock == BoardObject.Bravo_Static)
                {
                    BoardObject belowBlock = GetBoardObjectAtPosition(x, y - 1);

                    if (belowBlock == BoardObject.Empty || belowBlock == BoardObject.Ghost)
                    {
                        SetBoardObjectAtPosition(x, y - 1, thisBlock);

                        // If it was a ghost block, it gets reset to ghost at end of BeginPathfinding()
                        SetBoardObjectAtPosition(x, y, BoardObject.Empty);

                        BoardLogicScript.MoveSquircleAtPosTowardDirection(new Vector2Int(x, y), PathfindDirection.Down);

                        y = 0;
                    }
                }
            }
        }

        
    }

    void AllBlocksStatic()
    {
        // Go from left side to right, bottom to top
        for (int x = 0; x < BoardWidth; x++)
        {
            for (int y = 0; y < BoardHeight; y++)
            {
                BoardObject thisBlock = GetBoardObjectAtPosition(x, y);

                if (thisBlock == BoardObject.Alpha_Active)
                    thisBlock = BoardObject.Alpha_Static;
                else if (thisBlock == BoardObject.Bravo_Active)
                    thisBlock = BoardObject.Bravo_Static;

                SetBoardObjectAtPosition(x, y, thisBlock);
            }
        }
    }


    #endregion Block Manipulation

    #region Board Manipulation
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

    public Vector2Int GetBoardSize()
    {
        return new Vector2Int(BoardWidth, BoardHeight);
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

    void ChangeBoardSize(int _newBoardWidth)
    {
        int oldWidth = BoardWidth;
        int oldHeight = BoardHeight;

        int widthDiff = _newBoardWidth - oldWidth;
        
        if (widthDiff == 0)
        {
            return;
        }

        ClearGhostBlockList();

        List<BoardObject> tempBoard = new List<BoardObject>();

        int blocksChangePerSide = widthDiff / 2;


        if (Mathf.Sign(widthDiff) == 1)
        {
            #region Expansion Logic
            for (int y = 0; y < BoardHeight; y++)
            {
                #region Left Side
                for(int i = 0; i < blocksChangePerSide; i++)
                {
                    tempBoard.Add(BoardObject.Empty);
                }
                #endregion Left Side

                #region Center Pre-Existing Region
                for (int j = 0; j < oldWidth; j++)
                {
                    BoardObject tempObj = GetBoardObjectAtPosition(j, y);

                    if(tempObj == BoardObject.Ghost)
                        tempObj = BoardObject.Empty;

                    tempBoard.Add(tempObj);
                }

                #endregion Center Pre-Existing Region

                #region Right Side
                for (int k = 0; k < blocksChangePerSide; k++)
                {
                    tempBoard.Add(BoardObject.Empty);
                }
                #endregion Right Side
            }

            #endregion Expansion Logic
        }
        else
        {
            #region Reduction Logic

            blocksChangePerSide = Mathf.Abs(blocksChangePerSide);

            for (int y = 0; y < BoardHeight; y++)
            {
                for (int i = blocksChangePerSide; i < BoardWidth - blocksChangePerSide; i++)
                {
                    BoardObject tempObj = GetBoardObjectAtPosition(i, y);
                    
                    tempBoard.Add(tempObj);
                }
            }
            #endregion Reduction Logic
        }

        // Change the TileBottomLeftPosition afterward so it doesn't get modified by the Y-pos loop
        for(int i = 0; i < Math.Abs(blocksChangePerSide); i++)
        {
            if(Math.Sign(widthDiff) == 1)
            {
                TileBottomLeftPosition.x++;
            }
            else
            {
                TileBottomLeftPosition.x--;
            }
        }

        BoardWidth = _newBoardWidth;

        HORIZ_RIGHT_WALL_XPos_Playable = BoardWidth - 2;
        HORIZ_RIGHT_WALL_XPos_Sidewall = HORIZ_RIGHT_WALL_XPos_Playable + 1;

        Board = tempBoard;

        // Board Logic needs to be reconstructed first so the SetGhostBlock can find the *new* Squircle object pos
        BoardLogicScript.ReconstructBackdropArray();

        for (int y = 0; y < BoardHeight; y++)
        {
            SetGhostBlock(0, y);
            SetGhostBlock(BoardWidth - 1, y);
        }
    }

    void ResetGhostBlocks()
    {
        foreach (Vector2Int pos in GhostBlockList)
        {
            BoardObject currBoardObject = GetBoardObjectAtPosition(pos);
            if(currBoardObject == BoardObject.Alpha_Static || currBoardObject == BoardObject.Bravo_Static)
            {
                BoardLogicScript.DestroySquircleAtGridPos(pos);
            }

            SetBoardObjectAtPosition(pos, BoardObject.Ghost);
        }
    }

    List<Vector2Int> GhostBlockList;
    void SetGhostBlock(int x_, int y_)
    {
        SetGhostBlock(new Vector2Int(x_, y_));
    }

    void SetGhostBlock(Vector2Int _pos)
    {
        BoardObject currBoardObject = GetBoardObjectAtPosition(_pos);
        if (currBoardObject == BoardObject.Alpha_Static || currBoardObject == BoardObject.Bravo_Static)
        {
            BoardLogicScript.DestroySquircleAtGridPos(_pos);
        }

        SetBoardObjectAtPosition(_pos.x, _pos.y, BoardObject.Ghost);
        GhostBlockList.Add(_pos);
    }

    void ClearGhostBlockList()
    {
        GhostBlockList = new List<Vector2Int>();
    }

    #endregion Board Manipulation

    #region Console Output
    void Console_PrintBoard()
    {
        for (int y = BoardHeight - 1; y >= 0; y--)
        {
            string textLine = "" + y + ": ";
            for(int x = 0; x < BoardWidth; x++)
            {
                // BoardWidth * k = vertical position
                // j = horizontal position
                // BoardObject currBoardObject = Board[(BoardWidth * y) + x];
                BoardObject currBoardObject = GetBoardObjectAtPosition(x, y);

                if (currBoardObject == BoardObject.Empty)
                    textLine += "[  ]";
                else if (currBoardObject == BoardObject.Ghost)
                    textLine += "[*]";
                else if (currBoardObject == BoardObject.Alpha_Active || currBoardObject == BoardObject.Alpha_Static)
                    textLine += "[X]";
                else if (currBoardObject == BoardObject.Bravo_Active || currBoardObject == BoardObject.Bravo_Static)
                    textLine += "[O]";
                else if (currBoardObject == BoardObject.Filled)
                    textLine += "[=]";
                else
                {
                    // ERROR
                    print("ERROR: " + currBoardObject);
                }
            }
            print(textLine);
        }
        print("-------------------------------------------------------------------------");
    }

    void PrintBlockList(List<BoardObject> blockList)
    {
        for (int count = 0; count < blockList.Count; count++)
        {
            print("Block #: " + count);

            string output = "";
            for (int eachBlock = 0; eachBlock < blockList.Count; eachBlock++)
            {
                output += blockList[eachBlock].ToString();
                if (eachBlock != blockList.Count - 1)
                {
                    output += ",";
                }
            }
            print(output);
            print("-----");
        }
    }

    void PrintAllPositionsInList(List<PathBoardObject> _pathfindList)
    {
        print("Pathfind Count: " + _pathfindList.Count);

        string output = "";
        for (int i = 0; i < _pathfindList.Count; i++)
        {
            output += "[" + _pathfindList[i].Position + "]";
            if (i < _pathfindList.Count - 1)
            {
                output += ", ";
            }
        }
        print(output);
    }

    #endregion Console Output
}
