// <auto-generated />
namespace Thinktecture.IdentityServer.Core.Repositories.Migrations
{
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Resources;
    
    public sealed partial class ClientsTable : IMigrationMetadata
    {
        private readonly ResourceManager Resources = new ResourceManager(typeof(ClientsTable));
        
        string IMigrationMetadata.Id
        {
            get { return "201210150742445_ClientsTable"; }
        }
        
        string IMigrationMetadata.Source
        {
            get { return null; }
        }
        
        string IMigrationMetadata.Target
        {
            get { return Resources.GetString("Target"); }
        }
    }
}