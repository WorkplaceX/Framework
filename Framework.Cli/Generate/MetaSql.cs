namespace Framework.Cli.Generate
{
    using Framework.Cli.Config;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;

    /// <summary>
    /// Meta information about sql schema.
    /// </summary>
    internal class MetaSql
    {
        /// <summary>
        /// Constructor runs Schema.sql.
        /// </summary>
        /// <param name="isFrameworkDb">For internal use only.</param>
        public MetaSql(bool isFrameworkDb)
        {
            MetaSqlDbContext dbContext = new MetaSqlDbContext(isFrameworkDb);
            string sql = UtilFramework.FileLoad(UtilFramework.FolderName + "Framework/Framework.Cli/Generate/Sql/Schema.sql");
            this.List = dbContext.Schema.FromSqlRaw(sql).ToArray(); // Schema changes can cause timeout. Run sql command "exec sp_updatestats" on master database. If "select * from sys.columns" is slow, free up some memory.
            //
            // For Application filter out "dbo.Framework" tables.
            if (isFrameworkDb == false)
            {
                this.List = this.List.Where(item => !(item.SchemaName == "dbo" && item.TableName.StartsWith("Framework"))).ToArray();
            }
            else
            {
                this.List = this.List.Where(item => (item.SchemaName == "dbo" && item.TableName.StartsWith("Framework"))).ToArray();
            }
            // Filter out "sysdiagrams" table.
            this.List = this.List.Where(item => item.IsSystemTable == false).ToArray();
        }

        public readonly MetaSqlSchema[] List;
    }

    /// <summary>
    /// DbContext used to query database schema.
    /// </summary>
    internal class MetaSqlDbContext : DbContext
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
    internal class MetaSqlSchema
    {
        public string SchemaName { get; internal set; }

        /// <summary>
        /// Gets TableName. For example: "Raw.Wikipedia.Aircraft". See also property MetaCSharpSchema.TableNameCSharp.
        /// </summary>
        public string TableName { get; internal set; }

        public string FieldName { get; internal set; }

        public int FieldNameSort { get; internal set; }

        public bool IsView { get; internal set; }

        public byte SqlType { get; internal set; }

        public bool IsNullable { get; internal set; }

        public bool IsPrimaryKey { get; internal set; }

        public bool IsSystemTable { get; internal set; }
    }
}
