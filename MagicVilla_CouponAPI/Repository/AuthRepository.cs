using System.Collections.ObjectModel;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using MagicVilla_CouponAPI.Data;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Models.DTO;
using MagicVilla_CouponAPI.Repository.IRepository;
using Microsoft.IdentityModel.Tokens;

namespace MagicVilla_CouponAPI.Repository;

public class AuthRepository : IAuthRepository
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;
    private string secretKey;

    public AuthRepository(ApplicationDbContext db, IMapper mapper, IConfiguration configuration)
    {
        _db = db;
        _mapper = mapper;
        _configuration = configuration;
        secretKey = _configuration.GetValue<string>("ApiSettings:SecretKey");
    }

    public bool IsUniqueUser(string userName)
    {
        var user = _db.LocalUsers.FirstOrDefault(x => x.UserName == userName);
        if (user == null)
        {
            return true;
        }

        return false;
    }

    public async Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDto)
    {
        var user = _db.LocalUsers.SingleOrDefault(x => x.UserName == loginRequestDto.UserName && x.Password == loginRequestDto.Password);
        if (user == null)
        {
            return null;
        }
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(secretKey);
        var tokenDescription = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role),
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescription);
        LoginResponseDTO loginResponseDTO = new()
        {
            User = _mapper.Map<UserDTO>(user),
            Token = new JwtSecurityTokenHandler().WriteToken(token),
        };

        return loginResponseDTO;
    }

    public async Task<UserDTO> Register(RegistrationRequestDTO requestDto)
    {
        LocalUser userObj = new()
        {
            UserName = requestDto.UserName,
            Password = requestDto.Password,
            Name = requestDto.Name,
            Role = "customer"
        };
        await _db.LocalUsers.AddAsync(userObj);
        await _db.SaveChangesAsync();
        userObj.Password = "";
        return _mapper.Map<UserDTO>(userObj);
    }
}