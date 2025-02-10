using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

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

    [SerializeField, Range(5, 20)]
    int BoardWidth_Maximum;
    [SerializeField, Range(5, 20)]
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

    // Start is called before the first frame update
    void Start()
    {
        Init_Random();

        Init_Board();

        TEST_PresetBoard();

        SetValidActiveBlockTypes(BlockObject_Active_ThreeWide, BlockObject_Active_ThreeTall, BlockObject_Active_TwoByTwo);

        DetermineNextBlock();

        // Console_PrintBoard();

        PopulateNextFourBlocksList();

        // StartCoroutine(BeginPathfinding());
        // BeginPathfinding();

        /*
        for(int i = 0; i < Board.Count; i++)
        {
            print(Board[i]);
        }
        */
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
        ClearGhostBlockList();

        NextBlockList = new List<List<BoardObject>>();

        NextBlockListSize = new List<BlockSize>();

        // Sets whether 3 wide and 3 tall Active blocks are allowed.
        // Technically calls it's own values, but is safe.
        SetValidActiveBlockTypes(BlockObject_Active_ThreeWide, BlockObject_Active_ThreeTall, BlockObject_Active_TwoByTwo);

        // Extend width of board by 2 to include the Sidewalls
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
    }

    void TEST_PresetBoard()
    {
        // X = Alpha
        // O = Bravo

        /*
        for (int x = 0; x < BoardWidth; x++)
        {
            if (GetBoardObjectAtPosition(x, 2) != BoardObject.Ghost)
            {
                SetBoardObjectAtPosition(x, 2, BoardObject.Alpha_Static);
            }
        }
        */


            // CreateNewBlockOfType(TileType.ThreeTall, TileBottomLeftPosition);
            /*
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
            */

            // SetBoardObjectAtPosition(0, 2, BoardObject.Empty);
        }

    #region Gameplay Actions

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

    void DetermineNextBlock()
    {
        List<BlockSize> _blockTypes = new List<BlockSize>();

        if (BlockObject_Active_TwoByTwo)
            _blockTypes.Add(BlockSize.TwoByTwo);

        if (BlockObject_Active_ThreeWide)
            _blockTypes.Add(BlockSize.ThreeWide);

        if (BlockObject_Active_ThreeTall)
            _blockTypes.Add(BlockSize.ThreeTall);

        int randBlockSize = UnityEngine.Random.Range(0, _blockTypes.Count);

        BlockSize nextBlockType = _blockTypes[randBlockSize];

        Vector2Int blockPos = new Vector2Int();
        blockPos.x = (BoardWidth / 2) - 1;
        blockPos.y = BoardHeight - 2;

        if (nextBlockType == BlockSize.ThreeTall)
            --blockPos.y;

        CreateNewBlockOfType( nextBlockType, blockPos );
    }

    void SetValidActiveBlockTypes(bool threeWide_, bool threeTall, bool twoByTwo_ = true)
    {
        BlockObject_Active_TwoByTwo = twoByTwo_;
        BlockObject_Active_ThreeWide = threeWide_;
        BlockObject_Active_ThreeTall = threeTall;

        if(!threeWide_ && !threeTall)
        {
            BlockObject_Active_TwoByTwo = true;
        }
    }

    /// <summary>
    /// Creates a new 'Active' block of random tiles, starting at the Bottom Left coordinate given.
    /// </summary>
    /// <param name="_size">Applies a block varying in height and width.</param>
    /// <param name="_position">The bottom left coordinate for the block to spawn</param>
    void CreateNewBlockOfType(BlockSize _size, Vector2Int _position)
    {
        // Determine if all positions are empty and available
        Vector2Int boardPos = _position;
        int blockHeight = 2;
        int blockWidth = 2;
        if ( _size == BlockSize.ThreeWide )
            blockWidth = 3;
        else if ( _size == BlockSize.ThreeTall )
            blockHeight = 3;

        for( int y = _position.y; y < _position.y + blockHeight; y++ )
        {
            for( int x = _position.x; x < _position.x + blockWidth; x++ )
            {
                BoardObject tempBlock = GetBoardObjectAtPosition(x, y);

                // Bot left, Bot right, Mid left, Mid right, Top left, Top right
                if( tempBlock == BoardObject.Empty )
                {
                    BoardObject randomBlock = DetermineRandomIndividualBlock(true);
                    SetBoardObjectAtPosition(x, y, randomBlock);
                }
                else
                {
                    // TODO: END GAME - STARTING BLOCK POSITIONS ARE FULL
                }
            }
        }

        TileBottomLeftPosition = _position;
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

        for(int count = 0; count < NextBlockListSize.Count; count++)
        {
            print("Size: " + NextBlockListSize[count]);
            
            string output = "";
            for(int eachBlock = 0; eachBlock < NextBlockList[count].Count; eachBlock++)
            {
                output += NextBlockList[count][eachBlock].ToString();
                if(eachBlock != NextBlockList[count].Count - 1)
                {
                    output += ",";
                }
            }
            print(output);
            print("-----");
        }
    }

    void RotateClockwise()
    {
        // Store bottom left of active block list
        BoardObject tempBlock = GetBoardObjectAtPosition(TileBottomLeftPosition);

        int width = 2;
        if (CurrBlockSize == BlockSize.ThreeWide)
            width = 3;

        int height = 2;
        if(CurrBlockSize == BlockSize.ThreeTall )
        {
            height = 3;
        }

        for(int x = 0; x < width - 1; x++)
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

        int width = 2;
        if (CurrBlockSize == BlockSize.ThreeWide)
            width = 3;

        int height = 2;
        if (CurrBlockSize == BlockSize.ThreeTall)
        {
            height = 3;
        }

        for (int y = 0; y < height - 1; y++)
        {
            BoardObject shiftBlock = GetBoardObjectAtPosition(TileBottomLeftPosition.x, TileBottomLeftPosition.y + y + 1);
            SetBoardObjectAtPosition(TileBottomLeftPosition.x, TileBottomLeftPosition.y + y, shiftBlock);
        }

        for (int x = 0; x < width - 1; x++)
        {
            BoardObject shiftBlock = GetBoardObjectAtPosition(TileBottomLeftPosition.x + x + 1, TileBottomLeftPosition.y + height - 1);
            SetBoardObjectAtPosition(TileBottomLeftPosition.x + x, TileBottomLeftPosition.y + height - 1, shiftBlock);
        }

        for (int y = height - 1; y > 0; y--)
        {
            BoardObject prevBlock = GetBoardObjectAtPosition(TileBottomLeftPosition.x + width - 1, TileBottomLeftPosition.y + y - 1);
            SetBoardObjectAtPosition(TileBottomLeftPosition.x + width - 1, TileBottomLeftPosition.y + y, prevBlock);
        }

        for (int x = width - 1; x > 0; x--)
        {
            BoardObject shiftBlock = GetBoardObjectAtPosition(TileBottomLeftPosition.x + x - 1, TileBottomLeftPosition.y);
            SetBoardObjectAtPosition(TileBottomLeftPosition.x + x, TileBottomLeftPosition.y, shiftBlock);
        }

        SetBoardObjectAtPosition(TileBottomLeftPosition.x + 1, TileBottomLeftPosition.y, tempBlock);
    }

    void SoftDrop()
    {
        // Starting from the active Bottom Left corner,
        // 
        print(TileBottomLeftPosition);

        int tileHeight = 2;
        if(CurrBlockSize == BlockSize.ThreeTall)
            tileHeight = 3;

        int tileWidth = 2;
        if(CurrBlockSize == BlockSize.ThreeWide)
            tileWidth = 3;

        for(int x = TileBottomLeftPosition.x; x < TileBottomLeftPosition.x + tileWidth; x++ )
        {
            for(int y = TileBottomLeftPosition.y; y < TileBottomLeftPosition.y + tileHeight; y++ )
            {
                if(y - 1 > 0)
                {
                    BoardObject thisBlock = GetBoardObjectAtPosition(x, y);
                    BoardObject belowBlock = GetBoardObjectAtPosition(x, y - 1);

                    if (belowBlock == BoardObject.Empty || belowBlock == BoardObject.Ghost)
                    {
                        SetBoardObjectAtPosition(x, y - 1, thisBlock);
                        SetBoardObjectAtPosition(x, y, BoardObject.Empty);
                    }
                    else
                    {
                        HardDrop();
                        return;
                    }
                }
                else
                {
                    HardDrop();
                    return;
                }
            }
        }

        TileBottomLeftPosition.y -= 1;
    }

    void HardDrop()
    {
        print("HARD DROP");

        AllBlocksStatic();

        // Go from left side to right, bottom to top
        for (int x = 0; x < BoardWidth; x++)
        {
            for(int y = 1; y < BoardHeight; y++)
            {
                BoardObject thisBlock = GetBoardObjectAtPosition(x, y);

                if(thisBlock == BoardObject.Alpha_Static || thisBlock == BoardObject.Bravo_Static)
                {
                    BoardObject belowBlock = GetBoardObjectAtPosition(x, y - 1);

                    if(belowBlock == BoardObject.Empty || belowBlock == BoardObject.Ghost)
                    {
                        SetBoardObjectAtPosition(x, y - 1, thisBlock);

                        // If it was a ghost block, it gets reset to ghost at end of BeginPathfinding()
                        SetBoardObjectAtPosition(x, y, BoardObject.Empty);

                        y = 0;
                    }
                }
            }
        }

        BeginPathfinding();
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
                else if(thisBlock == BoardObject.Bravo_Active)
                    thisBlock = BoardObject.Bravo_Static;

                SetBoardObjectAtPosition(x, y, thisBlock);
            }
        }
    }
    #endregion

    void ResetGhostBlocks()
    {
        foreach(Vector2Int pos in GhostBlockList)
        {
            SetBoardObjectAtPosition(pos, BoardObject.Ghost);
        }
    }

    List<Vector2Int> GhostBlockList;
    void SetGhostBlock(int x_, int y_)
    {
        SetGhostBlock(new Vector2Int(x_, y_));
    }

    void SetGhostBlock(Vector2Int pos_)
    {
        SetBoardObjectAtPosition(pos_.x, pos_.y, BoardObject.Ghost);
        GhostBlockList.Add(pos_);
    }

    void ClearGhostBlockList()
    {
        GhostBlockList = new List<Vector2Int>();
    }

    #region Pathfinding Actions
    void BeginPathfinding()
    {
        bool alphaExists = true;
        bool bravoExists = true;

        AlphaPathfindList = new List<PathBoardObject>();
        BravoPathfindList = new List<PathBoardObject>();
        CurrentAlpha = 99;
        CurrentBravo = 99;
        AlphaThreads = 0;
        BravoThreads = 0;

        SuccessfulPathfindList_Alpha = new List<PathBoardObject>();
        SuccessfulPathfindList_Bravo = new List<PathBoardObject>();

        

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
        #endregion

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
        

        #region Horizontal Tests
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

        ResetGhostBlocks();
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

                    // If this block to the right is valid AND is along the right-hand side of the board, SUCCESS
                    if (nextPos.x == HORIZ_RIGHT_WALL_XPos_Playable)
                    {
                        SaveSuccessfulPathing(boardObjectType, validBoardObjects);
                        ThreadCounter(boardObjectType, false);
                        shouldContinue = false;
                        continue;
                    }
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

                if (tempBlock.LeftValid)
                {
                    validBoardObjects.Add(new PathBoardObject(nextPos, true, true, false, true));
                }
            }

            if (BugTestConsoleOutput)
            {
                print("Valid Positions remaining: " + validBoardObjects.Count);
                print("Valid Positions: ");
                foreach (PathBoardObject boardObject in validBoardObjects) { print(boardObject.Position); }
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
                            PrintAllPositionsInList(secondNewThread);

                        StartCoroutine(PathfindLogic(boardObjectType, secondNewThread));
                    }
                }

                // Default for the first valid new block position
                if (BugTestConsoleOutput)
                {
                    print("Adding " + validBoardObjects[0].Position + " position to list. Continuing this thread. Length: " + pathfindList.Count);
                    print("THREAD '0'");
                }
                    
                pathfindList.Add(validBoardObjects[0]);

                if (BugTestConsoleOutput)
                    PrintAllPositionsInList(pathfindList);

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

    void PrintAllPositionsInList(List<PathBoardObject> _pathfindList)
    {
        string output = "";
        for (int i = 0; i < _pathfindList.Count; i++)
        {
            output += "[" + _pathfindList[i].Position + "]";
            if(i < _pathfindList.Count - 1)
            {
                output += ", ";
            }
        }
        print(output);
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
            if(pathfindList.Count < SuccessfulPathfindList_Alpha.Count || SuccessfulPathfindList_Alpha == new List<PathBoardObject>())
            {
                SuccessfulPathfindList_Alpha = pathfindList;
                CurrentAlpha = SuccessfulPathfindList_Alpha.Count;
            }
        }
        else if(boardObjectType == BoardObject.Bravo_Static)
        {
            if (pathfindList.Count < SuccessfulPathfindList_Bravo.Count || SuccessfulPathfindList_Bravo == new List<PathBoardObject>())
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

        if (BugTestConsoleOutput)
        {
            if (AlphaThreads == 0 && BravoThreads == 0)
                print("--------" + "\nNO MORE THREADS" + "\n--------");
        }
        
        
    }

    #endregion

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            // RotateCounterClockwise();
            SoftDrop();
            print("-----------");
            print("-----------");
            print("-----------");
            Console_PrintBoard();
        }
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
    }
}
