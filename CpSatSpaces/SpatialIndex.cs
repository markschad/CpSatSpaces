using System.Collections;
using System.Collections.Immutable;

namespace Mms.CpSat.Spaces;

public readonly struct SpatialIndex : IEnumerable<DimensionIndex>, ISpace, IEquatable<SpatialIndex>
{
    public DimensionIndex[] DimensionIndices { get; }

    /// <summary>
    /// Gets the dimensions of this spatial index.
    /// </summary>
    public ImmutableSortedSet<Dimension> Dimensions => DimensionIndices
        .Select(index => index.Dimension)
        .ToImmutableSortedSet();

    /// <summary>
    /// Returns the index of the given dimension in this spatial index.
    /// </summary>
    public int this[Dimension dimension] => GetDimensionIndex(dimension);


    public SpatialIndex(params DimensionIndex[] indices)
    {
        // Ensure no duplicate dimensions by reference
        if (indices.Select(d => d.Dimension).Distinct().Count() != indices.Length)
        {
            throw new ArgumentException("All dimensions must be unique.");
        }

        // Ensure no duplicate dimensions by label
        if (indices.Select(d => d.Dimension.Label).Distinct().Count() != indices.Length)
        {
            throw new ArgumentException("All dimensions must have unique labels.");
        }

        // Sort dimensions by Id.
        var sortedIndices = indices.OrderBy(index => index.Dimension.Id).ToArray();
        DimensionIndices = sortedIndices;
    }
    
    /// <summary>
    /// Returns the index of the given dimension in this spatial index.
    /// </summary>
    public int GetDimensionIndex(Dimension dimension)
    {
        return DimensionIndices.Single(index => index.Dimension == dimension).Index;
    }

    /// <summary>
    /// Returns true if this spatial index is within the given space.
    /// </summary>
    public bool IsWithinSpace(ISpace space)
    {
        return space.Dimensions.SequenceEqual(Dimensions);
    }

    /// <summary>
    /// Returns true if this spatial index is within a sub-space of the given space.
    /// </summary>
    public bool IsWithinSubSpace(ISpace space)
    {
        return Dimensions.All(d => space.Dimensions.Contains(d));
    }

    /// <summary>
    /// Returns the flattened index of this spatial index.
    /// </summary>
    public int GetFlattenedIndex()
    {
        var flattenedIndex = 0;
        var multiplier = 1;

        for (int i = DimensionIndices.Length - 1; i >= 0; i--)
        {
            flattenedIndex += DimensionIndices[i].Index * multiplier;
            multiplier *= DimensionIndices[i].Dimension.Size;
        }

        return flattenedIndex;
    }
    
    /// <summary>
    /// Returns a new <see cref="SpatialIndex"/> which is the result of extracting the given dimensions from this
    /// spatial index. 
    /// </summary>
    public SpatialIndex Extract(Dimension[] dimensions)
    {
        var extractedIndices = DimensionIndices.Where(index => dimensions.Contains(index.Dimension)).ToArray();
        return new SpatialIndex(extractedIndices);
    }

    /// <summary>
    /// Creates a new spatial index which is the concatenation of this spatial index and the given spatial index.
    /// </summary>
    public SpatialIndex Concat(SpatialIndex other)
    {
        var concatenatedIndices = DimensionIndices.Concat(other.DimensionIndices).ToArray();
        return new SpatialIndex(concatenatedIndices);
    }
    
    public bool Equals(SpatialIndex other)
    {
        if (!IsWithinSpace(other))
        {
            return false;
        }

        return !DimensionIndices
            .Where((t, i) => 
                t.Index != other.DimensionIndices[i].Index || 
                t.Dimension != other.DimensionIndices[i].Dimension)
            .Any();
    }

    public override bool Equals(object? obj)
    {
        return obj is SpatialIndex other && Equals(other);
    }

    public override int GetHashCode()
    {
        return DimensionIndices.GetHashCode();
    }
    
    public static bool operator ==(SpatialIndex left, SpatialIndex right)
    {
        return left.Equals(right);
    }
    
    public static bool operator !=(SpatialIndex left, SpatialIndex right)
    {
        return !(left == right);
    }

    public static implicit operator DimensionIndex[](SpatialIndex spatialIndex) => spatialIndex.DimensionIndices;
    
    public override string ToString()
    {
        return "{ " + string.Join(", ", DimensionIndices.Select(index => index.ToString())) + " }";
    }

    public IEnumerator<DimensionIndex> GetEnumerator()
    {
        return ((IEnumerable<DimensionIndex>)DimensionIndices).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerable<SpatialIndex> IndicesAsEnumerable() => ISpace.IndicesAsEnumerable(this);
}
