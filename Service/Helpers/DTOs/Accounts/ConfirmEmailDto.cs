namespace Service.Helpers.DTOs.Accounts
{
    // Qeydiyyatda emailə gedən 4 rəqəmli kodu təsdiqləmək üçün
    public class ConfirmEmailDto
    {
        public string? Email { get; set; }
        public string? Code { get; set; }
    }
}
