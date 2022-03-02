namespace WebsiteBanDoGiaDung.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addfielduser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.User", "PayTotal", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.User", "PayTotal");
        }
    }
}
