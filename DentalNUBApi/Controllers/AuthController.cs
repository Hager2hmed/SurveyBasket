using DentalNUB.Api.Contracts.Requests;
using DentalNUB.Api.Contracts.Responses;
using DentalNUB.Api.Data;
using DentalNUB.Api.Entities;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DentalNUB.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using DocumentFormat.OpenXml.Spreadsheet;

namespace DentalNUB.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly DentalNUBDbContext _context;
    private readonly IConfiguration _config;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly JwtSettings _jwtSettings;

    public AuthController(
        ITokenService tokenService,
        DentalNUBDbContext context,
        IConfiguration config,
        IPasswordHasher<User> passwordHasher,
        IOptions<JwtSettings> jwtSettings)
    {
        _context = context;
        _config = config;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _jwtSettings = jwtSettings.Value;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
            return Unauthorized("الإيميل أو الباسورد غلط");

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
            return Unauthorized("الإيميل أو الباسورد غلط");

        var token = await _tokenService.CreateToken(user); // تأكد إن CreateToken بيرجع String فقط

        var response = new
        {
            Token = token,
            UserId = user.UserId,
            Role = user.Role,
            FullName = user.FullName
        };

        return Ok(response);
    }


    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim(ClaimTypes.Role, user.Role ?? "")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.Now.AddHours(2),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    [HttpPost("signup")]
    public async Task<IActionResult> SignUp([FromBody] RegisterRequest request)
    {
        // 1. Check if email already exists in Users table
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return BadRequest("Email already in use.");

        // 2. Map request to User entity
        var user = request.Adapt<User>();
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // 3. Add user to database
        _context.Users.Add(user);
        await _context.SaveChangesAsync(); // لازم أحفظ هنا علشان ياخد UserId

        // 4. Create specific role entity
        switch (user.Role)
        {
            case "Consultant":
                if (await _context.Consultants.AnyAsync(c => c.UserId == user.UserId))
                    return BadRequest("Consultant already exists for this user.");

                var consultant = new Consultant
                {
                    UserId = user.UserId,
                    ConsName = user.FullName,
                    ConsEmail = user.Email,
                    ConsPassword = user.PasswordHash // لو محتاج تخزن الباسورد هنا برضو
                };

                _context.Consultants.Add(consultant);
                break;

            case "Patient":
                if (await _context.Patients.AnyAsync(p => p.UserId == user.UserId))
                    return BadRequest("Patient already exists for this user.");

                var patient = new Patient
                {
                    UserId = user.UserId,
                    PatientName = user.FullName
                };

                _context.Patients.Add(patient);
                break;

                // لو عندك أدوار تانية زي Doctor مثلاً، زودها هنا بنفس الفكرة
        }

        // 5. Save changes again for role-specific entity
        await _context.SaveChangesAsync();

        // 6. Generate JWT Token
        var token = await _tokenService.CreateToken(user);

        // 7. Return response
        return Ok(new
        {
            Message = "Account created successfully.",
            Token = token,
            Name = user.FullName,
            Role = user.Role,
            RequiresAdditionalInfo = user.Role == "Doctor", // لو محتاج Doctor يكمّل بيانات بعد التسجيل
            UserId = user.UserId
        });
    }
    [HttpPost("forget-password")]
    public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
            return NotFound("البريد الإلكتروني غير مسجل.");

        var code = new Random().Next(1000, 9999).ToString();

        var resetCode = new PasswordResetCode
        {
            Email = request.Email,
            Code = code,
            Expiration = DateTime.UtcNow.AddMinutes(5)
        };

        _context.PasswordResetCodes.Add(resetCode);
        await _context.SaveChangesAsync();

        Console.WriteLine($"Verification code for {request.Email} is {code}");

        return Ok("تم إرسال الكود إلى بريدك الإلكتروني.");
    }

    [HttpPost("verify-code")]
    public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeRequest request)
    {
        var codeEntry = await _context.PasswordResetCodes
            .FirstOrDefaultAsync(c => c.Email == request.Email && c.Code == request.Code);

        if (codeEntry == null)
            return NotFound("الكود غير صحيح أو الإيميل غير صحيح.");

        if (codeEntry.Expiration < DateTime.UtcNow)
            return BadRequest("انتهت صلاحية الكود. برجاء طلب كود جديد.");

        return Ok("الكود صحيح، يمكنك الآن إعادة تعيين كلمة المرور.");
    }
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        // 1. نتحقق من وجود الكود والبريد في جدول PasswordResetCodes
        var codeEntry = await _context.PasswordResetCodes
            .FirstOrDefaultAsync(c => c.Email == request.Email && c.Code == request.Code);

        if (codeEntry == null)
            return NotFound("الكود غير صحيح أو الإيميل غير صحيح.");

        if (codeEntry.Expiration < DateTime.UtcNow)
            return BadRequest("انتهت صلاحية الكود. برجاء طلب كود جديد.");

        // 2. نجيب المستخدم
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
            return NotFound("المستخدم غير موجود.");

        // 3. نحدث الباسورد
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        // 4. نمسح الكود من جدول PasswordResetCodes
        _context.PasswordResetCodes.Remove(codeEntry);

        await _context.SaveChangesAsync();

        return Ok("تم تغيير كلمة المرور بنجاح.");
    }




}
