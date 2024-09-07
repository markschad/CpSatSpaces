using System.Collections.Immutable;
using Google.OrTools.Sat;

namespace Mms.CpSat.Spaces;


public interface ISpaceConstraintRightSideConstraint
{
    public void IsExactly(long rightSideTargetValue);
    public void IsAtMost(long rightSideTargetValue);
    public void IsAtLeast(long rightSideTargetValue);
}

public interface ISpaceConstraintRightSidePicker<TVarL, TVarR> where TVarL : IntVar where TVarR : IntVar
{
    public ISpaceConstraintRightSideConstraint If(VarSpace<TVarR> rightSpace);
}

public interface ISpaceConstraintLeftSideTarget<TVarL, TVarR> where TVarL : IntVar where TVarR : IntVar
{
    public ISpaceConstraintRightSidePicker<TVarL, TVarR> WillBe(long targetValue);

    public ISpaceConstraintRightSidePicker<TVarL, TVarR> CanBe(long targetValue);
}

public interface ISpaceConstraintLeftSidePicker<TVarL, TVarR> where TVarL : IntVar where TVarR : IntVar
{
    public ISpaceConstraintLeftSideTarget<TVarL, TVarR> InWhich(VarSpace<TVarL> leftSpace);
}

public class SpaceConstraintBuilder<TVarL, TVarR>(CpModel model) : 
    ISpaceConstraintLeftSidePicker<TVarL, TVarR>,
    ISpaceConstraintLeftSideTarget<TVarL, TVarR>, 
    ISpaceConstraintRightSidePicker<TVarL, TVarR>,
    ISpaceConstraintRightSideConstraint
    where TVarL : IntVar where TVarR : IntVar
{
    private VarSpace<TVarL>? _leftSpace;

    private VarSpace<TVarR>? _rightSpace;
    
    private long _leftSideTargetValue = 1;
    private LeftSideConstraints _leftSideConstraint = LeftSideConstraints.WillBe;

    private long RightSideTargetValue = 0;
    private BoundedLinearExpression? RightSideExpression;
    
    public ISpaceConstraintLeftSideTarget<TVarL, TVarR> InWhich(VarSpace<TVarL> leftSpace)
    {
        _leftSpace = leftSpace;
        return this;
    }

    public ISpaceConstraintRightSidePicker<TVarL, TVarR> WillBe(long targetValue)
    {
        _leftSideTargetValue = targetValue;
        _leftSideConstraint = LeftSideConstraints.WillBe;
        return this;
    }

    public ISpaceConstraintRightSidePicker<TVarL, TVarR> CanBe(long targetValue)
    {
        _leftSideTargetValue = targetValue;
        _leftSideConstraint = LeftSideConstraints.CanBe;
        return this;
    }

    public ISpaceConstraintRightSideConstraint If(VarSpace<TVarR> rightSpace)
    {
        _rightSpace = rightSpace;
        return this;
    }

    public void IsExactly(long rightSideTargetValue)
    {
        throw new NotImplementedException();
    }

    public void IsAtMost(long rightSideTargetValue)
    {
        throw new NotImplementedException();
    }

    public void IsAtLeast(long rightSideTargetValue)
    {
        ApplyConstraint((leftVar, rightVars) =>
        {
            var leftSideConstraintVar = GetLeftSideConstraintVar(leftVar);
            
            model.Add(LinearExpr.Sum(rightVars) >= rightSideTargetValue)
                .OnlyEnforceIf(leftSideConstraintVar);
            model.Add(LinearExpr.Sum(rightVars) < rightSideTargetValue)
                .OnlyEnforceIf(leftSideConstraintVar.Not());
        });
    }

    private BoolVar GetLeftSideConstraintVar(TVarL leftVar)
    {
        return _leftSideConstraint switch
        {
            LeftSideConstraints.WillBe => GetLeftSideWillBeConstraint(leftVar),
            LeftSideConstraints.CanBe => GetLeftSideCanBeConstraint(leftVar),
            _ => throw new InvalidOperationException()
        };
        
    }

    private BoolVar GetLeftSideWillBeConstraint(TVarL leftVar)
    {
        var leftSideConstraintVar = model.NewBoolVar(Guid.NewGuid().ToString());
        
        model.Add(leftVar == _leftSideTargetValue)
            .OnlyEnforceIf(leftSideConstraintVar);
        model.Add(leftVar != _leftSideTargetValue)
            .OnlyEnforceIf(leftSideConstraintVar.Not());

        return leftSideConstraintVar;
    }
    
    private BoolVar GetLeftSideCanBeConstraint(TVarL leftVar)
    {
        var leftSideConstraintVar = model.NewBoolVar(Guid.NewGuid().ToString());
        
        model.Add(leftVar != _leftSideTargetValue)
            .OnlyEnforceIf(leftSideConstraintVar.Not());

        return leftSideConstraintVar;
    }

    private void ApplyConstraint(Action<TVarL, IImmutableList<TVarR>> applyConstraint)
    {
        if (_leftSpace == null || _rightSpace == null)
        {
            throw new InvalidOperationException("Left and right spaces must be set before calling this method.");
        }
        
        var leftExceptRight = _leftSpace.Dimensions.Except(_rightSpace.Dimensions).ToArray();
        var leftSubSpaces = _leftSpace.SubSpacesAsEnumerable(leftExceptRight);
        var rightExceptLeft = _rightSpace.Dimensions.Except(_leftSpace.Dimensions).ToArray();
        var rightSubSpaces = _rightSpace.SubSpacesAsEnumerable(rightExceptLeft).ToArray();
        
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

    private enum LeftSideConstraints
    {
        WillBe,
        CanBe,
    }
}