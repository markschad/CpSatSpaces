using Google.OrTools.Sat;
using IntVar = Google.OrTools.Sat.IntVar;

namespace Mms.CpSat.Spaces.ConstraintBuilder;

public sealed class LeftSpaceSelector(CpModel model)
{
    public LeftTargetSelectorIntVar InWhich(VarSpace<IntVar> leftSpace)
    {
        return new LeftTargetSelectorIntVar(model, leftSpace);
    }
    
    public LeftTargetSelectorBoolVar InWhich(VarSpace<BoolVar> leftSpace)
    {
        return new LeftTargetSelectorBoolVar(model, leftSpace);
    }
}
