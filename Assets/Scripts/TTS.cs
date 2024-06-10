using System.Diagnostics;
using UnityEngine;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using System.Speech.Synthesis;
#endif

public class TTS : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            Speak("This might be the most difficult case I have ever come across... The case files might be correct, but this city is not!!");
        }
    }

    public void Speak(string text)
    {
        print("test anything");
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        SpeechSynthesizer synth = new SpeechSynthesizer();
        synth.Speak(text);
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        print("testMac");
        Process process = new Process();
        process.StartInfo.FileName = "/bin/bash";
        process.StartInfo.Arguments = "-c \"say " + text + "\"";
        process.StartInfo.UseShellExecute = false;
        process.Start();
#else
                Debug.LogWarning("TTS not supported on this platform.");
#endif
    }
}