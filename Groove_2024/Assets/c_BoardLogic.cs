using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class c_BoardLogic : MonoBehaviour
{
    GameObject GameLogicObject;
    GameLogic GameLogic;
    // Start is called before the first frame update
    void Start()
    {
        GameLogicObject = GameObject.Find("GameLogic");
        GameLogic = GameLogicObject.GetComponent<GameLogic>();

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
                GameObject tempBackdrop = GameObject.Instantiate(BackdropPrefab);
                // tempBackdrop.gameObject.transform.localScale = new Vector3(0.85f, 0.85f, 1.0f);
                tempBackdrop.name = "Backdrop";

                // Bottom Left Corner Pos + (.85 scale + 1.25 buffer) * GridPos
                // BottomLeft = Vector3(-7.5,-1.25,0)
                float leftPos = BoardWidth / 2f * -1f;
                Vector3 newPos = new Vector3(leftPos, -1.25f, 0);
                newPos.x += (1.25f) * x;
                newPos.y += (1.25f) * y;
                tempBackdrop.transform.position = newPos;

                tempBackdrop.transform.SetParent( GameObject.Find("BackdropArray").transform );
                BackdropObjects.Add( tempBackdrop );
            }
        }
    }

    int BoardWidth;
    int BoardHeight;
    float BlockScale = 0.85f;
    void UpdateBoardSize()
    {
        Vector2Int boardSize = GameLogic.GetBoardSize();
        BoardWidth  = boardSize.x;
        BoardHeight = boardSize.y;

        UpdateCameraPosition();
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
