using _1.PresentationLayer.ViewModels.MembersViewModels;

namespace _1.PresentationLayer.ViewModels.ProjectsViewModels
{
    public class ProjectViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ClientEmail { get; set; }
        public string Description { get; set; }
        public string? ImageUrl { get; set; } = "~/Pictures/Icons/projectlogo.svg";
        public string TimeLeft { get; set; }
        public List<MemberItemViewModel> Members { get; set; } = new();
        public bool IsCompleted { get; set; } // ✅ ENKEL BOOLEAN STATUS
    }


}
