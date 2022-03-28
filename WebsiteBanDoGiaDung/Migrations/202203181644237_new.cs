namespace WebsiteBanDoGiaDung.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _new : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProductOwner", "Longiude", c => c.String());
            AddColumn("dbo.ProductOwner", "Latitude", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProductOwner", "Latitude");
            DropColumn("dbo.ProductOwner", "Longiude");
        }
    }
}
