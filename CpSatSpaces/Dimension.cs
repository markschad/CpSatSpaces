namespace Mms.CpSat.Spaces;

public readonly struct Dimension(int size, string label)
{
    /// <summary>
    /// The size of this dimension.
    /// </summary>
    public readonly int Size = size;
    
    /// <summary>
    /// A string label for this dimension.
    /// </summary>
    public readonly string Label = label;
    
    /// <summary>
    /// An arbitrary unique identifier for this dimension.
    /// </summary>
    public readonly Guid Id = Guid.NewGuid();
    
    public bool Equals(Dimension other)
    {
        return Size == other.Size && Label == other.Label && Id.Equals(other.Id);
    }

    public override bool Equals(object? obj)
    {
        return obj is Dimension other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Size, Label, Id);
    }
    
    public static bool operator ==(Dimension left, Dimension right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Dimension left, Dimension right)
    {
        return !(left == right);
    }
}

public static class DimensionExtensions
{
    public static int GetVolume(this IEnumerable<Dimension> dimensions)
    {
        return dimensions.Aggregate(1, (acc, dim) => acc * dim.Size);
    }
}
