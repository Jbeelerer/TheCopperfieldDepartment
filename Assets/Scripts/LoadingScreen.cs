using UnityEngine;

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

    public void Show()
    {
        anim.SetBool("Hide", false);
    }

    public void Hide(bool instant = false)
    {
        anim.SetBool("Hide", true);
    }
}