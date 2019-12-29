namespace Framework.Cli.Generate
{
    using Framework.Cli.Command;
    using Framework.Cli.Config;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;

    /// <summary>
    /// Meta information about sql schema.
    /// </summary>
    public class MetaSql
    {
        /// <summary>
        /// Constructor runs Schema.sql.
        /// </summary>
        /// <param name="isFrameworkDb">For internal use only.</param>
        public MetaSql(bool isFrameworkDb, AppCli appCli)
        {
            MetaSqlDbContext dbContext = new MetaSqlDbContext(isFrameworkDb);
            string sql = UtilFramework.FileLoad(UtilFramework.FolderName + "Framework/Framework.Cli/Generate/Sql/Schema.sql");
            this.List = dbContext.Schema.FromSqlRaw(sql).ToArray(); // Schema changes can cause timeout. Run sql command sp_updatestats on master database.
            //
            // For Application filter out "dbo.Framework" tables.
            if (isFrameworkDb == false)
            {
                this.List = this.List.Where(item => !(item.SchemaName.StartsWith("dbo") && item.TableName.StartsWith("Framework"))).ToArray();
                this.List = appCli.CommandGenerateFilter(this.List); // Custom table name filtering for code generation.
            }
            else
            {
                this.List = this.List.Where(item => (item.SchemaName.StartsWith("dbo") && item.TableName.StartsWith("Framework"))).ToArray();
            }
            // Filter out "sysdiagrams" table.
            this.List = this.List.Where(item => item.IsSystemTable == false).ToArray();
        }

        public readonly MetaSqlSchema[] List;
    }

    /// <summary>
    /// DbContext used to query database schema.
    /// </summary>
    public class MetaSqlDbContext : DbContext
    {
        public MetaSqlDbContext(bool isFrameworkDb)
        {
            this.IsFrameworkDb = isFrameworkDb;
        }

        public readonly bool IsFrameworkDb;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = ConfigCli.ConnectionString(IsFrameworkDb);
            UtilFramework.Assert(string.IsNullOrEmpty(connectionString) == false, "ConnectionString is null!");
            optionsBuilder.UseSqlServer(connectionString);
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<MetaSqlSchema>().HasNoKey();
        }

        public DbSet<MetaSqlSchema> Schema { get; set; }
    }

    /// <summary>
    /// See also file Sql\Schema.sql
    /// </summary>
    public class MetaSqlSchema
    {
        public string SchemaName { get; set; }

        public string TableName { get; set; }

        public string FieldName { get; set; }

        public int FieldNameSort { get; set; }

        public bool IsView { get; set; }

        public byte SqlType { get; set; }

        public bool IsNullable { get; set; }

        public bool IsPrimaryKey { get; set; }

        public bool IsSystemTable { get; set; }
    }
}
