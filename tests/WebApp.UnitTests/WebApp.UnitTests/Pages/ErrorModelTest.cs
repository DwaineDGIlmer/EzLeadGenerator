using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Pages;

namespace WebApp.UnitTests.Pages;

public class ErrorModelTests
{
    [Fact]
    public void ShowRequestId_ReturnsFalse_WhenRequestIdIsNullOrEmpty()
    {
        var model = new ErrorModel
        {
            RequestId = null
        };
        Assert.False(model.ShowRequestId);

        model.RequestId = "";
        Assert.False(model.ShowRequestId);
    }

    [Fact]
    public void ShowRequestId_ReturnsTrue_WhenRequestIdIsNotEmpty()
    {
        ErrorModel model = new()
        {
            RequestId = "abc123"
        };
        Assert.True(model.ShowRequestId);
    }

    [Fact]
    public void OnGet_SetsRequestId_FromActivityOrHttpContext()
    {
        // Arrange
        var model = new ErrorModel();
        var httpContext = new DefaultHttpContext
        {
            TraceIdentifier = "trace-xyz"
        };
        var pageContext = new PageContext { HttpContext = httpContext };
        model.PageContext = pageContext;

        // Act
        model.OnGet();

        // Assert
        Assert.Equal("trace-xyz", model.RequestId);
    }
}