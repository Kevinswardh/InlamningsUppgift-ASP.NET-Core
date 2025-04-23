using _1.PresentationLayer.ViewModels.MembersViewModels;

public class UserListViewModel
{
    public List<MemberItemViewModel> Members { get; set; }

    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int PageCount { get; set; }
    public string Filter { get; set; }
    public string SearchQuery { get; set; }
    public string? SelectedSort { get; set; }

    // ✅ Komponerad tabmodell
    public TabListViewModel TabData { get; set; }
}
