namespace WebsiteBanDoGiaDung.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedb2 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ProductOwner",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Created_at = c.DateTime(nullable: false),
                        Created_by = c.Int(nullable: false),
                        Updated_at = c.DateTime(nullable: false),
                        Updated_by = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Product", "OwnerId", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Product", "OwnerId");
            DropTable("dbo.ProductOwner");
        }
    }
}
