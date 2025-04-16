using CrossCuttingConcerns.FormDTOs;
using SecurityLayer.Identity;

namespace SecurityLayer.IdentityFactory
{
    public static class ApplicationUserFactory
    {
        public static ApplicationUser CreateFromRegisterForm(RegisterForm form)
        {
            return new ApplicationUser
            {
                Email = form.Email,
                UserName = form.UserName,
                PhoneNumber = form.PhoneNumber,
                Position = "New Member"
            };
        }
    }
}
