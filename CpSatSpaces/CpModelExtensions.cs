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

    public static LeftSpaceSelector AddSpaceConstraint(this CpModel model)
    {
        return new LeftSpaceSelector(model);
    }
}
