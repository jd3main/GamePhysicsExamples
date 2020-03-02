using System.Collections.Generic;
using UnityEngine;

public abstract class Constraint
{
    public enum Relation
    {
        Equ,    // C=0
        Leq,    // C<=0
        Geq,    // C>=0
    }

    public PbdSystem pbd;
    public int[] i;
    public int arity => i.Length;
    public Relation relation = Relation.Equ;
    protected List<Particle> ptcs => pbd?.particles;
    protected readonly Vector3[] dp;

    public Constraint(int _arity)
    {
        i = new int[_arity];
        dp = new Vector3[_arity];
    }

    public Constraint(int[] indices)
    {
        i = indices.Clone() as int[];
        dp = new Vector3[arity];
    }


    abstract public float Value();

    abstract protected void Project();

    public bool TryProject()
    {
        if (!isSatisfied)
        {
            Project();
            return true;
        }
        return false;
    }

    protected void ProjectByGeneralFormula(Vector3[] gradient)
    {
        float s = GetS(gradient);

        if (isSatisfied)
        {
            return;
        }

        for (int j = 0; j < arity; j++)
        {
            dp[j] = -s * ptcs[i[j]].w * gradient[j];
        }

        for (int j = 0; j < arity; j++)
        {
            ptcs[i[j]].p += dp[j];
        }
    }

    public bool isSatisfied
    {
        get
        {
            float val = Value();
            switch (relation)
            {
                case Relation.Equ:
                    return val == 0;

                case Relation.Leq:
                    return val <= 0;

                case Relation.Geq:
                    return val >= 0;
            }
            return false;
        }
    }

    protected float GetS(Vector3[] gradient)
    {
        float numer = Value();
        if (numer == 0)
            return 0;

        float denom = 0;
        for (int j = 0; j < arity; j++)
        {
            denom += ptcs[i[j]].w * Vector3.SqrMagnitude(gradient[j]);
        }

        if (denom == 0)
        {/*
            Debug.LogWarning("s = " + numer + "/" + denom + "\n"
                + GetType().Name);

            for (int j = 0; j < arity; j++)
                Debug.Log("ptc[" + i[j] + "].w = " + ptcs[i[j]].w);

            Debug.Log("Gradient = ");
            for (int j = 0; j < arity; j++)
                Debug.Log(gradient[j]);

            if (GetType().Name == "WallConstraint")
            {
                WallConstraint wc = (this as WallConstraint);
                Debug.Log("wc.normal = " + wc.normal);
                Debug.Log("wc.shift = " + wc.shift);
            }
            else if (GetType().Name == "DistanceConstraint")
            {
                DistanceConstraint dc = (this as DistanceConstraint);
                Debug.Log("dc.d " + dc.d);
            }
            */
            return 0;
        }

        return numer / denom;
    }
}