using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinqCheckConstraints.Tests.Postgres.Migrations
{
    public partial class AddIntialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TestEntity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Sbyte = table.Column<short>(type: "smallint", nullable: false),
                    Byte = table.Column<byte>(type: "smallint", nullable: false),
                    Ushort = table.Column<int>(type: "integer", nullable: false),
                    Short = table.Column<short>(type: "smallint", nullable: false),
                    Uint = table.Column<long>(type: "bigint", nullable: false),
                    Int = table.Column<int>(type: "integer", nullable: false),
                    Ulong = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Long = table.Column<long>(type: "bigint", nullable: false),
                    String = table.Column<string>(type: "text", nullable: false),
                    DateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DateTimeOffset = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestEntity", x => x.Id);
                    table.CheckConstraint("CK_TestEntity_ByteSmallerThanFive", "(  \"Byte\" < 5)");
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestEntity");
        }
    }
}
