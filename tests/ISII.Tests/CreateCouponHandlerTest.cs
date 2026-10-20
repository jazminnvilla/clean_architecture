using NSubstitute;
using Ardalis.Result;
using Ardalis.SharedKernel;
using ISII.Web.CouponFeatures.Create;
using ISII.Web.Domain.CouponAggregate;
using ISII.Web.Domain.CouponAggregate.Specifications;

namespace ISII.Tests;

public class CreateCouponHandlerTest
{
    private static readonly DateTime Now = new DateTime(2026, 1, 1);
    private static readonly DateTime Expiration = Now.AddDays(30);

    [Fact]
    public async Task Handle_WithNewCode_ShouldSaveCouponAndReturnOk()
    {
        var repository = Substitute.For<IRepository<Coupon>>();
        repository.FirstOrDefaultAsync(Arg.Any<CouponByCodeSpec>(), Arg.Any<CancellationToken>())
            .Returns((Coupon?)null);
        var handler = new CreateCouponHandler(repository);
        var command = new CreateCouponCommand("verano10", 20, Expiration, Now);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal(ResultStatus.Ok, result.Status);
        Assert.Equal("VERANO10", result.Value.Code);
        Assert.Equal(20, result.Value.DiscountPercentage);
        await repository.Received(1).AddAsync(Arg.Any<Coupon>(), Arg.Any<CancellationToken>());
        await repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithExistingCode_ShouldReturnConflict()
    {
        var existingCoupon = Coupon.Create(CouponId.From(1), "VERANO10", 20, Expiration, Now);
        var repository = Substitute.For<IRepository<Coupon>>();
        repository.FirstOrDefaultAsync(Arg.Any<CouponByCodeSpec>(), Arg.Any<CancellationToken>())
            .Returns(existingCoupon);
        var handler = new CreateCouponHandler(repository);
        var command = new CreateCouponCommand("VERANO10", 20, Expiration, Now);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal(ResultStatus.Conflict, result.Status);
        await repository.DidNotReceive().AddAsync(Arg.Any<Coupon>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidDiscount_ShouldReturnInvalid()
    {
        var repository = Substitute.For<IRepository<Coupon>>();
        repository.FirstOrDefaultAsync(Arg.Any<CouponByCodeSpec>(), Arg.Any<CancellationToken>())
            .Returns((Coupon?)null);
        var handler = new CreateCouponHandler(repository);
        var command = new CreateCouponCommand("VERANO10", 150, Expiration, Now);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal(ResultStatus.Invalid, result.Status);
        await repository.DidNotReceive().AddAsync(Arg.Any<Coupon>(), Arg.Any<CancellationToken>());
    }
}
