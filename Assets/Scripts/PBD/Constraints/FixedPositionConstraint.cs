using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FixedPositionConstraint : Constraint
{
    public Vector3 initialPosition;

    public FixedPositionConstraint(int index, Vector3 pos) : base(new int[] { index })
    {
        initialPosition = pos;
    }

    override public float Value()
    {
        return Vector3.Distance(ptcs[0].p, initialPosition);
    }

    override protected void Project()
    {
        ptcs[0].p = initialPosition;
    }
}