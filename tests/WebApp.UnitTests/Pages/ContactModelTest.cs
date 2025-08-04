using WebApp.Pages;

namespace WebApp.UnitTests.Pages;

public class ContactModelTests
{
    [Fact]
    public void OnGet_DoesNotThrow()
    {
        // Arrange
        var model = new ContactModel();

        // Act & Assert
        var exception = Record.Exception(() => model.OnGet());
        Assert.Null(exception);
    }
}