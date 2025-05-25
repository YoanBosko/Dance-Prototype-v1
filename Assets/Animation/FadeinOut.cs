using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeinOut : MonoBehaviour
{
    private Animator mAnimator;
    public GameObject menuCanvas;

    [Header("Trigger Names (from Animator)")]
    public string fadeInTrigger = "IN";
    public string fadeOutTrigger = "OUT";

    [Header("Optional: Auto-disable delay after Fade Out")]
    public float fadeOutDelay = 1f;

    void Awake()
    {
        mAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        if (mAnimator != null)
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                FadeIn();
            }

            if (menuCanvas.gameObject.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.F))
                    FadeOut();
            }
        }
        
    }

    // Ini bisa dipanggil dari Button, Inspector, EventTrigger, dll
    public void FadeIn()
    {
        gameObject.SetActive(true);
        if (mAnimator && !string.IsNullOrEmpty(fadeInTrigger))
            mAnimator.SetTrigger(fadeInTrigger);
    }

    public void FadeOut()
    {
        if (mAnimator && !string.IsNullOrEmpty(fadeOutTrigger))
            mAnimator.SetTrigger(fadeOutTrigger);

        // StartCoroutine(DisableAfterDelay(fadeOutDelay));
    }

    private IEnumerator DisableAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }
}