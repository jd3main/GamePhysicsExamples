using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackAndForthSolver : Solver
{
    private bool rev;

    override public void Solve(IList<Constraint> constraints)
    {
        if (rev)
        {
            for (int i = constraints.Count - 1; i >= 0; i--)
            {
                constraints[i].TryProject();
            }
        }
        else
        {
            for (int i = 0; i < constraints.Count; i++)
            {
                constraints[i].TryProject();
            }
        }
        rev = !rev;
    }
}