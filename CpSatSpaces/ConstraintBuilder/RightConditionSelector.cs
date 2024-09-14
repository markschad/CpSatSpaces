using System.Collections.Immutable;
using Google.OrTools.Sat;

namespace Mms.CpSat.Spaces.ConstraintBuilder;


public abstract class RightConditionSelector<TLeftVar, TRightVar>(
    CpModel model,
    Space<TLeftVar> leftSpace,
    Space<TRightVar> rightSpace)
    where TLeftVar : IntVar
    where TRightVar : IntVar
{
    protected CpModel Model { get; } = model;
    protected Space<TLeftVar> LeftSpace { get; } = leftSpace;
    protected Space<TRightVar> RightSpace { get; } = rightSpace;
    
    protected void ApplyConstraintToEachLeftVar(ApplyConstraint<TLeftVar, TRightVar> applyConstraint)
    {
        var leftExceptRight = LeftSpace.Dimensions.Except(RightSpace.Dimensions).ToArray();
        var leftSubSpaces = LeftSpace.SubSpacesAsEnumerable(leftExceptRight);
        var rightExceptLeft = RightSpace.Dimensions.Except(LeftSpace.Dimensions).ToArray();
        var rightSubSpaces = RightSpace.SubSpacesAsEnumerable(rightExceptLeft).ToArray();

        foreach (var leftSubSpace in leftSubSpaces)
        {
            foreach (var leftVar in leftSubSpace.ValuesAsEnumerable())
            {
                var rightVars = rightSubSpaces
                    .Select(rightSubSpace => rightSubSpace.GetValue(leftVar.Index))
                    .ToImmutableArray();
                applyConstraint(leftVar.GetValue(), rightVars);
            }
        }
    }
}

public class RightConditionSelectorIntVar<TLeftVar>(
    CpModel model,
    Space<TLeftVar> leftSpace,
    GenerateLeftConstraint<TLeftVar> generateLeftConstraint,
    Space<IntVar> rightSpace)
    : RightConditionSelector<TLeftVar, IntVar>(model, leftSpace, rightSpace)
    where TLeftVar : IntVar
{
    public void SumsToExactly(long targetValue)
    {
        ApplyConstraintToEachLeftVar((leftVar, rightVars) =>
        {
            var leftSideConstraintVar = generateLeftConstraint(leftVar);
            
            Model.Add(LinearExpr.Sum(rightVars) == targetValue)
                .OnlyEnforceIf(leftSideConstraintVar);
            Model.Add(LinearExpr.Sum(rightVars) != targetValue)
                .OnlyEnforceIf(leftSideConstraintVar.Not());
        });
    }

    public void SumsToAtMost(long targetValue)
    {
        ApplyConstraintToEachLeftVar((leftVar, rightVars) =>
        {
            var leftSideConstraintVar = generateLeftConstraint(leftVar);
            
            Model.Add(LinearExpr.Sum(rightVars) <= targetValue)
                .OnlyEnforceIf(leftSideConstraintVar);
            Model.Add(LinearExpr.Sum(rightVars) > targetValue)
                .OnlyEnforceIf(leftSideConstraintVar.Not());
        });
    }

    public void SumsToAtLeast(long targetValue)
    {
        ApplyConstraintToEachLeftVar((leftVar, rightVars) =>
        {
            var leftSideConstraintVar = generateLeftConstraint(leftVar);
            
            Model.Add(LinearExpr.Sum(rightVars) >= targetValue)
                .OnlyEnforceIf(leftSideConstraintVar);
            Model.Add(LinearExpr.Sum(rightVars) < targetValue)
                .OnlyEnforceIf(leftSideConstraintVar.Not());
        });
    }
}

public class RightConditionSelectorBoolVar<TLeftVar>(
    CpModel model,
    Space<TLeftVar> leftSpace,
    GenerateLeftConstraint<TLeftVar> generateLeftConstraint,
    Space<BoolVar> rightSpace)
    : RightConditionSelector<TLeftVar, BoolVar>(model, leftSpace, rightSpace)
    where TLeftVar : IntVar
{
    public void HasTrueCountOfExactly(long targetValue)
    {
        ApplyConstraintToEachLeftVar((leftVar, rightVars) =>
        {
            var leftSideConstraintVar = generateLeftConstraint(leftVar);
            
            Model.Add(LinearExpr.Sum(rightVars) == targetValue)
                .OnlyEnforceIf(leftSideConstraintVar);
            Model.Add(LinearExpr.Sum(rightVars) != targetValue)
                .OnlyEnforceIf(leftSideConstraintVar.Not());
        });
    }
    
    public void HasTrueCountOfAtMost(long targetValue)
    {
        ApplyConstraintToEachLeftVar((leftVar, rightVars) =>
        {
            var leftSideConstraintVar = generateLeftConstraint(leftVar);
            
            Model.Add(LinearExpr.Sum(rightVars) <= targetValue)
                .OnlyEnforceIf(leftSideConstraintVar);
            Model.Add(LinearExpr.Sum(rightVars) > targetValue)
                .OnlyEnforceIf(leftSideConstraintVar.Not());
        });
    }
    
    public void HasTrueCountOfAtLeast(long targetValue)
    {
        ApplyConstraintToEachLeftVar((leftVar, rightVars) =>
        {
            var leftSideConstraintVar = generateLeftConstraint(leftVar);
            
            Model.Add(LinearExpr.Sum(rightVars) >= targetValue)
                .OnlyEnforceIf(leftSideConstraintVar);
            Model.Add(LinearExpr.Sum(rightVars) < targetValue)
                .OnlyEnforceIf(leftSideConstraintVar.Not());
        });
    }
}
