using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntroPresentation : MonoBehaviour
{

    [SerializeField] private Sprite[] slides;
    [SerializeField] private Image slideImage;
    [SerializeField] private GameObject mousePrompt;
    [SerializeField] private GameObject blackScreen;

    private Narration narration;

    private int currentSlideIndex = -1;

    void Start()
    {
        narration = FindFirstObjectByType<Narration>();

        slideImage.sprite = slides[0];
        mousePrompt.SetActive(false);

        ShowNextSlide();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && mousePrompt.activeInHierarchy)
        {
            ShowNextSlide();
            ShowMousePromptDelayed();
        }
    }

    private void ShowNextSlide()
    {
        currentSlideIndex++;
        //narration.PlaySequence("");
        slideImage.sprite = slides[currentSlideIndex];
        mousePrompt.SetActive(false);
        // TODO: Temporary fix, mouseprompt should be triggered when narration ends
        ShowMousePromptDelayed();
    }

    public void ShowMousePrompt()
    {
        mousePrompt.SetActive(true);
    }

    private IEnumerator ShowMousePromptDelayed()
    {
        yield return new WaitForSeconds(3f);
        ShowMousePrompt();
    }
}
