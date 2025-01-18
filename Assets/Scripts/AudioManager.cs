using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    private AudioSource audioSource;

    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    [SerializeField] private AudioMixerGroup musicMixerGroup;
    [SerializeField] private AudioMixerGroup voiceMixerGroup;

    private List<AudioSource> repeatingAudioSources = new List<AudioSource>();

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
        audioSource = CreateNewSfxSource();
    }

    public void PlayAudio(AudioClip audioClip, float volume = 1f, GameObject parent = null)
    {
        AudioSource newSource = CreateNewSfxSource(parent);
        newSource.clip = audioClip;
        newSource.volume = volume;
        newSource.Play();
        StartCoroutine(DeleteAudioSource(newSource, newSource.clip.length));
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
    public void PlayAudioRepeating(AudioClip audioClip, float fadeDuration = 0.01f, GameObject parent = null)
    {
        foreach (AudioSource source in repeatingAudioSources)
        {
            if (source.clip == audioClip)
            {
                return;
            }
        }

        AudioSource newSource = CreateNewSfxSource(parent);
        newSource.loop = true;
        newSource.clip = audioClip;
        newSource.volume = 0.9f;
        repeatingAudioSources.Add(newSource);
        StartCoroutine(FadeIn(newSource, fadeDuration));
    }

    public void StopAudioRepeating(AudioClip audioClip, float fadeDuration = 0f)
    {
        foreach (AudioSource source in repeatingAudioSources)
        {
            if (source.clip == audioClip)
            {
                repeatingAudioSources.Remove(source);
                StartCoroutine(FadeOut(source, fadeDuration));
                return;
            }
        }
    }

    private IEnumerator FadeIn(AudioSource source, float fadeDuration)
    {
        source.Play();

        float timeElapsed = 0;
        while (source && source.volume < 1)
        {
            source.volume = Mathf.Lerp(0, 1, timeElapsed / fadeDuration);
            timeElapsed += Time.deltaTime;
            yield return true;
        }
    }

    private IEnumerator FadeOut(AudioSource source, float fadeDuration)
    {
        float timeElapsed = 0;
        while (source && source.volume > 0)
        {
            source.volume = Mathf.Lerp(1, 0, timeElapsed / fadeDuration);
            timeElapsed += Time.deltaTime;
            yield return true;
        }

        if (!source)
            yield break;

        StartCoroutine(DeleteAudioSource(source));
    }

    private IEnumerator DeleteAudioSource(AudioSource source, float delay = 0)
    {
        yield return new WaitForSeconds(delay);

        source.Stop();
        Destroy(source);
    }

    public bool IsPlaying(AudioClip audioClip)
    {
        foreach (AudioSource source in repeatingAudioSources)
        {
            if (source.clip == audioClip)
            {
                return true;
            }
        }

        return false;
    }

    private AudioSource CreateNewSfxSource(GameObject parent = null)
    {
        AudioSource newSource;
        if (parent)
        {
            newSource = parent.AddComponent<AudioSource>();
        }
        else
        {
            newSource = gameObject.AddComponent<AudioSource>();
        }
        newSource.outputAudioMixerGroup = sfxMixerGroup;
        return newSource;
    }

    public void UpdateMixerValue(string parameterName, float value)
    {
        switch (parameterName)
        {
            case "Music Volume":
                musicMixerGroup.audioMixer.SetFloat(parameterName, Mathf.Log10(value) * 20);
                break;
            case "SFX Volume":
                sfxMixerGroup.audioMixer.SetFloat(parameterName, Mathf.Log10(value) * 20);
                break;
            case "Voice Volume":
                voiceMixerGroup.audioMixer.SetFloat(parameterName, Mathf.Log10(value) * 20);
                break;
        }
    }
}
