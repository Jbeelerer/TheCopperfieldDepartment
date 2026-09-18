using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class TimeMachine : MonoBehaviour
{
    [SerializeField] private GameObject newDayPrefab;

    private Animator anim;
    private CinemachineCamera cam;
    private CinemachineBrain camBrain;
    private AudioManager am;

    private void Start()
    {
        anim = GetComponent<Animator>();
        cam = GetComponentInChildren<CinemachineCamera>();
        camBrain = FindFirstObjectByType<CinemachineBrain>();
        am = AudioManager.instance;
    }

    /*private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            NewDayTransition();
        }
    }*/

    public void NewDayTransition()
    {
        cam.Priority = 15;
        anim.Play("NightTransition");
    }

    // Functions used during newDayTransition animation event
    public void StartNewDay()
    {
        GameObject g = Instantiate(newDayPrefab);
        GameManager.instance.SetGameState(GameState.Playing);
        cam.Priority = 0;
        am.FadeInMusic(4f);
    }
    public void PlaySoundDuringAnimation(AudioClip clip)
    {
        am.PlayAudio(clip);
    }
    public void FadeOutLowpassDuringAnimation()
    {
        am.FadeOutLowPassMusic(1f);
    } 
}
