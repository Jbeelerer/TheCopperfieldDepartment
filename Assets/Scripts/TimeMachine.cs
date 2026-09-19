using System;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;

public class TimeMachine : MonoBehaviour
{
    [SerializeField] private GameObject newDayPrefab;
    [SerializeField] private GameObject receiptCanvas;
    [SerializeField] private TMP_Text receiptHeadersText;
    [SerializeField] private TMP_Text receiptInfosText;

    private Animator anim;
    private CinemachineCamera cam;
    private AudioManager am;

    private void Start()
    {
        anim = GetComponent<Animator>();
        cam = GetComponentInChildren<CinemachineCamera>();
        am = AudioManager.instance;

        receiptCanvas.SetActive(false);
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
        var underlineSpace = "                         end";
        receiptCanvas.SetActive(true);
        receiptHeadersText.text = $"" +
            $"Name{underlineSpace}\n" +
            $"No.{underlineSpace}\n\n" +
            $"{GameManager.instance.GetCurrentDate().AddDays(-1).ToString("MM/dd/yyyy", new System.Globalization.CultureInfo("en-US"))}\n" +
            $"In{underlineSpace}\n" +
            $"Out{underlineSpace}\n" +
            $"Total{underlineSpace}";

        var timeIn = UnityEngine.Random.Range(3, 6);
        var timeOut = UnityEngine.Random.Range(8, 11);
        var totalTime = timeOut - timeIn + 12;
        receiptInfosText.text = $"" +
            $"Gary Clueson\n" +
            $"001842\n\n\n" +
            $"{TimeSpan.FromHours(timeIn):h\\:mm}am\n" +
            $"{TimeSpan.FromHours(timeOut):h\\:mm}pm\n" +
            $"{totalTime}h 00min";

        cam.Priority = 15;
        anim.Play("NightTransition");
    }

    // --- Functions used during newDayTransition animation event ---
    public void StartNewDay()
    {
        receiptCanvas.SetActive(false);
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
    // ---
}
