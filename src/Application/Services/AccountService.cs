using AutoMapper;
using IdeaShare.Application.Models.AppUserModels;
using IdeaShare.Application.Options;
using IdeaShare.Application.Utilities;
using IdeaShare.Domain;
using IdeaShare.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace IdeaShare.Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly JwtSettings _jwtSettings;
        private readonly ImageSettings _imageSettings;

        public AccountService(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<AppUser> signInManager, JwtSettings jwtSettings, ImageSettings imageSettings)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _jwtSettings = jwtSettings;
            _imageSettings = imageSettings;
        }

        public async Task<RequestWithPayloadResult<AppUser>> RegisterAsync(string username, string email, string password)
        {
            var emailAlreadyExists = (await _userManager.FindByEmailAsync(email)) != null;

            if(emailAlreadyExists)
            {
                return new RequestWithPayloadResult<AppUser>
                {
                    Errors = new Dictionary<string, string>() {{ "Email", "User with this email address already exists" } }
                };
            }

            var usernameAlreadyExists = (await _userManager.FindByNameAsync(username)) != null;

            if (usernameAlreadyExists)
            {
                return new RequestWithPayloadResult<AppUser>
                {
                    Errors = new Dictionary<string, string>() { { "Username", "This username is taken" } }
                };
            }

            var newUser = new AppUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = username,
                Email = email,
                RegistrationDate = DateTime.UtcNow,
            };

            var registrationResult = await _userManager.CreateAsync(newUser, password);

            if(!registrationResult.Succeeded)
            {
                return new RequestWithPayloadResult<AppUser>
                {
                    Errors = registrationResult.Errors.ToDictionary(x => x.Code, y => y.Description, StringComparer.OrdinalIgnoreCase)
                };
            }

            return new RequestWithPayloadResult<AppUser>
            {
                Success = true,
                Payload = newUser,
            };
        }

        public async Task<AppUserLoginResult> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return new AppUserLoginResult
                {
                    Errors = new Dictionary<string, string> { { "password", "Wrong email/password combination" } }
                };
            }

            var userHasValidPassword = await _userManager.CheckPasswordAsync(user, password);

            if (!userHasValidPassword)
            {
                return new AppUserLoginResult
                {
                    Errors = new Dictionary<string, string> { { "password", "Wrong email/password combination" } }
                };
            }

            var token = await GenerateTokenForUserAsync(user);
            
            return new AppUserLoginResult
            {
                Success = true,
                Token = token,
            };
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<AppUser> GetCurrentUserAsync(string id)
        {
            return await _userManager.Users.Include(x => x.Articles).ThenInclude(x => x.Likes).FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<BaseRequestResult> UpdateAsync(AppUser user, AppUserUpdateModel updateModel)
        {
            if (updateModel.NewPassword != null)
            {
                var passwordChangeResult = await _userManager.ChangePasswordAsync(user, updateModel.CurrentPassword, updateModel.NewPassword);

                if (!passwordChangeResult.Succeeded)
                {
                    return new BaseRequestResult
                    {
                        Errors = passwordChangeResult.Errors.ToDictionary(x => "CurrentPassword", y => y.Description)
                    };
                }
            }

            if(updateModel.ProfilePicture != null)
            {
                var uploadedFilePath = await ImageUtilities.ProcessUploadedFile(updateModel.ProfilePicture, _imageSettings.ProfilePicture.Location,
                    _imageSettings.ProfilePicture.Format, _imageSettings.ProfilePicture.Width, _imageSettings.ProfilePicture.Height);
                user.ProfilePicture = uploadedFilePath;
            }

            user.Email = updateModel.Email;
            user.FirstName = updateModel.FirstName;
            user.LastName = updateModel.LastName;
            user.IsRealNameHidden = updateModel.IsRealNameHidden;
            user.Description = updateModel.Description;

            var identityResult = await _userManager.UpdateAsync(user);

            return identityResult.Succeeded ? new BaseRequestResult
            {
                Success = true
            } : new BaseRequestResult
            {
                Success = false,
                Errors = identityResult.Errors.ToDictionary(x => x.Code, y => y.Description)
            };
        }

        public async Task<IdentityResult> ChangePaswordAsync(string id, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(id);
            return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        }

        private async Task<string> GenerateTokenForUserAsync(AppUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim("id", user.Id)
            };

            var userClaims = await _userManager.GetClaimsAsync(user);
            claims.AddRange(userClaims);

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var userRole in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole));
                var role = await _roleManager.FindByNameAsync(userRole);
                if (role == null) continue;
                var roleClaims = await _roleManager.GetClaimsAsync(role);

                foreach (var roleClaim in roleClaims)
                {
                    if (claims.Contains(roleClaim))
                        continue;

                    claims.Add(roleClaim);
                }
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(_jwtSettings.TokenLifetime),
                SigningCredentials =
                    new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
