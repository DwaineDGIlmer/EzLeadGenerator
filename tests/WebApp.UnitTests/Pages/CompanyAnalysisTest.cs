namespace WebApp.UnitTests.Pages;

public class CompanyAnalysisTest
{
    [Fact]
    public void CompanyAnalysisTest_CanBeConstructed()
    {
        // Arrange & Act
        var pageModel = new CompanyAnalysisTest();

        // Assert
        Assert.NotNull(pageModel);
    }
}
