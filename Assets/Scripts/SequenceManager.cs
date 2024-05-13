using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SequenceManager : MonoBehaviour
{
    private Animator animator;
    private GameManager gm;

    [SerializeField] private Image sticker;


    [SerializeField] private Sprite suspectFoundSticker;
    [SerializeField] private Sprite suspectNotFoundSticker;
    [SerializeField] private Sprite suspectSavedSticker;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        gm = GameManager.instance;
        sticker.sprite = gm.investigationState == investigationStates.SuspectFound ? suspectFoundSticker : gm.investigationState == investigationStates.SuspectSaved ? suspectSavedSticker : suspectNotFoundSticker;
        sticker.transform.GetComponentInChildren<Image>().sprite = sticker.sprite;
    }

    public void PlaySequence(string sequenceName)
    {
        animator.Play(sequenceName);
    }
}
