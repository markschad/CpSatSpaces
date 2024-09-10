using Google.OrTools.Sat;

namespace Mms.CpSat.Spaces;

public static class CpSolverExtensions
{
    public static void PrintSpace(this CpSolver solver, VarSpace<IntVar> varSpace)
    {
        foreach (var siv in varSpace.ValuesAsEnumerable())
        {
            var value = solver.Value(siv.GetValue());
            var text = $"{varSpace.GetName(siv.Index)} = {value}";
            Console.WriteLine(text);
        }
    }
    
    public static void PrintSpace(this CpSolver solver, VarSpace<BoolVar> varSpace)
    {
        foreach (var siv in varSpace.ValuesAsEnumerable())
        {
            if (!solver.BooleanValue(siv.GetValue())) continue;
            
            Console.WriteLine(varSpace.GetName(siv.Index));
        }
    }
}