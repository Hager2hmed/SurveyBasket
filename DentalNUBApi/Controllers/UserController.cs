﻿using DentalNUB.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace DentalNUB.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UserController : ControllerBase
{
    private readonly DentalNUBDbContext _context;

    public UserController(DentalNUBDbContext context)
    {
        _context = context;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetUserProfile()
    {
        // جيب الـ UserId من الـ JWT token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized("Invalid user ID.");
        }

        // جيب الـ Role من الـ token
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        // لو الـ Role هي Admin، ارجع Forbidden
        if (role == "Admin")
        {
            return Forbid("Admins are not allowed to access this endpoint.");
        }

        // جيب بيانات الـ User من الـ database
        var user = await _context.Users
            .Where(u => u.UserId == userId)
            .Select(u => new
            {
                u.FullName,
                u.Email
            })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return NotFound("User not found.");
        }

        // ارجع الـ response
        return Ok(new
        {
            FullName = user.FullName,
            Email = user.Email
        });


    }
}
