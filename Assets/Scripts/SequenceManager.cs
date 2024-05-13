using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SequenceManager : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private Animator stickerAnimator;
    private GameManager gm;

    [SerializeField] private Image sticker;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        gm = GameManager.instance;
        if (gm.investigationState != investigationStates.SuspectFound)
        {
            // change the sticker inside the animator of the sticker
            stickerAnimator.SetBool(gm.investigationState == investigationStates.SuspectSaved ? "saved" : "notFound", true);
        }
    }

    public void PlaySequence(string sequenceName)
    {
        animator.Play(sequenceName);
    }
}
