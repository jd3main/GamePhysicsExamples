using System.Collections.Generic;

namespace PBD
{
    public abstract class Solver
    {
        abstract public void Solve(IList<Constraint> constraints);
    }
}