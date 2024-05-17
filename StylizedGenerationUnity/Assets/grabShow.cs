using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class grabShow : MonoBehaviour
{
    public TMP_Text text;

    void Update()
    {
        text.text = Screen.currentResolution.width + "x" + Screen.currentResolution.height;
    }
}
