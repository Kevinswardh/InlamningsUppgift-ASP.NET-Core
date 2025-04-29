namespace _1.PresentationLayer.ViewModels.ProjectsViewModels
{
    public class ProjectsViewModel
    {
        public IEnumerable<ProjectViewModel> Projects { get; set; } = new List<ProjectViewModel>();

        public int AllCount => Projects.Count();
        public int StartedCount => Projects.Count(p => !p.IsCompleted);
        public int CompletedCount => Projects.Count(p => p.IsCompleted);

    }

}
