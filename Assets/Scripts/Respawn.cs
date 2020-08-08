using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawn : MonoBehaviour
{
    public KeyCode respawnKey = KeyCode.R;
    public bool recursive = true;

    private Record[] records;

    private struct Record
    {
        public Transform transform;
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;

        public Rigidbody rigidbody;
        public Vector3 velocity;

        public Record(Transform trans)
        {
            transform = trans;
            localPosition = trans.localPosition;
            localRotation = trans.localRotation;
            localScale = trans.localScale;

            rigidbody = trans.GetComponent<Rigidbody>();
            velocity = (rigidbody != null) ? rigidbody.velocity : Vector3.zero;
        }


        public void Respawn()
        {
            transform.localPosition = localPosition;
            transform.localRotation = localRotation;
            transform.localScale = localScale;
            if (rigidbody != null)
                rigidbody.velocity = velocity;
        }
    }

    private void Start()
    {
        var recordsList = new List<Record>();
        recordsList.Add(new Record(this.transform));
        if (recursive)
        {
            Transform[] transforms = GetComponentsInChildren<Transform>();
            foreach (Transform trans in transforms)
            {
                recordsList.Add(new Record(trans));
            }
        }

        records = recordsList.ToArray();
    }

    private void Update()
    {
        if (Input.GetKeyDown(respawnKey))
        {
            foreach (Record record in records)
            {
                record.Respawn();
            }
        }
    }
}