using Unity.Cinemachine;
using UnityEngine;

public class TrailerCameraMovement : MonoBehaviour
{
    public float duration = 10;
    private LerpTimer lerpTimer;
    private CinemachineSplineDolly dolly;
    void Start()
    {
        //startTime = Time.deltaTime;
        dolly = GetComponentInChildren<CinemachineSplineDolly>();
        lerpTimer = new LerpTimer(0, 1, duration);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            //GetComponent<CinemachineCamera>().Prioritize();
            GetComponentInChildren<CinemachineCamera>().Priority = 11;
            lerpTimer.Start();
        }

        dolly.CameraPosition = lerpTimer.Value;
    }
}

public class LerpTimer
{

    private float startTime;

    private float t;
    private float f;
    private float d;

    private bool started = false;

    public float ElapsedTime
    {
        get
        {
            if (started)
                return Time.time - startTime;
            else
                return 0;
        }
    }

    public float Value
    {
        get
        {
            if (started)
                return Mathf.SmoothStep(f, t, (ElapsedTime) / d);
            else
                return f;
        }
    }

    public LerpTimer(float from, float to, float duration)
    {
        t = to;
        f = from;
        d = duration;
    }

    public void Start()
    {
        startTime = Time.time;
        started = true;
    }
}