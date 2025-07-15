using System.Net;
using AutoMapper;
using FluentValidation;
using MagicVilla_CouponAPI;
using MagicVilla_CouponAPI.Data;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Models.DTO;
using MagicVilla_CouponAPI.Validations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(MappingConfig));
builder.Services.AddValidatorsFromAssembly(typeof(CouponCreateValidation).Assembly);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// GET
app.MapGet("/api/coupon", (ApplicationDbContext _db, ILogger<Program> _logger) =>
    {
        APIResponse response = new();
        _logger.LogInformation("Get all coupons");
        response.Result = _db.Coupons;
        response.IsSuccess = true;
        response.StatusCode = HttpStatusCode.OK;
        return Results.Ok(response);
    })
    .WithName("GetCoupons")
    .Produces<APIResponse>(201);

app.MapGet("/api/coupon/{id:int}", (ApplicationDbContext _db, ILogger<Program> _logger, int id) =>
    {
        APIResponse response = new();
        _logger.LogInformation($"Get coupon {id}");
        response.Result = _db.Coupons.FirstOrDefault(x => x.Id == id);
        response.IsSuccess = true;
        response.StatusCode = HttpStatusCode.OK;
        return Results.Ok(response);
    })
    .WithName("GetCoupon")
    .Produces<APIResponse>(201);

// POST

app.MapPost("/api/coupon",
        async (ApplicationDbContext _db, IMapper _mapper, IValidator<CouponCreateDto> _validation,
            [FromBody] CouponCreateDto coupon_C_DTO) =>
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

            if (_db.Coupons.Any(x => x.Name.ToLower() == coupon_C_DTO.Name.ToLower()))
            {
                response.ErrorMessages.Add("A coupon with the same name already exists");
                return Results.BadRequest(response);
            }

            Coupon coupon = _mapper.Map<Coupon>(coupon_C_DTO);
            
            _db.Coupons.Add(coupon);
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
        async (ApplicationDbContext _db, IMapper _mapper, IValidator<CouponUpdateDto> _validation,
            [FromBody] CouponUpdateDto coupon_U_DTO) =>
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

            if (_db.Coupons.Any(x => x.Name.ToLower() == coupon_U_DTO.Name.ToLower()))
            {
                response.ErrorMessages.Add("A coupon with the same name already exists");
                return Results.BadRequest(response);
            }

            Coupon coupon = _db.Coupons.FirstOrDefault(x => x.Id == coupon_U_DTO.Id);
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
app.MapDelete("/api/coupon/{id:int}", (ApplicationDbContext _db, IMapper _mapper, int id) =>
    {
        APIResponse response = new()
        {
            IsSuccess = false,
            StatusCode = HttpStatusCode.BadRequest,
        };
        var coupon = _db.Coupons.FirstOrDefault(x => x.Id == id);

        if (coupon == null)
        {
            response.ErrorMessages.Add($"Invalid id = {id}");
            return Results.BadRequest(response);
        }

        _db.Coupons.Remove(coupon);

        response.IsSuccess = true;
        response.StatusCode = HttpStatusCode.OK;
        return Results.Ok(response);
        // return Results.Ok(_db.Coupons.Remove(coupon));

        // return Results.NotFound();
    })
    .WithName("DeletedCoupon")
    .Produces<APIResponse>(201)
    .Produces(400);

app.UseHttpsRedirection();
app.Run();