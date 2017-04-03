namespace Framework.Build.DataAccessLayer
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
        public MetaSql()
        {
            MetaSqlDbContext dbContext = new MetaSqlDbContext();
            string sql = Util.FileLoad(ConnectionManager.SchemaFileName);
            this.List = dbContext.Schema.FromSql(sql).ToArray();
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

        public int FieldNameOrderBy { get; set; }

        public bool IsView { get; set; }

        public byte SqlType { get; set; }

        public bool IsNullable { get; set; }

        public bool IsPrimaryKey { get; set; }
    }
}
