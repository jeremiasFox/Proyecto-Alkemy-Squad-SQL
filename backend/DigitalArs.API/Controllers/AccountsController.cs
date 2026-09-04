using DigitalArs.API.Helpers;
using DigitalArs.Application.DTOs.Account;
using DigitalArs.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigitalArs.Application.Common.Interfaces;

namespace DigitalArs.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly AppDbContext _context;

    private readonly IAccountService _accountService;

    public AccountsController(
    AppDbContext context,
    IAccountService accountService)
    {
        _context = context;
        _accountService = accountService;
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMyAccount()
    {
        var userId = User.GetUserId();

        var account = await _context.Accounts
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .Select(a => new AccountResponseDto
            {
                Id = a.Id,
                Balance = a.Balance,
                CreatedAt = a.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (account is null)
        {
            return NotFound(new { Errors = new[] { "La cuenta no existe" } });
        }

        return Ok(account);
    }

    [HttpPost("deposit")]
    [Authorize]
    public async Task<IActionResult> Deposit(
    [FromBody] DepositRequestDto request,
    CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var response = await _accountService.DepositAsync(
            userId,
            request,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(int id)
    {
        var account = await _context.Accounts
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new AccountResponseDto
            {
                Id = a.Id,
                Balance = a.Balance,
                CreatedAt = a.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (account is null)
        {
            return NotFound(new { Errors = new[] { "La cuenta no existe" } });
        }

        return Ok(account);
    }
}