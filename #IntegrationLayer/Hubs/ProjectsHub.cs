using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _IntegrationLayer.Hubs
{
    public class ProjectsHub : Hub
    {
        // Ingen metod behövs just nu – vi använder bara Clients.User(...).SendAsync("ReloadProjects")
    }
}
