using ISII.Web.CouponFeatures.Create;

namespace ISII.Tests;

public class CreateCouponValidatorTest
{
    private static readonly DateTime ValidExpiration = DateTime.UtcNow.AddYears(1);
    private static readonly DateTime PastExpiration = DateTime.UtcNow.AddDays(-1);

    [Fact]
    public void Validate_WithValidRequest_ShouldBeValid()
    {
        var request = new CreateCouponRequest
        {
            Code = "VERANO10",
            DiscountPercentage = 20,
            ExpirationDate = ValidExpiration
        };

        var result = new CreateCouponValidator().Validate(request);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("ABC")]
    [InlineData("ABCDEFGHIJKLMNOPQRSTU")]
    [InlineData("VERANO-10")]
    public void Validate_WithInvalidCode_ShouldHaveCodeError(string code)
    {
        var request = new CreateCouponRequest
        {
            Code = code,
            DiscountPercentage = 20,
            ExpirationDate = ValidExpiration
        };

        var result = new CreateCouponValidator().Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Code");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Validate_WithDiscountOutOfRange_ShouldHaveDiscountError(int discount)
    {
        var request = new CreateCouponRequest
        {
            Code = "VERANO10",
            DiscountPercentage = discount,
            ExpirationDate = ValidExpiration
        };

        var result = new CreateCouponValidator().Validate(request);

        Assert.Contains(result.Errors, e => e.PropertyName == "DiscountPercentage");
    }

    [Fact]
    public void Validate_WithPastExpiration_ShouldHaveExpirationError()
    {
        var request = new CreateCouponRequest
        {
            Code = "VERANO10",
            DiscountPercentage = 20,
            ExpirationDate = PastExpiration
        };

        var result = new CreateCouponValidator().Validate(request);

        Assert.Contains(result.Errors, e => e.PropertyName == "ExpirationDate");
    }
}
