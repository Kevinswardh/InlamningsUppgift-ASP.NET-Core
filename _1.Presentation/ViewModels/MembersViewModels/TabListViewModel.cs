﻿namespace _1.PresentationLayer.ViewModels.MembersViewModels
{
    public class TabListViewModel
    {
        public string SelectedTab { get; set; } = "All";
        public int AllCount { get; set; }
        public int OnlineCount { get; set; }
        public int OfflineCount { get; set; }
    }

}
