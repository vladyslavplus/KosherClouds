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

        public async Task<UserProfileDto?> GetUserProfileAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.Users
                .AsNoTracking()
                .Where(u => u.Id == id)
                .Select(u => new UserProfileDto
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    EmailConfirmed = u.EmailConfirmed,
                    PhoneNumberConfirmed = u.PhoneNumberConfirmed,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt
                })
                .FirstOrDefaultAsync(cancellationToken);

            return user;
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
                    PhoneNumber = u.PhoneNumber,
                    FirstName = u.FirstName,
                    LastName = u.LastName
                })
                .FirstOrDefaultAsync(cancellationToken);

            return user;
        }

        public async Task<(bool Success, string? Error)> UpdateUserAsync(Guid userId, UpdateUserRequest request, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null)
                return (false, "User not found");

            var usernameChanged = false;
            var emailChanged = false;

            if (!string.IsNullOrWhiteSpace(request.UserName) && request.UserName != user.UserName)
            {
                var normalizedNewName = _userManager.NormalizeName(request.UserName);

                var isTaken = await _userManager.Users
                    .AsNoTracking()
                    .AnyAsync(u => u.NormalizedUserName == normalizedNewName && u.Id != userId, cancellationToken);

                if (isTaken)
                    return (false, "Username is already taken");

                user.UserName = request.UserName;
                usernameChanged = true;
            }

            if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != user.Email)
            {
                var normalizedNewEmail = _userManager.NormalizeEmail(request.Email);

                var isTaken = await _userManager.Users
                    .AsNoTracking()
                    .AnyAsync(u => u.NormalizedEmail == normalizedNewEmail && u.Id != userId, cancellationToken);

                if (isTaken)
                    return (false, "Email is already taken");

                user.Email = request.Email;
                user.EmailConfirmed = false;
                emailChanged = true;
            }

            if (request.PhoneNumber != null && request.PhoneNumber != user.PhoneNumber)
            {
                if (string.IsNullOrWhiteSpace(request.PhoneNumber))
                {
                    user.PhoneNumber = null;
                    user.PhoneNumberConfirmed = false;
                }
                else
                {
                    var isTaken = await _userManager.Users
                        .AsNoTracking()
                        .AnyAsync(u => u.PhoneNumber == request.PhoneNumber && u.Id != userId, cancellationToken);

                    if (isTaken)
                        return (false, "Phone number is already taken");

                    user.PhoneNumber = request.PhoneNumber;
                    user.PhoneNumberConfirmed = false;
                }
            }

            if (usernameChanged)
                await _userManager.UpdateNormalizedUserNameAsync(user);

            if (emailChanged)
                await _userManager.UpdateNormalizedEmailAsync(user);

            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return (false, string.Join(", ", result.Errors.Select(e => e.Description)));

            return (true, null);
        }

        public async Task<(bool Success, string? Error)> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null)
                return (false, "User not found");

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if (!result.Succeeded)
                return (false, string.Join(", ", result.Errors.Select(e => e.Description)));

            return (true, null);
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