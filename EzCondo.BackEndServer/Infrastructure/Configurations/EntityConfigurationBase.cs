using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text;

namespace EzCondo.BackEndServer.Infrastructure.Configurations
{
	internal abstract class EntityConfigurationBase<TEntity>(string table, string schema)
		: IEntityTypeConfiguration<TEntity> where TEntity : class
	{
		public void Configure(EntityTypeBuilder<TEntity> builder)
		{
			builder.ToTable(table, schema);
			
			ConfigureEntity(builder);

			foreach (var property in builder.Metadata.GetProperties())
			{
				property.SetColumnName(ToSnakeCase(property.Name));
			}
		}

		protected abstract void ConfigureEntity(EntityTypeBuilder<TEntity> builder);

		protected static void ConfigureUuidPrimaryKey(EntityTypeBuilder<TEntity> builder)
		{
			builder.HasKey("Id");
			builder.Property("Id")
				.HasDefaultValueSql("gen_random_uuid()");
		}

		protected static void ConfigureCreatedAt(EntityTypeBuilder<TEntity> builder) =>
			builder.Property("CreatedAt").HasDefaultValueSql("now()").IsRequired();

		protected static void ConfigureUpdatedAt(EntityTypeBuilder<TEntity> builder) =>
			builder.Property("UpdatedAt").HasDefaultValueSql("now()");

		protected static void ConfigureIsDeleted(EntityTypeBuilder<TEntity> builder) =>
			builder.Property("IsDeleted").HasDefaultValue(false).IsRequired();

		protected static void ConfigureDeletedAt(EntityTypeBuilder<TEntity> builder) =>
			builder.Property("DeletedAt");

		private static string ToSnakeCase(string value)
		{
			var result = new StringBuilder(value.Length + 8);

			for(var i = 0; i < value.Length; i++)
			{
				var current = value[i];

				if(char.IsUpper(current) && i > 0 && 
					(!char.IsUpper(value[i-1]) || (i + 1 < value.Length && char.IsLower(value[i + 1]))))
				{
					result.Append('_');
				}

				result.Append(char.ToLowerInvariant(current));
			}

			return result.ToString();
		}
	}
}
