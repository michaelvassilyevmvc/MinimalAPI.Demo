using System.Net;
using AutoMapper;
using FluentValidation;
using MagicVilla_CouponAPI;
using MagicVilla_CouponAPI.Data;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Models.DTO;
using MagicVilla_CouponAPI.Validations;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(MappingConfig));
builder.Services.AddValidatorsFromAssembly(typeof(CouponCreateValidation).Assembly);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// GET
app.MapGet("/api/coupon", (ILogger<Program> _logger) =>
    {
        APIResponse response = new();
        _logger.LogInformation("Get all coupons");
        response.Result = CouponStore.couponList;
        response.IsSuccess = true;
        response.StatusCode = HttpStatusCode.OK;
        return Results.Ok(response);
    })
    .WithName("GetCoupons")
    .Produces<APIResponse>(201);

app.MapGet("/api/coupon/{id:int}", (ILogger<Program> _logger, int id) =>
    {
        APIResponse response = new();
        _logger.LogInformation($"Get coupon {id}");
        response.Result = CouponStore.couponList.FirstOrDefault(x => x.Id == id);
        response.IsSuccess = true;
        response.StatusCode = HttpStatusCode.OK;
        return Results.Ok(response);
    })
    .WithName("GetCoupon")
    .Produces<APIResponse>(201);

// POST

app.MapPost("/api/coupon",
        async (IMapper _mapper, IValidator<CouponCreateDto> _validation, [FromBody] CouponCreateDto coupon_C_DTO) =>
        {
            APIResponse response = new()
            {
                IsSuccess = false,
                StatusCode = HttpStatusCode.BadRequest,
            };


            var validationResult = await _validation.ValidateAsync(coupon_C_DTO);
            if (!validationResult.IsValid)
            {
                response.ErrorMessages.Add(validationResult.Errors.FirstOrDefault()
                    .ToString());
                return Results.BadRequest(response);
            }

            if (CouponStore.couponList.Any(x => x.Name.ToLower() == coupon_C_DTO.Name.ToLower()))
            {
                response.ErrorMessages.Add("A coupon with the same name already exists");
                return Results.BadRequest(response);
            }

            Coupon coupon = _mapper.Map<Coupon>(coupon_C_DTO);

            int max = CouponStore.couponList.Max(x => x.Id);
            coupon.Id = max + 1;
            CouponStore.couponList.Add(coupon);
            CouponDTO couponDto = _mapper.Map<CouponDTO>(coupon);

            // return Results.CreatedAtRoute("GetCoupon", new { id = coupon.Id }, couponDto);

            response.Result = couponDto;
            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.Created;
            return Results.Ok(response);
            // return Results.Created($"/api/coupon/{coupon.Id}", coupon);
        })
    .WithName("CreatedCoupon")
    .Accepts<CouponCreateDto>("application/json")
    .Produces<APIResponse>(201)
    .Produces(400);

// PUT
app.MapPut("/api/coupon",
        async (IMapper _mapper, IValidator<CouponUpdateDto> _validation, [FromBody] CouponUpdateDto coupon_U_DTO) =>
        {
            APIResponse response = new()
            {
                IsSuccess = false,
                StatusCode = HttpStatusCode.BadRequest,
            };
            
            var validationResult = await _validation.ValidateAsync(coupon_U_DTO);
            if (!validationResult.IsValid)
            {
                response.ErrorMessages.Add(validationResult.Errors.FirstOrDefault()
                    .ToString());
                return Results.BadRequest(response);
            }
            if (CouponStore.couponList.Any(x => x.Name.ToLower() == coupon_U_DTO.Name.ToLower()))
            {
                response.ErrorMessages.Add("A coupon with the same name already exists");
                return Results.BadRequest(response);
            }
            
            Coupon coupon = CouponStore.couponList.FirstOrDefault(x => x.Id == coupon_U_DTO.Id);
            if (coupon == null)
            {
                response.ErrorMessages.Add($"A coupon with {coupon_U_DTO.Id} not exists");
                return Results.NotFound(response);
            }
            
            coupon.Name = coupon_U_DTO.Name;
            coupon.Percent = coupon_U_DTO.Percent;
            coupon.IsActive = coupon_U_DTO.IsActive;
            coupon.Created = coupon_U_DTO.Created;
            coupon.LastUpdated = coupon_U_DTO.LastUpdated;
            
            
            CouponDTO couponDto = _mapper.Map<CouponDTO>(coupon);
            response.Result = couponDto;
            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            return Results.Ok(response);
        })
    .WithName("UpdatedCoupon")
    .Accepts<CouponUpdateDto>("application/json")
    .Produces<APIResponse>(201)
    .Produces(400);

// DELETE
app.MapDelete("/api/coupon/{id:int}", (int id) =>
{
    var coupon = CouponStore.couponList.FirstOrDefault(x => x.Id == id);
    if (coupon != null)
        return Results.Ok(CouponStore.couponList.Remove(coupon));

    return Results.NotFound();
});

app.UseHttpsRedirection();
app.Run();