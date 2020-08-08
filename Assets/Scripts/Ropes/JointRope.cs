using UnityEngine;

public class JointRope : Rope
{
    [SerializeField] protected Mesh mesh = null;
    [SerializeField] private bool nested = true;


    private void Start()
    {
        Clear();
        InitSegments();
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
            segments[i].parent = segments[i - 1];
            segments[i].localRotation = Quaternion.identity;
            segments[i].localPosition = Vector3.down * SegmentLength;
            segments[i].localScale = Vector3.one;
            if (!nested)
                segments[i].parent = this.transform;
            segments[i].gameObject.layer = this.gameObject.layer;

            // Initialize the Rigidbody
            Rigidbody rigid = segments[i].gameObject.AddComponent<Rigidbody>();
            rigid.isKinematic = isKinematic;
            rigid.mass = MassPerSegment;
            rigid.drag = drag;
            rigid.angularDrag = angularDrag;
            rigid.interpolation = interpolation;
            rigid.collisionDetectionMode = collisionDetectionMode;

            // Initialize the Joint
            CharacterJoint joint = segmentGameObject.AddComponent<CharacterJoint>();
            joint.connectedBody = segments[i - 1].GetComponent<Rigidbody>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = new Vector3(0, -SegmentLength / 2, 0);
            joint.anchor = new Vector3(0, SegmentLength / 2, 0);

            // Initialize the Collider
            CapsuleCollider collider = segments[i].gameObject.AddComponent<CapsuleCollider>();
            collider.center = Vector3.zero;
            collider.radius = width / 2;
            collider.height = SegmentLength * 0.99f;
            collider.isTrigger = isTrigger;
            collider.sharedMaterial = physicMaterial;

            // Initialize the Mesh and Renderer
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

    private void Update()
    {
        Rigidbody rigid = segments[1].gameObject.GetComponent<Rigidbody>();
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