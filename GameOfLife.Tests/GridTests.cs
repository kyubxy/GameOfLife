using NUnit.Framework;

namespace GameOfLife.Tests;

[TestFixture]
public class GridTests
{
    [Test]
    public void TestGetDimensions()
    {
        var situation = new []
        {
            new[] {false, false, false},
            new[] {false, false, false},
            new[] {false, false, false},
            new[] {false, false, false},
        };
        
        Assert.AreEqual((3,4), Grid.GetDimensions(situation));
    }

    [Test]
    public void TestGetAdjCells()
    {
        var situation = new []
        {
            new[] {false, true, false, false},
            new[] {false, true, true, false},
            new[] {false, false, true, false},
            new[] {false, false, false, false},
        };

        Assert.AreEqual(3, Grid.CheckAdjCells(situation, 1, 1));
    }
    
    [Test]
    public void TestGetAdjCellsEdge()
    {
        var situation = new []
        {
            new[] {false, true, false, false},
            new[] {false, true, true, false},
            new[] {false, false, true, false},
            new[] {false, false, true, false},
        };

        Assert.AreEqual(2, Grid.CheckAdjCells(situation, 3, 3));
    }

    [Test]
    public void TestIsAlive()
    {
        var situation = new []
        {
            new[] {false, false, false},
            new[] {true, true, true},
            new[] {false, false, false}
        };
        
        Assert.IsFalse(Grid.IsAlive(situation, 2, 1));
    }
}