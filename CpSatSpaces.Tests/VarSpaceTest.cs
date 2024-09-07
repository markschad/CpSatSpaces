using FluentAssertions;
using JetBrains.Annotations;
using Mms.CpSat.Spaces;
using Xunit;

namespace CpSatSpaces.Tests;

[TestSubject(typeof(IntVarSpace))]
public class VarSpaceTest
{

    [Fact]
    public void GetName_ReturnsExpectedString()
    {
        var dimA = new Dimension(2, "A");
        var dimB = new Dimension(3, "B");
        var dimC = new Dimension(4, "C");

        var varSpace = new IntVarSpace(0, 1, dimA, dimB, dimC);

        var spatialIndex = new SpatialIndex(
            new DimensionIndex(dimA, 0),
            new DimensionIndex(dimB, 1),
            new DimensionIndex(dimC, 2));

        var name = varSpace.GetName(spatialIndex);

        name.Should().Be("A_0_B_1_C_2");
    }
}
