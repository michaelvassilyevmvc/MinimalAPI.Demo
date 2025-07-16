using System.Collections.ObjectModel;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using MagicVilla_CouponAPI.Data;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Models.DTO;
using MagicVilla_CouponAPI.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace MagicVilla_CouponAPI.Repository;

public class AuthRepository : IAuthRepository
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private string secretKey;

    public AuthRepository(ApplicationDbContext db, IMapper mapper, IConfiguration configuration,
        UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _db = db;
        _mapper = mapper;
        _configuration = configuration;
        _userManager = userManager;
        _roleManager = roleManager;
        secretKey = _configuration.GetValue<string>("ApiSettings:SecretKey");
    }

    public bool IsUniqueUser(string userName)
    {
        var user = _db.ApplicationUsers.FirstOrDefault(x => x.UserName == userName);
        if (user == null)
        {
            return true;
        }

        return false;
    }

    public async Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDto)
    {
        var user = _db.ApplicationUsers.SingleOrDefault(x =>
            x.UserName == loginRequestDto.UserName);
        // проверка по hash пароля на совпадения
        bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDto.Password);
        if (user == null || isValid == false)
        {
            return null;
        }

        // получение роли пользователя
        var roles = await _userManager.GetRolesAsync(user);

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(secretKey);
        var tokenDescription = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, roles.FirstOrDefault()),
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
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
        ApplicationUser userObj = new()
        {
            UserName = requestDto.UserName,
            Name = requestDto.Name,
            NormalizedEmail = requestDto.UserName.ToUpper(),
            Email = requestDto.UserName,
        };
        try
        {
            var result = await _userManager.CreateAsync(userObj, requestDto.Password);
            if (result.Succeeded)
            {
                if (!_roleManager.RoleExistsAsync("admin")
                        .GetAwaiter()
                        .GetResult())
                {
                    await _roleManager.CreateAsync(new IdentityRole("admin"));
                    await _roleManager.CreateAsync(new IdentityRole("customer"));
                }

                await _userManager.AddToRoleAsync(userObj, "admin");
                var user = _db.ApplicationUsers.FirstOrDefault(x => x.UserName == userObj.UserName);
                return _mapper.Map<UserDTO>(user);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
        return null;
    }
}