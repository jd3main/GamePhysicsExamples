using UnityEngine;

namespace PBD
{
    [System.Serializable]
    public class Particle
    {
        public Vector3 x;
        public Vector3 v;
        public Vector3 p;
        public float w;
        public Transform transform = null;
        public Rigidbody rigidbody = null;
        public bool useGravity = true;
        public Vector3 externalForce;

        public Particle(Rigidbody rigid)
        {
            x = rigid.position;
            v = rigid.velocity;
            p = Vector3.zero;
            w = 1f / rigid.mass;
            transform = rigid.transform;
            rigidbody = rigid;
            useGravity = rigidbody.useGravity;
        }

        public Particle(Transform trans, float mass = 1f)
        {
            x = trans.position;
            v = Vector3.zero;
            p = Vector3.zero;
            w = 1f / mass;
            transform = trans;
        }

        public void SyncStatus()
        {
            transform.position = x;
            if (rigidbody != null)
            {
                rigidbody.velocity = v;
                w = 1 / rigidbody.mass;
            }
        }
    }
}