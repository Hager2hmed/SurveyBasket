namespace DentalNUB.Api.Entities;

public class PasswordResetCode
{
    public int Id { get; set; }              // Primary key غالباً كان موجود
    public string Email { get; set; }        // البريد الإلكتروني المرتبط بالكود
    public string Code { get; set; }         // الكود العشوائي نفسه
    public DateTime Expiration { get; set; }
}
