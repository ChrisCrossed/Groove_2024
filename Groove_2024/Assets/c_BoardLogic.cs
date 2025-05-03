using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class c_BoardLogic : MonoBehaviour
{
    GameObject GameLogicObject;
    GameLogic GameLogic;

    List<GameObject> BackdropArray;
    List<GameObject> BlockArray;

    [SerializeField] private Material Mat_AlphaBlock;
    [SerializeField] private Material Mat_BravoBlock;
    [SerializeField] private Material Mat_EmptyBlock;

    [SerializeField] private Material Mat_AlphaBackdrop;
    [SerializeField] private Material Mat_BravoBackdrop;
    [SerializeField] private Material Mat_EmptyBackdrop;

    // Start is called before the first frame update
    void Start()
    {
        GameLogicObject = GameObject.Find("GameLogic");
        GameLogic = GameLogicObject.GetComponent<GameLogic>();

        BackdropArray = new List<GameObject>();
        BlockArray = new List<GameObject>();

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
    void InitializeBackdrop()
    {
        // Just a temporary test. Remember that you want Backdrop squares instead
        BackdropObjects = new List<GameObject>();

        for(int y = 0; y < BoardHeight; y++)
        {
            for(int x = 0; x < BoardWidth; x++)
            {
                GameObject tempBackdrop = CreateBackdropBlock(new Vector2Int(x, y));

                BackdropArray.Add(tempBackdrop);

                tempBackdrop.transform.SetParent( GameObject.Find("BackdropArray").transform );
                BackdropObjects.Add( tempBackdrop );
            }
        }
    }
    void ReconstructBackdropArray()
    {
        List<GameObject> newArray = new List<GameObject>();

        // Determine if width of board decreased or increased
        int oldWidth = BoardWidth;
        int oldHeight = BoardHeight;

        // UpdateBoardSize();
        // TESTING:
        BoardWidth += 2;

        int widthDiff = BoardWidth - oldWidth;

        if(widthDiff == 0)
        {
            return;
        }

        if( Mathf.Sign(widthDiff) == 1 )
        {
            int blocksAddPerSide = widthDiff / 2;

            for ( int y = 0; y < BoardHeight; y++ )
            {
                int currX;
                int yPos = (y * oldWidth);

                for ( int j = 0; j < blocksAddPerSide; j++ )
                {
                    print("Adding one block to left side");

                    // Creating a new X Position to each left side of the previous backdrop
                    currX = j - blocksAddPerSide + 1;

                    newArray.Add( CreateBackdropBlock( new Vector2Int(currX, y) ) );
                }

                for( int x = 0; x < oldWidth; x++ )
                {
                    newArray.Add( BackdropArray[y + x] );
                }

                for(int k = 0; k < blocksAddPerSide; k++ )
                {
                    print("Adding one block to right side");

                    // Adding new backdrop blocks to the right half of the new list
                    currX = oldWidth + k;

                    newArray.Add( CreateBackdropBlock( new Vector2Int( currX, y ) ) );
                }
            }
        }
        else
        {

        }

        BackdropArray = newArray;
    }

    GameObject CreateBackdropBlock(Vector2Int _gridPos)
    {
        GameObject tempBackdrop = GameObject.Instantiate(BackdropPrefab);

        // tempBackdrop.gameObject.transform.localScale = new Vector3(0.85f, 0.85f, 1.0f);
        tempBackdrop.name = "Backdrop";

        float leftPos = BoardWidth / 2f * -1f;
        Vector3 backdropPos = new Vector3(leftPos, -1.25f, 0);


        backdropPos.x += (1.25f) * _gridPos.x;
        backdropPos.y += (1.25f) * _gridPos.y;
        tempBackdrop.transform.position = backdropPos;

        return tempBackdrop;
    }

    public void AddBlockToBoard(Vector2 _pos, BoardObject _boardObjectType)
    {
        if(!(_boardObjectType == BoardObject.Alpha_Active || _boardObjectType == BoardObject.Bravo_Active))
        {
            print("WRONG BOARD OBJECT TYPE - 'AddBlockToBoard' (c_BoardLogic)");
            return;
        }

        GameObject tempSquircle = GameObject.Instantiate(SquirclePrefab);
        tempSquircle.name = "Alpha_Squircle";
        if(_boardObjectType == BoardObject.Bravo_Active)
        {
            tempSquircle.name = "Bravo_Squircle";
        }

        tempSquircle.gameObject.transform.localScale = new Vector3(0.7f, 0.7f, 1.0f);

        float leftPos = BoardWidth / 2f * -1f;
        Vector3 squirclePos = new Vector3(leftPos, -1.25f, 0);

        squirclePos.x = (1.25f) * _pos.x;
        squirclePos.y = (1.25f) * _pos.y;
        squirclePos.z += -0.35f;
        tempSquircle.transform.position = squirclePos;
    }

    int BoardWidth;
    int BoardHeight;
    float BlockScale = 0.85f;
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
    IEnumerator ChangeBlockColor(Vector2 _pos, BoardObject changeToBlock_)
    { 
        bool isDone = false;
        float fTimer = 0f;

        GameObject blockObj    = GetObjAtPosition(new Vector2(1, 1), true);
        GameObject backdropObj = GetObjAtPosition(new Vector2(1, 1), false);

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
    /// <param name="_isBlock"></param> True = Block Object, False = Backdrop Object
    /// <returns></returns>
    GameObject GetObjAtPosition(Vector2 _pos, bool _isBlock)
    {
        return null;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.L))
        {
            ReconstructBackdropArray();
        }
    }
}
