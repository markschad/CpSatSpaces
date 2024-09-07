using System.Collections;
using System.Collections.Immutable;

namespace Mms.CpSat.Spaces;

public readonly struct SpatialIndex : IEnumerable<DimensionIndex>, ISpace, IEquatable<SpatialIndex>
{
    public DimensionIndex[] Indices { get; }

    /// <summary>
    /// Gets the dimensions of this spatial index.
    /// </summary>
    public IImmutableList<Dimension> Dimensions => Indices.Select(index => index.Dimension).ToImmutableArray();


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
        Indices = sortedIndices;
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

        for (int i = Indices.Length - 1; i >= 0; i--)
        {
            flattenedIndex += Indices[i].Index * multiplier;
            multiplier *= Indices[i].Dimension.Size;
        }

        return flattenedIndex;
    }
    
    /// <summary>
    /// Returns a new <see cref="SpatialIndex"/> which is the result of extracting the given dimensions from this
    /// spatial index. 
    /// </summary>
    public SpatialIndex Extract(Dimension[] dimensions)
    {
        var extractedIndices = Indices.Where(index => dimensions.Contains(index.Dimension)).ToArray();
        return new SpatialIndex(extractedIndices);
    }

    /// <summary>
    /// Creates a new spatial index which is the concatenation of this spatial index and the given spatial index.
    /// </summary>
    public SpatialIndex Concat(SpatialIndex other)
    {
        var concatenatedIndices = Indices.Concat(other.Indices).ToArray();
        return new SpatialIndex(concatenatedIndices);
    }
    
    public bool Equals(SpatialIndex other)
    {
        if (!IsWithinSpace(other))
        {
            return false;
        }

        return !Indices
            .Where((t, i) => 
                t.Index != other.Indices[i].Index || 
                t.Dimension != other.Indices[i].Dimension)
            .Any();
    }

    public override bool Equals(object? obj)
    {
        return obj is SpatialIndex other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Indices.GetHashCode();
    }
    
    public static bool operator ==(SpatialIndex left, SpatialIndex right)
    {
        return left.Equals(right);
    }
    
    public static bool operator !=(SpatialIndex left, SpatialIndex right)
    {
        return !(left == right);
    }

    public static implicit operator DimensionIndex[](SpatialIndex spatialIndex) => spatialIndex.Indices;
    
    public override string ToString()
    {
        return "{ " + string.Join(", ", Indices.Select(index => index.ToString())) + " }";
    }

    public IEnumerator<DimensionIndex> GetEnumerator()
    {
        return ((IEnumerable<DimensionIndex>)Indices).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerable<SpatialIndex> IndicesAsEnumerable() => ISpace.IndicesAsEnumerable(this);
}
