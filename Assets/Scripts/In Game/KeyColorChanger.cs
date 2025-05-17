using UnityEngine;
using System;
using System.Collections.Generic;

public class KeyColorChanger : MonoBehaviour
{
    [Serializable]
    public class KeyColorConfig
    {
        public KeyCode key;                       // Tombol keyboard
        public GameObject targetObject;           // Objek panah yang berubah warna & punya animasi
        public GameObject glowObject;             // Objek glow yang menyala saat ditekan
        public GameObject beamObject;             // Objek beam
        public Color targetColor = Color.white;   // Warna saat ditekan

        [HideInInspector] public Color originalColor;
        [HideInInspector] public SpriteRenderer renderer;
        [HideInInspector] public Animator targetAnimator;      // NEW: Animator untuk targetObject
        [HideInInspector] public Animator glowAnimator;
        [HideInInspector] public Animator beamAnimator;
    }

    public List<KeyColorConfig> keyConfigs = new List<KeyColorConfig>();

    void Start()
    {
        foreach (var config in keyConfigs)
        {
            // Renderer & warna awal
            if (config.targetObject != null)
            {
                config.renderer = config.targetObject.GetComponent<SpriteRenderer>();
                if (config.renderer != null)
                {
                    config.originalColor = config.renderer.color;
                }

                // Ambil Animator untuk targetObject
                config.targetAnimator = config.targetObject.GetComponent<Animator>();
            }

            // Animator glow
            if (config.glowObject != null)
            {
                config.glowAnimator = config.glowObject.GetComponent<Animator>();
                config.glowObject.SetActive(false);
            }

            // Animator beam
            if (config.beamObject != null)
            {
                config.beamAnimator = config.beamObject.GetComponent<Animator>();
                config.beamObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        foreach (var config in keyConfigs)
        {
            if (config.renderer == null) continue;

            if (Input.GetKey(config.key))
            {
                // Warna saat hold
                Color holdColor = config.targetColor;
                holdColor.a = 1f;
                config.renderer.color = holdColor;

                // Aktifkan animasi targetObject
                if (config.targetAnimator != null)
                {
                    config.targetAnimator.SetBool("InHolding", true);
                }

                // Aktifkan glow
                if (config.glowObject != null)
                {
                    config.glowObject.SetActive(true);
                    config.glowAnimator.SetBool("IsHolding", true);
                }

                // Aktifkan beam
                if (config.beamObject != null)
                {
                    config.beamObject.SetActive(true);
                    config.beamAnimator.SetBool("IsHolding", true);
                }
            }

            if (Input.GetKeyUp(config.key))
            {
                // Reset warna
                config.renderer.color = config.originalColor;

                // Matikan animasi targetObject
                if (config.targetAnimator != null)
                {
                    config.targetAnimator.SetBool("InHolding", false);
                }

                // Nonaktifkan glow
                if (config.glowObject != null)
                {
                    config.glowAnimator.SetBool("IsHolding", false);
                }

                // Nonaktifkan beam
                if (config.beamObject != null)
                {
                    config.beamAnimator.SetBool("IsHolding", false);
                }
            }
        }
    }
}
