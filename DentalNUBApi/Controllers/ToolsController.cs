using DentalNUB.Api.Contracts.Requests;
using DentalNUB.Api.Data;
using DentalNUB.Api.Entities;
using DentalNUB.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DentalNUB.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToolsController : ControllerBase
    {
        private readonly DentalNUBDbContext _context;
        private readonly IImageService _imageService;

        public ToolsController(DentalNUBDbContext context, IImageService imageService)
        {
            _context = context;
            _imageService = imageService;
        }

        // دالة رفع أداة جديدة
        [HttpPost]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> CreateToolPost([FromForm] CreateToolRequest request)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized("User ID not found in token");

            var userId = int.Parse(userIdClaim.Value);
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null)
                return BadRequest("Doctor not found");

            var imageUrl = await _imageService.UploadAsync(request.Image, "ToolImages");

            var toolPost = new ToolPost
            {
                DoctorID = doctor.DoctorID,
                ToolName = request.ToolName,
                Price = request.IsFree ? null : request.Price,
                IsFree = request.IsFree,
                ImageUrl = imageUrl
            };

            _context.ToolPosts.Add(toolPost);
            await _context.SaveChangesAsync();

            return Ok(new { toolPost.ToolPostID });
        }

        // دالة عرض كل الأدوات
        [HttpGet]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetAllTools()
        {
            var tools = await _context.ToolPosts
                .Include(t => t.Doctor)
                .Select(t => new
                {
                    t.ToolPostID,
                    t.ToolName,
                    t.Price,
                    t.IsFree,
                    t.ImageUrl,
                    DoctorName = t.Doctor.DoctorName
                })
                .ToListAsync();

            return Ok(tools);
        }

        // دالة عرض أداة واحدة بالتفصيل
        [HttpGet("{id}")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetToolDetails(int id)
        {
            var tool = await _context.ToolPosts
                .Include(t => t.Doctor)
                .Where(t => t.ToolPostID == id)
                .Select(t => new
                {
                    t.ToolPostID,
                    t.ToolName,
                    t.Price,
                    t.IsFree,
                    t.ImageUrl,
                    DoctorName = t.Doctor.DoctorName,
                    DoctorPhone = t.Doctor.DoctorPhone
                })
                .FirstOrDefaultAsync();

            if (tool == null)
                return NotFound("Tool not found");

            return Ok(tool);
        }

        // دالة شراء الأداة
        [HttpPost("buy/{toolPostId}")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> BuyTool(int toolPostId)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized("User ID not found in token");

            var userId = int.Parse(userIdClaim.Value);
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null)
                return BadRequest("Doctor not found");

            var toolPost = await _context.ToolPosts
                .Include(t => t.Doctor)
                .FirstOrDefaultAsync(t => t.ToolPostID == toolPostId);

            if (toolPost == null)
                return NotFound("Tool not found");

            // بمجرد شراء الأداة، نقوم بعرض التفاصيل، بما في ذلك رقم الهاتف
            return Ok(new
            {
                toolPost.ToolName,
                toolPost.Price,
                toolPost.ImageUrl,
                toolPost.Doctor.DoctorName,
                toolPost.Doctor.DoctorPhone // عرض رقم تليفون الدكتور صاحب الأداة
            });
        }
    }
}