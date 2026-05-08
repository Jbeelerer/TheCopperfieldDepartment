using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance;

    private Animator anim;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        anim = GetComponentInChildren<Animator>();
    }

    public void SwitchScene(string targetScene) => StartCoroutine(SwitchSceneCoroutine(targetScene));

    private IEnumerator SwitchSceneCoroutine(string targetScene)
    {
        anim.SetBool("Hide", false);

        yield return null;

        var oldScene = SceneManager.GetActiveScene().name;

        EventSystem.current.gameObject.SetActive(false);

        AsyncOperation op = SceneManager.LoadSceneAsync(targetScene, LoadSceneMode.Additive);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
            yield return null;

        op.allowSceneActivation = true;

        while (!op.isDone)
            yield return null;

        Scene newScene = SceneManager.GetSceneByName(targetScene);
        SceneManager.SetActiveScene(newScene);

        yield return SceneManager.UnloadSceneAsync(oldScene);

        anim.SetBool("Hide", true);
    }
}