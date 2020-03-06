using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Rope : MonoBehaviour
{
    // Appearance settings
    [SerializeField] protected int segmentCount = 30;

    [SerializeField] protected float width = 0.1f;
    [SerializeField] protected float length = 3f;
    public float SegmentLength => length / segmentCount;
    [SerializeField] protected Material material = null;

    // Physics settings
    [SerializeField] protected bool rootFixed = true;

    [SerializeField] protected float mass = 0.5f;
    public float MassPerSegment => mass / segmentCount;
    [SerializeField] protected float drag = 0.01f;
    [SerializeField] protected float angularDrag = 0.01f;

    [SerializeField] protected CollisionDetectionMode collisionDetectionMode = CollisionDetectionMode.Continuous;
    [SerializeField] protected RigidbodyInterpolation interpolation = RigidbodyInterpolation.Interpolate;

    [SerializeField] protected bool isKinematic = false;
    [SerializeField] protected bool isTrigger = false;
    [SerializeField] protected PhysicMaterial physicMaterial = null;

    // Contents of the rope
    public Transform[] segments { get; protected set; } = null;


    protected void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireSphere(Vector3.zero, 0.1f);

        for (int i = 0; i < segmentCount; i++)
        {
            Gizmos.DrawWireCube(Vector3.down * SegmentLength * (i + 0.5f),
                new Vector3(width, SegmentLength, width));
        }
        Gizmos.DrawWireCube(Vector3.down * length / 2, new Vector3(width, length, width));
    }
}