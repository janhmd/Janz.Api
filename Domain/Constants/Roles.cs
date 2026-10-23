namespace Domain.Constants;

public static class Roles
{
    public const string User = "User";               // Mağazanın müştərisi, admin panelə girişi yoxdur
    public const string Support = "Support";         // Sifarişlərə və istifadəçilərə baxır, sifariş statusunu dəyişir
    public const string Editor = "Editor";           // Jurnal, səhifələr, icma və rəylər
    public const string Admin = "Admin";             // Mağaza və məzmun: rol verə və ayarları dəyişə bilməz
    public const string SuperAdmin = "SuperAdmin";   // Hər şey, üstəlik rol vermək və ayarlar

    public static readonly string[] All = { User, Support, Editor, Admin, SuperAdmin };

    // Admin panelə girə bilən rollar
    public static readonly string[] Staff = { Support, Editor, Admin, SuperAdmin };

    // İstifadəçinin bir neçə rolu olsa, ən yüksəyi götürülür (token və cavabdakı "Role" üçün)
    public static string Highest(IEnumerable<string> roles)
    {
        var list = roles.ToList();
        for (var i = All.Length - 1; i >= 0; i--)
        {
            if (list.Contains(All[i]))
            {
                return All[i];
            }
        }

        return User;
    }
}
