using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class c_SquircleLogic : MonoBehaviour
{
    static float TransitionTimer_MAX = 0.15f;
    float TransitionTimer;

    public Material AlphaBackdropMaterial;
    public Material BravoBackdropMaterial;

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

    public void InitializeSquircle(BoardObject _boardObjectType)
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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
