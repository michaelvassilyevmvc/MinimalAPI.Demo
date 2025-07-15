using MagicVilla_CouponAPI.Data;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace MagicVilla_CouponAPI.Repository;

public class CouponRepository: ICouponRepository
{
    private readonly ApplicationDbContext _db;

    public CouponRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ICollection<Coupon>> GetAllAsync() => await _db.Coupons.AsNoTracking().ToListAsync();

    public async Task<Coupon> GetAsync(int id)
    {
        return await _db.Coupons.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Coupon> GetAsync(string couponName)
    {
        return await _db.Coupons.AsNoTracking().FirstOrDefaultAsync(x => x.Name.ToLower() == couponName.ToLower());
    }

    public async Task CreateAsync(Coupon coupon)
    {
        await _db.AddAsync(coupon);
    }

    public async Task  UpdateAsync(Coupon coupon)
    {
         _db.Coupons.Update(coupon);
         await _db.SaveChangesAsync();
    }

    public  void RemoveAsync(Coupon coupon)
    {
         _db.Coupons.Remove(coupon);
    }

    public async Task SaveAsync()
    {
        await _db.SaveChangesAsync();
    }
}