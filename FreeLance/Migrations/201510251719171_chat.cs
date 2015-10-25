namespace FreeLance.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class chat : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ChatMessages", "ModificationDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ChatMessages", "ModificationDate", c => c.DateTime(nullable: false));
        }
    }
}
