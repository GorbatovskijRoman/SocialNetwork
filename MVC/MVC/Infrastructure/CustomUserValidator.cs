using Microsoft.AspNet.Identity;
using System.Linq;
using System.Threading.Tasks;
using MVC.Models;

namespace MVC.Infrastructure
{
    public class CustomUserValidator : UserValidator<AppUser>
    {
        public CustomUserValidator(AppUserManager manager)
           : base(manager)
        { }

        public override async Task<IdentityResult> ValidateAsync(AppUser user)
        {
            IdentityResult result = await base.ValidateAsync(user);
            
          //доп. проверка пользователя

            return result;
        }
    }
}