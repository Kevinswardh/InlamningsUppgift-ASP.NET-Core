﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Collections.Generic;
using ApplicationLayer_ServiceLayer_.UserManagment.UserService.Interface;
using __Cross_cutting_Concerns.ServiceInterfaces;
using __Cross_cutting_Concerns.FormDTOs;
using CrossCuttingConcerns.FormDTOs;
using DomainLayer_BusinessLogicLayer_.DomainModel;
using DomainLayer_BusinessLogicLayer_.InfraInterfaces;
using Microsoft.AspNetCore.Identity;

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
                    IsOnline = u.IsOnline,
                    ImageUrl = u.ImageUrl
                }).ToList(),
                CurrentPage = page,
                TotalPages = totalPages,
                SearchQuery = search,
                Filter = role
            };
        }

        public async Task<UserEntity> GetUserByIdAsync(string id)
        {
            return await _userRepo.GetByIdAsync(id);
        }

        public async Task UpdateUserAsync(UserEntity updatedUser)
        {
            var existingUser = await _userRepo.GetByIdAsync(updatedUser.Id);
            if (existingUser == null) return;

            if (existingUser.Role != updatedUser.Role)
            {
                await _userRepo.UpdateUserRoleAsync(updatedUser.Id, updatedUser.Role);
            }

            await _userRepo.UpdateUserAsync(updatedUser);
        }

        public async Task DeleteUserAsync(string id)
        {
            await _userRepo.DeleteUserAsync(id);
        }

        public async Task<bool> IsInRoleAsync(UserEntity user, string role)
        {
            return await _userRepo.IsInRoleAsync(user, role);
        }

        public async Task<(string? ImageUrl, string? UserName)> GetUserProfileForLayoutAsync(ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return (null, null);

            var entity = await _userRepo.GetByIdAsync(userId);
            if (entity == null) return (null, null);

            return (entity.ImageUrl, entity.UserName);
        }

        public async Task<bool> UseExternalProfilePictureAsync(string userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.ExternalImageUrl))
                return false;

            user.ImageUrl = user.ExternalImageUrl;
            await _userRepo.UpdateUserAsync(user);

            return true;
        }

        public async Task<List<string>> GetUserRolesAsync(ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return new List<string>();

            var userEntity = await _userRepo.GetByIdAsync(userId);
            if (userEntity == null)
                return new List<string>();

            return new List<string> { userEntity.Role };
        }

        public string GetUserId(ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        public async Task<List<MemberItemDTO>> GetAllUsersAsync()
        {
            var users = await _userRepo.GetAllUsersAsync();
            return users.Select(u => new MemberItemDTO
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Position = u.Position,
                Role = u.Role,
                IsOnline = u.IsOnline,
                ImageUrl = u.ImageUrl
            }).ToList();
        }

        public async Task<UserEntity?> GetUserByEmailAsync(string email)
        {
            return await _userRepo.GetUserByEmailAsync(email);
        }
  

    }
}
