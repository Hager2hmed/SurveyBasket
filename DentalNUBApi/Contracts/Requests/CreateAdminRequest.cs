﻿namespace DentalNUB.Api.Contracts.Requests;

public class CreateAdminRequest
{
    public string FullName { get; set; } =string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } =string.Empty ;
}
