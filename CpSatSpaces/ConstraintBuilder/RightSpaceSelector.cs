using System.Collections.Immutable;
using Google.OrTools.Sat;

namespace Mms.CpSat.Spaces.ConstraintBuilder;


public delegate ILiteral GenerateLeftConstraint<in TLeftVar>(TLeftVar leftVar) where TLeftVar : IntVar;

public delegate void ApplyConstraint<in TLeftVar, TRightVar>(TLeftVar leftVar, IImmutableList<TRightVar> rightVars)
    where TLeftVar : IntVar;


public class RightSpaceSelector<TLeftVar>(
    CpModel model,
    Space<TLeftVar> leftSpace,
    GenerateLeftConstraint<TLeftVar> generateLeftConstraint)
    where TLeftVar : IntVar
{
    public RightConditionSelectorIntVar<TLeftVar> OnlyIf(IntVarSpace rightSpace)
    {
        return new RightConditionSelectorIntVar<TLeftVar>(model, leftSpace, generateLeftConstraint, rightSpace);
    }

    public RightConditionSelectorBoolVar<TLeftVar> OnlyIf(BoolVarSpace rightSpace)
    {
        return new RightConditionSelectorBoolVar<TLeftVar>(model, leftSpace, generateLeftConstraint, rightSpace);
    }
}
