using System.Linq;
using FluentAssertions;
using Google.OrTools.Sat;
using JetBrains.Annotations;
using Mms.CpSat.Spaces;
using Xunit;
using Xunit.Abstractions;

namespace CpSatSpaces.Tests;

[TestSubject(typeof(CpModelExtensions))]
public class CpModelExtensionsTest
{
    private static readonly Dimension DimA = new(2, "A");
    private static readonly Dimension DimB = new(3, "B");
    private static readonly Dimension DimC = new(4, "C");
    private static readonly Dimension DimD = new(2, "D");

    private static readonly BoolVarSpace SpaceAbc = new(DimA, DimB, DimC);
    private static readonly BoolVarSpace SpaceBcd = new(DimB, DimC, DimD);

    private readonly ITestOutputHelper _testOutputHelper;
    
    private readonly CpModel _model;

    public CpModelExtensionsTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        
        _model = new CpModel();
        
        _model.InitSpace(SpaceAbc);
        _model.InitSpace(SpaceBcd);
        
    }

    [Fact]
    public void OnAlignedDimensions_AddsExpectedConstraints()
    {
        _model.AddSpaceConstraint<BoolVar, BoolVar>()
            .InWhich(SpaceAbc)
            .WillBe(0)
            .If(SpaceBcd)
            .IsAtLeast(1);
        
        // // Arrange
        // _model.AddSpaceConstraint(SpaceAbc, SpaceBcd)
        //     .OnAlignedDimensions((v, w) =>
        //     {
        //         _model.Add(LinearExpr.Sum(w) > 0)
        //             .OnlyEnforceIf(v);
        //         _model.Add(LinearExpr.Sum(w) == 0)
        //             .OnlyEnforceIf(v.Not());
        //     });

        var b = 1;
        foreach (var v in SpaceBcd.ValuesAsEnumerable())
        {
            _model.Add(v.GetValue() == b);
            b = 1 - b;
        }
        
        // Act
        var solver = new CpSolver();
        var result = solver.Solve(_model);
        
        // Assert
        result.Should().Be(CpSolverStatus.Optimal);

        var bConstraintSpace = SpaceBcd.Except(SpaceAbc);
        
        foreach (var v in SpaceAbc.ValuesAsEnumerable())
        {
            var subSpaceB = SpaceBcd.GetSubSpace(v.Index.Extract(bConstraintSpace.Dimensions.ToArray()));

            if (solver.BooleanValue(v.GetValue()))
            {
                subSpaceB.ValuesAsEnumerable()
                    .Any(w => solver.BooleanValue(w.GetValue()))
                    .Should().BeTrue();
            }
            else
            {
                subSpaceB.ValuesAsEnumerable()
                    .All(w => solver.BooleanValue(w.GetValue()))
                    .Should().BeFalse();
            }
        }
        

    }
}
