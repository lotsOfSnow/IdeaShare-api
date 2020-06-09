using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace IdeaShare.Extensions.System.Security.Claims
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetId(this ClaimsPrincipal principal)
        {
            return principal.Claims.First(i => i.Type == "id").Value;
        }

        public static string GetUsername(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.Name).Value;
        }

        public static string GetToken(this ClaimsPrincipal principal)
        {
            return principal.Claims.First(i => i.Type == JwtRegisteredClaimNames.Jti).Value;
        }
    }
}
