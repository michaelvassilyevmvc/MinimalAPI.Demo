using MagicVilla_CouponAPI.Data;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Models.DTO;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
app.MapPost("/api/coupon", ([FromBody] CouponCreateDto coupon_C_DTO) =>
    {
        if (CouponStore.couponList.Any(x => x.Name.ToLower() == coupon_C_DTO.Name.ToLower()))
        {
            return Results.BadRequest("A coupon with the same name already exists");
        }

        Coupon coupon = new()
        {
            Name = coupon_C_DTO.Name,
            Percent = coupon_C_DTO.Percent,
            IsActive = coupon_C_DTO.IsActive,
        };

        int max = CouponStore.couponList.Max(x => x.Id);
        coupon.Id = max + 1;
        CouponStore.couponList.Add(coupon);
        CouponDTO couponDto = new()
        {
            Id = coupon.Id,
            Name = coupon.Name,
            Percent = coupon.Percent,
            IsActive = coupon.IsActive,
            Created = coupon.Created
        };
        
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