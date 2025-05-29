using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class c_SquircleLogic : MonoBehaviour
{
    // Transitions between colors
    static float MoveTimer_MAX = 0.05f;

    public Material AlphaBackdropMaterial;
    public Material BravoBackdropMaterial;

    [ReadOnly(true)]
    public Vector2Int GridCoords;

    GameObject Mdl_Alpha;
    GameObject Mdl_Bravo;

    // Start is called before the first frame update
    void Awake()
    {
        gameObject.GetComponent<MeshRenderer>().enabled = false;

        Mdl_Alpha = gameObject.transform.Find("mdl_Alpha").gameObject;
        Mdl_Alpha.SetActive(false);

        Mdl_Bravo = gameObject.transform.Find("mdl_Bravo").gameObject;
        Mdl_Bravo.SetActive(false);
    }

    public void InitializeSquircle(BoardObject _boardObjectType, Vector2Int _gridCoords)
    {
        if(_boardObjectType == BoardObject.Alpha_Static || _boardObjectType == BoardObject.Alpha_Active)
        {
            gameObject.GetComponent<MeshRenderer>().material = AlphaBackdropMaterial;

            gameObject.GetComponent<MeshRenderer>().enabled = true;

            Mdl_Alpha.SetActive(true);
        }
        else if(_boardObjectType == BoardObject.Bravo_Static || _boardObjectType== BoardObject.Bravo_Active)
        {
            gameObject.GetComponent<MeshRenderer>().material = BravoBackdropMaterial;

            gameObject.GetComponent<MeshRenderer>().enabled = true;

            Mdl_Bravo.SetActive(true);
        }

        GridCoords = _gridCoords;
    }

    bool IsMoving = false;
    float MoveTimer;
    Vector3 OldPosition;
    Vector3 NewPosition;
    public void GoToPosition(Vector3 _newPosition)
    {
        IsMoving = true;
        MoveTimer = 0f;
        OldPosition = gameObject.transform.position;
        NewPosition = _newPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsMoving)
        {
            if(MoveTimer < MoveTimer_MAX)
            {
                MoveTimer += Time.deltaTime;

                if(MoveTimer > MoveTimer_MAX)
                {
                    MoveTimer = MoveTimer_MAX;

                    IsMoving = false;
                }

                float lerp = MoveTimer / MoveTimer_MAX;
                Vector3 currPos = Vector3.Lerp(OldPosition, NewPosition, lerp);
                gameObject.transform.position = currPos;
            }
        }
    }
}
