using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class Orbit : MonoBehaviour
{
    public enum IntegrationMethod
    {
        Euler,
        SemiImplicitEuler,
        Verlet,
        VelocityVerlet,
        Real,
    }

    public IntegrationMethod integrationMethod;
    public bool circularMotion = true;

    public float G = 1;
    public float M = 10;
    public Transform center;
    public Transform orbiter;
    public Vector3 A;
    public Vector3 V;
    public Vector3 X { get => orbiter.position; set { orbiter.position = value; } }
    public Vector3 R => center.position - X;
    public Vector3 Ac => (G * M * R.normalized / R.sqrMagnitude);

    private Vector3 prevX;
    private Vector3 nextX;
    private float realAngularVelocity;
    private float realAngle;
    private float radius;

    public bool recordEnergy = true;
    [HideInInspector] public const float m = 1;
    [HideInInspector] public float kineticEnergy;
    [HideInInspector] public float potentialEnergy;
    public float totalEnergy => kineticEnergy + potentialEnergy;


    private void Start()
    {
        X = orbiter.position;
        if (circularMotion)
            SetupCircularMotionSpeed();

        if (integrationMethod == IntegrationMethod.Verlet)
        {
            float dt = Time.fixedDeltaTime;
            prevX = X - V * dt + 0.5f * Ac * dt * dt;
        }

        // ω = V/R
        radius = R.magnitude;
        realAngularVelocity = Mathf.Sqrt(G * M / radius) / radius;
        realAngle = Mathf.Atan2(X.z, X.x);
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        A = Ac;
        switch (integrationMethod)
        {
            case IntegrationMethod.Euler:
                X = X + (V * dt);
                V = V + (A * dt);
                break;

            case IntegrationMethod.SemiImplicitEuler:
                V = V + (A * dt);
                X = X + (V * dt);
                break;

            case IntegrationMethod.Verlet:
                Vector3 tmpX = X;
                X += X - prevX + A * dt * dt;
                prevX = tmpX;
                break;

            case IntegrationMethod.VelocityVerlet:
                V = V + (A * dt);
                nextX = X + (V * dt);
                V = (nextX - X) / dt;
                X = nextX;
                break;

            case IntegrationMethod.Real:
                realAngle += realAngularVelocity * dt;
                X = radius * new Vector3(Mathf.Cos(realAngle), 0, Mathf.Sin(realAngle));
                break;
        }

        // Record Energy
        if (recordEnergy)
        {
            potentialEnergy = -G * M * m / R.magnitude;
            if (integrationMethod == IntegrationMethod.Verlet)
            {
                V = (X - prevX) / dt;
            }
            kineticEnergy = 0.5f * m * V.sqrMagnitude;
        }
    }

    private void SetupCircularMotionSpeed()
    {
        // Ac = V^2/R = GMm/R^2
        // V^2 = GMm/R
        // V = sqrt(GMm/R)
        Vector3 direction = Vector3.Cross(Vector3.up, R).normalized;
        V = Mathf.Sqrt(G * M / R.magnitude) * direction;
    }
}