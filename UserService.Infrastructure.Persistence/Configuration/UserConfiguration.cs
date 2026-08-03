using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Persistence.Configuration
{
    internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable(nameof(User));

            builder.HasKey(user => user.Id);

            builder.Property(user => user.Id)
                   .ValueGeneratedOnAdd();

            builder.Property(user => user.Name)
                   .HasMaxLength(60)
                   .IsRequired();

            // Email — ValueObject
            builder.OwnsOne(user => user.Email, email =>
            {
                email.Property(e => e.Value)
                     .HasColumnName("Email")
                     .HasMaxLength(100)
                     .IsRequired();
            });

            builder.Property(user => user.Password)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(user => user.EmailConfirmed)
                   .IsRequired();

            builder.Property(user => user.IsActive)
                   .IsRequired();

            // Enum → string
            builder.Property(user => user.Role)
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .IsRequired();
        }
    }
}
