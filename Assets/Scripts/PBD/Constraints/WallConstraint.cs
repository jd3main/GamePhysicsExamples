using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PBD
{
    public class WallConstraint : Constraint
    {
        public Vector3 normal;
        public float shift;

        public override void DrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(ptcs[i[0]].p, normal);
        }

        public WallConstraint(int index, Vector3 pos, Vector3 dir) : base(new int[] { index })
        {
            normal = dir.normalized;
            shift = -Vector3.Dot(pos, normal);
        }

        public WallConstraint(int index, Vector3 normal, float shift) : base(new int[] { index })
        {
            this.normal = normal;
            this.shift = shift;
        }

        override public float Value()
        {
            return Vector4.Dot(world.particles[i[0]].p, normal) + shift;
        }

        override protected void Project()
        {
            Vector3[] grad = new Vector3[1];
            grad[0] = normal;
            ProjectByGeneralFormula(grad);
        }
    }
}