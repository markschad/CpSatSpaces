using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Google.OrTools.Sat;
using Mms.CpSat.Spaces;
using Xunit;

namespace CpSatSpaces.Tests;

public class CpModelWithSpacesFixture
{
    public readonly Dimension DimA = new(2, "A");
    public readonly Dimension DimB = new(3, "B");
    public readonly Dimension DimC = new(4, "C");
    public readonly Dimension DimD = new(5, "D");

    public readonly IntVarSpace SpaceAbc;
    public readonly IntVarSpace SpaceCd;

    public readonly CpModel Model = new CpModel();
    
    public CpModelWithSpacesFixture()
    {
        SpaceAbc = new IntVarSpace(0, 10, DimA, DimB, DimC);
        SpaceCd = new IntVarSpace(0, 10, DimC, DimD);
        
        Model.InitSpace(SpaceAbc);
        Model.InitSpace(SpaceCd);
    }
    
    public void ForEachIntersection(CpSolver solver, Action<IntVar, IEnumerable<IntVar>> action)
    {
        for (int a = 0; a < DimA.Size; a++)
        {
            for (int b = 0; b < DimB.Size; b++)
            {
                for (int c = 0; c < DimC.Size; c++)
                {
                    DimensionIndex[] dimIndicesAbc =
                    [
                        (DimA, a),
                        (DimB, b),
                        (DimC, c)
                    ];
                    var spaceAbcIndex = new SpatialIndex(dimIndicesAbc);
                    var varAbc = SpaceAbc.GetValue(spaceAbcIndex);
                        
                    var varsCd = new List<IntVar>();
                    for (var d = 0; d < DimD.Size; d++)
                    {
                        DimensionIndex[] dimIndicesCd =
                        [
                            (DimC, c),
                            (DimD, d)
                        ];
                        var spaceCdIndex = new SpatialIndex(dimIndicesCd);
                        varsCd.Add(SpaceCd.GetValue(spaceCdIndex));
                    }

                    action(varAbc, varsCd);
                }
            }
        }
    }
}