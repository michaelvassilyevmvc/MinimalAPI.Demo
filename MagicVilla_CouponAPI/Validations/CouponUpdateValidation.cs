using FluentValidation;
using MagicVilla_CouponAPI.Models.DTO;

namespace MagicVilla_CouponAPI.Validations;

public class CouponUpdateValidation: AbstractValidator<CouponUpdateDto>
{
    public CouponUpdateValidation()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required");
        RuleFor(x => x.Percent)
            .InclusiveBetween(1, 100);
        
    }
}