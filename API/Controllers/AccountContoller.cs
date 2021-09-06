using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountContoller : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;

        public AccountContoller(DataContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
          if(await UserExist(registerDto.Username))
           return BadRequest("Username is taken");

          using var mac = new HMACSHA512(); 

          var user = new AppUser{
              UserName = registerDto.Username.ToLower(),
              PaasswordHash = mac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
              PaasswordSalt = mac.Key
          };

          _context.Users.Add(user);
          await _context.SaveChangesAsync();

          return new UserDto
          {
              Username = user.UserName,
              Token = _tokenService.CreateToken(user)
          };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> LoginDto(LoginDto logindto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(x => x.UserName == logindto.Username);

            if(user == null)
              return Unauthorized("Not authorized");

            using var mac = new HMACSHA512(user.PaasswordSalt);

            var computedHash = mac.ComputeHash(Encoding.UTF8.GetBytes(logindto.Password));

            for(int i =0; i<computedHash.Length; i++)
            {
               if(computedHash[i] != user.PaasswordHash[i])
                return Unauthorized("Invaild password");
            }

          return new UserDto
          {
              Username = user.UserName,
              Token = _tokenService.CreateToken(user)
          };
        }

        private async Task<bool> UserExist(string username)
        {
          return await _context.Users.AnyAsync(user => user.UserName.ToLower() == username);
        }
    }
}