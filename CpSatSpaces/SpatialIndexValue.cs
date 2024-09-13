namespace Mms.CpSat.Spaces;

public readonly struct SpatialIndexValue<TVar>(SpatialIndex index, Space<TVar> space)
{
    public readonly SpatialIndex Index = index;

    public readonly Space<TVar> Space = space;

    public TVar GetValue()
    {
        return Space.GetValue(Index);
    }
    
    public SpatialIndexValue<TVar> SetValue(TVar value)
    {
        Space.SetValue(value, Index);
        return this;
    }
}
