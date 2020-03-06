using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PBD
{
    public class GaussSeidelSolver : Solver
    {
        override public void Solve(IList<Constraint> constraints)
        {
            for (int i = 0; i < constraints.Count; i++)
            {
                constraints[i].TryProject();
            }
        }
    }
}