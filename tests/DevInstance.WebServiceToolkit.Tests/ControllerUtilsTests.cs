using DevInstance.WebServiceToolkit.Controllers;
using DevInstance.WebServiceToolkit.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace DevInstance.WebServiceToolkit.Tests;

public class ControllerUtilsTests
{
    private sealed class TestController : ControllerBase { }

    private sealed class TestEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;
        public string ApplicationName { get; set; } = "Tests";
        public string ContentRootPath { get; set; } = "";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    private sealed class PaymentRequiredException : WebServiceException
    {
        public PaymentRequiredException() : base(402, "Pay up") { }
    }

    private static TestController CreateController(string environment = "Production")
    {
        var services = new ServiceCollection()
            .AddSingleton<IHostEnvironment>(new TestEnvironment { EnvironmentName = environment })
            .BuildServiceProvider();

        return new TestController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = services }
            }
        };
    }

    private static (int Status, WebServiceError Error) Run(Exception ex, string environment = "Production")
    {
        var controller = CreateController(environment);
        var result = controller.HandleWebRequestAsync<string>(() => throw ex).GetAwaiter().GetResult();

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        var error = Assert.IsType<WebServiceError>(objectResult.Value);
        return (objectResult.StatusCode!.Value, error);
    }

    [Fact]
    public async Task Success_ReturnsHandlerResult()
    {
        var controller = CreateController();

        var result = await controller.HandleWebRequestAsync<string>(async () =>
        {
            await Task.Yield();
            return controller.Ok("value");
        });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal("value", ok.Value);
    }

    [Theory]
    [MemberData(nameof(MappedExceptions))]
    public void KnownExceptions_MapToStatusAndBody(Exception ex, int status, WebServiceErrorType type)
    {
        var (actualStatus, error) = Run(ex);

        Assert.Equal(status, actualStatus);
        Assert.Equal(type, error.ErrorType);
        Assert.Equal(ex.Message, error.Message);
    }

    public static TheoryData<Exception, int, WebServiceErrorType> MappedExceptions => new()
    {
        { new BadRequestException("bad"), 400, WebServiceErrorType.Validation },
        { new UnauthorizedException("who"), 401, WebServiceErrorType.General },
        { new ForbiddenException("no"), 403, WebServiceErrorType.General },
        { new RecordNotFoundException("abc"), 404, WebServiceErrorType.General },
        { new RecordConflictException("abc"), 409, WebServiceErrorType.General },
        { new UnprocessableEntityException("rule"), 422, WebServiceErrorType.Validation },
        { new PaymentRequiredException(), 402, WebServiceErrorType.General },
    };

    [Fact]
    public void PropertyName_IsCarriedToBody()
    {
        var (_, error) = Run(new BadRequestException("Name is required", "Name"));

        Assert.Equal("Name", error.PropertyName);
    }

    [Fact]
    public void UnhandledException_InProduction_HidesMessageAndStackTrace()
    {
        var (status, error) = Run(new InvalidOperationException("db password is hunter2"));

        Assert.Equal(500, status);
        Assert.Equal(WebServiceErrorType.Exception, error.ErrorType);
        Assert.Equal(ControllerUtils.UnexpectedErrorMessage, error.Message);
    }

    [Fact]
    public void UnhandledException_InDevelopment_ExposesMessage()
    {
        var (status, error) = Run(new InvalidOperationException("boom"), Environments.Development);

        Assert.Equal(500, status);
        Assert.Equal("boom", error.Message);
    }

    [Fact]
    public void SyncHandler_UsesSameMapping()
    {
        var controller = CreateController();

        var result = controller.HandleWebRequest<string>(() => throw new RecordNotFoundException("x"));

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(404, objectResult.StatusCode);
        Assert.IsType<WebServiceError>(objectResult.Value);
    }

    [Fact]
    public void ErrorType_NumbersMatchBlazorToolkitServiceActionErrorType()
    {
        // BlazorToolkit clients deserialize this enum as ServiceActionErrorType by number.
        Assert.Equal(0, (int)WebServiceErrorType.Unknown);
        Assert.Equal(1, (int)WebServiceErrorType.Exception);
        Assert.Equal(2, (int)WebServiceErrorType.General);
        Assert.Equal(3, (int)WebServiceErrorType.Validation);
    }
}
