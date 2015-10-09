namespace FreeLance.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LawContractDiff : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ContractModels",
                c => new
                    {
                        ContractId = c.Int(nullable: false, identity: true),
                        Details = c.String(),
                        Status = c.Int(nullable: false),
                        Cost = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Freelancer_Id = c.String(nullable: false, maxLength: 128),
                        Problem_ProblemId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ContractId)
                .ForeignKey("dbo.AspNetUsers", t => t.Freelancer_Id)
                .ForeignKey("dbo.ProblemModels", t => t.Problem_ProblemId)
                .Index(t => t.Freelancer_Id)
                .Index(t => t.Problem_ProblemId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        IsApprovedByCoordinator = c.Boolean(nullable: false),
                        FIO = c.String(),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.DocumentPackageModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsApproved = c.Boolean(nullable: false),
                        FilePassportFace = c.String(),
                        FilePassportRegistration = c.String(),
                        Adress = c.String(),
                        Phone = c.String(),
                        PaymentDetails = c.String(),
                        Freelancer_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.Freelancer_Id)
                .Index(t => t.Freelancer_Id);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.ProblemModels",
                c => new
                    {
                        ProblemId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Description = c.String(nullable: false),
                        Cost = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Status = c.Int(nullable: false),
                        Employer_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.ProblemId)
                .ForeignKey("dbo.AspNetUsers", t => t.Employer_Id)
                .Index(t => t.Employer_Id);
            
            CreateTable(
                "dbo.SubscriptionModels",
                c => new
                    {
                        SubscriptionId = c.Int(nullable: false, identity: true),
                        Freelancer_Id = c.String(maxLength: 128),
                        Problem_ProblemId = c.Int(),
                    })
                .PrimaryKey(t => t.SubscriptionId)
                .ForeignKey("dbo.AspNetUsers", t => t.Freelancer_Id)
                .ForeignKey("dbo.ProblemModels", t => t.Problem_ProblemId)
                .Index(t => t.Freelancer_Id)
                .Index(t => t.Problem_ProblemId);
            
            CreateTable(
                "dbo.LawContracts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Path = c.String(nullable: false),
                        EndData = c.DateTime(nullable: false),
                        LawContractTemplate_Id = c.Int(nullable: false),
                        User_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.LawContractTemplates", t => t.LawContractTemplate_Id)
                .ForeignKey("dbo.AspNetUsers", t => t.User_Id)
                .Index(t => t.LawContractTemplate_Id)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.LawContractTemplates",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Path = c.String(nullable: false),
                        Name = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.LawFaces", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.LawFaces",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.LawContracts", "User_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.LawContracts", "LawContractTemplate_Id", "dbo.LawContractTemplates");
            DropForeignKey("dbo.LawContractTemplates", "Id", "dbo.LawFaces");
            DropForeignKey("dbo.ContractModels", "Problem_ProblemId", "dbo.ProblemModels");
            DropForeignKey("dbo.SubscriptionModels", "Problem_ProblemId", "dbo.ProblemModels");
            DropForeignKey("dbo.SubscriptionModels", "Freelancer_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.ProblemModels", "Employer_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.ContractModels", "Freelancer_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.DocumentPackageModels", "Freelancer_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.LawContractTemplates", new[] { "Id" });
            DropIndex("dbo.LawContracts", new[] { "User_Id" });
            DropIndex("dbo.LawContracts", new[] { "LawContractTemplate_Id" });
            DropIndex("dbo.SubscriptionModels", new[] { "Problem_ProblemId" });
            DropIndex("dbo.SubscriptionModels", new[] { "Freelancer_Id" });
            DropIndex("dbo.ProblemModels", new[] { "Employer_Id" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.DocumentPackageModels", new[] { "Freelancer_Id" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.ContractModels", new[] { "Problem_ProblemId" });
            DropIndex("dbo.ContractModels", new[] { "Freelancer_Id" });
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.LawFaces");
            DropTable("dbo.LawContractTemplates");
            DropTable("dbo.LawContracts");
            DropTable("dbo.SubscriptionModels");
            DropTable("dbo.ProblemModels");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.DocumentPackageModels");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.ContractModels");
        }
    }
}
