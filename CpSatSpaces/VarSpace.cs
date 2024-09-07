using Google.OrTools.Sat;

namespace Mms.CpSat.Spaces;

public abstract class VarSpace<TVar>(params Dimension[] dimensions) : Space<TVar>(dimensions) where TVar : IntVar
{
    /// <summary>
    /// Returns the name of the variable at the given index.
    /// </summary>
    public string GetName(SpatialIndex spatialIndex)
    {
        if (!spatialIndex.IsWithinSpace(this))
        {
            throw new ArgumentException("The given spatial index does not have the same dimensions as this space.");
        }
        
        var varLabels = spatialIndex.Indices
            .OrderBy(di => di.Dimension.Label)
            .Select(di => $"{di.Dimension.Label}_{di.Index}")
            .ToArray();
        return string.Join("_", varLabels);
    }
    
    public abstract void InitializeVars(CpModel model);
}

public sealed class IntVarSpace(int lowerBound, int upperBound, params Dimension[] dimensions) : VarSpace<IntVar>(dimensions)
{
    public int LowerBound { get; } = lowerBound;

    public int UpperBound { get; } = upperBound;
    
    public override void InitializeVars(CpModel model)
    {
        foreach (var spatialIndex in IndicesAsEnumerable())
        {
            this[spatialIndex] = model.NewIntVar(LowerBound, UpperBound, GetName(spatialIndex));
        }
    }
}

public sealed class BoolVarSpace(params Dimension[] dimensions)
    : VarSpace<BoolVar>(dimensions)
{
    public override void InitializeVars(CpModel model)
    {
        foreach (var spatialIndex in IndicesAsEnumerable())
        {
            this[spatialIndex] = model.NewBoolVar(GetName(spatialIndex));
        }
    }
}
