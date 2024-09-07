using FluentAssertions;
using JetBrains.Annotations;
using Mms.CpSat.Spaces;
using Xunit;

namespace CpSatSpaces.Tests;

[TestSubject(typeof(DimensionExtensions))]
public class DimensionExtensionsTest
{

    [Fact]
    public void GetVolume_ReturnsExpectedValue()
    {
        var dimA = new Dimension(2, "A");
        var dimB = new Dimension(3, "B");
        var dimC = new Dimension(4, "C");

        Dimension[] dimensions = [dimA, dimB, dimC];

        var volume = dimensions.GetVolume();
        
        const int expectedValue = 2 * 3 * 4; // 24
        volume.Should().Be(expectedValue);
    }
}
