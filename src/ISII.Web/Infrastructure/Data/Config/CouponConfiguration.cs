using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ISII.Web.Domain.CouponAggregate;

namespace ISII.Web.Infrastructure.Data.Config;

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
  public void Configure(EntityTypeBuilder<Coupon> builder)
  {
    builder.Property(entity => entity.Id)
      .HasValueGenerator<VogenIntIdValueGenerator<AppDbContext, Coupon, CouponId>>()
      .HasVogenConversion()
      .IsRequired();

    builder.Property(entity => entity.Code)
      .HasMaxLength(Coupon.CodeMaxLength)
      .IsRequired();

    builder.Property(entity => entity.DiscountPercentage)
      .IsRequired();

    builder.Property(entity => entity.ExpirationDate)
      .IsRequired();

    builder.Property(entity => entity.IsActive)
      .IsRequired();

    builder.HasIndex(entity => entity.Code).IsUnique();
  }
}
