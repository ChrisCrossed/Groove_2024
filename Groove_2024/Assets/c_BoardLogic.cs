using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class c_BoardLogic : MonoBehaviour
{
    GameObject GameLogicObject;
    GameLogic GameLogic;

    GameObject BackdropGameObject;

    List<GameObject> BackdropArray;
    GameObject[] SquircleArray;
    //List<GameObject> SquircleArray;
    // Or do I want an array so spaces can be empty?

    [SerializeField] private Material Mat_AlphaBlock;
    [SerializeField] private Material Mat_BravoBlock;
    [SerializeField] private Material Mat_EmptyBlock;

    [SerializeField] private Material Mat_AlphaBackdrop;
    [SerializeField] private Material Mat_BravoBackdrop;
    [SerializeField] private Material Mat_EmptyBackdrop;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Init_BoardLogic()
    {
        GameLogicObject = GameObject.Find("GameLogic");
        GameLogic = GameLogicObject.GetComponent<GameLogic>();

        BackdropGameObject = GameObject.Find("BackdropArray");

        BackdropArray = new List<GameObject>();


        UpdateBoardSize();
        InitializeBackdrop();
        TestBoard();
    }

    void TestBoard()
    {

    }

    public GameObject SquirclePrefab;
    public GameObject BackdropPrefab;
    List<GameObject> BackdropObjects;
    int leftWidth;
    int rightWidth;
    void InitializeBackdrop()
    {
        // Reset Squircle Array
        SquircleArray = new GameObject[BoardWidth * BoardHeight];

        defaultLeftPos = BoardWidth / 2f * -1f;

        // Reset backdrop objects list
        BackdropObjects = new List<GameObject>();

        int widthToIncrease = BoardWidth / 2;

        // Only increment counter for one row (to know true width)
        // In other words, evaluates expansion from center outward
        for(int i = 0; i < widthToIncrease; i++)
        {
            leftWidth++;
            rightWidth++;
        }

        StartingHalfBoardWidth = leftWidth;

        for (int y = 0; y < BoardHeight; y++)
        {
            // construct left half, and increase 'Left Width' counter for future work
            List<GameObject> tempList = new List<GameObject>();
            for (int x = widthToIncrease; x > 0; x--)
            {
                GameObject tempBackdrop = CreateBackdropBlock(new Vector2Int(x, y));
                
                tempBackdrop.gameObject.transform.name = "Backdrop_" + x + "_" + y;

                tempList.Add(tempBackdrop);
            }

            // Reverse list before continuing
            tempList.Reverse();

            // Construct right half, and increase 'Right Width' counter for future work
            for (int x = 0; x < widthToIncrease; x++)
            {
                GameObject tempBackdrop = CreateBackdropBlock(new Vector2Int(x + widthToIncrease + 1, y));
                

                tempBackdrop.gameObject.transform.name = "Backdrop_" + (x + widthToIncrease + 1) + "_" + y;

                tempList.Add(tempBackdrop);
            }

            for (int i = 0; i < tempList.Count; i++)
                BackdropArray.Add(tempList[i]);

        }
    }

    int StartingHalfBoardWidth;
    public void ReconstructBackdropArray()
    {
        List<GameObject> newArray = new List<GameObject>();

        // Determine if width of board decreased or increased
        int oldWidth = BoardWidth;
        int oldHeight = BoardHeight;

        UpdateBoardSize();

        // Need to get the *new* board size first to resize the Squircle Array
        GameObject[] tempSquircleArray = new GameObject[BoardWidth * BoardHeight];

        int widthDiff = BoardWidth - oldWidth;
        int blocksChangePerSide = widthDiff / 2;

        if(widthDiff == 0)
        {
            return;
        }

        if( Mathf.Sign(widthDiff) == 1 )
        {
            #region Expansion Logic
            for ( int y = 0; y < BoardHeight; y++ )
            {
                #region Left Side
                List<GameObject> leftArray = new List<GameObject>();
                for ( int j = 0; j < blocksChangePerSide; j++ )
                {
                    // Grows column toward the left (-leftWidth) by one block
                    GameObject tempBlock = CreateBackdropBlock(new Vector2Int(-leftWidth + StartingHalfBoardWidth, y));
                    

                    leftArray.Add(tempBlock);
                }

                // Reverse and add to List
                leftArray.Reverse();
                foreach(GameObject obj in leftArray)
                    newArray.Add(obj);
                #endregion Left Side

                #region Center Pre-Existing Region
                for ( int x = 0; x < oldWidth; x++ )
                {
                    int oldArrayPosition = (y * oldWidth) + x;
                    int newArrayPosition = (y * BoardWidth) + (blocksChangePerSide + x);

                    newArray.Add( BackdropArray[ oldArrayPosition ]);

                    if( SquircleArray[ oldArrayPosition ] != null )
                    {
                        Vector2Int newGridCoords = SquircleArray[oldArrayPosition].gameObject.GetComponent<c_SquircleLogic>().GridCoords;
                        newGridCoords.x += blocksChangePerSide;

                        SquircleArray[oldArrayPosition].gameObject.GetComponent<c_SquircleLogic>().GridCoords = newGridCoords;

                        tempSquircleArray[ newArrayPosition ] = SquircleArray[ oldArrayPosition ];

                        print("New Coords: " + tempSquircleArray[ newArrayPosition ].gameObject.GetComponent<c_SquircleLogic>().GridCoords);
                    }
                }
                #endregion Center Pre-Existing Region

                #region Right Region
                for (int k = 0; k < blocksChangePerSide; k++ )
                {
                    // Grows column toward the right (rightWidth) by one block
                    newArray.Add(CreateBackdropBlock(new Vector2Int(rightWidth + StartingHalfBoardWidth + 1, y)));
                }
                #endregion Right Region
            }

            leftWidth += blocksChangePerSide;
            rightWidth += blocksChangePerSide;
            #endregion Expansion Logic
        }
        else
        {
            blocksChangePerSide = Mathf.Abs(blocksChangePerSide);

            #region Reduction Logic
            for (int y = 0; y < BoardHeight; y++)
            {
                int currYPos = y * oldWidth;

                // Reduce Left Side (TODO: Update this to fade blocks out from center-outward)
                for (int i = 0; i < blocksChangePerSide; i++)
                {
                    // Reduces column toward the left (-leftWidth) by one block
                    DestroyBackdropBlock(currYPos + i);

                    // TODO: If there *was* a Squircle Object at this position, tell it to delete itself
                }

                for(int j = blocksChangePerSide; j < oldWidth - blocksChangePerSide; ++j)
                {
                    newArray.Add( BackdropArray[currYPos + j] );

                    int oldArrayPosition = (y * oldWidth) + j;
                    int newArrayPosition = (y * BoardWidth) + (j - blocksChangePerSide);

                    if (SquircleArray[oldArrayPosition] != null)
                    {
                        print("Success:");
                        // Get new grid coordinate and set tempSquircleArray to refer to that board object
                        
                        print("X: " + (j - blocksChangePerSide) + ", Y: " + y);

                        Vector2Int newGridCoords = SquircleArray[oldArrayPosition].gameObject.GetComponent<c_SquircleLogic>().GridCoords;
                        newGridCoords.x -= blocksChangePerSide;

                        SquircleArray[oldArrayPosition].gameObject.GetComponent<c_SquircleLogic>().GridCoords = newGridCoords;

                        tempSquircleArray[newArrayPosition] = SquircleArray[oldArrayPosition];
                        print(tempSquircleArray[newArrayPosition].gameObject.GetComponent<c_SquircleLogic>().GridCoords);
                    }
                }

                for ( int k = oldWidth - blocksChangePerSide; k < oldWidth; k++)
                {
                    // Reduces column toward the left (-leftWidth) by one block
                    DestroyBackdropBlock(currYPos + k);

                    // TODO: If there *was* a Squircle Object at this position, tell it to delete itself
                }
            }
            
            leftWidth -= blocksChangePerSide;
            rightWidth -= blocksChangePerSide;

            #endregion Reduction Logic
        }

        BackdropArray = newArray;
        SquircleArray = tempSquircleArray;
    }

    
    GameObject CreateBackdropBlock(Vector2Int _gridPos)
    {
        GameObject tempBackdrop = GameObject.Instantiate(BackdropPrefab);

        // tempBackdrop.gameObject.transform.localScale = new Vector3(0.85f, 0.85f, 1.0f);
        tempBackdrop.name = "Backdrop";

        tempBackdrop.transform.position = GetWorldPosition(_gridPos);

        tempBackdrop.transform.SetParent(BackdropGameObject.transform);

        return tempBackdrop;
    }

    public void AddSquircleToBoard(Vector2Int _gridPos, BoardObject _boardObjectType)
    {
        if(!(_boardObjectType == BoardObject.Alpha_Active || _boardObjectType == BoardObject.Bravo_Active))
        {
            print("WRONG BOARD OBJECT TYPE - 'AddSquircleToBoard' (c_BoardLogic)");
            return;
        }

        // Get Relative BackDrop Array Position
        /*
        int index = ((_gridPos.y * BoardWidth) + _gridPos.x);
        Vector3 worldPos = BackdropArray[index].gameObject.transform.position;
        */
        Vector3 worldPos = GetWorldPosition(_gridPos, true);


        GameObject tempSquircle = GameObject.Instantiate(SquirclePrefab);
        tempSquircle.name = "Alpha_Squircle";
        if(_boardObjectType == BoardObject.Bravo_Active)
        {
            tempSquircle.name = "Bravo_Squircle";
        }

        float squircleScaleSize = 0.7f;
        tempSquircle.GetComponent<c_SquircleLogic>().InitializeSquircle(_boardObjectType, _gridPos, squircleScaleSize);

        tempSquircle.transform.position = worldPos;

        SquircleArray[(BoardWidth * _gridPos.y) + _gridPos.x] = tempSquircle;
    }

    public void MoveSquircleAtPosTowardDirection(Vector2Int _gridPos, PathfindDirection _direction )
    {
        GameObject tempSquircle = GetObjAtPosition(_gridPos, true);
        Vector2Int newGridPos = _gridPos;

        // Squircle Object is stored above. Empty this Array position.
        SquircleArray[newGridPos.y * BoardWidth + newGridPos.x] = null;

        switch (_direction)
        {
            case PathfindDirection.Up:
                newGridPos.y++;
                break;
            case PathfindDirection.Down:
                // Move Y position down a space
                newGridPos.y--;
                break;
            case PathfindDirection.Left:
                // Move X position left
                newGridPos.x--;
                break;
            case PathfindDirection.Right:
                newGridPos.x++;
                break;
            case PathfindDirection.None:
            default:
                break;
        }

        // Set Array position to the current Squircle Object
        SquircleArray[newGridPos.y * BoardWidth + newGridPos.x] = tempSquircle;

        Vector3 newWorldPos = GetWorldPosition(newGridPos, true);
        tempSquircle.GetComponent<c_SquircleLogic>().GridCoords = newGridPos;
        tempSquircle.GetComponent<c_SquircleLogic>().GoToPosition(newWorldPos);
    }

    public void RotateSquirclesAtBottomLeftPos_CounterClockwise(Vector2Int _botLeftPos, BlockSize _squircleGroupSize)
    {
        GameObject tempSquircle = GetObjAtPosition(_botLeftPos, true);
        Vector2Int tempGridPos = _botLeftPos;

        int width = 2;
        if (_squircleGroupSize == BlockSize.ThreeWide)
            width = 3;

        int height = 2;
        if (_squircleGroupSize == BlockSize.ThreeTall)
            height = 3;

        Vector2Int gridPos = new Vector2Int();

        // Blocks on Left Side shift Down
        for (int y = 0; y < height - 1; y++)
        {
            gridPos = new Vector2Int(_botLeftPos.x, _botLeftPos.y + y + 1);
            MoveSquircleAtPosTowardDirection(gridPos, PathfindDirection.Down);
        }

        // Blocks on Top shift Left
        for (int x = 0; x < width - 1; x++)
        {
            // Board Logic Squircle Array Manipulation
            gridPos = new Vector2Int(_botLeftPos.x + x + 1, _botLeftPos.y + height - 1);
            MoveSquircleAtPosTowardDirection(gridPos, PathfindDirection.Left);
        }

        // Blocks on Right Side shift Up
        for (int y = height - 1; y > 0; y--)
        {
            // Board Logic Squircle Array Manipulation
            gridPos = new Vector2Int(_botLeftPos.x + width - 1, _botLeftPos.y + y - 1);
            MoveSquircleAtPosTowardDirection(gridPos, PathfindDirection.Up);
        }

        // Blocks on Bottom Side shift Right
        for (int x = width - 1; x > 1; x--)
        {
            // Board Logic Squircle Array Manipulation
            gridPos = new Vector2Int(_botLeftPos.x + x - 1, _botLeftPos.y);
            MoveSquircleAtPosTowardDirection(gridPos, PathfindDirection.Right);
        }


        tempGridPos.x++;
        SquircleArray[tempGridPos.y * BoardWidth + tempGridPos.x] = tempSquircle;

        Vector3 newWorldPos = GetWorldPosition(tempGridPos, true);
        tempSquircle.GetComponent<c_SquircleLogic>().GridCoords = tempGridPos;
        tempSquircle.GetComponent<c_SquircleLogic>().GoToPosition(newWorldPos);
    }

    public void RotateSquirclesAtBottomLeftPos_Clockwise(Vector2Int _botLeftPos, BlockSize _squircleGroupSize)
    {
        GameObject tempSquircle = GetObjAtPosition(_botLeftPos, true);
        Vector2Int tempGridPos = _botLeftPos;

        int width = 2;
        if (_squircleGroupSize == BlockSize.ThreeWide)
            width = 3;

        int height = 2;
        if (_squircleGroupSize == BlockSize.ThreeTall)
            height = 3;

        Vector2Int gridPos = new Vector2Int();

        // Blocks on Bottom shift Left
        for (int x = 0; x < width - 1; x++)
        {
            gridPos = new Vector2Int(x + _botLeftPos.x + 1, _botLeftPos.y);
            MoveSquircleAtPosTowardDirection(gridPos, PathfindDirection.Left);
        }

        // Blocks on Right shift Down
        for (int y = 0; y < height - 1; y++)
        {
            // Board Logic Squircle Array Manipulation
            gridPos = new Vector2Int(_botLeftPos.x + width - 1, _botLeftPos.y + y + 1);
            MoveSquircleAtPosTowardDirection(gridPos, PathfindDirection.Down);
        }

        for (int x = width - 1; x > 0; x--)
        {
            // Board Logic Squircle Array Manipulation
            gridPos = new Vector2Int(_botLeftPos.x + x - 1, _botLeftPos.y + height - 1);
            MoveSquircleAtPosTowardDirection(gridPos, PathfindDirection.Right);
        }

        // Blocks on Left Side shift Up
        for (int y = height - 1; y > 1; y--)
        {
            // Board Logic Squircle Array Manipulation
            gridPos = new Vector2Int(_botLeftPos.x, _botLeftPos.y + y - 1);
            MoveSquircleAtPosTowardDirection(gridPos, PathfindDirection.Up);
        }


        tempGridPos.y++;
        SquircleArray[tempGridPos.y * BoardWidth + tempGridPos.x] = tempSquircle;

        Vector3 newWorldPos = GetWorldPosition(tempGridPos, true);
        tempSquircle.GetComponent<c_SquircleLogic>().GridCoords = tempGridPos;
        tempSquircle.GetComponent<c_SquircleLogic>().GoToPosition(newWorldPos);
    }

    int BoardWidth;
    int BoardHeight;
    float BlockScale = 0.85f;
    GameObject[] BoardArray;
    void UpdateBoardSize()
    {
        Vector2Int boardSize = GameLogic.GetBoardSize();
        BoardWidth  = boardSize.x;
        BoardHeight = boardSize.y;

        // Remember this is VR, don't move the camera. Move the game objects.
        // UpdateCameraPosition();
    }

    public GameObject CameraObject;
    void UpdateCameraPosition()
    {
        Vector3 newCameraPos = new Vector3();
        newCameraPos.x = (BlockScale / 2f);
        newCameraPos.y = (BoardHeight / 2f) - (BlockScale / 2f);
        newCameraPos.z = -12f;
        CameraObject.transform.position = newCameraPos;
    }

    float BlockTimeChange_MAX = 0.2f;
    IEnumerator ChangeBackdropColor(Vector2 _pos, BoardObject changeToBlock_)
    { 
        bool isDone = false;
        float fTimer = 0f;

        GameObject blockObj    = GetObjAtPosition(new Vector2Int(1, 1), true);
        GameObject backdropObj = GetObjAtPosition(new Vector2Int(1, 1), false);

        Material oldBlockMat = null;
        Material oldBackdropMat = null;
        Material newMat = null;

        switch (changeToBlock_)    
        {
            case BoardObject.Alpha_Active:
            case BoardObject.Alpha_Static:
                newMat = Mat_AlphaBlock;
                break;
            case BoardObject.Bravo_Active:
            case BoardObject.Bravo_Static:
                newMat = Mat_BravoBlock;
                break;
            case BoardObject.Filled:
                break;
            case BoardObject.Filled_Alpha:
                break;
            case BoardObject.Filled_Bravo:
                break;
            case BoardObject.Ghost:
                break;
            case BoardObject.Empty:
            default:
                newMat = Mat_EmptyBlock;
                break;
        }

        while(!isDone)
        {
            fTimer += Time.deltaTime;
            if(fTimer > BlockTimeChange_MAX )
            {
                fTimer = 1f;
                isDone = true;
            }

            float perc = fTimer / BlockTimeChange_MAX;

            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForEndOfFrame();
    }

    /// <summary>
    /// Returns the GameObject at the board position given
    /// </summary>
    /// <param name="_pos"></param> Position on the board
    /// <param name="_isSquircle"></param> True = Squircle Object, False = Backdrop Object
    /// <returns></returns>
    GameObject GetObjAtPosition(Vector2Int _gridPos, bool _isSquircle)
    {
        GameObject obj = null;

        if ( _isSquircle )
        {
            obj = SquircleArray[(_gridPos.y * BoardWidth) + _gridPos.x];
        }
        else
        {
            obj = BackdropArray[(_gridPos.y * BoardWidth) + _gridPos.x];
        }

        if (obj != null)
        {
            return obj;
        }

        
        return null;
    }

    void DestroyBackdropBlock(int _BackdropArrayPos)
    {
        GameObject blockRemove = BackdropArray[_BackdropArrayPos];

        // Already removing old block during 'Reduction Logic' process
        // BackdropArray.RemoveAt(_BackdropArrayPos);
        
        GameObject.Destroy(blockRemove);
    }

    public void DestroySquircleAtGridPos(Vector2Int _gridPos)
    {
        GameObject tempSquircle = GetObjAtPosition(_gridPos, true);
        tempSquircle.GetComponent<c_SquircleLogic>().DestroySquircle();

        SquircleArray[_gridPos.y * BoardWidth + _gridPos.x] = null;
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if(Input.GetKeyDown(KeyCode.P))
        {
            AddBlockToBoard(new Vector2Int( (BoardWidth / 2) - 1, BoardHeight - 1) , BoardObject.Bravo_Active);
        }

        if(Input.GetKeyDown(KeyCode.O))
        {
            foreach(GameObject squircle in SquircleArray)
            {
                if(squircle != null)
                {
                    Vector2Int gridCoords = squircle.GetComponent<c_SquircleLogic>().GridCoords;
                    print("Old: " + gridCoords);
                    if(gridCoords.y > 0)
                    {
                        gridCoords.y--;
                    }

                    // Assign new location to Squircle
                    squircle.GetComponent<c_SquircleLogic>().GridCoords = gridCoords;
                    
                    print("New: " + gridCoords);

                    squircle.GetComponent<c_SquircleLogic>().GoToPosition( GetWorldPosition(new Vector2Int(gridCoords.x, gridCoords.y), true ));
                }
            }
        }
        */
    }

    float defaultLeftPos;
    Vector3 GetWorldPosition(Vector2Int _gridCoords, bool _isSquircle = false)
    {
        // Vector3 tempPos = new Vector3(defaultLeftPos, -1.25f, 0);
        Vector3 tempPos = new Vector3();

        tempPos.x = (1.25f) * _gridCoords.x;
        tempPos.y = (1.25f) * _gridCoords.y;

        // tempPos.z = -3.05f;
        if (_isSquircle)
        {
            tempPos = GetObjAtPosition(_gridCoords, false).transform.position;
            tempPos.z += -0.35f;
        }
        
        return tempPos;
    }
}
