using NSubstitute;
using Ardalis.Result;
using Ardalis.SharedKernel;
using ISII.Web.CouponFeatures.GetById;
using ISII.Web.Domain.CouponAggregate;
using ISII.Web.Domain.CouponAggregate.Specifications;

namespace ISII.Tests;

public class GetCouponHandlerTest
{
    private static readonly DateTime Now = new DateTime(2026, 1, 1);
    private static readonly DateTime Expiration = Now.AddDays(30);

    [Fact]
    public async Task Handle_WhenCouponDoesNotExist_ShouldReturnNotFound()
    {
        var repository = Substitute.For<IReadRepository<Coupon>>();
        repository.FirstOrDefaultAsync(Arg.Any<CouponByIdSpec>(), Arg.Any<CancellationToken>())
            .Returns((Coupon?)null);
        var handler = new GetCouponHandler(repository);
        var query = new GetCouponQuery(CouponId.From(1));

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Equal(ResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task Handle_WhenCouponExists_ShouldReturnCouponData()
    {
        var coupon = Coupon.Create(CouponId.From(1), "VERANO10", 20, Expiration, Now);
        var repository = Substitute.For<IReadRepository<Coupon>>();
        repository.FirstOrDefaultAsync(Arg.Any<CouponByIdSpec>(), Arg.Any<CancellationToken>())
            .Returns(coupon);
        var handler = new GetCouponHandler(repository);
        var query = new GetCouponQuery(CouponId.From(1));

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Equal(ResultStatus.Ok, result.Status);
        Assert.Equal("VERANO10", result.Value.Code);
        Assert.Equal(20, result.Value.DiscountPercentage);
    }
}
