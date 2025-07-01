using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class c_BackdropLogic : MonoBehaviour
{
    // Transitions between colors
    static float MoveTimer_MAX = 0.25f;
    static float DestroyTimer_MAX = 0.25f;

    GameObject mdl_GhostBlock;
    GameObject mdl_Grid;


    // Start is called before the first frame update
    void Awake()
    {
        mdl_GhostBlock = gameObject.transform.Find("GhostBlock").gameObject;
        mdl_GhostBlock.SetActive(false);

        mdl_Grid = gameObject.transform.Find("Grid").gameObject;
        mdl_Grid.SetActive(false);
    }

    public void InitializeBackdrop(bool _isGrid = true)
    {
        if (_isGrid)
        {
            mdl_Grid.SetActive(true);
            return;
        }

        if (!_isGrid)
        {
            mdl_GhostBlock.SetActive(true);
            return;
        }
    }

    bool IsDestroy = false;
    float DestroyTimer;
    public void DestroyBackdrop()
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

    void Update_MoveBackdrop()
    {

    }

    void Update_DestroyBackdrop()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(IsMoving)
        {
            Update_MoveBackdrop();
        }
        else if (IsDestroy)
        {
            Update_DestroyBackdrop();
        }
    }
}
