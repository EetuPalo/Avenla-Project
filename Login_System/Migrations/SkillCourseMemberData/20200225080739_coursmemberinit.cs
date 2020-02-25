using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Login_System.Migrations.SkillCourseMemberData
{
    public partial class coursmemberinit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletionDate",
                table: "SkillCourseMembers",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CourseID",
                table: "SkillCourseMembers",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CourseName",
                table: "SkillCourseMembers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "SkillCourseMembers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletionDate",
                table: "SkillCourseMembers");

            migrationBuilder.DropColumn(
                name: "CourseID",
                table: "SkillCourseMembers");

            migrationBuilder.DropColumn(
                name: "CourseName",
                table: "SkillCourseMembers");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "SkillCourseMembers");
        }
    }
}
