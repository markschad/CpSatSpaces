using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Mms.CpSat.Spaces;

public interface ISpace
{
    ImmutableSortedSet<Dimension> Dimensions { get; }

    IEnumerable<SpatialIndex> IndicesAsEnumerable();

    public static IEnumerable<SpatialIndex> IndicesAsEnumerable(ISpace space)
    {
        var dimensionIndices = space.Dimensions
            .Select(d => new DimensionIndex(d, 0))
            .ToArray();

        do
        {
            yield return new SpatialIndex((DimensionIndex[])dimensionIndices.Clone());
        } while (IncrementDimension(space.Dimensions.Count - 1));

        yield break;


        bool IncrementDimension(int d)
        {
            if (d < 0) return false;

            if (dimensionIndices[d] < space.Dimensions[d].Size - 1)
            {
                dimensionIndices[d]++;
                return true;
            }

            dimensionIndices[d] = new DimensionIndex(space.Dimensions[d], 0);
            return IncrementDimension(d - 1);
        }
    }
}

public class Space : ISpace
{
    public ImmutableSortedSet<Dimension> Dimensions { get; }

    protected readonly Space? ParentSpace;
    protected readonly SpatialIndex ParentSpaceMapping;

    public Space(params Dimension[] dimensions)
    {
        Dimensions = dimensions.ToImmutableSortedSet();
    }

    public Space(IEnumerable<Dimension> dimensions)
    {
        Dimensions = dimensions.ToImmutableSortedSet();
    }

    protected Space(IEnumerable<Dimension> dimensions, Space parentSpace, SpatialIndex parentSpaceMapping)
    {
        Dimensions = dimensions.ToImmutableSortedSet();
        ParentSpace = parentSpace;
        ParentSpaceMapping = parentSpaceMapping;
    }
   
    public SpatialIndex GetParentSpatialIndex(SpatialIndex spatialIndex)
    {
        if (ParentSpace == null)
        {
            throw new ArgumentException("Space is not a sub-space.");
        }

        if (!spatialIndex.IsWithinSpace(this))
        {
            throw new ArgumentException("The index must be within this space.");
        }

        return ParentSpaceMapping.Concat(spatialIndex);
    }
    
    public SpatialIndex GetParentSpatialIndex()
    {
        if (ParentSpace == null)
        {
            throw new ArgumentException("Space is not a sub-space.");
        }

        var originIndex = Dimensions
            .Select(d => new DimensionIndex(d, 0))
            .ToArray();
        var subSpaceOrigin = new SpatialIndex(originIndex);
        
        return ParentSpaceMapping.Concat(subSpaceOrigin);
    }

    /// <summary>
    /// Returns the space that is the intersection of this space and the other set of Dimensions.
    /// </summary>
    public Space Intersect(ISpace otherSpace)
    {
        var intersectingDimensions = Dimensions
            .Where(otherSpace.Dimensions.Contains)
            .ToArray();
        return new Space(intersectingDimensions);
    }

    /// <summary>
    /// Returns the space that is the difference of this space and the other set of Dimensions.
    /// </summary>
    public Space Except(ISpace otherSpace)
    {
        var exceptDimensions = Dimensions
            .Where(d => !otherSpace.Dimensions.Contains(d))
            .ToArray();
        return new Space(exceptDimensions);
    }

    /// <summary>
    /// Returns a new space which is a combination of the dimensions of this space and the other set of dimensions.
    /// </summary>
    public Space Combine(ISpace otherSpace)
    {
        var combinedSpace = Dimensions
            .Concat(otherSpace.Dimensions)
            .Distinct()
            .ToArray();
        return new Space(combinedSpace);
    }

    /// <summary>
    /// Returns the sub-space of this space which is constrained by the indices of the given dimensions.
    /// </summary>
    /// <param name="partialIndex">The dimension indices with which to constraint the sub-space.</param>
    /// <returns>A new <see cref="Space"/> which is a sub-space of this space, constrained by the given
    /// dimensions.</returns>
    /// <exception cref="ArgumentException">Constraining dimensions not apart of this space.</exception>
    public virtual ISpace GetSubSpace<TSpace>(SpatialIndex partialIndex)
    {
        if (!partialIndex.IsWithinSubSpace(this))
        {
            throw new ArgumentException("Cannot constrain space by dimensions not in the space.");
        }

        var subSpace = Except(partialIndex);
        return new Space(
            subSpace.Dimensions.ToImmutableArray(),
            this,
            partialIndex);
    }

    public IEnumerable<SpatialIndex> IndicesAsEnumerable() => ISpace.IndicesAsEnumerable(this);
    
    public static implicit operator Space(Dimension[] dimensions) => new(dimensions);
}

public class Space<TValue> : Space
{
    public static Space<TValue> Empty = new Space<TValue>(ImmutableList<Dimension>.Empty);

    private readonly TValue[]? _values;

    public Space(params Dimension[] dimensions) : base(dimensions)
    {
        _values = new TValue[Dimensions.GetVolume()];
    }

    public Space(IImmutableList<Dimension> dimensions) : base(dimensions)
    {
        _values = new TValue[Dimensions.GetVolume()];
    }

    protected Space(IImmutableList<Dimension> dimensions, Space<TValue> parentSpace, SpatialIndex parentSpaceMapping)
        : base(dimensions, parentSpace, parentSpaceMapping)
    {
    }

    /// <summary>
    /// Gets the value at the given spatial index.
    /// </summary>
    /// <param name="spatialIndex">The index of the value to set.</param>
    /// <returns>The value.</returns>
    /// <exception cref="ArgumentException">The given <see cref="SpatialIndex"/> does not uniquely identify a variable in this space.</exception>
    public TValue GetValue(SpatialIndex spatialIndex)
    {
        if (!spatialIndex.IsWithinSpace(this))
        {
            throw new ArgumentException("The given spatial index does not have the same dimensions as this space.");
        }

        if (_values != null)
        {
            return _values[spatialIndex.GetFlattenedIndex()];
        }

        if (ParentSpace != null)
        {
            return ((Space<TValue>)ParentSpace).GetValue(GetParentSpatialIndex(spatialIndex));
        }

        throw new UnreachableException("Space has no values and no parent space.");
    }

    /// <summary>
    /// Sets the value at the given spatial index.
    /// </summary>
    /// <param name="value">The value to set.</param>
    /// <param name="spatialIndex">The index of the value to set.</param>
    /// <exception cref="ArgumentException">The given <see cref="SpatialIndex"/> does not uniquely identify a variable in this space.</exception>
    public void SetValue(TValue value, SpatialIndex spatialIndex)
    {
        if (!spatialIndex.IsWithinSpace(this))
        {
            throw new ArgumentException("The given spatial index does not have the same dimensions as this space.");
        }

        if (_values != null)
        {
            _values[spatialIndex.GetFlattenedIndex()] = value;
            return;
        }

        if (ParentSpace != null)
        {
            var parentSpatialIndex = ParentSpaceMapping.Concat(spatialIndex);
            ((Space<TValue>)ParentSpace).SetValue(value, parentSpatialIndex);
            return;
        }

        throw new UnreachableException("Space has no values and no parent space.");
    }

    public TValue this[SpatialIndex spatialIndex]
    {
        get => GetValue(spatialIndex);
        set => SetValue(value, spatialIndex);
    }

    /// <summary>
    /// Returns the subspace of this space which is constrained by the indices of the given dimensions.
    /// </summary>
    /// <param name="partialIndex">The dimension indices with which to constraint the subspace.</param>
    /// <returns>A new <see cref="Space{TValue}"/> which is a subspace of this space, constrained by the given
    /// dimensions.</returns>
    /// <exception cref="ArgumentException">Constraining dimensions not part of this space.</exception>
    public Space<TValue> GetSubSpace(SpatialIndex partialIndex)
    {
        if (!partialIndex.IsWithinSubSpace(this))
        {
            throw new ArgumentException("Cannot constrain space by dimensions not in the space.");
        }

        var subSpace = Except(partialIndex);
        return new Space<TValue>(
            subSpace.Dimensions.ToImmutableArray(),
            this,
            partialIndex);
    }
    
    public IEnumerable<Space<TValue>> SubSpacesAsEnumerable(ISpace constrainingSpace)
    {
        if (constrainingSpace.Dimensions.Any(d => !Dimensions.Contains(d)))
        {
            throw new ArgumentException("Cannot constrain space by dimensions not in the space.");
        }
        
        var indices = constrainingSpace.IndicesAsEnumerable().ToArray();
        foreach (var spatialIndex in constrainingSpace.IndicesAsEnumerable())
        {
            yield return GetSubSpace(spatialIndex);
        }
    }

    public IEnumerable<Space<TValue>> SubSpacesAsEnumerable(params Dimension[] constrainingDimensions)
    {
        return SubSpacesAsEnumerable(new Space(constrainingDimensions));
    }

    public IEnumerable<SpatialIndexValue<TValue>> ValuesAsEnumerable()
    {
        return IndicesAsEnumerable().Select(spatialIndex => new SpatialIndexValue<TValue>(spatialIndex, this));
    }
}

public static class SpaceExtensions
{
    public static IEnumerable<ValueTuple<TVarA, TVarB>> ValuesAsEnumerable<TVarA, TVarB>(this ValueTuple<Space<TVarA>, Space<TVarB>> spaces)
    {
        if (!spaces.Item1.Dimensions.SequenceEqual(spaces.Item2.Dimensions))
        {
            throw new ArgumentException("The given spaces do not have the same dimensions.");
        }

        return spaces.Item1.IndicesAsEnumerable()
            .Select(spatialIndex => (spaces.Item1.GetValue(spatialIndex), spaces.Item2.GetValue(spatialIndex)));
    }
}
