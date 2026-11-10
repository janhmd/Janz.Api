using Service.Helpers.DTOs.Contact;

namespace Service.Services.Interfaces
{
    public interface IContactService
    {
        // Müştəri: əlaqə formu (giriş edibsə userId gəlir)
        Task CreateAsync(CreateContactMessageDto model, string? userId);

        // Admin və Support: mesajlar, status = "new" | "answered" | boş (hamısı)
        Task<IEnumerable<ContactMessageDto>> GetAllAsync(string? status);
        Task<ContactMessageDto> GetByIdAsync(int id);
        Task ReplyAsync(int id, ReplyContactMessageDto model);

        // Müştəri: öz yazdığı mesajlar və aldığı cavablar (e-poçt gəlməsə də saytda görünür)
        Task<IEnumerable<ContactMessageDto>> GetMineAsync(string userId);
    }
}
