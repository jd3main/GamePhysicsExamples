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



    private void Start()
    {
        X = orbiter.position;
        if (circularMotion)
            SetupCircularMotionSpeed();
        prevX = X - V * Time.fixedDeltaTime;

        // ω = V/R
        radius = R.magnitude;
        realAngularVelocity = Mathf.Sqrt(G * M / radius) / radius;
        realAngle = Mathf.Atan2(X.z, X.x);

        Debug.Log("Initial X = " + X);
        Debug.Log("Initial V = " + V);
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
                // X(t+Δt) = 2*X(t) - X(t-Δt) + A(t)*Δt^2
                nextX = 2 * X - prevX + A * dt * dt;
                prevX = X;
                X = nextX;
                break;

            case IntegrationMethod.VelocityVerlet:
                // The following codes are equivilent to:
                //      V(t+1) = V(t) + A(t)*Δt
                //      X(t+Δt) = X(t) + V(t+1)*Δt
                //      V(t+Δt) = (X(t+Δt) - X(t)) / dt
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