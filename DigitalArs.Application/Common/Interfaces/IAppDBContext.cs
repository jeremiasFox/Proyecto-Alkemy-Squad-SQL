using DigitalArs.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalArs.Application.Common.DTOs.Interfaces;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}