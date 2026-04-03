using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleMenuOption : MonoBehaviour
{
    public TitleOption optionType;

    public void HoverAnimStart()
    {
        GetComponent<Animator>().SetBool("Hovering", true);
    }

    public void HoverAnimStop()
    {
        GetComponent<Animator>().SetBool("Hovering", false);
    }

    public void Disable()
    {
        GetComponent<Collider>().enabled = false;
    }

    public void Enable()
    {
        GetComponent<Collider>().enabled = true;
    }
}
