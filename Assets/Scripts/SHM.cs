using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHM : MonoBehaviour
{
    public float radius;
    public float freq;
    public Vector3 direction;
    Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.position;
    }
    void Update()
    {
        transform.position = initialPosition + Mathf.Sin(Time.time*2*Mathf.PI*freq) * direction * radius;
    }
}
