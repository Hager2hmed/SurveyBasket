﻿using DentalNUB.Api.Contracts.Requests;
using DentalNUB.Api.Contracts.Responses;
using DentalNUB.Api.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace DentalNUB.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Patient")]
public class StaticChatController : ControllerBase
{
    private readonly DentalNUBDbContext _context;

    public StaticChatController(DentalNUBDbContext context)
    {
        _context = context;
    }

    // Endpoint 1: لعرض الأسئلة
    [Authorize(Roles = "Patient")]

    [HttpGet("GetQuestions")]
    public async Task<ActionResult<IEnumerable<QuestionResponse>>> GetQuestions()
    {
        var questions = await _context.Questions
                                      .Select(q => q.Adapt<QuestionResponse>())
                                      .ToListAsync();

        return Ok(questions);
    }

    // Endpoint 2: لعرض الإجابة الخاصة بسؤال معين
    [HttpGet("{id}/answer")]
    public async Task<ActionResult<AnswerResponse>> GetAnswer([FromRoute]int id)
    {
        var answer = await _context.Answers
                                   .Where(a => a.QuestID == id)
                                   .FirstOrDefaultAsync();

        if (answer == null)
        {
            return NotFound("الإجابة مش موجودة.");
        }

        return Ok(answer.Adapt<AnswerResponse>());
    }
}

