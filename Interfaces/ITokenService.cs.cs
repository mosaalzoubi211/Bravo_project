using Bravo.Models;

namespace Bravo.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(AppUser user);
    }
}