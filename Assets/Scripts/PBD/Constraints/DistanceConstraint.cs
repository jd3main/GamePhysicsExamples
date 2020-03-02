using UnityEngine;

public class DistanceConstraint : Constraint
{
    public float d;

    public DistanceConstraint(int i0, int i1, float distance) : base(new int[] { i0, i1 })
    {
        d = distance;
    }

    public override float Value()
    {
        return Vector3.Distance(ptcs[i[0]].p, ptcs[i[1]].p) - d;
    }

    override protected void Project()
    {
        float inv_wSum = 1 / (ptcs[i[0]].w + ptcs[i[1]].w);
        float val = Value();
        Vector3 norm = Normal();
        dp[0] = -ptcs[i[0]].w * inv_wSum * val * norm;
        dp[1] = ptcs[i[1]].w * inv_wSum * val * norm;
        ptcs[i[0]].p += dp[0];
        ptcs[i[1]].p += dp[1];

        // Debug.DrawLine(ptcs[i[0]].p, ptcs[i[1]].p, Color.red);
    }

    private Vector3 Normal()
    {
        Vector3 diff = ptcs[i[0]].p - ptcs[i[1]].p;
        if (diff == Vector3.zero)
            diff = ptcs[i[0]].x - ptcs[i[1]].x;
        return diff.normalized;
    }
}