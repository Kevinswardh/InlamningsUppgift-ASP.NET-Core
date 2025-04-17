using ApplicationLayer_ServiceLayer_.UserManagment.UserService.Interface;
using CrossCuttingConcerns.FormDTOs;
using DomainLayer_BusinessLogicLayer_.Entities;
using DomainLayer_BusinessLogicLayer_.InfraInterfaces;
using __Cross_cutting_Concerns.ServiceInterfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using __Cross_cutting_Concerns.FormDTOs;
using __Cross_cutting_Concerns.ServiceInterfaces;

namespace ApplicationLayer_ServiceLayer_.UserManagment.UserService
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly IUserStatusService _statusService;

        public UserService(IUserRepository userRepo, IUserStatusService statusService)
        {
            _userRepo = userRepo;
            _statusService = statusService;
        }

        public async Task<NewMembersDTO> GetUsersFilteredAsync(string role, string? search, string tab, string sortBy, int page, int pageSize)
        {
            var users = await _userRepo.GetUsersByRoleAsync(role);

            // Uppdatera IsOnline här med hjälp av statusService
            foreach (var u in users)
            {
                u.IsOnline = _statusService.IsOnline(u.Email);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lower = search.ToLower();
                users = users.Where(u =>
                    u.Email.ToLower().Contains(lower) ||
                    u.UserName.ToLower().Contains(lower) ||
                    u.Position.ToLower().Contains(lower)).ToList();
            }

            if (tab == "Online")
                users = users.Where(u => u.IsOnline).ToList();
            else if (tab == "Offline")
                users = users.Where(u => !u.IsOnline).ToList();

            users = sortBy switch
            {
                "Email" => users.OrderBy(u => u.Email).ToList(),
                "Position" => users.OrderBy(u => u.Position).ToList(),
                _ => users.OrderBy(u => u.UserName).ToList()
            };

            var totalPages = (int)Math.Ceiling(users.Count / (double)pageSize);
            var paged = users.Skip((page - 1) * pageSize).Take(pageSize);

            return new NewMembersDTO
            {
                Members = paged.Select(u => new MemberItemDTO
                {
                    Email = u.Email,
                    UserName = u.UserName,
                    PhoneNumber = u.PhoneNumber,
                    Position = u.Position,
                    Role = u.Role,
                    IsOnline = u.IsOnline
                }).ToList(),
                CurrentPage = page,
                TotalPages = totalPages,
                SearchQuery = search,
                Filter = role
            };
        }
    }
}
