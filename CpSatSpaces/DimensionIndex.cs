namespace Mms.CpSat.Spaces;

public readonly struct DimensionIndex
{
    public Dimension Dimension { get; }
    public int Index { get; }

    public DimensionIndex(Dimension dimension, int index)
    {
        if (index < 0 || index >= dimension.Size)
        {
            throw new ArgumentException("The index is out of bounds.");
        }

        Dimension = dimension;
        Index = index;
    }
    
    public bool Equals(DimensionIndex other)
    {
        return Dimension.Equals(other.Dimension) && Index == other.Index;
    }

    public override bool Equals(object? obj)
    {
        return obj is DimensionIndex other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Dimension, Index);
    }

    public static DimensionIndex operator ++(DimensionIndex dimensionIndex)
    {
        if (dimensionIndex.Index == dimensionIndex.Dimension.Size - 1)
        {
            throw new InvalidOperationException("Cannot increment index beyond the size of the dimension.");
        }
        return new DimensionIndex(dimensionIndex.Dimension, dimensionIndex.Index + 1);
    }

    public static DimensionIndex operator --(DimensionIndex dimensionIndex)
    {
        if (dimensionIndex.Index == 0)
        {
            throw new InvalidOperationException("Cannot decrement index below 0.");
        }
        return new DimensionIndex(dimensionIndex.Dimension, dimensionIndex.Index - 1);
    }
    
    public static DimensionIndex operator +(DimensionIndex lhs, int rhs)
    {
        if (lhs.Index + rhs >= lhs.Dimension.Size)
        {
            throw new InvalidOperationException("Cannot increment index beyond the size of the dimension.");
        }
        return new DimensionIndex(lhs.Dimension, lhs.Index + rhs);
    }
    
    public static DimensionIndex operator -(DimensionIndex lhs, int rhs)
    {
        if (lhs.Index - rhs < 0)
        {
            throw new InvalidOperationException("Cannot decrement index below 0.");
        }
        return new DimensionIndex(lhs.Dimension, lhs.Index - rhs);
    }
    
    public static bool operator ==(DimensionIndex lhs, DimensionIndex rhs) =>
        lhs.Equals(rhs);

    public static bool operator !=(DimensionIndex lhs, DimensionIndex rhs) => !(lhs == rhs);

    public static bool operator <(DimensionIndex lhs, int rhs) =>
        lhs.Index < rhs;

    public static bool operator >(DimensionIndex lhs, int rhs) =>
        lhs.Index > rhs;
    
    public static implicit operator DimensionIndex((Dimension dimension, int index) tuple) =>
        new DimensionIndex(tuple.dimension, tuple.index);

    public static implicit operator ValueTuple<Dimension, int>(DimensionIndex dimensionIndex) =>
        (dimensionIndex.Dimension, dimensionIndex.Index);

    public override string ToString()
    {
        return $"({Dimension.Label}={Index})";
    }
}
