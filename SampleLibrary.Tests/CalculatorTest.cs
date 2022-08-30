using Xunit;

namespace SampleLibrary.Tests;

public class UnitTest1
{
    [Fact]
    public void OneAddOneShouldBeTwo()
    {
        var calculator = new Calculator();
        var expected = 2;

        var actual = calculator.Add(1, 1);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TenDivideTwoShouldBeFive()
    {
        var calculator = new Calculator();
        var expected = 5;

        var actual = calculator.Divide(10, 2);

        Assert.Equal(expected, actual);
    }
}