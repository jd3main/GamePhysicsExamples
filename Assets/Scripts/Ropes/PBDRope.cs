using System.Collections;
using UnityEditor;
using UnityEngine;
using PBD;

[RequireComponent(typeof(World))]
public class PBDRope : Rope
{
    public enum LongRangeConstraintsMode
    {
        /* Sorted by calculation time (accending) */
        None,       // Without long range constraints
        Linear,     // O(n) numbers of long range constraints
        Normal,     // O(n*log(n)) numbers of long range constraints (recommanded)
        Dense,      // O(n^2) numbers of long range constraints
    }

    public enum SolverType
    {
        GaussSeidel,
        BackAndForth,
    }

    [SerializeField] protected Mesh mesh = null;

    [SerializeField] protected LongRangeConstraintsMode longRangeConstraintsMode = LongRangeConstraintsMode.Normal;
    [SerializeField] protected SolverType solverType = SolverType.GaussSeidel;
    [SerializeField] protected bool enableAdjustRotation = true;

    [SerializeField] protected World world;

    private void Start()
    {
        world = GetComponent<World>();
        Clear();
        InitSegments();
        InitPbdWorld();
    }

    private void LateUpdate()
    {
        if (enableAdjustRotation)
            AdjustRotation();
    }

    public void Clear()
    {
        if (segments != null)
        {
            foreach (var s in segments)
            {
                if (s != null)
                {
                    Destroy(s.gameObject);
                }
            }
        }
        segments = null;
    }

    protected void InitSegments()
    {
        segments = new Transform[segmentCount];
        segments[0] = (new GameObject("s0")).transform;
        segments[0].parent = this.transform;
        segments[0].localRotation = Quaternion.identity;
        segments[0].localPosition = Vector3.zero;
        segments[0].localScale = Vector3.one;
        segments[0].gameObject.layer = this.gameObject.layer;

        Rigidbody rootRigid = segments[0].gameObject.AddComponent<Rigidbody>();
        if (rootFixed)
        {
            rootRigid.mass = float.PositiveInfinity;
            rootRigid.useGravity = false;
        }

        for (int i = 1; i < segments.Length; i++)
        {
            GameObject segmentGameObject = new GameObject("s" + i);
            segments[i] = segmentGameObject.transform;
            segments[i].parent = this.transform;
            segments[i].localRotation = Quaternion.identity;
            segments[i].localPosition = Vector3.down * SegmentLength * i;
            segments[i].localScale = Vector3.one;
            segments[i].gameObject.layer = this.gameObject.layer;

            // Init Rigidbody
            Rigidbody rigid = segments[i].GetComponent<Rigidbody>();
            if (rigid == null)
                rigid = segments[i].gameObject.AddComponent<Rigidbody>();

            rigid.isKinematic = isKinematic;
            rigid.useGravity = false;
            rigid.mass = MassPerSegment;
            rigid.drag = 0;
            rigid.angularDrag = 0;
            rigid.interpolation = interpolation;
            rigid.collisionDetectionMode = collisionDetectionMode;
            rigid.constraints = RigidbodyConstraints.FreezeRotation;

            // Init Collider
            CapsuleCollider collider = segments[i].gameObject.AddComponent<CapsuleCollider>();
            collider.center = Vector3.zero;
            collider.radius = width / 2;
            collider.height = SegmentLength * 0.99f;
            collider.isTrigger = isTrigger;
            collider.sharedMaterial = physicMaterial;

            // Init Meshes and Renderers
            Vector3 scale = new Vector3(width, SegmentLength, width);
            scale.x /= mesh.bounds.size.x;
            scale.y /= mesh.bounds.size.y;
            scale.z /= mesh.bounds.size.z;
            mesh = GetScaledMesh(mesh, scale);
            MeshFilter meshFilter = segments[i].gameObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;
            MeshRenderer renderer = segments[i].gameObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
        }
    }

    protected Mesh GetScaledMesh(Mesh originalMesh, Vector3 scale)
    {
        Vector3[] newVertices = originalMesh.vertices;
        for (int i = 0; i < newVertices.Length; i++)
            newVertices[i] = Vector3.Scale(newVertices[i], scale);

        Mesh newMesh = new Mesh
        {
            name = "clone",
            vertices = newVertices,
            triangles = originalMesh.triangles,
            normals = originalMesh.normals,
            uv = originalMesh.uv
        };
        newMesh.RecalculateNormals();
        newMesh.RecalculateBounds();
        return newMesh;
    }


    protected void InitPbdWorld()
    {
        world.AddParticle(new Particle(segments[0], MassPerSegment));
        world.AddStaticConstraint(new AttachConstraint(0, this.transform, Vector3.zero));
        world.particles[0].useGravity = false;
        for (int i = 1; i < segments.Length; i++)
        {
            world.AddParticle(new Particle(segments[i], MassPerSegment));
            var C = new DistanceConstraint(i - 1, i, SegmentLength);
            C.relation = Constraint.Relation.Equ;
            world.AddConstraint(new DistanceConstraint(i - 1, i, SegmentLength));
        }

        switch (longRangeConstraintsMode)
        {
            case LongRangeConstraintsMode.None:
                break;

            case LongRangeConstraintsMode.Linear:
                for (int i = 2; i < segments.Length; i *= 2)
                {
                    for (int j = i; j < segments.Length; j += i)
                    {
                        var C = new DistanceConstraint(j - i, j, SegmentLength * i);
                        C.relation = Constraint.Relation.Leq;
                        world.AddConstraint(C);
                    }
                }
                break;

            case LongRangeConstraintsMode.Normal: // N*log(N)
                for (int i = 2; i < segments.Length; i *= 2)
                {
                    for (int j = i; j < segments.Length; j++)
                    {
                        var C = new DistanceConstraint(j - i, j, SegmentLength * i);
                        C.relation = Constraint.Relation.Leq;
                        world.AddConstraint(C);
                    }
                }
                break;

            case LongRangeConstraintsMode.Dense: // N*(N-1)/2
                for (int i = 0; i < segments.Length; i++)
                {
                    for (int j = i + 2; j < segments.Length; j++)
                    {
                        var C = new DistanceConstraint(i, j, SegmentLength * (j - i));
                        C.relation = Constraint.Relation.Leq;
                        world.AddConstraint(C);
                    }
                }
                break;
        }

        switch (solverType)
        {
            case SolverType.GaussSeidel:
                world.solver = new GaussSeidelSolver();
                break;

            case SolverType.BackAndForth:
                world.solver = new BackAndForthSolver();
                break;
        }
    }

    protected void AdjustRotation()
    {
        for (int i = 1; i < segments.Length; i++)
        {
            Vector3 direction = (segments[i - 1].position - segments[i].position).normalized;
            Quaternion deltaRot = Quaternion.FromToRotation(segments[i].up, direction);
            segments[i].rotation = deltaRot * segments[i].rotation;
        }
    }
}