using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public AudioSource audioSource;
    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void PlayAudio(AudioClip audioClip)
    {
        audioSource.clip = audioClip;
        audioSource.Play();
    }
    public IEnumerator ResetAudio(float length)
    {
        yield return new WaitForSeconds(length);
        audioSource.pitch = 1;
        audioSource.time = 0;
    }
    public void PlayReverseAudio(AudioClip audioClip)
    {
        audioSource.clip = audioClip;
        audioSource.pitch = -1;
        audioSource.time = audioSource.clip.length - 0.01f;
        audioSource.Play();
        StartCoroutine(ResetAudio(audioSource.clip.length));
    }

    public void SkipAmount(float amount)
    {
        if (audioSource.time + amount < audioSource.clip.length)
        {
            audioSource.Stop();
            audioSource.time += amount;
            audioSource.Play();
        }

    }
}
