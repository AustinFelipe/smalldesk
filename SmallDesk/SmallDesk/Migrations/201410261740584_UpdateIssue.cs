namespace SmallDesk.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateIssue : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Issues", "UserThatReported_Id", c => c.String(nullable: false, maxLength: 128));
            CreateIndex("dbo.Issues", "UserThatReported_Id");
            AddForeignKey("dbo.Issues", "UserThatReported_Id", "dbo.AspNetUsers", "Id", cascadeDelete: false);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Issues", "UserThatReported_Id", "dbo.AspNetUsers");
            DropIndex("dbo.Issues", new[] { "UserThatReported_Id" });
            DropColumn("dbo.Issues", "UserThatReported_Id");
        }
    }
}
