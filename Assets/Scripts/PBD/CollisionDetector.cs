using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetector : MonoBehaviour
{
    public Vector3 lastImpulse;
    public Vector3 impulse;
    void FixedUpdate()
    {
        lastImpulse = impulse;
        impulse = Vector3.zero;
    }

    void OnCollisionEnter(Collision col)
    {
        impulse += col.impulse;
        var renderer = this.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.red;
        }
    }
    void OnCollisionExit(Collision col)
    {
        var renderer = this.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.white;
        }
    }

    void OnCollisionStay(Collision col)
    {
        impulse += col.impulse;
    }
}
