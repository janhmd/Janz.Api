using Service.Helpers.DTOs.Assistant;

namespace Service.Services.Interfaces
{
    public interface IAssistantService
    {
        Task<AssistantAnswerDto> AskAsync(AskAssistantDto model);
    }
}
