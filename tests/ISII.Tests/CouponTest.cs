using ISII.Web.Domain.CouponAggregate;

namespace ISII.Tests;

public class CouponTest
{
    private static readonly DateTime Now = new DateTime(2026, 1, 1);
    private static readonly DateTime ValidExpiration = Now.AddDays(30);
    private static readonly CouponId ValidId = CouponId.From(1);

    [Fact]
    public void Create_WithValidData_ShouldAssignValues()
    {
        const string code = "VERANO10";
        const int discount = 20;

        var coupon = Coupon.Create(ValidId, code, discount, ValidExpiration, Now);

        Assert.Equal(ValidId, coupon.Id);
        Assert.Equal(code, coupon.Code);
        Assert.Equal(discount, coupon.DiscountPercentage);
        Assert.Equal(ValidExpiration, coupon.ExpirationDate);
    }

    [Fact]
    public void Create_ShouldCreateActiveCoupon()
    {
        var coupon = Coupon.Create(ValidId, "VERANO10", 20, ValidExpiration, Now);

        Assert.True(coupon.IsActive);
    }

    [Fact]
    public void Create_ShouldNormalizeCode()
    {
        const string inputCode = "  verano10 ";

        var coupon = Coupon.Create(ValidId, inputCode, 20, ValidExpiration, Now);

        Assert.Equal("VERANO10", coupon.Code);
    }

    [Theory]
    [InlineData("ABCD", 1)]
    [InlineData("ABCDEFGHIJKLMNOPQRST", 100)]
    public void Create_WithBoundaryValues_ShouldSucceed(string code, int discount)
    {
        var coupon = Coupon.Create(ValidId, code, discount, ValidExpiration, Now);

        Assert.Equal(code, coupon.Code);
        Assert.Equal(discount, coupon.DiscountPercentage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyCode_ShouldThrow(string? code)
    {
        Assert.Throws<ArgumentException>(() => Coupon.Create(ValidId, code!, 20, ValidExpiration, Now));
    }

    [Fact]
    public void Create_WithCodeTooShort_ShouldThrow()
    {
        const string code = "ABC";

        Assert.Throws<ArgumentException>(() => Coupon.Create(ValidId, code, 20, ValidExpiration, Now));
    }

    [Fact]
    public void Create_WithCodeTooLong_ShouldThrow()
    {
        const string code = "ABCDEFGHIJKLMNOPQRSTU";

        Assert.Throws<ArgumentException>(() => Coupon.Create(ValidId, code, 20, ValidExpiration, Now));
    }

    [Theory]
    [InlineData("VERANO-10")]
    [InlineData("VER ANO")]
    public void Create_WithInvalidCharacters_ShouldThrow(string code)
    {
        Assert.Throws<ArgumentException>(() => Coupon.Create(ValidId, code, 20, ValidExpiration, Now));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    [InlineData(-5)]
    public void Create_WithDiscountOutOfRange_ShouldThrow(int discount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Coupon.Create(ValidId, "VERANO10", discount, ValidExpiration, Now));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithExpirationNotInFuture_ShouldThrow(int days)
    {
        var expiration = Now.AddDays(days);

        Assert.Throws<ArgumentException>(() => Coupon.Create(ValidId, "VERANO10", 20, expiration, Now));
    }

    [Fact]
    public void IsValidAt_BeforeExpiration_ShouldReturnTrue()
    {
        var coupon = Coupon.Create(ValidId, "VERANO10", 20, ValidExpiration, Now);

        var result = coupon.IsValidAt(Now.AddDays(10));

        Assert.True(result);
    }

    [Theory]
    [InlineData(30)]
    [InlineData(31)]
    public void IsValidAt_AtOrAfterExpiration_ShouldReturnFalse(int days)
    {
        var coupon = Coupon.Create(ValidId, "VERANO10", 20, ValidExpiration, Now);

        var result = coupon.IsValidAt(Now.AddDays(days));

        Assert.False(result);
    }

    [Fact]
    public void Deactivate_ShouldMakeCouponInactiveAndInvalid()
    {
        var coupon = Coupon.Create(ValidId, "VERANO10", 20, ValidExpiration, Now);

        coupon.Deactivate();

        Assert.False(coupon.IsActive);
        Assert.False(coupon.IsValidAt(Now.AddDays(10)));
    }
}
