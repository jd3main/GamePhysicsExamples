using System.Collections;
using UnityEditor;
using UnityEngine;

public class PBDRope : MonoBehaviour
{
    public enum LongRangeConstraintsMode
    {
        // Sorted by calculation time (accending)
        None = 0,

        Linear,
        Normal,
        FullConnected,
    }

    public enum SolverType
    {
        GaussSeidel,
        BackAndForth,
    }

    // Appearance settings
    [SerializeField] private int segmentCount = 30;

    [SerializeField] private float width = 0.1f;
    [SerializeField] private float length = 3f;
    public float SegmentLength => length / segmentCount;
    [SerializeField] private Mesh mesh = null;
    [SerializeField] private Material material = null;

    // Physics settings
    [SerializeField] private bool rootFixed = true;

    [SerializeField] private float mass = 0.5f;
    public float MassPerSegment => mass / segmentCount;
    [SerializeField] private float drag = 0.01f;
    [SerializeField] private float angularDrag = 0.01f;

    [SerializeField] private CollisionDetectionMode collisionDetectionMode = CollisionDetectionMode.Continuous;
    [SerializeField] private RigidbodyInterpolation interpolation = RigidbodyInterpolation.Interpolate;

    [SerializeField] private bool isKinematic = false;
    [SerializeField] private bool isTrigger = false;
    [SerializeField] private PhysicMaterial physicMaterial = null;
    [SerializeField] private LongRangeConstraintsMode longRangeConstraintsMode = LongRangeConstraintsMode.Normal;
    [SerializeField] private SolverType solverType = SolverType.GaussSeidel;
    [SerializeField] private bool enableAdjustRotation = true;

    // Contents of the rope
    public Transform[] segments { get; protected set; } = null;

    public PbdSystem Pbd => pbd;
    [SerializeField] private PbdSystem pbd = new PbdSystem();

    private void OnDrawGizmos()
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

    private void Start()
    {
        Init();
    }

    private void FixedUpdate()
    {
        pbd.Solve(Time.fixedDeltaTime);
        if (enableAdjustRotation)
            AdjustRotation();
    }

    private void Update()
    {
        AdjustRotation();
    }

    public void Init()
    {
        Clear();
        InitSegments();
        InitPbd();
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

        pbd.particles.Clear();
        pbd.constraints.Clear();
    }

    public int GetNearestSegmentId(Vector3 pos)
    {
        int nearId = -1;
        float nearDist = float.PositiveInfinity;
        for (int i = 0; i < segments.Length; i++)
        {
            float dist = Vector3.Distance(segments[i].position, pos);
            if (dist < nearDist)
            {
                nearId = i;
                nearDist = dist;
            }
        }
        return nearId;
    }

    private void InitSegments()
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
            segments[i].parent = segments[i - 1];
            segments[i].localRotation = Quaternion.identity;
            segments[i].localPosition = Vector3.down * SegmentLength;
            segments[i].localScale = Vector3.one;
            segments[i].parent = this.transform;
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
            collider.height = SegmentLength - 0.1f;
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

    private Mesh GetScaledMesh(Mesh originalMesh, Vector3 scale)
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


    private void InitPbd()
    {
        pbd.AddParticle(new Particle(segments[0], MassPerSegment));
        pbd.AddStaticConstraint(new FixedPositionConstraint(0, segments[0].position));
        pbd.particles[0].useGravity = false;
        for (int i = 1; i < segments.Length; i++)
        {
            pbd.AddParticle(new Particle(segments[i], MassPerSegment));
            var C = new DistanceConstraint(i - 1, i, SegmentLength);
            C.relation = Constraint.Relation.Equ;
            pbd.AddConstraint(new DistanceConstraint(i - 1, i, SegmentLength));
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
                        pbd.AddConstraint(C);
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
                        pbd.AddConstraint(C);
                    }
                }
                break;

            case LongRangeConstraintsMode.FullConnected: // N*(N-1)/2
                for (int i = 0; i < segments.Length; i++)
                {
                    for (int j = i + 2; j < segments.Length; j++)
                    {
                        var C = new DistanceConstraint(i, j, SegmentLength * (j - i));
                        C.relation = Constraint.Relation.Leq;
                        pbd.AddConstraint(C);
                    }
                }
                break;
        }

        switch (solverType)
        {
            case SolverType.GaussSeidel:
                pbd.solver = new GaussSeidelSolver();
                break;

            case SolverType.BackAndForth:
                pbd.solver = new BackAndForthSolver();
                break;
        }
    }

    private void AdjustRotation()
    {
        for (int i = 1; i < segments.Length; i++)
        {
            Vector3 direction = (segments[i - 1].position - segments[i].position).normalized;
            Quaternion deltaRot = Quaternion.FromToRotation(segments[i].up, direction);
            segments[i].rotation = deltaRot * segments[i].rotation;
        }
    }
}