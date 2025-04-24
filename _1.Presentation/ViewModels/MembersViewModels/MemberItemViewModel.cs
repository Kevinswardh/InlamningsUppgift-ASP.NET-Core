namespace _1.PresentationLayer.ViewModels.MembersViewModels
{
    public class MemberItemViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Position { get; set; }
        public string Role { get; set; }
        public bool IsOnline { get; set; }
        public IFormFile? ProfileImage { get; set; } // för POST (uppladdning)
        public string? ImageUrl { get; set; } // för att visa bilden

    }
}
