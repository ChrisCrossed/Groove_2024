using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class c_BoardLogic : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        InitializeSquircles();
        TestBoard();
    }

    void TestBoard()
    {

    }

    public GameObject SquirclePrefab;
    List<GameObject> Squircles;
    void InitializeSquircles()
    {
        // Just a temporary test. Remember that you want Backdrop squares instead
        Squircles = new List<GameObject>();

        for(int y = 0; y < 3; y++)
        {
            for(int x = 0; x < 12; x++)
            {
                GameObject tempSquircle = GameObject.Instantiate(SquirclePrefab);
                tempSquircle.gameObject.transform.localScale = new Vector3(0.85f, 0.85f, 1.0f);
                tempSquircle.name = "TestSquircle";

                // Bottom Left Corner Pos + (.85 scale + 1.25 buffer) * GridPos
                // BottomLeft = Vector3(-7.5,-1.25,0)
                Vector3 newPos = new Vector3(-7.5f, -1.25f, 0);
                newPos.x += (1.25f) * x;
                newPos.y += (1.25f) * y;
                tempSquircle.transform.position = newPos;
            }
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
