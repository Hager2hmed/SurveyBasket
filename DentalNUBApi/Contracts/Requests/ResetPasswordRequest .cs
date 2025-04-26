namespace DentalNUB.Api.Contracts.Requests;

public class ResetPasswordRequest
{
    public string Email { get; set; }
    public string Code { get; set; } // الكود اللي اتبعت
    public string NewPassword { get; set; } // الباسورد الجديد
}
