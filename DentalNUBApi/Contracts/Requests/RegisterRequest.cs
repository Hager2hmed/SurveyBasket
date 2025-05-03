﻿namespace DentalNUB.Api.Contracts.Requests;

public record RegisterRequest
{
    public string FullName { get; set; } = string.Empty;      
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
