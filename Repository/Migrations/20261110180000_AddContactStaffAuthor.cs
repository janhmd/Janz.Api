using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Repository.Data;

#nullable disable

namespace Repository.Migrations
{
    // Yazışmada cavabı yazan işçini saxlayır: müştəriyə onun şəkli və adı göstərilir
    [DbContext(typeof(AppDbContext))]
    [Migration("20261110180000_AddContactStaffAuthor")]
    public partial class AddContactStaffAuthor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AnsweredById",
                table: "ContactMessages",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StaffUserId",
                table: "ContactEntries",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "AnsweredById", table: "ContactMessages");
            migrationBuilder.DropColumn(name: "StaffUserId", table: "ContactEntries");
        }
    }
}
