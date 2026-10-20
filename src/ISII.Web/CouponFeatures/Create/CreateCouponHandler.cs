using ISII.Web.Domain.CouponAggregate;
using ISII.Web.Domain.CouponAggregate.Specifications;

namespace ISII.Web.CouponFeatures.Create;

public record CreateCouponCommand(
  string Code,
  int DiscountPercentage,
  DateTime ExpirationDate,
  DateTime Now) : ICommand<Result<CouponDto>>;

public class CreateCouponHandler(IRepository<Coupon> repository)
  : ICommandHandler<CreateCouponCommand, Result<CouponDto>>
{
  public async ValueTask<Result<CouponDto>> Handle(CreateCouponCommand request, CancellationToken cancellationToken)
  {
    // a. Normalizar el código con Trim() y ToUpperInvariant()
    var normalizedCode = request.Code.Trim().ToUpperInvariant();

    // b. Buscar con CouponByCodeSpec(código normalizado)
    var spec = new CouponByCodeSpec(normalizedCode);
    var existingCoupon = await repository.FirstOrDefaultAsync(spec, cancellationToken);
    if (existingCoupon != null)
    {
      return Result<CouponDto>.Conflict($"Ya existe un cupón con el código {normalizedCode}.");
    }

    // c. Crear el cupón con Coupon.Create(...)
    Coupon coupon;
    try
    {
      coupon = Coupon.Create(
        CouponId.New,
        request.Code,
        request.DiscountPercentage,
        request.ExpirationDate,
        request.Now);
    }
    catch (ArgumentException ex)
    {
      return Result<CouponDto>.Invalid(new ValidationError
      {
        Identifier = ex.ParamName ?? string.Empty,
        ErrorMessage = ex.Message
      });
    }

    // d. Guardar con AddAsync y SaveChangesAsync
    await repository.AddAsync(coupon, cancellationToken);
    await repository.SaveChangesAsync(cancellationToken);

    // e. Devolver un CouponDto con los cinco datos del cupón guardado
    return new CouponDto(
      coupon.Id,
      coupon.Code,
      coupon.DiscountPercentage,
      coupon.ExpirationDate,
      coupon.IsActive);
  }
}
