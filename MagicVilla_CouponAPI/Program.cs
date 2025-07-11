using MagicVilla_CouponAPI.Data;
using MagicVilla_CouponAPI.Models;
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

app.MapGet("/api/coupon", () => Results.Ok(CouponStore.couponList));
app.MapGet("/api/coupon/{id:int}", (int id) => Results.Ok(CouponStore.couponList.FirstOrDefault(x => x.Id == id)));
app.MapPost("/api/coupon", ([FromBody] Coupon coupon) =>
{
    if (coupon.Id != 0 || string.IsNullOrEmpty(coupon.Name))
    {
        return Results.BadRequest("Invalid Id or Name");
    }

    if (CouponStore.couponList.Any(x => x.Name.ToLower() == coupon.Name.ToLower()))
    {
        return Results.BadRequest("A coupon with the same name already exists");
    }
    int max = CouponStore.couponList.Max(x => x.Id);
    coupon.Id = max + 1;
    CouponStore.couponList.Add(coupon);
    return Results.Created($"/api/coupon/{coupon.Id}",coupon);
});
app.MapPut("/api/coupon", () => { });
app.MapDelete("/api/coupon/{id:int}", (int id) =>
{
    var coupon = CouponStore.couponList.FirstOrDefault(x => x.Id == id);
    if (coupon != null)
        return Results.Ok(CouponStore.couponList.Remove(coupon));
    
    return Results.NotFound();
});

app.UseHttpsRedirection();
app.Run();
