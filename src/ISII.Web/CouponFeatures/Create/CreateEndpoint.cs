using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using ISII.Web.CouponFeatures;
using ISII.Web.Domain.CouponAggregate;

namespace ISII.Web.CouponFeatures.Create;

public sealed class CreateCouponRequest
{
  public string Code { get; init; } = string.Empty;
  public int DiscountPercentage { get; init; }
  public DateTime ExpirationDate { get; init; }
}

public class CreateEndpoint(IMediator mediator)
  : Endpoint<CreateCouponRequest,
             Results<Created<CouponRecord>, ValidationProblem, ProblemHttpResult>>
{
  public override void Configure()
  {
    Post("/Coupons");
    AllowAnonymous();
    Tags("Coupons");

    Summary(s =>
    {
      s.Summary = "Crear un cupón";
      s.Description = "Crea un cupón de descuento. El código debe ser único, tener entre 4 y 20 letras o números, el descuento debe estar entre 1 y 100 y el vencimiento debe ser futuro.";
      s.ExampleRequest = new CreateCouponRequest
      {
        Code = "VERANO10",
        DiscountPercentage = 20,
        ExpirationDate = new DateTime(2030, 12, 31)
      };
      s.ResponseExamples[201] = new CouponRecord(1, "VERANO10", 20, new DateTime(2030, 12, 31), true);

      s.Responses[201] = "Cupón creado";
      s.Responses[400] = "Datos inválidos";
      s.Responses[409] = "Ya existe un cupón con ese código";
    });

    Description(builder => builder
      .Accepts<CreateCouponRequest>()
      .Produces<CouponRecord>(201, "application/json")
      .ProducesProblem(400)
      .ProducesProblem(409));
  }

  public override async Task<Results<Created<CouponRecord>, ValidationProblem, ProblemHttpResult>>
    ExecuteAsync(CreateCouponRequest request, CancellationToken ct)
  {
    var command = new CreateCouponCommand(
      request.Code,
      request.DiscountPercentage,
      request.ExpirationDate,
      DateTime.UtcNow);

    var result = await mediator.Send(command, ct);

    return result.Status switch
    {
      ResultStatus.Ok => TypedResults.Created(
        $"/Coupons/{result.Value.Id.Value}",
        new CouponRecord(
          result.Value.Id.Value,
          result.Value.Code,
          result.Value.DiscountPercentage,
          result.Value.ExpirationDate,
          result.Value.IsActive)),

      ResultStatus.Conflict => TypedResults.Problem(
        title: "Código duplicado",
        detail: string.Join("; ", result.Errors),
        statusCode: StatusCodes.Status409Conflict),

      ResultStatus.Invalid => TypedResults.ValidationProblem(
        result.ValidationErrors
          .GroupBy(e => e.Identifier ?? string.Empty)
          .ToDictionary(
            g => g.Key,
            g => g.Select(e => e.ErrorMessage).ToArray())),

      _ => TypedResults.Problem(
        title: "No se pudo crear el cupón",
        detail: string.Join("; ", result.Errors),
        statusCode: StatusCodes.Status400BadRequest)
    };
  }
}

public sealed class CreateCouponValidator : Validator<CreateCouponRequest>
{
  public CreateCouponValidator()
  {
    RuleFor(x => x.Code)
      .NotEmpty()
      .WithMessage("El código es obligatorio")
      .Length(Coupon.CodeMinLength, Coupon.CodeMaxLength)
      .WithMessage("El código debe tener entre 4 y 20 caracteres")
      .Matches("^[A-Za-z0-9]+$")
      .WithMessage("El código solo puede contener letras y números");

    RuleFor(x => x.DiscountPercentage)
      .InclusiveBetween(Coupon.MinDiscount, Coupon.MaxDiscount)
      .WithMessage("El descuento debe estar entre 1 y 100");

    RuleFor(x => x.ExpirationDate)
      .GreaterThan(_ => DateTime.UtcNow)
      .WithMessage("La fecha de vencimiento debe ser futura");
  }
}
