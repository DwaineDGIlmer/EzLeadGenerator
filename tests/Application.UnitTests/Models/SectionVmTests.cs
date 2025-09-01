using Application.Models;

namespace Application.UnitTests.Models;

public sealed class SectionVmTests
{
    [Fact]
    public void SectionVm_DefaultConstructor_InitializesProperties()
    {
        var section = new SectionVm();

        Assert.Equal(string.Empty, section.Title);
        Assert.NotNull(section.Items);
        Assert.Empty(section.Items);
    }

    [Fact]
    public void SectionVm_SetTitleAndItems_PropertiesAreSetCorrectly()
    {
        var items = new List<OrganicResultItem>
    {
        new() { Title = "Test", Url = "http://test.com" }
    };

        var section = new SectionVm
        {
            Title = "Jobs",
            Items = items
        };

        Assert.Equal("Jobs", section.Title);
        Assert.Equal(items, section.Items);
        Assert.Single(section.Items);
        Assert.Equal("Test", section.Items[0].Title);
    }
}
