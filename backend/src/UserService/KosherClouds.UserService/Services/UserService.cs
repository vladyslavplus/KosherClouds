using KosherClouds.ServiceDefaults.Helpers;
using KosherClouds.UserService.DTOs.User;
using KosherClouds.UserService.Entities;
using KosherClouds.UserService.Parameters;
using KosherClouds.UserService.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace KosherClouds.UserService.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISortHelperFactory _sortFactory;
        private readonly bool _isInMemory;

        public UserService(UserManager<ApplicationUser> userManager, ISortHelperFactory sortFactory, bool isInMemory = false)
        {
            _userManager = userManager;
            _sortFactory = sortFactory;
            _isInMemory = isInMemory;
        }

        public async Task<PagedList<ApplicationUser>> GetUsersAsync(UserParameters parameters, CancellationToken cancellationToken = default)
        {
            IQueryable<ApplicationUser> query = _userManager.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(parameters.UserName))
            {
                query = _isInMemory
                    ? query.Where(u => u.UserName != null && u.UserName.Contains(parameters.UserName, StringComparison.OrdinalIgnoreCase))
                    : query.Where(u => EF.Functions.ILike(u.UserName!, $"%{parameters.UserName}%"));
            }

            if (!string.IsNullOrWhiteSpace(parameters.Email))
            {
                query = _isInMemory
                    ? query.Where(u => u.Email != null && u.Email.Contains(parameters.Email, StringComparison.OrdinalIgnoreCase))
                    : query.Where(u => EF.Functions.ILike(u.Email!, $"%{parameters.Email}%"));
            }

            if (!string.IsNullOrWhiteSpace(parameters.PhoneNumber))
            {
                query = _isInMemory
                    ? query.Where(u => u.PhoneNumber != null && u.PhoneNumber.Contains(parameters.PhoneNumber, StringComparison.OrdinalIgnoreCase))
                    : query.Where(u => u.PhoneNumber != null && EF.Functions.ILike(u.PhoneNumber, $"%{parameters.PhoneNumber}%"));
            }

            if (parameters.CreatedAtFrom.HasValue)
                query = query.Where(u => u.CreatedAt >= parameters.CreatedAtFrom.Value);

            if (parameters.CreatedAtTo.HasValue)
                query = query.Where(u => u.CreatedAt <= parameters.CreatedAtTo.Value);

            if (parameters.EmailConfirmed.HasValue)
                query = query.Where(u => u.EmailConfirmed == parameters.EmailConfirmed.Value);

            if (parameters.PhoneNumberConfirmed.HasValue)
                query = query.Where(u => u.PhoneNumberConfirmed == parameters.PhoneNumberConfirmed.Value);

            var sortHelper = _sortFactory.Create<ApplicationUser>();
            query = sortHelper.ApplySort(query, parameters.OrderBy);

            return await PagedList<ApplicationUser>.ToPagedListAsync(
                query,
                parameters.PageNumber,
                parameters.PageSize,
                cancellationToken);
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _userManager.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }

        public async Task<UserPublicDto?> GetUserPublicInfoAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.Users
                .AsNoTracking()
                .Where(u => u.Id == id)
                .Select(u => new UserPublicDto
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName
                })
                .FirstOrDefaultAsync(cancellationToken);

            return user;
        }

        public async Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return false;

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }
    }
}