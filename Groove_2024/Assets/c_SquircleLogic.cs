using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class c_SquircleLogic : MonoBehaviour
{
    // Transitions between colors
    static float MoveTimer_MAX = 0.03f;
    static float DestroyTimer_MAX = 0.03f;

    public Material AlphaBackdropMaterial;
    public Material BravoBackdropMaterial;

    [ReadOnly(true)]
    public Vector2Int GridCoords;

    GameObject Mdl_Alpha;
    GameObject Mdl_Bravo;

    float SquircleScale;

    // Start is called before the first frame update
    void Awake()
    {
        gameObject.GetComponent<MeshRenderer>().enabled = false;

        Mdl_Alpha = gameObject.transform.Find("mdl_Alpha").gameObject;
        Mdl_Alpha.SetActive(false);

        Mdl_Bravo = gameObject.transform.Find("mdl_Bravo").gameObject;
        Mdl_Bravo.SetActive(false);
    }

    public void InitializeSquircle(BoardObject _boardObjectType, Vector2Int _gridCoords, float _modelScale)
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

        SquircleScale = _modelScale;
        gameObject.transform.localScale = new Vector3(_modelScale, _modelScale, 1.0f);
    }

    bool IsDestroy = false;
    float DestroyTimer;
    public void DestroySquircle()
    {
        IsDestroy = true;
        DestroyTimer = 0f;
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
            Update_MoveSquircle();
        }
        else if(IsDestroy)
        {
            Update_DestroySquircle();
        }
    }

    void Update_MoveSquircle()
    {
        if (MoveTimer < MoveTimer_MAX)
        {
            MoveTimer += Time.deltaTime;

            if (MoveTimer > MoveTimer_MAX)
            {
                MoveTimer = MoveTimer_MAX;

                IsMoving = false;
            }

            float lerp = MoveTimer / MoveTimer_MAX;
            Vector3 currPos = Vector3.Lerp(OldPosition, NewPosition, lerp);
            gameObject.transform.position = currPos;
        }
    }

    void Update_DestroySquircle()
    {
        if (DestroyTimer < DestroyTimer_MAX)
        {
            DestroyTimer += Time.deltaTime;

            if(DestroyTimer > DestroyTimer_MAX)
            {
                GameObject.Destroy(gameObject);
                return;
            }
                
            float lerp = DestroyTimer / DestroyTimer_MAX;
            lerp *= -1f;
            float squircleSize = lerp * SquircleScale;

            gameObject.transform.localScale = new Vector3(squircleSize, squircleSize, 1.0f);

        }
    }
}
