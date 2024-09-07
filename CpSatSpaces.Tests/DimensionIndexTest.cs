using System;
using FluentAssertions;
using JetBrains.Annotations;
using Mms.CpSat.Spaces;
using Xunit;

namespace CpSatSpaces.Tests;

[TestSubject(typeof(DimensionIndex))]
public class DimensionIndexTest
{

    [Fact]
    public void Constructor_CreatesInstanceWithExpectedValues()
    {
        var dimA = new Dimension(2, "A");
        
        var di = new DimensionIndex(dimA, 0);
        
        di.Dimension.Should().Be(dimA);
        di.Index.Should().Be(0);
    }
    
    [Fact]
    public void Constructor_ThrowsArgumentException_WhenIndexIsOutOfBounds()
    {
        var dimA = new Dimension(2, "A");
        
        Assert.Throws<ArgumentException>(() => new DimensionIndex(dimA, 2));
        Assert.Throws<ArgumentException>(() => new DimensionIndex(dimA, -1));
    }
    
    [Fact]
    public void Equals_ReturnsTrue_WhenDimensionAndIndexAreEqual()
    {
        var dimA = new Dimension(2, "A");
        
        var di1 = new DimensionIndex(dimA, 0);
        var di2 = new DimensionIndex(dimA, 0);
        
        di1.Equals(di2).Should().BeTrue();
    }
    
    [Fact]
    public void Equals_ReturnsFalse_WhenDimensionOrIndexAreNotEqual()
    {
        var dimA = new Dimension(2, "A");
        var dimB = new Dimension(2, "B");
        
        var di1 = new DimensionIndex(dimA, 0);
        var di2 = new DimensionIndex(dimA, 1);
        var di3 = new DimensionIndex(dimB, 0);
        
        di1.Equals(di2).Should().BeFalse();
        di2.Equals(di3).Should().BeFalse();
    }
    
    [Fact]
    public void Addition_ReturnsExpectedDimensionIndex()
    {
        var dimA = new Dimension(6, "A");
        var di = new DimensionIndex(dimA, 0);
        
        var result = di + 3;
        
        var expectedResult = new DimensionIndex(dimA, 3);
        result.Should().Be(expectedResult);
    }
    
    [Fact]
    public void Addition_ThrowsInvalidOperationException_WhenIndexIsOutOfBounds()
    {
        var dimA = new Dimension(6, "A");
        var di = new DimensionIndex(dimA, 5);
        
        Assert.Throws<InvalidOperationException>(() => di + 1);
    }
    
    [Fact]
    public void Subtraction_ReturnsExpectedDimensionIndex()
    {
        var dimA = new Dimension(6, "A");
        var di = new DimensionIndex(dimA, 5);
        
        var result = di - 3;
        
        var expectedResult = new DimensionIndex(dimA, 2);
        result.Should().Be(expectedResult);
    }
    
    [Fact]
    public void Subtraction_ThrowsInvalidOperationException_WhenIndexIsOutOfBounds()
    {
        var dimA = new Dimension(6, "A");
        var di = new DimensionIndex(dimA, 0);
        
        Assert.Throws<InvalidOperationException>(() => di - 1);
    }
}
