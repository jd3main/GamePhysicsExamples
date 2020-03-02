using System.Collections;
using UnityEditor;
using UnityEngine;

public class JointRope : MonoBehaviour
{
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
    [SerializeField] private bool nested = true;
    public Transform[] segments { get; protected set; } = null;

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

    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        Clear();
        InitSegments();
    }

    public void Clear()
    {
        if (segments != null)
        {
            foreach (var bone in segments)
            {
                if (bone != null)
                {
                    Destroy(bone.gameObject);
                }
            }
        }
        segments = null;
    }

    public int GetNearestBoneId(Vector3 pos)
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
        rootRigid.isKinematic = isKinematic;
        rootRigid.mass = MassPerSegment;
        rootRigid.drag = drag;
        rootRigid.angularDrag = angularDrag;
        rootRigid.interpolation = interpolation;
        rootRigid.collisionDetectionMode = collisionDetectionMode;
        if (rootFixed)
        {
            rootRigid.isKinematic = rootFixed;
            rootRigid.constraints = RigidbodyConstraints.FreezeAll;
        }

        for (int i = 1; i < segments.Length; i++)
        {
            GameObject segmentGameObject = new GameObject("s" + i);
            segments[i] = segmentGameObject.transform;
            segments[i].parent = nested ? segments[i - 1] : this.transform;
            segments[i].localRotation = Quaternion.identity;
            segments[i].localPosition = Vector3.down * SegmentLength;
            segments[i].localScale = Vector3.one;
            segments[i].gameObject.layer = this.gameObject.layer;

            // Init Rigidbody
            Rigidbody rigid = segments[i].gameObject.AddComponent<Rigidbody>();
            rigid.isKinematic = isKinematic;
            rigid.mass = MassPerSegment;
            rigid.drag = drag;
            rigid.angularDrag = angularDrag;
            rigid.interpolation = interpolation;
            rigid.collisionDetectionMode = collisionDetectionMode;

            // Init Joint
            HingeJoint joint = segmentGameObject.AddComponent<HingeJoint>();
            joint.connectedBody = segments[i - 1].GetComponent<Rigidbody>();
            joint.enablePreprocessing = false;
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = new Vector3(0, -SegmentLength / 2, 0);
            joint.anchor = new Vector3(0, SegmentLength / 2, 0);

            // Init Colliders
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
}