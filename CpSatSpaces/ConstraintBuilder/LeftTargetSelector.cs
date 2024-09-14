using Google.OrTools.Sat;

namespace Mms.CpSat.Spaces.ConstraintBuilder;


public sealed class LeftTargetSelectorIntVar(CpModel model, Space<IntVar> leftSpace)
{
    /// <summary>
    /// The variable in the left-space must be equal to the target value if the right-space condition is met.
    /// </summary>
    public RightSpaceSelector<IntVar> WillBe(long targetValue)
    {
        return new RightSpaceSelector<IntVar>(model, leftSpace, leftVar =>
        {
            var leftSideConstraintVar = model.NewBoolVar(Guid.NewGuid().ToString());
            
            model.Add(leftVar == targetValue)
                .OnlyEnforceIf(leftSideConstraintVar);
            model.Add(leftVar != targetValue)
                .OnlyEnforceIf(leftSideConstraintVar.Not());

            return leftSideConstraintVar;
        });
    }

    /// <summary>
    /// The variable in the left-space can only be equal to the target value if the right-space condition is met.
    /// </summary>
    public RightSpaceSelector<IntVar> CanBe(long targetValue)
    {
        return new RightSpaceSelector<IntVar>(model, leftSpace, leftVar =>
        {
            var leftSideConstraintVar = model.NewBoolVar(Guid.NewGuid().ToString());
            
            model.Add(leftVar != targetValue)
                .OnlyEnforceIf(leftSideConstraintVar.Not());

            return leftSideConstraintVar;
        });
    }
    
    public IntersectionSpaceSelectorTwo<IntVar, TVar2> And<TVar2>(Space<TVar2> space2)
        where TVar2 : IntVar
    {
        return new IntersectionSpaceSelectorTwo<IntVar, TVar2>(model, leftSpace, space2);
    }
}

public sealed class LeftTargetSelectorBoolVar(CpModel model, Space<BoolVar> leftSpace)
{
    /// <summary>
    /// The variable in the left-space must be true if the right-space condition is met.
    /// </summary>
    public RightSpaceSelector<BoolVar> WillBeTrue()
    {
        return new RightSpaceSelector<BoolVar>(model, leftSpace, leftVar =>
        {
            var leftSideConstraintVar = model.NewBoolVar(Guid.NewGuid().ToString());
            
            model.Add(leftVar == 1)
                .OnlyEnforceIf(leftSideConstraintVar);
            model.Add(leftVar != 1)
                .OnlyEnforceIf(leftSideConstraintVar.Not());

            return leftSideConstraintVar;
        });
    }

    /// <summary>
    /// The variable in the left-space can only be true if the right-space condition is met.
    /// </summary>
    public RightSpaceSelector<BoolVar> CanBeTrue()
    {
        return new RightSpaceSelector<BoolVar>(model, leftSpace, leftVar =>
        {
            var leftSideConstraintVar = model.NewBoolVar(Guid.NewGuid().ToString());
            
            model.Add(leftVar != 1)
                .OnlyEnforceIf(leftSideConstraintVar.Not());

            return leftSideConstraintVar;
        });
    }

    /// <summary>
    /// The variable in the left-space must be false if the right-space condition is met.
    /// </summary>
    public RightSpaceSelector<BoolVar> WillBeFalse()
    {
        return new RightSpaceSelector<BoolVar>(model, leftSpace, leftVar =>
        {
            var leftSideConstraintVar = model.NewBoolVar(Guid.NewGuid().ToString());
            
            model.Add(leftVar == 0)
                .OnlyEnforceIf(leftSideConstraintVar);
            model.Add(leftVar != 0)
                .OnlyEnforceIf(leftSideConstraintVar.Not());

            return leftSideConstraintVar;
        });
    }

    /// <summary>
    /// The variable in the left-space can only be false if the right-space condition is met.
    /// </summary>
    public RightSpaceSelector<BoolVar> CanBeFalse()
    {
        return new RightSpaceSelector<BoolVar>(model, leftSpace, leftVar =>
        {
            var leftSideConstraintVar = model.NewBoolVar(Guid.NewGuid().ToString());
            
            model.Add(leftVar != 0)
                .OnlyEnforceIf(leftSideConstraintVar.Not());

            return leftSideConstraintVar;
        });
    }
    
    public IntersectionSpaceSelectorTwo<BoolVar, TVar2> And<TVar2>(Space<TVar2> space2)
        where TVar2 : IntVar
    {
        return new IntersectionSpaceSelectorTwo<BoolVar, TVar2>(model, leftSpace, space2);
    }
}
