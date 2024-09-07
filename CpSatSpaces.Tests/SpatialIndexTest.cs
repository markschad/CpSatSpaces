using FluentAssertions;
using JetBrains.Annotations;
using Mms.CpSat.Spaces;
using Xunit;

namespace CpSatSpaces.Tests;

[TestSubject(typeof(SpatialIndex))]
public class SpatialIndexTest
{

    [Fact]
    public void Equals_ReturnsTrue_WhenAllDimensionsAndIndicesAreTheSame()
    {
        var dimA = new Dimension(2, "A");
        var dimB = new Dimension(3, "B");
        var dimC = new Dimension(4, "C");
        
        var si1 = new SpatialIndex(
            new DimensionIndex(dimA, 0), 
            new DimensionIndex(dimB, 1), 
            new DimensionIndex(dimC, 2));
        
        var si2 = new SpatialIndex(
            new DimensionIndex(dimA, 0), 
            new DimensionIndex(dimB, 1), 
            new DimensionIndex(dimC, 2));
        
        si1.Equals(si2).Should().BeTrue();
    }
    
    [Fact]
    public void IsWithinSpace_ReturnsTrue_WhenAllIndicesAreWithinTheSpace()
    {
        var dimA = new Dimension(2, "A");
        var dimB = new Dimension(3, "B");
        var dimC = new Dimension(4, "C");
        
        var space = new Space(dimA, dimB, dimC);
        
        var si = new SpatialIndex(
            new DimensionIndex(dimA, 0), 
            new DimensionIndex(dimB, 1), 
            new DimensionIndex(dimC, 2));
        
        si.IsWithinSpace(space).Should().BeTrue();
    }
    
    [Fact]
    public void IsWithinSpace_ReturnsFalse_WhenAnyDimensionIsMissing()
    {
        var dimA = new Dimension(2, "A");
        var dimB = new Dimension(3, "B");
        var dimC = new Dimension(4, "C");
        
        var space = new Space(dimA, dimB, dimC);
        
        var si = new SpatialIndex(
            new DimensionIndex(dimA, 0), 
            new DimensionIndex(dimB, 1));
        
        si.IsWithinSpace(space).Should().BeFalse();
    }
    
}
