namespace DentalNUB.Api.Contracts.Requests;

public record QuestionResponse
{
    public int QuestID { get; set; }
    public string QuesText { get; set; } = string.Empty;
}
