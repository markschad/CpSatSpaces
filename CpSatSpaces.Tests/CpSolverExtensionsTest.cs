using FluentAssertions;
using Google.OrTools.ConstraintSolver;
using Google.OrTools.Sat;
using JetBrains.Annotations;
using Mms.CpSat.Spaces;
using Xunit;

namespace CpSatSpaces.Tests;

[TestSubject(typeof(CpSolverExtensions))]
public class CpSolverExtensionsTest
{
    private static readonly Dimension DimA = new(2, "A");
    private static readonly Dimension DimB = new(3, "B");
    private static readonly Dimension DimC = new(4, "C");
    
    private static readonly IntVarSpace IntVarSpaceAbc = new(0, 10, DimB, DimC, DimA);
    private static readonly BoolVarSpace BoolVarSpaceAbc = new(DimA, DimB, DimC);
    
    private static readonly CpModel Model = new();

    [Theory]
    [InlineData(0, 0, 0, 0, "A_0_B_0_C_0 = 0")]
    [InlineData(1, 2, 3, 6, "A_1_B_2_C_3 = 6")]
    [InlineData(1, 2, 1, 2, "A_1_B_2_C_1 = 2")]
    public void ToString_OnIntVarSpace_ReturnsExpectedValue(int a, int b, int c, int v, string expectedToContains)
    {
        // Arrange
        Model.InitSpace(IntVarSpaceAbc);
        var siv = new SpatialIndex((DimA, a), (DimB, b), (DimC, c));
        Model.Add(IntVarSpaceAbc.GetValue(siv) == v);
        var solver = new CpSolver();
        var result = solver.Solve(Model);
        
        // Act
        var actual = solver.ToString(IntVarSpaceAbc);
        
        // Assert
        actual.Should().Contain(expectedToContains);
    }
    
    [Theory]
    [InlineData(0, 0, 0, "A_0_B_0_C_0")]
    [InlineData(1, 2, 3,  "A_1_B_2_C_3")]
    [InlineData(1, 2, 1,  "A_1_B_2_C_1")]
    public void ToString_OnBoolVarSpace_ExpectedToContain(int a, int b, int c, string expectedToContains)
    {
        // Arrange
        Model.InitSpace(BoolVarSpaceAbc);
        var siv = new SpatialIndex((DimA, a), (DimB, b), (DimC, c));
        Model.Add(BoolVarSpaceAbc.GetValue(siv) == 1);
        var solver = new CpSolver();
        var result = solver.Solve(Model);
        
        // Act
        var actual = solver.ToString(BoolVarSpaceAbc);
        
        // Assert
        actual.Should().Contain(expectedToContains);
    }
    
    [Theory]
    [InlineData(0, 0, 0, "A_0_B_0_C_0")]
    [InlineData(1, 2, 3,  "A_1_B_2_C_3")]
    [InlineData(1, 2, 1,  "A_1_B_2_C_1")]
    public void ToString_OnBoolVarSpace_ExpectedToNotContain(int a, int b, int c, string expectedToContains)
    {
        // Arrange
        Model.InitSpace(BoolVarSpaceAbc);
        var siv = new SpatialIndex((DimA, a), (DimB, b), (DimC, c));
        Model.Add(BoolVarSpaceAbc.GetValue(siv) == 0);
        var solver = new CpSolver();
        var result = solver.Solve(Model);
        
        // Act
        var actual = solver.ToString(BoolVarSpaceAbc);
        
        // Assert
        actual.Should().NotContain(expectedToContains);
    }
}