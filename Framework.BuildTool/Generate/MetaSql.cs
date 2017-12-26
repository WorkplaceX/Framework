namespace Framework.BuildTool.DataAccessLayer
{
    using Framework.Server;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    /// <summary>
    /// Meta information about sql schema.
    /// </summary>
    public class MetaSql
    {
        /// <summary>
        /// Run Schema.sql
        /// </summary>
        /// <param name="isFrameworkDb">For internal use only.</param>
        public MetaSql(bool isFrameworkDb, AppBuildTool appBuildTool)
        {
            MetaSqlDbContext dbContext = new MetaSqlDbContext(isFrameworkDb);
            string sql = UtilGenerate.FileLoad(ConnectionManager.SchemaFileName);
            this.List = dbContext.Schema.FromSql(sql).ToArray();
            //
            // For Application filter out "dbo.Framework" tables.
            if (isFrameworkDb == false)
            {
                this.List = this.List.Where(item => !(item.SchemaName.StartsWith("dbo") && item.TableName.StartsWith("Framework"))).ToArray();
                this.List = appBuildTool.GenerateFilter(this.List); // Custom table name filtering for code generation.
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
            optionsBuilder.UseSqlServer(ConnectionManagerServer.ConnectionString(IsFrameworkDb));
        }

        public DbSet<MetaSqlSchema> Schema { get; set; }
    }

    /// <summary>
    /// See also file Sql\Schema.sql
    /// </summary>
    public class MetaSqlSchema
    {
        [Key]
        public Guid IdView { get; set; }

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
