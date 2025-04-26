namespace DentalNUB.Api.Contracts.Requests;

public class VerifyCodeRequest
{
    public string Email { get; set; }
    public string Code { get; set; }
}
