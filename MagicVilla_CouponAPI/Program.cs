using System.Net;
using AutoMapper;
using FluentValidation;
using MagicVilla_CouponAPI;
using MagicVilla_CouponAPI.Data;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Models.DTO;
using MagicVilla_CouponAPI.Repository;
using MagicVilla_CouponAPI.Repository.IRepository;
using MagicVilla_CouponAPI.Validations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ICouponRepository, CouponRepository>();
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
app.MapGet("/api/coupon", async (ICouponRepository _couponRepo, ILogger<Program> _logger) =>
    {
        APIResponse response = new();
        _logger.LogInformation("Get all coupons");
        response.Result = await _couponRepo.GetAllAsync();
        response.IsSuccess = true;
        response.StatusCode = HttpStatusCode.OK;
        return Results.Ok(response);
    })
    .WithName("GetCoupons")
    .Produces<APIResponse>(201);

app.MapGet("/api/coupon/{id:int}", async (ICouponRepository _couponRepo, ILogger<Program> _logger, int id) =>
    {
        APIResponse response = new();
        _logger.LogInformation($"Get coupon {id}");
        response.Result = await _couponRepo.GetAsync(id);
        response.IsSuccess = true;
        response.StatusCode = HttpStatusCode.OK;
        return Results.Ok(response);
    })
    .WithName("GetCoupon")
    .Produces<APIResponse>(201);

// POST

app.MapPost("/api/coupon",
        async (ICouponRepository _couponRepo, IMapper _mapper, IValidator<CouponCreateDto> _validation,
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

            if (await _couponRepo.GetAsync(coupon_C_DTO.Name) != null)
            {
                response.ErrorMessages.Add("A coupon with the same name already exists");
                return Results.BadRequest(response);
            }

            Coupon coupon = _mapper.Map<Coupon>(coupon_C_DTO);

            await _couponRepo.CreateAsync(coupon);
            await _couponRepo.SaveAsync();
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
        async (ICouponRepository _couponRepo, IMapper _mapper, IValidator<CouponUpdateDto> _validation,
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


            Coupon coupon = await _couponRepo.GetAsync(coupon_U_DTO.Id);
            if (coupon == null)
            {
                response.ErrorMessages.Add($"A coupon with {coupon_U_DTO.Id} not exists");
                return Results.NotFound(response);
            }

            Coupon couponByName = await _couponRepo.GetAsync(coupon_U_DTO.Name);

            if (couponByName != null && couponByName.Id != coupon_U_DTO.Id)
            {
                response.ErrorMessages.Add("A coupon with the same name already exists");
                return Results.BadRequest(response);
            }

            await _couponRepo.UpdateAsync(_mapper.Map<Coupon>(coupon_U_DTO));
            // coupon.Name = coupon_U_DTO.Name;
            // coupon.Percent = coupon_U_DTO.Percent;
            // coupon.IsActive = coupon_U_DTO.IsActive;
            // coupon.Created = coupon_U_DTO.Created;
            // coupon.LastUpdated = coupon_U_DTO.LastUpdated;
            // _db.Coupons.Update(_mapper.Map<Coupon>(coupon_U_DTO));
            await _couponRepo.SaveAsync();

            response.Result = _mapper.Map<CouponDTO>(await _couponRepo.GetAsync(coupon_U_DTO.Id));
            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            return Results.Ok(response);
        })
    .WithName("UpdatedCoupon")
    .Accepts<CouponUpdateDto>("application/json")
    .Produces<APIResponse>(201)
    .Produces(400);

// DELETE
app.MapDelete("/api/coupon/{id:int}", async (ICouponRepository _couponRepo, IMapper _mapper, int id) =>
    {
        APIResponse response = new()
        {
            IsSuccess = false,
            StatusCode = HttpStatusCode.BadRequest,
        };
        var coupon = await _couponRepo.GetAsync(id);

        if (coupon == null)
        {
            response.ErrorMessages.Add($"Invalid id = {id}");
            return Results.BadRequest(response);
        }

        _couponRepo.RemoveAsync(coupon);
        await _couponRepo.SaveAsync();

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