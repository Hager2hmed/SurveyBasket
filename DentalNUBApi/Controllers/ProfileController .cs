﻿using DentalNUB.Api.Contracts.Requests;
using DentalNUB.Api.Contracts.Responses;
using DentalNUB.Api.Data;
using DentalNUB.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DentalNUB.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class ProfileController : ControllerBase
{
    private readonly DentalNUBDbContext _context;

    public ProfileController(DentalNUBDbContext context)
    {
        _context = context;
    }
    [HttpGet("Get profile info")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        var roleClaim = User.FindFirst(ClaimTypes.Role);

        if (userIdClaim == null || roleClaim == null)
            return Unauthorized();

        int userId = int.Parse(userIdClaim.Value);
        string role = roleClaim.Value;

        if (role == "Doctor")
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Clinic)  // إضافة الكلينك هنا
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
                return NotFound("Doctor not found");

            var response = new DoctorProfileResponse
            {
                DoctorName = doctor.DoctorName!,
                DoctorEmail = doctor.DoctorEmail,
                DoctorPhone = doctor.DoctorPhone,
                DoctorYear = doctor.DoctorYear,
                UniversityID = doctor.UniversityID,
                ClinicName = doctor.Clinic?.ClinicName ?? "No Clinic Assigned"  // إضافة اسم الكلينك هنا
            };

            return Ok(response);
        }
        else if (role == "Patient")
        {
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
                return NotFound("Patient not found");

            var response = new PatientProfileResponse
            {
                PatientName = patient.PatientName,
                Email = patient.User?.Email ?? "",
                PatPhone = patient.PatPhone
            };

            return Ok(response);
        }
        else if (role == "Consultant")
        {
            var consultant = await _context.Consultants
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (consultant == null)
                return NotFound("Consultant not found");

            var response = new ConsultantProfileResponse
            {
                ConsName = consultant.ConsName,
                ConsEmail = consultant.ConsEmail ?? ""
            };

            return Ok(response);
        }

        return BadRequest("Invalid role");
    }
    [HttpPut("Edit doctor profile")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> EditDoctorProfile([FromBody] EditDoctorProfileRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();

        int userId = int.Parse(userIdClaim.Value);

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var doctor = await _context.Doctors
                .Include(d => d.ClinicSection)
                .Include(d => d.Clinic)
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
                return NotFound("Doctor not found");

            if (string.IsNullOrEmpty(request.DoctorPhone) || request.DoctorYear == 0 || string.IsNullOrEmpty(request.ClinicName))
            {
                return BadRequest("Phone number, Academic Year, and Clinic name are required.");
            }

            bool phoneChanged = doctor.DoctorPhone != request.DoctorPhone;
            bool yearChanged = doctor.DoctorYear != request.DoctorYear;
            bool clinicChanged = doctor.Clinic?.ClinicName != request.ClinicName;

            if (phoneChanged)
            {
                doctor.DoctorPhone = request.DoctorPhone;
            }

            if (yearChanged)
            {
                doctor.DoctorYear = request.DoctorYear;
            }

            if (clinicChanged || yearChanged)
            {
                // remove from old section if exists
                if (doctor.ClinicSection != null)
                {
                    doctor.ClinicSection.Doctors.Remove(doctor);
                }

                // Get clinic by name
                var newClinic = await _context.Clinics.FirstOrDefaultAsync(c => c.ClinicName == request.ClinicName);
                if (newClinic == null)
                {
                    return BadRequest("Invalid Clinic Name.");
                }

                // Try to find existing section for same clinic and year
                var section = await _context.clinicSections
                    .Include(s => s.Doctors)
                    .FirstOrDefaultAsync(s => s.ClinicID == newClinic.ClinicID && s.DoctorYear == request.DoctorYear);

                if (section == null)
                {
                    section = new ClinicSection
                    {
                        ClinicID = newClinic.ClinicID,
                        SectionName = $"{newClinic.ClinicName}-Y{request.DoctorYear}",
                        DoctorYear = request.DoctorYear,
                        MaxStudents = newClinic.MaxStudent, // ← خدها من العيادة نفسها
                        Doctors = new List<Doctor>()
                    };

                    _context.clinicSections.Add(section);
                }


                doctor.Clinic = newClinic;
                doctor.ClinicSection = section;

                if (!section.Doctors.Contains(doctor))
                {
                    section.Doctors.Add(doctor);
                }
            }

            // التأكد من وجود الدكتور قبل الحفظ النهائي
            var existsBeforeSave = await _context.Doctors.AnyAsync(d => d.UserId == userId);
            if (!existsBeforeSave)
            {
                return NotFound("Doctor no longer exists.");
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok("Doctor profile updated successfully.");
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            return Conflict("The data was modified by another user. Please try again.");
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, "An error occurred while updating the doctor profile.");
        }
    }



}
