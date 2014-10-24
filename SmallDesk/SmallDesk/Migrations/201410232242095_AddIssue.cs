namespace SmallDesk.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIssue : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Issues",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CreatedAt = c.DateTime(nullable: false),
                        ExpectedAt = c.DateTime(nullable: false),
                        ResolvedAt = c.DateTime(nullable: false),
                        ProblemData = c.String(nullable: false),
                        SolutionData = c.String(),
                        IsSolved = c.Boolean(nullable: false),
                        User_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.User_Id, cascadeDelete: true)
                .Index(t => t.User_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Issues", "User_Id", "dbo.AspNetUsers");
            DropIndex("dbo.Issues", new[] { "User_Id" });
            DropTable("dbo.Issues");
        }
    }
}
