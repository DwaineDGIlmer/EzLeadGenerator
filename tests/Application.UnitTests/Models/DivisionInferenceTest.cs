using Application.Models;

namespace Application.UnitTests.Models;

sealed public class DivisionInferenceTest
{
    [Fact]
    public void Constructor_ShouldInitializePropertiesWithDefaults()
    {
        var inference = new DivisionInference();

        Assert.Equal(string.Empty, inference.Division);
        Assert.Equal(string.Empty, inference.Reasoning);
        Assert.Equal(0, inference.Confidence);
    }

    [Fact]
    public void Properties_ShouldSetAndGetValues()
    {
        var inference = new DivisionInference
        {
            Division = "Sales",
            Reasoning = "Based on department structure",
            Confidence = 85
        };

        Assert.Equal("Sales", inference.Division);
        Assert.Equal("Based on department structure", inference.Reasoning);
        Assert.Equal(85, inference.Confidence);
    }
}