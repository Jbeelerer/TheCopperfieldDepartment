using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private int day = 0;
    private float time = 0;
    private float dayLength = 50;
    private int interval = 10;
    // Start is called before the first frame update
    void Start()
    {
        // singleton
        if (FindObjectsOfType(GetType()).Length > 1)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
    }
    private void FixedUpdate()
    {
        // implement day 
        time += Time.deltaTime;
        // print(time % dayLength / intervalAmount == 0); 
        print((int)(time % interval));
        if (time % interval == 0)
        {
            print("teeeeeeest");
            print(time);
        }
        if (dayLength <= time)
        {
            day++;
            print("EndDay");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
