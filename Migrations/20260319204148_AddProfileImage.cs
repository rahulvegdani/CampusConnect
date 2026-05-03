using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampusConnect.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProfileImage",
                table: "UserProfiles",
                newName: "ProfileImagePath");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProfileImagePath",
                table: "UserProfiles",
                newName: "ProfileImage");
        }
    }
}
