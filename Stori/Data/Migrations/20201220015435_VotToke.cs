using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Stori.Data.Migrations
{
    public partial class VotToke : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StoriNode",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    Parent = table.Column<Guid>(nullable: false),
                    CreationDate = table.Column<DateTime>(nullable: false),
                    Creator = table.Column<Guid>(nullable: false),
                    Action = table.Column<string>(maxLength: 256, nullable: false),
                    Content = table.Column<string>(maxLength: 2048, nullable: false),
                    Votes = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoriNode", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "VoteToken",
                columns: table => new
                {
                    UserID = table.Column<Guid>(nullable: false),
                    Node = table.Column<Guid>(nullable: false),
                    Token = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoteToken", x => x.UserID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StoriNode");

            migrationBuilder.DropTable(
                name: "VoteToken");
        }
    }
}
