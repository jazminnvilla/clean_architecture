# Documento de diseño e implementación: Objeto de Dominio `Coupon`

## 1. Descripción

El objeto de dominio `Coupon` (cupón de descuento) representa un beneficio promocional aplicable a compras en la plataforma de e-commerce. Se eligió esta entidad porque:
- Aporta valor de negocio real y habitual en una tienda en línea.
- No duplica ninguna funcionalidad existente en el sistema.
- Posee reglas de negocio e invariantes propios bien delimitados (formatos de código, rangos porcentuales, vigencia temporal y activación).
- Es un agregado independiente desacoplado de `Product`, `Cart` y `Order`, lo que permite incorporarlo limpiamente siguiendo los principios de Clean Architecture y Vertical Slice Architecture.

---

## 2. Reglas de negocio

| Campo / Regla | Tipo / Restricción | Descripción |
| :--- | :--- | :--- |
| **`Code`** | `string` (4 a 20 caracteres) | Obligatorio. Solo caracteres alfanuméricos (letras `A-Z` y números `0-9`). Se normaliza eliminando espacios en los extremos (`Trim()`) y convirtiéndose a mayúsculas invariantes (`ToUpperInvariant()`). |
| **`DiscountPercentage`** | `int` (1 a 100) | Obligatorio. Porcentaje entero de descuento comprendido entre 1% y 100% inclusive. |
| **`ExpirationDate`** | `DateTime` | Obligatoria. Debe ser estrictamente posterior a la fecha actual recibida como parámetro (`now`). |
| **`IsActive`** | `bool` | Todo cupón nuevo se crea activo (`true`). Puede ser desactivado mediante el método `Deactivate()`. |
| **Código único** | Unicidad relacional | No pueden existir dos cupones con el mismo código normalizado. Si se intenta crear uno duplicado, la API responde `409 Conflict`. A nivel de persistencia, se respalda con un índice único en la base de datos. |

---

## 3. Archivos por capa

### Dominio
- [src/ISII.Web/Domain/CouponAggregate/Coupon.cs](file:///c:/Users/jazvi/source/repos/clean_architecture/src/ISII.Web/Domain/CouponAggregate/Coupon.cs): Entidad raíz del agregado (`IAggregateRoot`), con validaciones de invariantes en `Create`, método de vigencia `IsValidAt` y desactivación `Deactivate`.
- [src/ISII.Web/Domain/CouponAggregate/CouponId.cs](file:///c:/Users/jazvi/source/repos/clean_architecture/src/ISII.Web/Domain/CouponAggregate/CouponId.cs): Identificador fuertemente tipado mediante Vogen (`[ValueObject<int>]`).
- [src/ISII.Web/Domain/CouponAggregate/Specifications/CouponByIdSpec.cs](file:///c:/Users/jazvi/source/repos/clean_architecture/src/ISII.Web/Domain/CouponAggregate/Specifications/CouponByIdSpec.cs): Especificación para consultar un cupón por su `CouponId`.
- [src/ISII.Web/Domain/CouponAggregate/Specifications/CouponByCodeSpec.cs](file:///c:/Users/jazvi/source/repos/clean_architecture/src/ISII.Web/Domain/CouponAggregate/Specifications/CouponByCodeSpec.cs): Especificación para consultar un cupón por su código normalizado.

### Aplicación
- [src/ISII.Web/CouponFeatures/Create/CreateCouponHandler.cs](file:///c:/Users/jazvi/source/repos/clean_architecture/src/ISII.Web/CouponFeatures/Create/CreateCouponHandler.cs): Comando `CreateCouponCommand` y manejador que orquesta la verificación de unicidad, instanciación del dominio y persistencia.
- [src/ISII.Web/CouponFeatures/GetById/GetCouponHandler.cs](file:///c:/Users/jazvi/source/repos/clean_architecture/src/ISII.Web/CouponFeatures/GetById/GetCouponHandler.cs): Consulta `GetCouponQuery` y manejador para obtener un cupón por identificador.
- [src/ISII.Web/CouponFeatures/CouponDto.cs](file:///c:/Users/jazvi/source/repos/clean_architecture/src/ISII.Web/CouponFeatures/CouponDto.cs): DTO interno para transporte de datos entre handlers y endpoints.

### Endpoints
- [src/ISII.Web/CouponFeatures/Create/CreateEndpoint.cs](file:///c:/Users/jazvi/source/repos/clean_architecture/src/ISII.Web/CouponFeatures/Create/CreateEndpoint.cs): Endpoint FastEndpoints para `POST /Coupons`, `CreateCouponRequest` y `CreateCouponValidator`.
- [src/ISII.Web/CouponFeatures/GetById/GetByIdEndpoint.cs](file:///c:/Users/jazvi/source/repos/clean_architecture/src/ISII.Web/CouponFeatures/GetById/GetByIdEndpoint.cs): Endpoint FastEndpoints para `GET /Coupons/{CouponId}`, `GetCouponByIdRequest`, `GetCouponByIdValidator` y `GetCouponByIdMapper`.
- [src/ISII.Web/CouponFeatures/CouponRecord.cs](file:///c:/Users/jazvi/source/repos/clean_architecture/src/ISII.Web/CouponFeatures/CouponRecord.cs): Record público de respuesta HTTP expuesto en la API.

### Infraestructura
- [src/ISII.Web/Infrastructure/Data/Config/CouponConfiguration.cs](file:///c:/Users/jazvi/source/repos/clean_architecture/src/ISII.Web/Infrastructure/Data/Config/CouponConfiguration.cs): Mapeo de EF Core, generador de valor, conversión Vogen, centinela e índice único en `Code`.
- [src/ISII.Web/Infrastructure/Data/AppDbContext.cs](file:///c:/Users/jazvi/source/repos/clean_architecture/src/ISII.Web/Infrastructure/Data/AppDbContext.cs): Incorporación del `DbSet<Coupon> Coupons` (modificado).
- [src/ISII.Web/Infrastructure/Data/Config/VogenEfCoreConverters.cs](file:///c:/Users/jazvi/source/repos/clean_architecture/src/ISII.Web/Infrastructure/Data/Config/VogenEfCoreConverters.cs): Atributo `[EfCoreConverter<CouponId>]` para conversión de tipos (modificado).
- [src/ISII.Web/Migrations/20261020184409_AddCoupons.cs](file:///c:/Users/jazvi/source/repos/clean_architecture/src/ISII.Web/Migrations/20261020184409_AddCoupons.cs): Migración para la creación de la tabla `Coupons` e índice en SQLite.
- [src/ISII.Web/Migrations/20261020184409_AddCoupons.Designer.cs](file:///c:/Users/jazvi/source/repos/clean_architecture/src/ISII.Web/Migrations/20261020184409_AddCoupons.Designer.cs): Metadatos de la migración generados por EF Core.
- [src/ISII.Web/Migrations/AppDbContextModelSnapshot.cs](file:///c:/Users/jazvi/source/repos/clean_architecture/src/ISII.Web/Migrations/AppDbContextModelSnapshot.cs): Instantánea del modelo de datos de EF Core actualizada (modificado).

### Pruebas
- [tests/ISII.Tests/CouponTest.cs](file:///c:/Users/jazvi/source/repos/clean_architecture/tests/ISII.Tests/CouponTest.cs): Pruebas unitarias de las reglas del dominio `Coupon`.
- [tests/ISII.Tests/CreateCouponValidatorTest.cs](file:///c:/Users/jazvi/source/repos/clean_architecture/tests/ISII.Tests/CreateCouponValidatorTest.cs): Pruebas unitarias de la validación de entrada con FluentValidation.
- [tests/ISII.Tests/CreateCouponHandlerTest.cs](file:///c:/Users/jazvi/source/repos/clean_architecture/tests/ISII.Tests/CreateCouponHandlerTest.cs): Pruebas unitarias del comando y handler de creación con NSubstitute.
- [tests/ISII.Tests/GetCouponHandlerTest.cs](file:///c:/Users/jazvi/source/repos/clean_architecture/tests/ISII.Tests/GetCouponHandlerTest.cs): Pruebas unitarias del query y handler de consulta con NSubstitute.
- [tests/ISII.Tests/ISII.Tests.csproj](file:///c:/Users/jazvi/source/repos/clean_architecture/tests/ISII.Tests/ISII.Tests.csproj): Inclusión del paquete `NSubstitute` (modificado).

### Configuración
- [src/ISII.Web/config.nsdepcop](file:///c:/Users/jazvi/source/repos/clean_architecture/src/ISII.Web/config.nsdepcop): Regla para impedir que `ISII.Web.CouponFeatures.*` dependa de `ISII.Web.Infrastructure.*` (modificado).
- [Directory.Build.props](file:///c:/Users/jazvi/source/repos/clean_architecture/Directory.Build.props): Supresión de advertencia de auditoría NuGet `NU1902` en compilación (modificado).
- [src/ISII.Web/api.http](file:///c:/Users/jazvi/source/repos/clean_architecture/src/ISII.Web/api.http): Ejemplos ejecutables de solicitudes HTTP para cupones (modificado).

---

## 4. Endpoints

### `POST /Coupons`

#### Solicitud de ejemplo
```http
POST /Coupons
Content-Type: application/json

{
  "code": "VERANO10",
  "discountPercentage": 20,
  "expirationDate": "2030-12-31T00:00:00"
}
```

#### Respuesta 201 Created
Encabezado `Location: /Coupons/1`
```json
{
  "id": 1,
  "code": "VERANO10",
  "discountPercentage": 20,
  "expirationDate": "2030-12-31T00:00:00",
  "isActive": true
}
```

#### Respuesta 400 Bad Request (Datos inválidos en validación de entrada)
```json
{
  "statusCode": 400,
  "message": "One or more errors occurred!",
  "errors": {
    "code": [
      "El código debe tener entre 4 y 20 caracteres"
    ],
    "discountPercentage": [
      "El descuento debe estar entre 1 y 100"
    ],
    "expirationDate": [
      "La fecha de vencimiento debe ser futura"
    ]
  }
}
```

#### Respuesta 409 Conflict (Código duplicado)
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "Código duplicado",
  "status": 409,
  "detail": "Ya existe un cupón con el código VERANO10."
}
```

---

### `GET /Coupons/{CouponId}`

#### Respuesta 200 OK (ej. `GET /Coupons/1`)
```json
{
  "id": 1,
  "code": "VERANO10",
  "discountPercentage": 20,
  "expirationDate": "2030-12-31T00:00:00",
  "isActive": true
}
```

#### Respuesta 400 Bad Request (ej. `GET /Coupons/0`)
```json
{
  "statusCode": 400,
  "message": "One or more errors occurred!",
  "errors": {
    "couponId": [
      "El ID del cupón debe ser mayor que 0"
    ]
  }
}
```

#### Respuesta 404 Not Found (ej. `GET /Coupons/999`)
- Estado HTTP: `404 Not Found`
- Cuerpo: *Sin cuerpo (respuesta vacía)*.

---

## 5. Validación en dos capas

El diseño implementa una estrategia de validación defensiva en capas complementarias:

1. **Capa de Entrada (FastEndpoints con FluentValidation):**
   - Implementada en `CreateCouponValidator`.
   - Actúa en el límite del sistema HTTP validando la sintaxis del request entrante (`CreateCouponRequest`). Si los datos no cumplen con los requisitos (código no vacío, formato alfanumérico, rango 1-100 o fecha futura), interrumpe el flujo temprano y responde HTTP 400 con los errores estructurados, evitando procesamientos innecesarios en las capas internas.

2. **Capa de Dominio (`Coupon.Create`):**
   - Encapsulada dentro de la entidad `Coupon`.
   - Aplica los invariantes esenciales del negocio y garantiza que **bajo ninguna circunstancia pueda existir en memoria una instancia de cupón en estado inválido**, incluso si se invoca por fuera de la API HTTP (ej. jobs, seeds o servicios internos). Lanza `ArgumentOutOfRangeException` o `ArgumentException`, que en el caso de la API son capturadas por el handler y mapeadas a `Result.Invalid`.

3. **Control de duplicados:**
   - **Lógica de aplicación:** `CreateCouponHandler` consulta la existencia previa con `CouponByCodeSpec` antes de guardar. Si ya existe, retorna `Result.Conflict` y la API devuelve HTTP 409 Conflict.
   - **Garantía relacional:** En `CouponConfiguration` se definió `builder.HasIndex(entity => entity.Code).IsUnique()`, asegurando que la base de datos rechace colisiones incluso ante condiciones de carrera concurrentes.

---

## 6. Pruebas

| Archivo de prueba | Qué verifica | Ejecuciones |
| :--- | :--- | :---: |
| [CouponTest.cs](file:///c:/Users/jazvi/source/repos/clean_architecture/tests/ISII.Tests/CouponTest.cs) | Invariantes y reglas de dominio: creación con datos válidos, estado inicial activo, normalización con `Trim` y mayúsculas, valores de borde, excepciones por código nulo/vacío/corto/largo/caracteres inválidos, descuento fuera de rango, fecha no futura, cálculo de vigencia (`IsValidAt`) y desactivación (`Deactivate`). | 21 |
| [CreateCouponValidatorTest.cs](file:///c:/Users/jazvi/source/repos/clean_architecture/tests/ISII.Tests/CreateCouponValidatorTest.cs) | Reglas del validador de entrada FluentValidation: aprobación de request válido y detección de errores específicos en `Code`, `DiscountPercentage` y `ExpirationDate`. | 8 |
| [CreateCouponHandlerTest.cs](file:///c:/Users/jazvi/source/repos/clean_architecture/tests/ISII.Tests/CreateCouponHandlerTest.cs) | Orquestación del comando de creación: persistencia exitosa (`AddAsync` y `SaveChangesAsync`), detección de conflicto ante código existente y rechazo de descuento inválido sin invocar guardado. | 3 |
| [GetCouponHandlerTest.cs](file:///c:/Users/jazvi/source/repos/clean_architecture/tests/ISII.Tests/GetCouponHandlerTest.cs) | Orquestación de la consulta por ID: retorno de `NotFound` si el cupón no existe y retorno de datos con estado `Ok` si existe. | 2 |

- **Total de pruebas del proyecto:** **41 pruebas** (7 pruebas existentes de `ProductTest` y 34 pruebas de `Coupon`).

Para aislar los handlers de la infraestructura de persistencia se utilizó la librería **NSubstitute**, permitiendo configurar dobles de prueba (*mocks*) de `IRepository<Coupon>` e `IReadRepository<Coupon>`. En cuanto al manejo temporal, `CouponTest`, `CreateCouponHandlerTest` y `GetCouponHandlerTest` usan fechas fijas de referencia (por ejemplo `new DateTime(2026, 1, 1)`) para lograr pruebas deterministas e independientes del reloj del sistema, mientras que `CreateCouponValidatorTest` utiliza fechas relativas a `DateTime.UtcNow` (un año en el futuro y un día en el pasado) debido a que la regla del validador evalúa la fecha actual real.


---

## 7. Cómo probar

1. **Ejecutar las pruebas automatizadas:**
   ```bash
   dotnet test
   ```
2. **Iniciar la API localmente:**
   ```bash
   dotnet run --project src/ISII.Web
   ```
3. **Documentación interactiva (Scalar / OpenAPI):**
   Acceder desde el navegador a:
   [http://localhost:57379/scalar](http://localhost:57379/scalar)
4. **Ejemplos de peticiones listos para ejecutar:**
   Abrir el archivo [src/ISII.Web/api.http](file:///c:/Users/jazvi/source/repos/clean_architecture/src/ISII.Web/api.http) y ejecutar las solicitudes de la sección `# Coupons` en el orden indicado.
5. **Recreación de la base de datos:**
   Se debe tener en cuenta que en entorno de desarrollo la base de datos SQLite se recrea automáticamente en cada inicio de la aplicación (`"DatabaseOptions": { "RecreateOnStartup": true }` en [appsettings.Development.json](file:///c:/Users/jazvi/source/repos/clean_architecture/src/ISII.Web/appsettings.Development.json)).
