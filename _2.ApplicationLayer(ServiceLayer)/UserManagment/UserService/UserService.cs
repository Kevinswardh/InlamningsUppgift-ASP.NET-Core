using DomainLayer_BusinessLogicLayer_.Entities;
using DomainLayer_BusinessLogicLayer_.InfraInterfaces;
using __Cross_cutting_Concerns.ServiceInterfaces;
using CrossCuttingConcerns.FormDTOs;
using System;
using System.Linq;
using System.Threading.Tasks;
using ApplicationLayer_ServiceLayer_.UserManagment.UserService.Interface;
using __Cross_cutting_Concerns.FormDTOs;

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

            foreach (var u in users)
                u.IsOnline = _statusService.IsOnline(u.Email);

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
                    Id = u.Id,
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

        public async Task<User> GetUserByIdAsync(string id)
        {
            return await _userRepo.GetByIdAsync(id);
        }

        public async Task UpdateUserAsync(User updatedUser)
        {
            var existingUser = await _userRepo.GetByIdAsync(updatedUser.Id);
            if (existingUser == null) return;

            // Om rollen har ändrats, byt roll
            if (existingUser.Role != updatedUser.Role)
            {
                await _userRepo.UpdateUserRoleAsync(updatedUser.Id, updatedUser.Role);
            }

            // Uppdatera övriga fält
            await _userRepo.UpdateUserAsync(updatedUser);
        }

        public async Task DeleteUserAsync(string id)
        {
            await _userRepo.DeleteUserAsync(id);
        }

        public async Task<bool> IsInRoleAsync(User user, string role)
        {
            return await _userRepo.IsInRoleAsync(user, role);
        }
    }
}
