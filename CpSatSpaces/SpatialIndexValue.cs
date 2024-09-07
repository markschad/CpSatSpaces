namespace Mms.CpSat.Spaces;

public struct SpatialIndexValue<TVar>(SpatialIndex index, Space<TVar> space)
{
    public SpatialIndex Index = index;

    public Space<TVar> Space = space;

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
