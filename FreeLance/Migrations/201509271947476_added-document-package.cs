namespace FreeLance.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addeddocumentpackage : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DocumentPackageModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsApproved = c.Boolean(nullable: false),
                        User_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.User_Id)
                .Index(t => t.User_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DocumentPackageModels", "User_Id", "dbo.AspNetUsers");
            DropIndex("dbo.DocumentPackageModels", new[] { "User_Id" });
            DropTable("dbo.DocumentPackageModels");
        }
    }
}
