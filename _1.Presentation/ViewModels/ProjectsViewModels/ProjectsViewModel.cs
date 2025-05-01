using _1.PresentationLayer.ViewModels.MembersViewModels;

namespace _1.PresentationLayer.ViewModels.ProjectsViewModels
{
    public class ProjectsViewModel
    {
        public IEnumerable<ProjectViewModel> Projects { get; set; } = new List<ProjectViewModel>();

        // Flikar
        public string SelectedTab { get; set; } = "All";
        public int AllCount { get; set; }
        public int StartedCount { get; set; }
        public int CompletedCount { get; set; }

        // Medlemmar
        public List<MemberItemViewModel> AllMembers { get; set; } = new();

        // Pagineringsstöd
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 8;
    }
}
