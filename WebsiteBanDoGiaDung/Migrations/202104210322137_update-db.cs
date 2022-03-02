namespace WebsiteBanDoGiaDung.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedb : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GroupUser",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Level = c.Int(nullable: false),
                        Description = c.String(),
                        Created_at = c.DateTime(nullable: false),
                        Created_by = c.Int(nullable: false),
                        Updated_at = c.DateTime(nullable: false),
                        Updated_by = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            AddColumn("dbo.User", "GroupId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.User", "GroupId");
            DropTable("dbo.GroupUser");
        }
    }
}
