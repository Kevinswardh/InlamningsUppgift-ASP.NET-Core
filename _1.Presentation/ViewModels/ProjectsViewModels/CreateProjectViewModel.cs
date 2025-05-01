using _1.PresentationLayer.ViewModels.MembersViewModels;
using System.ComponentModel.DataAnnotations;

namespace _1.PresentationLayer.ViewModels.ProjectsViewModels
{
    public class CreateProjectViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string ClientEmail { get; set; }

        public string Description { get; set; }


        public IFormFile? ImageFile { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Budget must be greater than 0")]
        public decimal Budget { get; set; }

        public List<string> MemberIds { get; set; } = new();

    }
}
