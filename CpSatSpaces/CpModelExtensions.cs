using System.Collections.Immutable;
using Google.OrTools.Sat;

namespace Mms.CpSat.Spaces;

public static class CpModelExtensions
{
    public static void InitSpace<TVarA>(this CpModel model, VarSpace<TVarA> varSpace)
        where TVarA : IntVar
    {
        varSpace.InitializeVars(model);
    }

    /// <summary>
    /// Adds a constraint to the model that all variables in sub-space A are true if any variables in sub-space B are true. 
    /// </summary>
    public static void AddSubSpaceTrueIfAnyOtherwiseFalse<TVarA, TVarB>(
        this CpModel model,
        VarSpace<TVarA> varSpaceA,
        VarSpace<TVarB> varSpaceB,
        ISpace intersectionSpace)
        where TVarA : IntVar where TVarB : IntVar
    {
    }

    public static SpaceConstraintBuilder<TVarA, TVarB> AddSpaceConstraint<TVarA, TVarB>(
        this CpModel model) where TVarA : IntVar where TVarB : IntVar
    {
        return new SpaceConstraintBuilder<TVarA, TVarB>(model);
    }
}

public sealed class SpaceConstraintT<TVarA, TVarB>(CpModel model, VarSpace<TVarA> spaceA, VarSpace<TVarB> spaceB)
    where TVarA : IntVar where TVarB : IntVar
{
    public void OnAlignedDimensions(Action<TVarA, IImmutableList<TVarB>> applyConstraint)
    {
        var exceptA = spaceA.Dimensions.Except(spaceB.Dimensions).ToArray();
        var subSpaceEnumerableA = spaceA.SubSpacesAsEnumerable(exceptA);
        var intersectB = spaceB.Dimensions.Except(spaceA.Dimensions).ToArray();
        var subSpaceEnumerableB = spaceB.SubSpacesAsEnumerable(intersectB).ToArray();
        foreach (var subSpaceA in subSpaceEnumerableA)
        {
            foreach (var a in subSpaceA.ValuesAsEnumerable())
            {
                var bVars = subSpaceEnumerableB
                    .Select(subSpaceB => subSpaceB.GetValue(a.Index))
                    .ToImmutableArray();
                applyConstraint(a.GetValue(), bVars);
            }
        }
    }
}
