using IdeaShare.Application.Options;
using IdeaShare.Domain;
using IdeaShare.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Application.UnitTests.Services
{
    public class AccountServiceTests
    {
        private readonly AccountService _accountService;
        private readonly Mock<IUserStore<AppUser>> _userStoreMock;
        private readonly Mock<UserManager<AppUser>> _mockUserManager;
        private readonly Mock<SignInManager<AppUser>> _mockSignInManager;
        private readonly JwtSettings _mockJwtSettings;
        private readonly ImageSettings _mockImageSettings = new ImageSettings();
        private readonly List<AppUser> _users = new List<AppUser>();

        public AccountServiceTests()
        {
            _mockJwtSettings = GetMockJwtSettings();
            _userStoreMock = GetMockUserStore();
            _mockUserManager = GetMockUserManager();
            _mockSignInManager = GetMockSignInManager();

            _accountService = new AccountService(_mockUserManager.Object, GetMockRoleManager().Object,
                _mockSignInManager.Object, _mockJwtSettings, _mockImageSettings);

            _mockUserManager.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync((string username) =>
            {
                if (username == "valid")
                {
                    return new AppUser
                    {
                        Id = "1",
                        Email = "valid@valid.com",
                        UserName = "valid",
                    };
                }

                return null;
            });

            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success).Callback<AppUser, string>((x, y) => _users.Add(x));

            _mockUserManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((string email) =>
            {
                if (email == "valid@valid.com")
                {
                    return new AppUser
                    {
                        Id = "1",
                        Email = "valid@valid.com",
                        UserName = "valid",
                    };
                }

                return null;
            });

            _mockUserManager.Setup(x => x.CheckPasswordAsync(It.IsAny<AppUser>(), It.IsAny<string>())).ReturnsAsync((AppUser user, string password) =>
            {
                return user.Email == "valid@valid.com" && password == "valid";
            });

            _mockUserManager.Setup(x => x.GetClaimsAsync(It.IsAny<AppUser>())).ReturnsAsync(new List<Claim>());
            _mockUserManager.Setup(x => x.GetRolesAsync(It.IsAny<AppUser>())).ReturnsAsync(new List<string>());
        }

        [Fact]
        public async Task RegisterAsync_ShouldAddNewUser_IfCredentialsAreNotTaken()
        {
            await _accountService.RegisterAsync("newUser", "new@user.com", "secret");

            Assert.Single(_users);
        }

        [Fact]
        public async Task RegisterAsync_ShouldNotAddNewUser_IfCredentialsAreTaken()
        {
            var result = await _accountService.RegisterAsync("valid", "valid@valid.com", "secret");

            Assert.Empty(_users);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnTrue_WithCorrectCredentials()
        {
            string email = "valid@valid.com",
                password = "valid";

            var result = await _accountService.LoginAsync(email, password);

            Assert.True(result.Success);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnFalse_WithIncorrectCredentials()
        {
            string email = "not@existing.user",
                password = "secret";

            var result = await _accountService.LoginAsync(email, password);

            Assert.False(result.Success);
        }


        public static Mock<RoleManager<IdentityRole>> GetMockRoleManager()
        {
            var roleStore = new Mock<IRoleStore<IdentityRole>>();
            return new Mock<RoleManager<IdentityRole>>(roleStore.Object, null, null, null, null);
        }

        public static JwtSettings GetMockJwtSettings()
        {
            return new JwtSettings
            {
                SecretKey = "asdasdasdasdasasdasd",
                TokenLifetime = TimeSpan.FromHours(1)
            };
        }

        public static Mock<IUserStore<AppUser>> GetMockUserStore()
        {
            return new Mock<IUserStore<AppUser>>();
        }

        public static Mock<UserManager<AppUser>> GetMockUserManager()
        {
            return new Mock<UserManager<AppUser>>(GetMockUserStore().Object,
                null, null, null, null, null, null, null, null);
        }

        public static Mock<SignInManager<AppUser>> GetMockSignInManager()
        {
            var _contextAccessor = new Mock<IHttpContextAccessor>();
            var _userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<AppUser>>();

            return new Mock<SignInManager<AppUser>>(GetMockUserManager().Object,
                           _contextAccessor.Object, _userPrincipalFactory.Object, null, null, null);
        }
    }
}
