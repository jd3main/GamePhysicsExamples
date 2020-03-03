using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PbdSystem
{
    public List<Constraint> constraints { get; private set; } = new List<Constraint>();
    public List<Constraint> contactConstraints { get; private set; } = new List<Constraint>();
    public List<Constraint> staticConstraints { get; private set; } = new List<Constraint>();
    public int solverIterations = 25;

    public int N => particles.Count;
    public List<Particle> particles = new List<Particle>();

    public bool useGravity = true;
    public Vector3 gravity = Physics.gravity;

    public Solver solver = new GaussSeidelSolver();

    private Dictionary<Transform, int> particleId = new Dictionary<Transform, int>();

    public bool enableExportPositions = true;
    public bool enableCollisionConstraints = true;
    public bool enableExternalContact = true;

    public float minCollisionVelocity = 0.005f;

    public float damp = 0.01f;
    public float sleepThreshold = 0.005f;


    public PbdSystem()
    { }

    public void Solve(float dt)
    {
        // Manage external forces
        if (enableExternalContact)
        {
            HandleExternalContact(dt);
        }

        foreach (Particle ptc in particles)
        {
            ptc.v += dt * ptc.w * ptc.externalForce;
            if (ptc.useGravity)
                ptc.v += gravity * dt;
        }

        // Damp velocities
        foreach (Particle ptc in particles)
            ptc.v *= (1 - damp);

        // Predict positions
        foreach (Particle ptc in particles)
            ptc.p = ptc.x + dt * ptc.v;

        // Generate collision constraintss
        if (enableCollisionConstraints)
        {
            /* Not Yet Implemented */
        }

        // Solve normal constraints
        for (int i = 0; i < solverIterations; i++)
        {
            solver.Solve(constraints);
            solver.Solve(contactConstraints);
            solver.Solve(staticConstraints);
        }
        contactConstraints.Clear();

        // Calculate velocities from positions
        foreach (Particle ptc in particles)
        {
            ptc.v = (ptc.p - ptc.x) / dt;
        }

        // Update position
        foreach (Particle ptc in particles)
            ptc.x = ptc.p;
        ExportPositions();

        // Update velocity
        ExportVelocities();
    }


    public void HandleExternalContact(float dt)
    {
        for (int i = 0; i < N; i++)
        {
            if (particles[i].rigidbody != null)
            {
                Vector3 dv = particles[i].rigidbody.velocity - particles[i].v;
                Vector3 dx = particles[i].rigidbody.position - (particles[i].x + particles[i].rigidbody.velocity * dt);

                Vector3 pushOutTarget = particles[i].x + dx + dv * dt;
                Vector3 dir = dx + dv * dt;
                WallConstraint collConstraint = new WallConstraint(i, pushOutTarget, dir);
                collConstraint.pbd = this;
                collConstraint.relation = Constraint.Relation.Geq;
                contactConstraints.Add(collConstraint);
                particles[i].v += dir;

                Debug.DrawLine(pushOutTarget - new Vector3(0, 0, 0.1f), pushOutTarget + new Vector3(0, 0, 0.1f), Color.green);
                Debug.DrawLine(pushOutTarget - new Vector3(0, 0.1f, 0), pushOutTarget + new Vector3(0, 0.1f, 0), Color.green);
                Debug.DrawLine(pushOutTarget - new Vector3(0.1f, 0, 0), pushOutTarget + new Vector3(0.1f, 0, 0), Color.green);
                Debug.DrawLine(particles[i].x, pushOutTarget, Color.red);
                Debug.DrawRay(pushOutTarget, dir.normalized, Color.yellow);
            }
        }
    }

    public int GetParticleIdByTransform(Transform transform)
    {
        if (particleId.ContainsKey(transform))
            return particleId[transform];
        else
            return -1;
    }

    public void AddParticle(Particle particle)
    {
        particleId[particle.transform] = particles.Count;
        particles.Add(particle);
        Rigidbody rigid = particle.transform.GetComponent<Rigidbody>();
        if (rigid != null)
            particle.rigidbody = rigid;
    }

    public void RemoveParticle(Transform transform)
    {
        int id = GetParticleIdByTransform(transform);
        if (id == -1)
            return;
        particleId.Remove(transform);
        particles.RemoveAt(id);
    }

    public void AddConstraint(Constraint constraint)
    {
        constraint.pbd = this;
        constraints.Add(constraint);
        return;
    }

    public void AddStaticConstraint(Constraint constraint)
    {
        constraint.pbd = this;
        constraints.Add(constraint);
        staticConstraints.Add(constraint);
        return;
    }

    public void RemoveConstraint(Constraint constraint)
    {
        constraints.Remove(constraint);
    }

    public void ExportPositions()
    {
        for (int i = 0; i < N; i++)
        {
            particles[i].ExportPosition();
        }
    }

    public void ExportVelocities()
    {
        for (int i = 0; i < N; i++)
        {
            particles[i].ExportVelocity();
        }
    }
}