using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeController : MonoBehaviour
{
    public float timeScale = 1f;
    public float fixedDeltaTime = 0.02f;
    private float originalFixedDeltaTime;

    private void Awake()
    {
        Time.timeScale = timeScale;
        originalFixedDeltaTime = Time.fixedDeltaTime;
        Time.fixedDeltaTime = fixedDeltaTime;
    }

    private void Update()
    {
        Time.timeScale = timeScale;
        Time.fixedDeltaTime = fixedDeltaTime;
    }

    private void OnDestroy()
    {
        Time.timeScale = 1;
        Time.fixedDeltaTime = originalFixedDeltaTime;
    }
}