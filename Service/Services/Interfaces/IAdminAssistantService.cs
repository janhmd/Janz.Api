using Service.Helpers.DTOs.Assistant;

namespace Service.Services.Interfaces
{
    public interface IAdminAssistantService
    {
        Task<AssistantAnswerDto> AskAsync(AskAssistantDto model);
    }
}
