using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Radio : MonoBehaviour
{
    [SerializeField] private AudioClip[] radioMusicChanel1;
    [SerializeField] private AudioClip[] radioMusicChanel2;
    [SerializeField] private AudioClip[] radioTalkShow2;
    [SerializeField] private AudioClip staticSound;

    private AudioSource audioSource;

    private int chanelAmount = 3;
    private int currentChanel = 1;

    private AudioManager audioManager;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioManager = FindObjectOfType<AudioManager>();
        PlayChanel(false);
    }

    public void ChangeChanel()
    {
        audioManager.PlayAudio(staticSound, 0.3f);
        currentChanel++;
        if (currentChanel > chanelAmount)
        {
            currentChanel = 1;
        }
        PlayChanel();
    }
    private void PlayChanel(bool fromStart = false)
    {
        float volume = 0.5f;
        StopAllCoroutines();
        switch (currentChanel)
        {
            case 1:
                audioSource.clip = radioMusicChanel1[Random.Range(0, radioMusicChanel1.Length)];
                break;
            case 2:
                audioSource.clip = radioMusicChanel2[Random.Range(0, radioMusicChanel2.Length)];
                break;
            case 3:
                audioSource.clip = radioTalkShow2[Random.Range(0, radioTalkShow2.Length)];
                volume = 0.8f;
                break;
        }
        if (fromStart)
        {
            audioSource.time = 0;
        }
        else
        {
            audioSource.time = Random.Range(0, audioSource.clip.length / 2);
            StartCoroutine(StartFade(audioSource, 1f, volume));

        }
        audioSource.Play();
        StartCoroutine(HandleNextSong(audioSource));
    }
    private IEnumerator HandleNextSong(AudioSource source)
    {
        var waitForClipRemainingTime = new WaitForSeconds(GetClipRemainingTime(source));
        yield return waitForClipRemainingTime;
        PlayChanel(true);
        // Trigger event here!
    }
    public static float GetClipRemainingTime(AudioSource source)
    {
        float remainingTime = (source.clip.length - source.time) / source.pitch;
        return source.pitch < 0f ?
            (source.clip.length + remainingTime) :
            remainingTime;
    }
    public static IEnumerator StartFade(AudioSource audioSource, float duration, float targetVolume)
    {
        float currentTime = 0;
        float start = 0;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
            yield return null;
        }
        yield break;
    }
}
