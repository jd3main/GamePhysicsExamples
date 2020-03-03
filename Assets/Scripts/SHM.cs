using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHM : MonoBehaviour
{
    public float radius;
    public float freq;
    public Vector3 direction;
    private Vector3 initialPosition;

    private void Start()
    {
        initialPosition = transform.position;
    }

    private void Update()
    {
        transform.position = initialPosition + Mathf.Sin(Time.time * 2 * Mathf.PI * freq) * direction * radius;
    }
}