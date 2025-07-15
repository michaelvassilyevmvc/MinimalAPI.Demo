using System.Net;
using AutoMapper;
using FluentValidation;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Models.DTO;
using MagicVilla_CouponAPI.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace MagicVilla_CouponAPI.Endpoints;

public static class AuthEndpoints
{
    public static void ConfigureAuthEndpoints(this WebApplication app)
    {
        // LOGIN
        app.MapPost("/api/login", Login)
            .WithName("Login")
            .Accepts<LoginRequestDTO>("application/json")
            .Produces<APIResponse>(200)
            .Produces(400);
        // REGISTER
        app.MapPost("/api/register", Register)
            .WithName("Register")
            .Accepts<RegistrationRequestDTO>("application/json")
            .Produces<APIResponse>(200)
            .Produces(400);
    }


    private static async Task<IResult> Login(IAuthRepository _authRepo,
        [FromBody] LoginRequestDTO model)
    {
        APIResponse response = new()
        {
            IsSuccess = false,
            StatusCode = HttpStatusCode.BadRequest,
        };

        var loginResponse = await _authRepo.Login(model);
        if (loginResponse == null)
        {
            response.ErrorMessages.Add("Username or password is incorrect.");
            return Results.BadRequest(response);
        }

        response.Result = loginResponse;
        response.IsSuccess = true;
        response.StatusCode = HttpStatusCode.OK;
        return Results.Ok(response);
    }

    private static async Task<IResult> Register(IAuthRepository _authRepo,
        [FromBody] RegistrationRequestDTO model)
    {
        APIResponse response = new()
        {
            IsSuccess = false,
            StatusCode = HttpStatusCode.BadRequest,
        };
        
        bool ifUserNameisUnique =  _authRepo.IsUniqueUser(model.UserName);
        if (!ifUserNameisUnique)
        {
            response.ErrorMessages.Add("Username is already existed.");
            return Results.BadRequest(response);
        }

        var registerResponse = await _authRepo.Register(model);
        if (registerResponse == null || string.IsNullOrEmpty(registerResponse.UserName))
        {
            return Results.BadRequest(response);
        }

        response.IsSuccess = true;
        response.StatusCode = HttpStatusCode.OK;
        return Results.Ok(response);
    }
}