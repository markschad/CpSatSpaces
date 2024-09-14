using System.Text;
using Google.OrTools.Sat;

namespace Mms.CpSat.Spaces;

public static class CpSolverExtensions
{
    public static string ToString(this CpSolver solver, VarSpace<IntVar> varSpace)
    {
        var sb = new StringBuilder();
        foreach (var siv in varSpace.ValuesAsEnumerable())
        {
            var v = siv.GetValue();
            var value = solver.Value(v);
            var text = $"{varSpace.GetName(siv.Index)} = {value}";
            sb.AppendLine(text);
        }

        return sb.ToString();
    }
    
    public static string ToString(this CpSolver solver, VarSpace<BoolVar> varSpace)
    {
        var sb = new StringBuilder();
        foreach (var siv in varSpace.ValuesAsEnumerable())
        {
            if (!solver.BooleanValue(siv.GetValue())) continue;
            
            sb.AppendLine(varSpace.GetName(siv.Index));
        }

        return sb.ToString();
    }
}