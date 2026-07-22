using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Extensions;
using TaskFlow.Api.Contracts.Authentication;
using TaskFlow.Application.Authentication.Login;
using TaskFlow.Application.Authentication.Register;
using TaskFlow.Api.Authentication;
using TaskFlow.Application.Authentication.RefreshToken;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/authentication")]
public class AuthenticationController : ControllerBase
{
    private readonly ISender _sender;


    public AuthenticationController(ISender sender)
    {
        _sender = sender;
    }


    [HttpPost("register")]
    public async Task<IActionResult> Register(
        ApiRegisterRequest request,
        CancellationToken cancellationToken)
    {
        var command =
            new RegisterCommand(
                request.Email,
                request.DisplayName,
                request.Password);

        var result = await _sender.Send(
            command,
            cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToProblemDetails();
        }

        return Ok(new
        {
            Id = result.Value
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        ApiLoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Email, request.Password);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToProblemDetails();
        }

        return Ok(result.Value);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCommand(request.RefreshToken);

        var result =
            await _sender.Send(
                command,
                cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error.ToProblemDetails();
        }

        return Ok(result.Value);
    }
}