using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.UI;

public class TextFlashTest : MonoBehaviour
{
    [SerializeField] GameObject GO_Text;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        StartCoroutine( TextFlash() );
    }

    IEnumerator TextFlash()
    {
        bool flip = true;

        while(true)
        {
            yield return new WaitForSeconds(1.0f);

            flip = !flip;

            GO_Text.SetActive(flip);
        }

        yield return null;
    }
}
