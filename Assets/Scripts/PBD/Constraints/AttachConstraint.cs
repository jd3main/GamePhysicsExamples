using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PBD
{
    public class AttachConstraint : Constraint
    {
        public Transform target;
        public Vector3 offset;

        private Vector3 anchorPosition => target.transform.TransformPoint(offset);

        public override void DrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(ptcs[i[0]].p, anchorPosition);
        }

        public AttachConstraint(int index, Transform target, Vector3 offset)
            : base(new int[] { index })
        {
            this.target = target;
            this.offset = offset;
        }

        override public float Value()
        {
            return Vector3.Distance(ptcs[0].p, anchorPosition);
        }

        override protected void Project()
        {
            ptcs[0].p = anchorPosition;
        }
    }
}