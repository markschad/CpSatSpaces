using System.Collections.Immutable;
using Google.OrTools.Sat;
using Mms.CpSat.Spaces.ConstraintBuilder;

namespace Mms.CpSat.Spaces;

public static class CpModelExtensions
{
    public static void InitSpace<TVarA>(this CpModel model, VarSpace<TVarA> varSpace)
        where TVarA : IntVar
    {
        varSpace.InitializeVars(model);
    }

    public static LeftTargetSelectorIntVar AddConstraintThatSpace(this CpModel model, IntVarSpace leftSpace)
    {
        return new LeftTargetSelectorIntVar(model, leftSpace);
    }
    
    public static LeftTargetSelectorBoolVar AddConstraintThatSpace(this CpModel model, BoolVarSpace leftSpace)
    {
        return new LeftTargetSelectorBoolVar(model, leftSpace);
    }
}
