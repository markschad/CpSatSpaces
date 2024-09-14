using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using JetBrains.Annotations;
using Mms.CpSat.Spaces;
using Xunit;

namespace CpSatSpaces.Tests;

[TestSubject(typeof(Space))]
public class SpaceTest
{

    public class IndiciesAsEnumerable_Test()
    {
        private static readonly Dimension DimA = new(2, "A");
        private static readonly Dimension DimB = new(3, "B");
        private static readonly Dimension DimC = new(2, "C");
        
        private readonly Space<int> _space = new(DimA, DimB, DimC);
        
        [Fact]
        public void GeneratesCorrectNumberOfIndices()
        {
            var indices = _space.IndicesAsEnumerable().ToArray();
            indices.Length.Should().Be(DimA.Size * DimB.Size * DimC.Size);
        }

        [Fact]
        public void GeneratesCorrectIndices()
        {
            var indices = _space.IndicesAsEnumerable().ToArray();

            var expectedIndices = new List<SpatialIndex>();
            for (var a = 0; a < DimA.Size; a++)
            {
                for (var b = 0; b < DimB.Size; b++)
                {
                    for (var c = 0; c < DimC.Size; c++)
                    {
                        expectedIndices.Add(new SpatialIndex([
                            new DimensionIndex(DimA, a),
                            new DimensionIndex(DimB, b),
                            new DimensionIndex(DimC, c)
                        ]));
                    }
                }
            }

            indices.Should().BeEquivalentTo(expectedIndices);
        }

        [Fact]
        public void GeneratedIndicesAreWithinCorrectSpace()
        {
            var indices = _space.IndicesAsEnumerable().ToArray();
            
            foreach (var index in indices)
            {
                index.IsWithinSpace(_space).Should().BeTrue();
            }
        }
        
    }
    

    [Fact]
    public void Equivalence()
    {
        var dimA = new Dimension(2, "A");
        var dimB = new Dimension(3, "B");
        var dimC = new Dimension(2, "C");

        var space1 = new Space<int>(dimA, dimB, dimC);
        var space2 = new Space<int>(dimA, dimB, dimC);

        space1.Should().BeEquivalentTo(space2);
    }

    [Fact]
    public void GetIntersectingDimensions_ReturnsExpectedSpace()
    {
        var dimA = new Dimension(2, "A");
        var dimB = new Dimension(3, "B");
        var dimC = new Dimension(2, "C");
        var dimD = new Dimension(4, "D");

        var space1 = new Space<int>(dimA, dimB, dimC);
        var space2 = new Space<int>(dimB, dimC, dimD);

        var intersectingDimensions = space1.Intersect(space2).Dimensions;
        Dimension[] expectedValue = [dimB, dimC];

        intersectingDimensions.Should().BeEquivalentTo(expectedValue);
    }

    [Fact]
    public void GetDifferingDimensions_ReturnsExpectedSpace()
    {
        var dimA = new Dimension(2, "A");
        var dimB = new Dimension(3, "B");
        var dimC = new Dimension(2, "C");

        var space1 = new Space<int>(dimA, dimB, dimC);
        var space2 = new Space<int>(dimC);

        var differingDimensions = space1.Except(space2).Dimensions;
        Dimension[] expectedValue = [dimA, dimB];

        differingDimensions.Should().BeEquivalentTo(expectedValue);
    }

    [Fact]
    public void GetCombinedDimensions_ReturnsExpectedSpace()
    {
        var dimA = new Dimension(2, "A");
        var dimB = new Dimension(3, "B");
        var dimC = new Dimension(2, "C");
        var dimD = new Dimension(4, "D");

        var space1 = new Space<int>(dimA, dimB, dimC);
        var space2 = new Space<int>(dimC, dimD);

        var combinedDimensions = space1.Combine(space2).Dimensions;
        Dimension[] expectedValue = [dimA, dimB, dimC, dimD];

        combinedDimensions.Should().BeEquivalentTo(expectedValue);
    }

    public class GetSubSpaceTest
    {
        private readonly Dimension _dimA = new(4, "A");
        private readonly Dimension _dimB = new(5, "B");
        private readonly Dimension _dimC = new(6, "C");

        private readonly Space<int> _space;

        public GetSubSpaceTest()
        {
            _space = new Space<int>(_dimA, _dimB, _dimC);

            // Set dummy values
            var dummy = 0;
            foreach (var si in _space.IndicesAsEnumerable())
            {
                _space[si] = dummy++;
            }
        }


        [Fact]
        public void HasCorrectDimensions()
        {
            // Arrange
            var partialIndex = new SpatialIndex(new DimensionIndex(_dimA, 2));
            var subSpace = _space.GetSubSpace(partialIndex);
            
            Dimension[] expectedDimensions = [_dimB, _dimC];
            subSpace.Dimensions.Should().BeEquivalentTo(expectedDimensions);
        }

        [Fact]
        public void GetsValueFromParentSpace()
        {
            // Arrange
            var partialIndex = new SpatialIndex(new DimensionIndex(_dimA, 2));
            var subSpace = _space.GetSubSpace(partialIndex);
            
            // Act
            var subSpaceIndex = new SpatialIndex(
                new DimensionIndex(_dimB, 0),
                new DimensionIndex(_dimC, 0));
            var result = subSpace[subSpaceIndex];
            
            // Assert
            var parentIndex = new SpatialIndex(
                new DimensionIndex(_dimA, 2),
                new DimensionIndex(_dimB, 0),
                new DimensionIndex(_dimC, 0));
            var expectedValue = _space[parentIndex];
            result.Should().Be(expectedValue);
        }

        [Fact]
        public void SetsValueInParentSpace()
        {
            // Arrange
            var partialIndex = new SpatialIndex(new DimensionIndex(_dimA, 2));
            var subSpace = _space.GetSubSpace(partialIndex);
            
            // Act
            var subSpaceIndex = new SpatialIndex(
                new DimensionIndex(_dimB, 0),
                new DimensionIndex(_dimC, 0));
            subSpace[subSpaceIndex] = 999;

            // Assert
            var parentSpaceIndex = new SpatialIndex(
                new DimensionIndex(_dimA, 2),
                new DimensionIndex(_dimB, 0),
                new DimensionIndex(_dimC, 0));
            _space[parentSpaceIndex].Should().Be(999);
        }
    }

    public class SpaceTValueTest
    {
        private static readonly Dimension _dimA = new(2, "A");
        private static readonly Dimension _dimB = new(3, "B");
        private static readonly Dimension _dimC = new(4, "C");
        
        private readonly Space<int> _spaceA = new(_dimA, _dimB, _dimC);
        
        
        [Fact]
        public void SubSpacesAsEnumerable_ReturnsCorrectNumberOfSubSpaces()
        {
            var subSpaces = _spaceA.SubSpacesAsEnumerable(_dimA).ToArray();
            
            subSpaces.Length.Should().Be(_dimA.Size);  // 2
        }

        [Fact]
        public void SubSpacesAsEnumerable_ReturnsSubSpacesInExpectedOrder()
        {
            var subSpaces = _spaceA.SubSpacesAsEnumerable(_dimA).ToArray();
            
            var si0 = new SpatialIndex((_dimA, 0));
            var expectedSubSpace0 = _spaceA.GetSubSpace(si0);
            var si1 = new SpatialIndex((_dimA, 0));
            var expectedSubSpace1 = _spaceA.GetSubSpace(si1);
            
            subSpaces[0].Should().BeEquivalentTo(expectedSubSpace0);
            subSpaces[1].Should().BeEquivalentTo(expectedSubSpace1);
        }
    }

    [Fact]
    public void GetParentSpaceMapping_WithNoArgs_ReturnsExpectedValue()
    {
        // Arrangee
        var dimA = new Dimension(4, "A");
        var dimB = new Dimension(5, "B");
        var dimC = new Dimension(6, "C");
        
        var parentSpace = new Space<int>(dimA, dimB, dimC);
        var subSpace = parentSpace.GetSubSpace(new SpatialIndex(new DimensionIndex(dimA, 2)));
        
        // Act
        var mapping = subSpace.GetParentSpatialIndex();
        
        // Assert
        mapping.GetDimensionIndex(dimA).Should().Be(2);
    }
}
