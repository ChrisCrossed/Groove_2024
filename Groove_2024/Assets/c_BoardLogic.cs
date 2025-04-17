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
        Squircles = new List<GameObject>();

        GameObject tempSquircle = GameObject.Instantiate(SquirclePrefab);
        tempSquircle.gameObject.transform.localScale = new Vector3(0.85f, 0.85f, 1.0f);
        tempSquircle.name = "TestSquircle";
        tempSquircle.transform.position = new Vector3(4, 4, 4);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
