using DevInstance.WebServiceToolkit.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace DevInstance.WebServiceToolkit.Controllers;

public static class ControllerUtils
{
    public delegate ActionResult<T> WebHandler<T>();
    public delegate Task<ActionResult<T>> WebHandlerAsync<T>();

    private static ActionResult<T> HandleException<T>(ControllerBase controller, Exception ex)
    {
        return controller.Problem(detail: ex.StackTrace, title: ex.Message);
    }

    public static async Task<ActionResult<T>> HandleWebRequestAsync<T>(this ControllerBase controller, WebHandlerAsync<T> handler)
    {
        try
        {
            return await handler();
        }
        catch (RecordNotFoundException)
        {
            return controller.NotFound();
        }
        catch (RecordConflictException)
        {
            return controller.Conflict();
        }
        catch (UnauthorizedException ex)
        {
            return controller.Unauthorized(ex.Message);
        }
        catch (BadRequestException ex)
        {
            return controller.BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return HandleException<T>(controller, ex);
        }
    }

    public static ActionResult<T> HandleWebRequest<T>(this ControllerBase controller, WebHandler<T> handler)
    {
        try
        {
            return handler();
        }
        catch (RecordNotFoundException)
        {
            return controller.NotFound();
        }
        catch (RecordConflictException)
        {
            return controller.Conflict();
        }
        catch (UnauthorizedException ex)
        {
            return controller.Unauthorized(ex.Message);
        }
        catch (BadRequestException ex)
        {
            return controller.BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return HandleException<T>(controller, ex);
        }
    }
}
