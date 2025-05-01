namespace _1.PresentationLayer.ViewModels.ProjectsViewModels
{
    public class EditProjectViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string ClientEmail { get; set; }

        public string Description { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public decimal Budget { get; set; }

        public string? ImageUrl { get; set; }

        public IFormFile? ImageFile { get; set; }

        public List<string> MemberIds { get; set; } = new();
    }
}
