﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Google.OrTools.ConstraintSolver;
using Google.OrTools.Sat;
using JetBrains.Annotations;
using Mms.CpSat.Spaces;
using Xunit;
using Xunit.Abstractions;
using IntVar = Google.OrTools.Sat.IntVar;

namespace CpSatSpaces.Tests;

[TestSubject(typeof(SpaceConstraintBuilder<,>))]
public class SpaceConstraintBuilderTest
{
    /// <summary>
    /// Tests the <see cref="SpaceConstraintBuilder{T1,T2}.WillBe"/> method to ensure values are correctly set in the
    /// left-side space based on constraints on the right-side space.
    /// </summary>
    public class WillBeTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly StringWriter _consoleWriter;
        private readonly CpModelWithSpacesFixture _fixture = new CpModelWithSpacesFixture();

        public WillBeTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            
            _consoleWriter = new StringWriter();
            Console.SetOut(_consoleWriter);

            // Apply fixed values to the right-side space (C[4] x D[5])
            int[][] spaceCdValues =
            [
                [0, 1, 1, 2, 2],    // 6
                [1, 2, 0, 3, 4],    // 10
                [1, 0, 6, 6, 1],    // 14
                [5, 2, 3, 3, 3]     // 16
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
            var spaceConstraintBuilder = new SpaceConstraintBuilder<IntVar, IntVar>(_fixture.Model);
            var solver = new CpSolver();
            
            // Act
            spaceConstraintBuilder
                .InWhich(_fixture.SpaceAbc)
                .WillBe(1)
                .If(_fixture.SpaceCd)
                .IsExactly(10);
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
            var spaceConstraintBuilder = new SpaceConstraintBuilder<IntVar, IntVar>(_fixture.Model);
            var solver = new CpSolver();
            
            // Act
            spaceConstraintBuilder
                .InWhich(_fixture.SpaceAbc)
                .WillBe(1)
                .If(_fixture.SpaceCd)
                .IsAtMost(10);
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
    }
}