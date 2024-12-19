
using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;

namespace DigireadProject.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<DigireadProject.libraryProject_digireadEntities>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }
    } 
}