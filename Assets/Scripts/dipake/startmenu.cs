using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class startmenu : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();

        // Check if the Animator is attached
        if (animator == null)
        {
            Debug.LogError("Animator component not found on this GameObject!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (animator != null)
            {
                animator.SetTrigger("Startmenu");  // Assuming you are using a trigger in Animator for "Logo"
            }
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            if (animator != null)
            {
                animator.SetTrigger("Startmenu2");  // Assuming you are using a trigger in Animator for "LogoPutar"
            }
        }
    }
}
