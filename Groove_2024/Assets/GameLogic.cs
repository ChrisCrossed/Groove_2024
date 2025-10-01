using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
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

        // SetBoardObjectAtPosition(1, 1, BoardObject.Alpha_Active);
        // BoardLogicScript.AddSquircleToBoard(new Vector2Int(1, 1), BoardObject.Alpha_Active);

        List<Vector2Int> alphaPos = new List<Vector2Int>();
        List<Vector2Int> bravoPos = new List<Vector2Int>();

        bravoPos.Add(new Vector2Int(1, 1));
        bravoPos.Add(new Vector2Int(2, 1));
        bravoPos.Add(new Vector2Int(3, 1));
        bravoPos.Add(new Vector2Int(4, 1));
        bravoPos.Add(new Vector2Int(5, 1));
        bravoPos.Add(new Vector2Int(6, 1));

        bravoPos.Add(new Vector2Int(1, 2));
        alphaPos.Add(new Vector2Int(2, 2));
        bravoPos.Add(new Vector2Int(3, 2));
        alphaPos.Add(new Vector2Int(4, 2));
        alphaPos.Add(new Vector2Int(5, 2));
        alphaPos.Add(new Vector2Int(6, 2));

        bravoPos.Add(new Vector2Int(1, 3));
        alphaPos.Add(new Vector2Int(2, 3));
        alphaPos.Add(new Vector2Int(3, 3));
        bravoPos.Add(new Vector2Int(4, 3));
        alphaPos.Add(new Vector2Int(5, 3));
        bravoPos.Add(new Vector2Int(6, 3));

        bravoPos.Add(new Vector2Int(1, 4));
        bravoPos.Add(new Vector2Int(2, 4));
        bravoPos.Add(new Vector2Int(3, 4));
        bravoPos.Add(new Vector2Int(4, 4));
        bravoPos.Add(new Vector2Int(5, 4));
        alphaPos.Add(new Vector2Int(6, 4));

        bravoPos.Add(new Vector2Int(1, 5));
        alphaPos.Add(new Vector2Int(2, 5));
        alphaPos.Add(new Vector2Int(3, 5));
        alphaPos.Add(new Vector2Int(4, 5));
        alphaPos.Add(new Vector2Int(5, 5));
        alphaPos.Add(new Vector2Int(6, 5));
        
        alphaPos.Add(new Vector2Int(1, 6));
        alphaPos.Add(new Vector2Int(2, 6));
        alphaPos.Add(new Vector2Int(3, 6));
        bravoPos.Add(new Vector2Int(4, 6));
        bravoPos.Add(new Vector2Int(5, 6));
        bravoPos.Add(new Vector2Int(6, 6));

        alphaPos.Add(new Vector2Int(1, 7));
        alphaPos.Add(new Vector2Int(2, 7));
        bravoPos.Add(new Vector2Int(3, 7));
        alphaPos.Add(new Vector2Int(5, 7));
        alphaPos.Add(new Vector2Int(6, 7));

        foreach(Vector2Int pos in alphaPos)
        {
            SetBoardObjectAtPosition(pos, BoardObject.Alpha_Active);
            BoardLogicScript.AddSquircleToBoard(pos, BoardObject.Alpha_Active);
        }

        foreach(Vector2Int pos in bravoPos)
        {
            SetBoardObjectAtPosition(pos, BoardObject.Bravo_Active);
            BoardLogicScript.AddSquircleToBoard(pos, BoardObject.Bravo_Active);
        }
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

    bool StartPathfindingLogic()
    {
        List<int> finalPathfind = c_PathfindingLogic.StartPathfindingLogic( Board, BoardWidth );
        bool foundPath = false;

        // print(finalPathfind == null);

        if(finalPathfind != null && finalPathfind.Count > 1)
        {
            if (BugTestConsoleOutput)
                print("Clearing " + finalPathfind.Count + " Blocks");

            DetermineScoreFromScoreLine(finalPathfind);

            StartCoroutine(AnimateScoreLine(finalPathfind));

            foundPath = true;
        }
        return foundPath;
    }

    void DetermineScoreFromScoreLine(List<int> _finalPathfind)
    {
        // When the bool flips, add 1 to mult instead of points
        bool movingHoriz = true;
        int points = 1;
        int mult = 1;
        Vector2Int prevPos = new Vector2Int(_finalPathfind[0] % BoardWidth, _finalPathfind[0] / BoardWidth);

        for(int i = 1; i < _finalPathfind.Count; i++)
        {
            Vector2Int nextPos = new Vector2Int(_finalPathfind[i] % BoardWidth, _finalPathfind[i] / BoardWidth);

            print(prevPos + ", " + nextPos);

            // If the X position is NOT the same, moving Horizontal.
            if(prevPos.x != nextPos.x)
            {
                if(movingHoriz)
                {
                    points++;
                    print("[x] Adding to Points: " + points);
                }
                else
                {
                    mult++;
                    print("[x] Adding to Mult: " + mult);
                    movingHoriz = !movingHoriz;
                }
            }
            // If the Y position is NOT the same, moving Vertical
            else if(prevPos.y != nextPos.y)
            {
                if(!movingHoriz)
                {
                    points++;
                    print("[y] Adding to Points: " + points);
                }
                else
                {
                    mult++;
                    print("[y] Adding to Mult: " + mult);
                    movingHoriz = !movingHoriz;
                }
            }
            else
            {
                print("DETERMINE SCORE FROM SCORELINE: SHOULD NOT BE HERE: " + prevPos + ", " + nextPos);
            }

            prevPos = nextPos;
        }

        print("Total Scoreline Score: [" + points + " x " + mult + "] = " + (points * mult));
    }

    IEnumerator AnimateScoreLine(List<int> _finalPathfind)
    {
        for (int i = 0; i < _finalPathfind.Count; i++)
        {
            Vector2Int _pos = new Vector2Int();
            _pos.x = _finalPathfind[i] % BoardWidth;
            _pos.y = _finalPathfind[i] / BoardWidth;

            if (BugTestConsoleOutput)
                print("Clearing: " + _pos);

            SetBoardObjectAtPosition(_pos, BoardObject.Empty);
            BoardLogicScript.DestroySquircleAtGridPos(_pos);

            yield return new WaitForSeconds( 2.0f / _finalPathfind.Count );
        }

        yield return true;
    }

    IEnumerator HardDropPathfindLoop()
    {
        // Disable player action and Perform HardDrop once
        SetGamePlayingState(false);

        HardDrop();
        ResetGhostBlocks();
        
        bool donePathfinding = false;

        while (!donePathfinding)
        {
            #region Run one PathfindingLogic loop and store data
            List<int> finalPathfind = c_PathfindingLogic.StartPathfindingLogic(Board, BoardWidth);
            bool foundPath = false;

            if (finalPathfind != null && finalPathfind.Count > 1)
            {
                if (BugTestConsoleOutput)
                    print("Clearing " + finalPathfind.Count + " Blocks");

                DetermineScoreFromScoreLine(finalPathfind);

                StartCoroutine(AnimateScoreLine(finalPathfind));

                foundPath = true;

                // If it's successful, start animation and wait until completion
                yield return new WaitForSecondsRealtime(2.0f);
            }
            else // If it was successful and the animation is complete, Perform HardDrop, re-run PathfindingLogic, and repeat Animation Loop.
            {
                donePathfinding = true;
            }
            #endregion Run one PathfindingLogic loop and store data

            HardDrop();
        }

        BlockSize nextBlockSize = NextBlockListSize[0];
        List<BoardObject> nextBlock = GetNextBlock(true);
        PlaceNewSquircleGroupOfType(nextBlockSize, nextBlock);

        // Wait until entire process is complete, and then Enable player action.
        SetGamePlayingState(true);

        yield return true;
    }

    
    #endregion Pathfinding Logic

    // Update is called once per frame
    float EscapeTime;
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
                StartCoroutine(HardDropPathfindLoop());

                /*
                SetGamePlayingState(false);
                HardDrop();
                ResetGhostBlocks();

                // StartCoroutine(PerformTimedAction(StartPathfindingLogic));
                while(StartPathfindingLogic())
                {
                    HardDrop();
                }

                BlockSize nextBlockSize = NextBlockListSize[0];
                List<BoardObject> nextBlock = GetNextBlock(true);
                PlaceNewSquircleGroupOfType(nextBlockSize, nextBlock);
                
                SetGamePlayingState(true);
                */
            }

            if(Input.GetKeyDown(KeyCode.G))
            {
                ShiftBoardLeft();
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

                while (StartPathfindingLogic())
                {
                    HardDrop(true);
                }
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


            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if(Time.time - EscapeTime < 0.5f)
                {
                    Application.Quit();
                    print("QUIT");
                }

                EscapeTime = Time.time;
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

        if (BugTestConsoleOutput)
            Console_PrintBoard();
    }

    void HardDrop(bool staticOnly = false)
    {
        // print("HARD DROP");

        // In case I only want blocks that are already 'Static' to move, not Active blocks the player has control over.
        // This will most likely be used for mid-round board size changes, or static-block horizontal shifts.
        if(!staticOnly)
        {
            AllBlocksStatic();
        }

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


    void ShiftBoardLeft()
    {
        // TODO: Bug - When I shift the board with Active Blocks in the ghost columns, fake block models remain in the column

        // In the future, I'm probably only going as high as 2 or 3 rows below the top.
        for (int x = 1; x < BoardWidth_Maximum + 1; x++)
        {
            for(int y = 0; y < BoardHeight_Maximum; y++)
            {
                Vector2Int gridPos = new Vector2Int(x, y);
                
                Vector2Int nextPos = gridPos;
                nextPos.x -= 1;
                
                print("Pos: " + gridPos);

                BoardObject blockToShift = GetBoardObjectAtPosition(gridPos);
                BoardObject nextBlockObj = GetBoardObjectAtPosition(nextPos);

                if (blockToShift == BoardObject.Alpha_Active || blockToShift == BoardObject.Bravo_Active || blockToShift == BoardObject.Ghost)
                    continue;

                if (nextBlockObj == BoardObject.Alpha_Active || nextBlockObj == BoardObject.Bravo_Active)
                    continue;

                SetBoardObjectAtPosition(x - 1, y, blockToShift);
                SetBoardObjectAtPosition(x, y, BoardObject.Empty);

                // Board Logic Squircle Object Manipulation
                if(blockToShift != BoardObject.Empty)
                {
                    BoardLogicScript.MoveSquircleAtPosTowardDirection(gridPos, PathfindDirection.Left);
                }
            }
        }

        HardDrop(true);
        ResetGhostBlocks();
    }

    void ShiftBoardRight()
    {
        // HardDrop StaticOnly (just in case)
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

            // If Active Blocks are on the far left or right side, shift them inward
            if(IsGamePlaying)
            {
                int blockWidth = 2;
                if (CurrBlockSize == BlockSize.ThreeWide)
                    blockWidth = 3;

                if (TileBottomLeftPosition.x == HORIZ_LEFT_WALL_XPos_Playable || TileBottomLeftPosition.x == HORIZ_LEFT_WALL_XPos_Playable - 1)
                    ShiftRight();
                else if (TileBottomLeftPosition.x + blockWidth - 1 == HORIZ_RIGHT_WALL_XPos_Playable || TileBottomLeftPosition.x + blockWidth - 2 == HORIZ_RIGHT_WALL_XPos_Playable)
                    ShiftLeft();
            }

            // Continue with Reduction Logic
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

    public void PF_OutputTest(string _text)
    {
        print(_text);
    }

    #endregion Console Output

    bool timedOut;
    CancellationToken ct;
    private IEnumerator PerformTimedAction(Action action, int timeout = 1)
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        ct = cts.Token;

        Coroutine timeoutCoroutine = StartCoroutine(TimeoutChecker(timeout));
        var t = Task.Run(action, ct);
        yield return new WaitWhile(() => t.Status != TaskStatus.RanToCompletion && !timedOut);

        if(timedOut)
        {
            cts.Cancel();
            Debug.Log("Task Timed Out");
        }
        else
        {
            StopCoroutine(timeoutCoroutine);
            Debug.Log("Task successfully completed");
        }
    }

    private IEnumerator TimeoutChecker(float timeout)
    {
        timedOut = false;
        while (timeout > 0)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }
        timedOut = true;
    }

}
