namespace SmallDesk.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveRequiredFromResolvedAtIssue : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Issues", "ResolvedAt", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Issues", "ResolvedAt", c => c.DateTime(nullable: false));
        }
    }
}
