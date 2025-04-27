using DentalNUB.Api.Contracts.Requests;
using DentalNUB.Api.Contracts.Responses;
using DentalNUB.Api.Data;
using DentalNUB.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace DentalNUB.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Doctor")]
    public class DoctorsController : ControllerBase
    {
        private readonly DentalNUBDbContext _context;

        public DoctorsController(DentalNUBDbContext context)
        {
            _context = context;
        }

       
        [HttpPost("complete-profile")]
        public async Task<IActionResult> CompleteDoctorProfile([FromBody] CompleteDoctorProfileRequest request)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized("User ID not found in token");

            var userId = int.Parse(userIdClaim.Value);
            var user = await _context.Users.FindAsync(userId);

            if (user == null || user.Role != "Doctor")
                return BadRequest("Invalid user or role");

            var clinic = await _context.Clinics
                .Include(c => c.Doctors)
                .FirstOrDefaultAsync(c => c.ClinicName == request.ClinicName);

            if (clinic == null)
                return BadRequest("Clinic not found");

                var existingDoctors = await _context.Doctors
        .Include(d => d.ClinicSection) // عشان نجيب السيكشن اللي الدكتور عليه
        .Where(d => d.ClinicID == clinic.ClinicID && d.DoctorYear == request.DoctorYear)
        .ToListAsync();

            var groupedBySection = existingDoctors
                .GroupBy(d => d.ClinicSection!.SectionName)
                .OrderBy(g => g.Key)
                .ToList();

            string sectionName;
            int orderInSection;

            if (groupedBySection.Count == 0)
            {
                sectionName = $"{clinic.ClinicName} A";
                orderInSection = 1;
            }
            else
            {
                var lastGroup = groupedBySection.Last();
                if (lastGroup.Count() < 30)
                {
                    sectionName = lastGroup.Key;
                    orderInSection = lastGroup.Count() + 1;
                }
                else
                {
                    char lastLetter = lastGroup.Key.Last();
                    char newLetter = (char)(lastLetter + 1);
                    sectionName = $"{clinic.ClinicName} {newLetter}";
                    orderInSection = 1;
                }
            }

            // ابحث عن السيكشن لو موجود، لو مش موجود انشئه
            var clinicSection = await _context.clinicSections
                .FirstOrDefaultAsync(s => s.ClinicID == clinic.ClinicID && s.SectionName == sectionName && s.DoctorYear == request.DoctorYear);

            if (clinicSection == null)
            {
                clinicSection = new ClinicSection
                {
                    ClinicID = clinic.ClinicID,
                    SectionName = sectionName,
                    DoctorYear = request.DoctorYear,
                    MaxStudents = 30
                };

                _context.clinicSections.Add(clinicSection);
                await _context.SaveChangesAsync();
            }

            var doctor = new Doctor
            {
                DoctorName = user.FullName,
                DoctorEmail = user.Email,
                DoctorPassword = user.PasswordHash,
                DoctorPhone = request.DoctorPhone,
                DoctorYear = request.DoctorYear,
                UniversityID = request.UniversityID,
                ClinicID = clinic.ClinicID,
                SectionOrder = orderInSection,
                SectionID = clinicSection.SectionID,
                UserId = user.UserId
            };

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            return Ok($"✅ تم إكمال الملف بنجاح! السيكشن: {sectionName}، ترتيبك فيه: {orderInSection}");
        }


        [Authorize(Roles = "Doctor")]
        [HttpGet("cases")]
        public async Task<IActionResult> GetDoctorCases()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdString, out int userId))
                return Unauthorized("User ID is not valid.");

            var doctor = await _context.Doctors
                .Include(d => d.Patientcases)
                    .ThenInclude(c => c.Patient)
                .FirstOrDefaultAsync(d => d.UserId == userId); // تمام كده لأنه int

            if (doctor == null)
                return NotFound("Doctor not found.");

            var response = doctor.Patientcases
                .Select(c => new CasesDetailsResponse
                {
                    CaseID = c.CaseID,
                    PatientName = c.Patient.PatientName,
                    Age = c.Patient.Age,
                    PatPhone = c.Patient.PatPhone,
                    ChronicalDiseases = c.Patient.ChronicalDiseases
                }).ToList();

            return Ok(response);
        }


        [Authorize(Roles = "Doctor")]
        [HttpGet("cases/{caseId}")]
        public async Task<IActionResult> GetCaseDetails(int caseId)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdString, out int userId))
                return Unauthorized("User ID is not valid.");

            var doctor = await _context.Doctors
                .Include(d => d.Patientcases)
                    .ThenInclude(c => c.Patient)
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
                return NotFound("Doctor not found.");

            var patientCase = doctor.Patientcases
                .FirstOrDefault(c => c.CaseID == caseId);

            if (patientCase == null)
                return NotFound("Case not found or not assigned to this doctor.");

            var response = new CasesDetailsResponse
            {
                CaseID = patientCase.CaseID,
                PatientName = patientCase.Patient.PatientName,
                Age = patientCase.Patient.Age,
                PatPhone = patientCase.Patient.PatPhone,
                ChronicalDiseases = patientCase.Patient.ChronicalDiseases
            };

            return Ok(response);
        }

        [HttpGet("case/{caseId}/diagnosis")]
        public async Task<IActionResult> GetDiagnosisForCase(int caseId)
        {
            // جيب الـ UserId من الـ JWT token عشان نتأكد إن الدكتور هو اللي له صلاحية على الـ PatientCase
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized("Invalid user ID.");
            }

            // تحقق إن الـ PatientCase موجود ومرتبط بالدكتور
            var patientCase = await _context.PatientCases
                .Include(pc => pc.Doctor)
                .FirstOrDefaultAsync(pc => pc.CaseID == caseId && pc.Doctor.UserId == userId);

            if (patientCase == null)
            {
                return NotFound("Case not found or not assigned to this doctor.");
            }

            // جيب الـ Diagnose المرتبط بالـ PatientCase
            var diagnose = await _context.Diagnoses
                .Where(d => d.DiagnoseID == patientCase.DiagnoseID)
                .Select(d => new
                {
                    d.AssignedClinic,
                    d.FinalDiagnose
                })
                .FirstOrDefaultAsync();

            if (diagnose == null)
            {
                return NotFound("No diagnosis found for this case.");
            }

            // ارجع الـ response
            return Ok(new
            {
                AssignedClinic = diagnose.AssignedClinic,
                FinalDiagnose = diagnose.FinalDiagnose
            });
        }

    }
}