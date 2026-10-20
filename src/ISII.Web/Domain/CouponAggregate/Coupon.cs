namespace ISII.Web.Domain.CouponAggregate;

public class Coupon : EntityBase<Coupon, CouponId>, IAggregateRoot
{
  public const int CodeMinLength = 4;
  public const int CodeMaxLength = 20;
  public const int MinDiscount = 1;
  public const int MaxDiscount = 100;

  // Constructor privado para EF Core
  private Coupon() { }

  // Constructor privado con asignación inicial
  private Coupon(CouponId id, string code, int discountPercentage, DateTime expirationDate)
  {
    Id = id;
    Code = code;
    DiscountPercentage = discountPercentage;
    ExpirationDate = expirationDate;
    IsActive = true;
  }

  public string Code { get; private set; } = string.Empty;
  public int DiscountPercentage { get; private set; }
  public DateTime ExpirationDate { get; private set; }
  public bool IsActive { get; private set; }

  public static Coupon Create(
    CouponId id,
    string code,
    int discountPercentage,
    DateTime expirationDate,
    DateTime now)
  {
    // a. Code no puede ser nulo ni vacío
    if (string.IsNullOrWhiteSpace(code))
    {
      throw new ArgumentException("El código no puede ser nulo ni vacío.", nameof(code));
    }

    // b. Code se normaliza: Trim() y ToUpperInvariant()
    var normalizedCode = code.Trim().ToUpperInvariant();

    // c. Code normalizado debe tener entre 4 y 20 caracteres
    if (normalizedCode.Length < CodeMinLength || normalizedCode.Length > CodeMaxLength)
    {
      throw new ArgumentException($"El código debe tener entre {CodeMinLength} y {CodeMaxLength} caracteres.", nameof(code));
    }

    // d. Code normalizado solo puede tener letras A-Z y números 0-9
    if (!normalizedCode.All(c => (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9')))
    {
      throw new ArgumentException("El código solo puede contener letras y números.", nameof(code));
    }

    // e. DiscountPercentage debe estar entre 1 y 100
    if (discountPercentage < MinDiscount || discountPercentage > MaxDiscount)
    {
      throw new ArgumentOutOfRangeException(
        nameof(discountPercentage),
        $"El porcentaje de descuento debe estar entre {MinDiscount} y {MaxDiscount}.");
    }

    // f. ExpirationDate debe ser mayor que now
    if (expirationDate <= now)
    {
      throw new ArgumentException("La fecha de expiración debe ser posterior a la fecha actual.", nameof(expirationDate));
    }

    return new Coupon(id, normalizedCode, discountPercentage, expirationDate);
  }

  public bool IsValidAt(DateTime now) => IsActive && now < ExpirationDate;

  public void Deactivate()
  {
    IsActive = false;
  }
}
