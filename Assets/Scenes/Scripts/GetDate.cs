using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GetDate : MonoBehaviour
{
    public Text text;

    // Update is called once per frame
    void Update()
    {
        text.text = System.DateTime.Now.ToString("dd/MM/yyyy   HH:mm:ss ");
    }
}
