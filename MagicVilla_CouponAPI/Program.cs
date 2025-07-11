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
        _logger.LogInformation("Get all coupons");
        return Results.Ok(CouponStore.couponList);
    })
    .Produces<IEnumerable<Coupon>>(201);
app.MapGet("/api/coupon/{id:int}", (int id) => Results.Ok(CouponStore.couponList.FirstOrDefault(x => x.Id == id)))
    .WithName("GetCoupon")
    .Produces<Coupon>(201);

// POST

app.MapPost("/api/coupon",
        (IMapper _mapper, IValidator<CouponCreateDto> _validation, [FromBody] CouponCreateDto coupon_C_DTO) =>
        {
            var validationResult = _validation.ValidateAsync(coupon_C_DTO)
                .GetAwaiter()
                .GetResult();
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(validationResult.Errors.FirstOrDefault().ToString());
            }
            
            if (CouponStore.couponList.Any(x => x.Name.ToLower() == coupon_C_DTO.Name.ToLower()))
            {
                return Results.BadRequest("A coupon with the same name already exists");
            }

            Coupon coupon = _mapper.Map<Coupon>(coupon_C_DTO);

            int max = CouponStore.couponList.Max(x => x.Id);
            coupon.Id = max + 1;
            CouponStore.couponList.Add(coupon);
            CouponDTO couponDto = _mapper.Map<CouponDTO>(coupon);

            return Results.CreatedAtRoute("GetCoupon", new { id = coupon.Id }, couponDto);
            // return Results.Created($"/api/coupon/{coupon.Id}", coupon);
        })
    .WithName("CreatedCoupon")
    .Accepts<CouponCreateDto>("application/json")
    .Produces<CouponDTO>(201)
    .Produces(400);

// PUT
app.MapPut("/api/coupon", () => { });

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