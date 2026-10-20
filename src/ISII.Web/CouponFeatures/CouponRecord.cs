namespace ISII.Web.CouponFeatures;

public record CouponRecord(int Id, string Code, int DiscountPercentage, DateTime ExpirationDate, bool IsActive);
