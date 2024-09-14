using System.Collections;
using Google.OrTools.Sat;

namespace Mms.CpSat.Spaces.ConstraintBuilder;

public class IntersectionSpaceSelectorOne<TVar1>(
    CpModel model,
    VarSpace<TVar1> space1)
    where TVar1 : IntVar
{
    public IntersectionSpaceSelectorTwo<TVar1, TVar2> And<TVar2>(Space<TVar2> space2)
        where TVar2 : IntVar
    {
        return new IntersectionSpaceSelectorTwo<TVar1, TVar2>(model, space1, space2);
    }
}

public delegate void FormConstraintTwo<TVar1, TVar2>(CpModel model, SpatialIndex si, Space<TVar1> subSpace1,
    Space<TVar2> subSpace2)
    where TVar1 : IntVar
    where TVar2 : IntVar;

public class IntersectionSpaceSelectorTwo<TVar1, TVar2>(
    CpModel model,
    Space<TVar1> space1,
    Space<TVar2> space2)
    where TVar1 : IntVar
    where TVar2 : IntVar
{
    public void FormConstraint(FormConstraintTwo<TVar1, TVar2> formConstraint)
    {
        var intersectingDimensions = space1.Dimensions
            .Intersect(space2.Dimensions)
            .ToArray();
        var intersectionSpace = new Space(intersectingDimensions);

        foreach (var si in intersectionSpace.IndicesAsEnumerable())
        {
            var subSpace1 = space1.GetSubSpace(si);
            var subSpace2 = space2.GetSubSpace(si);
            formConstraint(model, si, subSpace1, subSpace2);
        }
    }

    public IntersectionSpaceSelectorThree<TVar1, TVar2, TVar3> And<TVar3>(Space<TVar3> space3)
        where TVar3 : IntVar
    {
        return new IntersectionSpaceSelectorThree<TVar1, TVar2, TVar3>(model, space1, space2, space3);
    }
}

public delegate void FormConstraintThree<TVar1, TVar2, TVar3>(CpModel model, SpatialIndex si,
    Space<TVar1> subSpace1,
    Space<TVar2> subSpace2, Space<TVar3> subSpace3)
    where TVar1 : IntVar
    where TVar2 : IntVar
    where TVar3 : IntVar;

public class IntersectionSpaceSelectorThree<TVar1, TVar2, TVar3>(
    CpModel model,
    Space<TVar1> space1,
    Space<TVar2> space2,
    Space<TVar3> space3)
    where TVar1 : IntVar
    where TVar2 : IntVar
    where TVar3 : IntVar
{
    public void FormConstraint(FormConstraintThree<TVar1, TVar2, TVar3> formConstraint)
    {
        var intersectingDimensions = space1.Dimensions
            .Intersect(space2.Dimensions)
            .Intersect(space3.Dimensions)
            .ToArray();
        var intersectionSpace = new Space(intersectingDimensions);

        foreach (var si in intersectionSpace.IndicesAsEnumerable())
        {
            var subSpace1 = space1.GetSubSpace(si);
            var subSpace2 = space2.GetSubSpace(si);
            var subSpace3 = space3.GetSubSpace(si);
            formConstraint(model, si, subSpace1, subSpace2, subSpace3);
        }
    }
}