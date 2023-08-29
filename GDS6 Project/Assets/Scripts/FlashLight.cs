using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flashlight : MonoBehaviour
{
    public GameObject flashlight;

    public bool on;
    public bool off;

    public KeyCode flashlightKey = KeyCode.F;



    void Start()
    {
        off = true;
        flashlight.SetActive(false);
    }




    void Update()
    {
        if (off && Input.GetKeyDown(flashlightKey))
        {
            flashlight.SetActive(true);
            off = false;
            on = true;
        }
        else if (on && Input.GetKeyDown(flashlightKey))
        {
            flashlight.SetActive(false);
            off = true;
            on = false;
        }



    }
}