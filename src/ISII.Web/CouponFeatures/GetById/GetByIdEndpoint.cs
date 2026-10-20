using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using ISII.Web.Domain.CouponAggregate;
using ISII.Web.Extensions;

namespace ISII.Web.CouponFeatures.GetById;

public sealed class GetCouponByIdRequest
{
  public const string Route = "/Coupons/{CouponId}";
  public int CouponId { get; init; }
}

public class GetByIdEndpoint(IMediator mediator)
  : Endpoint<GetCouponByIdRequest,
             Results<Ok<CouponRecord>,
                     NotFound,
                     ProblemHttpResult>,
             GetCouponByIdMapper>
{
  public override void Configure()
  {
    Get(GetCouponByIdRequest.Route);
    AllowAnonymous();

    Summary(s =>
    {
      s.Summary = "Obtener un cupón por ID";
      s.Description = "Obtiene un cupón a partir de su identificador. Devuelve el ID, el código, el porcentaje de descuento, la fecha de vencimiento y si está activo.";
      s.ExampleRequest = new GetCouponByIdRequest { CouponId = 1 };
      s.ResponseExamples[200] = new CouponRecord(1, "VERANO10", 20, new DateTime(2030, 12, 31), true);

      s.Responses[200] = "Cupón encontrado";
      s.Responses[404] = "No existe un cupón con el ID indicado";
    });

    Tags("Coupons");

    Description(builder => builder
      .Accepts<GetCouponByIdRequest>()
      .Produces<CouponRecord>(200, "application/json")
      .ProducesProblem(404));
  }

  public override async Task<Results<Ok<CouponRecord>, NotFound, ProblemHttpResult>>
    ExecuteAsync(GetCouponByIdRequest request, CancellationToken ct)
  {
    var result = await mediator.Send(new GetCouponQuery(CouponId.From(request.CouponId)), ct);

    return result.ToGetByIdResult(Map.FromEntity);
  }
}

public sealed class GetCouponByIdValidator : Validator<GetCouponByIdRequest>
{
  public GetCouponByIdValidator()
  {
    RuleFor(x => x.CouponId)
      .GreaterThan(0)
      .WithMessage("El ID del cupón debe ser mayor que 0");
  }
}

public sealed class GetCouponByIdMapper
  : Mapper<GetCouponByIdRequest, CouponRecord, CouponDto>
{
  public override CouponRecord FromEntity(CouponDto e)
    => new(e.Id.Value, e.Code, e.DiscountPercentage, e.ExpirationDate, e.IsActive);
}
