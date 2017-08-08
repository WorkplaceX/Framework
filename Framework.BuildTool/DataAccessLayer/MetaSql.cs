namespace Framework.BuildTool.DataAccessLayer
{
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
        /// <param name="isFramework">For internal use only.</param>
        public MetaSql(bool isFramework)
        {
            MetaSqlDbContext dbContext = new MetaSqlDbContext();
            string sql = Util.FileLoad(ConnectionManager.SchemaFileName);
            this.List = dbContext.Schema.FromSql(sql).ToArray();
            //
            // Filter out "dbo.Framework" tables.
            if (isFramework == false)
            {
                this.List = this.List.Where(item => !(item.SchemaName.StartsWith("dbo") && item.TableName.StartsWith("Framework"))).ToArray();
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
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(Framework.Server.ConnectionManager.ConnectionString);
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
