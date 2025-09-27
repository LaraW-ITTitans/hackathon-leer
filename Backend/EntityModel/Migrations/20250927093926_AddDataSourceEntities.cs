using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITTitans.Hackathon2025.EntityModel.Migrations
{
    /// <inheritdoc />
    public partial class AddDataSourceEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataSource",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(2500)", maxLength: 2500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSource", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataSourceRequirement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DataSourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    SkillId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSourceRequirement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataSourceRequirement_DataSource_DataSourceId",
                        column: x => x.DataSourceId,
                        principalTable: "DataSource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DataSourceRequirement_Skill_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skill",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DataSourceRequirement_DataSourceId",
                table: "DataSourceRequirement",
                column: "DataSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_DataSourceRequirement_SkillId",
                table: "DataSourceRequirement",
                column: "SkillId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataSourceRequirement");

            migrationBuilder.DropTable(
                name: "DataSource");
        }
    }
}
