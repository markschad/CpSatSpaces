﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Google.OrTools.ConstraintSolver;
using Google.OrTools.Sat;
using JetBrains.Annotations;
using Mms.CpSat.Spaces;
using Mms.CpSat.Spaces.ConstraintBuilder;
using Xunit;
using Xunit.Abstractions;
using IntVar = Google.OrTools.Sat.IntVar;

namespace CpSatSpaces.Tests;

[TestSubject(typeof(LeftTargetSelectorIntVar))]
[TestSubject(typeof(RightSpaceSelector<IntVar>))]
[TestSubject(typeof(RightConditionSelectorIntVar<IntVar>))]
public class ConstraintBuilderTest
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly StringWriter _consoleWriter;
    private readonly CpModelWithSpacesFixture _fixture = new CpModelWithSpacesFixture();

    public ConstraintBuilderTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;

        _consoleWriter = new StringWriter();
        Console.SetOut(_consoleWriter);

        // Apply fixed values to the right-side space (C[4] x D[5])
        int[][] spaceCdValues =
        [
            [0, 1, 1, 2, 2], // 6
            [1, 2, 0, 3, 4], // 10
            [1, 0, 6, 6, 1], // 14
            [5, 2, 3, 3, 3] // 16
        ];

        foreach (var siv in _fixture.SpaceCd.ValuesAsEnumerable())
        {
            var c = siv.Index[_fixture.DimC];
            var d = siv.Index[_fixture.DimD];
            var value = spaceCdValues[c][d];
            _fixture.Model.Add(siv.GetValue() == value);
        }
    }

    [Fact]
    public void SolvesWithExpectedValue_WhenPairedWith_IsExactly()
    {
        // Arrange
        var solver = new CpSolver();

        // Act
        new LeftSpaceSelector(_fixture.Model)
            .InWhich(_fixture.SpaceAbc)
            .WillBe(1)
            .OnlyIf(_fixture.SpaceCd)
            .SumsToExactly(10);
        var solverStatus = solver.Solve(_fixture.Model);

        // Assert
        solverStatus.Should().Be(CpSolverStatus.Optimal);

        DimensionIndex[] di;

        // Check [ 0, 0, 0 ] != 1
        di =
        [
            (_fixture.DimA, 0),
            (_fixture.DimB, 0),
            (_fixture.DimC, 0)
        ];
        solver.Value(_fixture.SpaceAbc.GetValue(new SpatialIndex(di))).Should().NotBe(1);

        // Check [ 0, 0, 1 ] == 1
        di =
        [
            (_fixture.DimA, 0),
            (_fixture.DimB, 0),
            (_fixture.DimC, 1)
        ];
        solver.Value(_fixture.SpaceAbc.GetValue(new SpatialIndex(di))).Should().Be(1);

        // Check [ 1, 0, 0 ] != 1
        di =
        [
            (_fixture.DimA, 1),
            (_fixture.DimB, 0),
            (_fixture.DimC, 0)
        ];
        solver.Value(_fixture.SpaceAbc.GetValue(new SpatialIndex(di))).Should().NotBe(1);

        // Check [ 0, 1, 1 ] == 1
        di =
        [
            (_fixture.DimA, 0),
            (_fixture.DimB, 1),
            (_fixture.DimC, 1)
        ];
        solver.Value(_fixture.SpaceAbc.GetValue(new SpatialIndex(di))).Should().Be(1);
    }

    [Fact]
    public void SolvesWithExpectedValue_WhenPairedWith_AtMost()
    {
        // Arrange
        var solver = new CpSolver();

        // Act
        new LeftSpaceSelector(_fixture.Model)
            .InWhich(_fixture.SpaceAbc)
            .WillBe(1)
            .OnlyIf(_fixture.SpaceCd)
            .SumsToAtMost(10);
        var solverStatus = solver.Solve(_fixture.Model);

        // Assert
        solverStatus.Should().Be(CpSolverStatus.Optimal);

        DimensionIndex[] di;

        // Check [ 0, 0, 0 ] = 1
        di =
        [
            (_fixture.DimA, 0),
            (_fixture.DimB, 0),
            (_fixture.DimC, 0)
        ];
        solver.Value(_fixture.SpaceAbc.GetValue(new SpatialIndex(di))).Should().Be(1);

        // Check [ 0, 0, 1 ] == 0
        di =
        [
            (_fixture.DimA, 0),
            (_fixture.DimB, 0),
            (_fixture.DimC, 1)
        ];
        solver.Value(_fixture.SpaceAbc.GetValue(new SpatialIndex(di))).Should().Be(1);

        // Check [ 0, 0, 2 ] != 1
        di =
        [
            (_fixture.DimA, 0),
            (_fixture.DimB, 0),
            (_fixture.DimC, 2)
        ];
        solver.Value(_fixture.SpaceAbc.GetValue(new SpatialIndex(di))).Should().NotBe(1);

        // Check [ 0, 0, 3 ] != 1
        di =
        [
            (_fixture.DimA, 0),
            (_fixture.DimB, 0),
            (_fixture.DimC, 3)
        ];
        solver.Value(_fixture.SpaceAbc.GetValue(new SpatialIndex(di))).Should().NotBe(1);
    }

    [Fact]
    public void IntersectionSpaceSelector_FormsConstraint_WithCorrectVars()
    {
        // Arrange
        var dimA = new Dimension(4, "A");
        var dimB = new Dimension(5, "B");
        var dimC = new Dimension(6, "C");
        var dimD = new Dimension(3, "D");
        var dimE = new Dimension(3, "E");
        
        var spaceAde = new IntVarSpace(0, 10,dimA, dimD, dimE);
        var spaceBde = new IntVarSpace(0, 10, dimB, dimD, dimE);
        var spaceCde = new IntVarSpace(0, 10, dimC, dimD, dimE);
        
        var model = new CpModel();
        model.InitSpace(spaceAde);
        model.InitSpace(spaceBde);
        model.InitSpace(spaceCde);
        
        // Act
        model.AddSpaceConstraint()
            .InWhich(spaceAde)
            .And(spaceBde)
            .And(spaceCde)
            .FormConstraint((m, si, ade, bde, cde) =>
            {
                // Assert
                ade.ValuesAsEnumerable().Count().Should().Be(4);
                bde.ValuesAsEnumerable().Count().Should().Be(5);
                cde.ValuesAsEnumerable().Count().Should().Be(6);
            });

    }
}
