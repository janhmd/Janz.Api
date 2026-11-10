namespace Service.Helpers.DTOs.Contact
{
    public class CreateContactMessageDto
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Topic { get; set; }
        public string? Message { get; set; }
    }
}
