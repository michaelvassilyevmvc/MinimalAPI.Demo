using MagicVilla_CouponAPI.Models.DTO;

namespace MagicVilla_CouponAPI.Repository.IRepository;

public interface IAuthRepository
{
    bool IsUniqueUser(string userName);
    Task<LoginResponseDTO> Authenticate(LoginRequestDTO loginRequestDto);
    Task<UserDTO> Register(RegistrationRequestDTO requestDto);
}