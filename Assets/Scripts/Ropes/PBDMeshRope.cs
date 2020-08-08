using System.Collections;
using UnityEditor;
using UnityEngine;
using PBD;

[RequireComponent(typeof(SkinnedMeshRenderer))]
[RequireComponent(typeof(World))]
public class PBDMeshRope : PBDRope
{
    [SerializeField] protected int meshSides;

    private void Start()
    {
        world = GetComponent<World>();
        Clear();
        InitSegments();
        InitMesh();
        InitPbdWorld();
    }

    private void LateUpdate()
    {
        if (enableAdjustRotation)
            AdjustRotation();
    }


    protected new void InitSegments()
    {
        segments = new Transform[segmentCount + 1];
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
            collider.height = SegmentLength - 0.1f;
            collider.isTrigger = isTrigger;
            collider.sharedMaterial = physicMaterial;
        }
    }

    protected void InitMesh()
    {
        // Init Meshes and the Renderer
        SkinnedMeshRenderer renderer = this.gameObject.GetComponent<SkinnedMeshRenderer>();
        Mesh mesh = MeshMaker.Cylinder(width, length, segmentCount, meshSides);
        Vector3[] vertices = mesh.vertices;
        BoneWeight[] boneWeights = new BoneWeight[vertices.Length];
        Matrix4x4[] bindPoses = new Matrix4x4[segments.Length];

        for (int i = 0; i < boneWeights.Length; i++)
        {
            int id = Mathf.RoundToInt(-vertices[i].y / SegmentLength);
            boneWeights[i].boneIndex0 = id;
            boneWeights[i].weight0 = 1;
        }
        mesh.boneWeights = boneWeights;

        for (int i = 0; i < segments.Length; i++)
        {
            bindPoses[i] = segments[i].worldToLocalMatrix * transform.localToWorldMatrix;
        }


        mesh.bindposes = bindPoses;
        renderer.bones = segments;
        renderer.sharedMesh = mesh;

        renderer.localBounds = new Bounds(new Vector3(0, -length / 2, 0),
                                          new Vector3(width, length, width));

        renderer.allowOcclusionWhenDynamic = false;
        renderer.updateWhenOffscreen = true;
        renderer.sharedMaterial = material;
    }
}