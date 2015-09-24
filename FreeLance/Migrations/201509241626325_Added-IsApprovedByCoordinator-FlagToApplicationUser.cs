namespace FreeLance.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedIsApprovedByCoordinatorFlagToApplicationUser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "IsApprovedByCoordinator", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "IsApprovedByCoordinator");
        }
    }
}
