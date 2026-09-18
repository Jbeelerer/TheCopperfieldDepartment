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

    private float originalMusicVolume = 1f;
    private float originalSFXVolume = 1f;

    private float lowpassOnValue = 290f;
    private float lowpassOffValue = 22000f;

    private List<AudioSource> repeatingAudioSources = new List<AudioSource>();

    // Start is called before the first frame update
    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(instance);
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
    public void PlayAudioRepeating(AudioClip audioClip, float fadeDuration = 0.01f, float volume = 1f, GameObject parent = null)
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
        newSource.volume = volume;
        repeatingAudioSources.Add(newSource);
        StartCoroutine(FadeIn(newSource, fadeDuration, volume));
    }

    public void StopAudioRepeating(AudioClip audioClip, float fadeDuration = 0.01f)
    {
        foreach (AudioSource source in repeatingAudioSources)
        {
            if (source.clip == audioClip)
            {
                repeatingAudioSources.Remove(source);
                StartCoroutine(FadeOut(source, fadeDuration, source.volume));
                return;
            }
        }
    }

    public void FadeOutMusic(float fadeDuration)
    {
        StartCoroutine(FadeOutMusicCoroutine(fadeDuration));
    }

    public void FadeInMusic(float fadeDuration)
    {
        StartCoroutine(FadeInMusicCoroutine(fadeDuration));
    }

    public void FadeOutLowPassMusic(float fadeDuration)
    {
        StartCoroutine(FadeOutLowPassMusicCoroutine(fadeDuration));
    }

    public void FadeInLowPassMusic(float fadeDuration)
    {
        StartCoroutine(FadeInLowPassMusicCoroutine(fadeDuration));
    }

    private IEnumerator FadeIn(AudioSource source, float fadeDuration, float maxVolume)
    {
        source.Play();

        float timeElapsed = 0;
        while (source && source.volume < maxVolume)
        {
            source.volume = Mathf.Lerp(0, maxVolume, timeElapsed / fadeDuration);
            timeElapsed += Time.deltaTime;
            yield return true;
        }
    }

    private IEnumerator FadeOut(AudioSource source, float fadeDuration, float maxVolume)
    {
        float timeElapsed = 0;
        while (source && source.volume > 0)
        {
            source.volume = Mathf.Lerp(maxVolume, 0, timeElapsed / fadeDuration);
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

    private IEnumerator FadeOutMusicCoroutine(float fadeDuration)
    {
        //float fadeTime = 2f;
        float t = fadeDuration;
        float musicVolume = FindFirstObjectByType<SettingsMenu>(FindObjectsInactive.Include).musicVolume;
        while (t > 0)
        {
            yield return null;
            t -= Time.deltaTime;
            UpdateMixerValue("Music Volume", musicVolume * (t / fadeDuration));
        }
        yield break;
    }

    private IEnumerator FadeInMusicCoroutine(float fadeDuration)
    {
        float t = fadeDuration;
        float musicVolume = FindFirstObjectByType<SettingsMenu>(FindObjectsInactive.Include).musicVolume;
        while (t > 0)
        {
            yield return null;
            t -= Time.deltaTime;
            UpdateMixerValue("Music Volume", musicVolume * (1 - (t / fadeDuration)));
        }
        yield break;
    }

    private IEnumerator FadeInLowPassMusicCoroutine(float fadeTime)
    {
        float t = fadeTime;
        //float lowpassStartValue = 290f;
        //float lowpassEndValue = 22000f;
        float lowpassDifference = lowpassOffValue - lowpassOnValue;
        while (t > 0)
        {
            yield return null;
            t -= Time.deltaTime;
            musicMixerGroup.audioMixer.SetFloat("MusicLowpassCutoff", lowpassOnValue + (lowpassDifference * (t / fadeTime)));
        }
        yield break;
    }

    private IEnumerator FadeOutLowPassMusicCoroutine(float fadeTime)
    {
        float t = fadeTime;
        //float lowpassOnValue = 290f;
        //float lowpassEndValue = 22000f;
        float lowpassDifference = lowpassOffValue - lowpassOnValue;
        while (t > 0)
        {
            yield return null;
            t -= Time.deltaTime;
            musicMixerGroup.audioMixer.SetFloat("MusicLowpassCutoff", lowpassOffValue - (lowpassDifference * (t / fadeTime)));
        }
        yield break;
    }

    public bool IsPlayingRepeated(AudioClip audioClip)
    {
        return repeatingAudioSources.Exists(source => source.clip == audioClip);
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
            case "Title Music Volume":
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

    public void StartSequenceMix()
    {  
        musicMixerGroup.audioMixer.GetFloat("Music Volume", out originalMusicVolume);
        musicMixerGroup.audioMixer.SetFloat("Music Volume", Mathf.Log10(0.0001f) * 20);
        sfxMixerGroup.audioMixer.GetFloat("SFX Volume", out originalSFXVolume);
        sfxMixerGroup.audioMixer.SetFloat("SFX Volume", Mathf.Log10(0.0001f) * 20);  
    } 
    public void EndSequenceMix()
    {  
        musicMixerGroup.audioMixer.SetFloat("Music Volume", originalMusicVolume );
        sfxMixerGroup.audioMixer.SetFloat("SFX Volume", originalSFXVolume );
    } 
}
