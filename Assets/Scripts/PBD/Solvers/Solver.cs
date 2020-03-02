using System.Collections.Generic;

public abstract class Solver
{
    abstract public void Solve(IList<Constraint> constraints);
}