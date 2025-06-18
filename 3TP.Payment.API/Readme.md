##  Documentacion de proyecto ApiRest para procesar pagos con NMI


# 💳 3TP.Payment.SDK
Este SDK permite procesar pagos de forma segura a través del procesador de pagos
**NMI (Network Merchants Inc.)** con validaciones avanzadas y registro de logs de solicitudes y respuestas.
Es una solución completa y escalable para procesar pagos con NMI Gateway en aplicaciones .NET 8,
utilizando principios de Clean Architecture, DDD y buenas prácticas del sector financiero.

---
## 🚀 Características

- Soporte para operaciones de pago NMI: `sale`, `auth`, `capture`, `refund`, `void`, `update`
- Arquitectura desacoplada con MediatR, AutoMapper y FluentValidation
- Registro de logs de transacción request/response
- Persistencia basada en EF Core y patrón Unit of Work
- Pipelines configurables para validaciones y logging
- Fábricas de DbContext para multitenancy o uso en background
- Validación completa de los datos antes de enviar al gateway.
- Soporte para pagos con tarjeta de crédito (`creditcard`) y pagos ACH (`check`). **ACH sin implemntacion temporalmente**  
- Validaciones avanzadas de autenticación 3D Secure (3DS).
- Soporte para verificación antifraude Kount (`TransactionSessionId`). **Sin implementacion en al version actual temporalmente**
- Registro estructurado de solicitudes y respuestas en base de datos.
- FluentValidation para validaciones automáticas.
- API Key dinámica por `Tenant` para gestionar múltiples terminales de pago.
---

## 🛠️ Instalación

### 1. Agrega la referencia al SDK

```bash
# Desde un paquete NuGet
> dotnet add package ThreeTP.Payment.Infrastructure

# O como proyecto local
> dotnet add reference ../3TP.Payment.Infrastructure/3TP.Payment.Infrastructure.csproj
```

### 2. Agrega `appsettings.json` en el consumidor

```json
{
  "ConnectionStrings": {
    "NmiDb": "Server=localhost;Database=NmiDb;User Id=sa;Password=YourStrong!Pass;"
  }
}
```

### 3. En `Program.cs` del consumidor

```csharp
builder.Services
    .AddInfrastructure(builder.Configuration)
    .AddApplication();
```

---

## 🧱 Estructura del proyecto

```txt
src/
├── 3TP.Payment.API              # Proyecto consumidor (ASP.NET Core)
├── 3TP.Payment.Application      # Casos de uso, validaciones, handlers
├── 3TP.Payment.Domain           # Entidades del dominio
├── 3TP.Payment.Infrastructure  # SDK: EF Core, Repos, Gateway NMI, UoW
```

---

## 🧩 Componentes integrables

- `INmiPaymentGateway` : interfaz que ejecuta peticiones HTTP al gateway
- `IUnitOfWork` : persistencia controlada
- `IRepository<T>` : repositorio genérico para entidades
- `INmiDbContextFactory` : fábrica aislada para crear `DbContext` por demanda

---

## 🛡️ Pipelines configurados

- `ValidationBehavior<TRequest, TResponse>` → Ejecuta reglas de FluentValidation
- `LoggingBehavior<TRequest, TResponse>` → Registra duración y errores por comando

---

## ⚙️ Migraciones EF Core

```bash
# Ejecutar desde el directorio raíz del repositorio
> dotnet ef migrations add Init \
    --project src/3TP.Payment.Infrastructure \
    --startup-project src/3TP.Payment.API
```

> La clase `NmiDbContextDesignTimeFactory` obtiene la cadena de conexión desde `appsettings.json` o variables de entorno

---

## 🧪 Ejemplo de uso

```csharp
var result = await _mediator.Send(new AuthorizePaymentCommand(new SaleTransactionRequestDto
{
    CardNumber = "4111111111111111",
    Expiry = "1226",
    CVV = "999",
    Amount = 12.99m,
    SecurityKey = "abc-123",
    Email = "cliente@mail.com"
}));
```
---

## 📚 Validaciones de Solicitud de Pago (Payment Request Validation)

Antes de procesar una solicitud de pago, el sistema aplica las siguientes validaciones:

### 📝 **Campos Requeridos**

| Campo | Regla de Validación | Descripción |
|:--|:--|:--|
| `TypeTransaction` | **Obligatorio** | Tipo de transacción (por ejemplo: `sale`, `auth`). |
| `SecurityKey` | **Obligatorio** | Clave de seguridad (API Key) del terminal asociada al Tenant. |
| `Amount` | **Mayor que 0** | Monto de la transacción. |
| `Currency` | **Obligatorio** | Moneda en formato ISO 4217 (por ejemplo: `USD`, `EUR`). |
| `PaymentType` | **Obligatorio** | Tipo de pago (`creditcard` o `check`). |

---

### 💳 **Validaciones para pagos con tarjeta (Credit Card)**

**Cuando `PaymentType` = `creditcard`:**

| Campo | Regla de Validación |
|:--|:--|
| `CreditCardNumber` | **Obligatorio** — Número de tarjeta de crédito. |
| `CreditCardExpiration` | **Obligatorio** — Fecha de expiración en formato MMYY. (Ejemplo: `1225` para diciembre de 2025) |

---

### 🏦 **Validaciones para pagos ACH (Cheque)**

**Cuando `PaymentType` = `check`:**

| Campo | Regla de Validación |
|:--|:--|
| `CheckName` | **Obligatorio** — Nombre del titular de la cuenta. |
| `CheckAba` | **Obligatorio** — Número ABA del banco. |
| `CheckAccount` | **Obligatorio** — Número de cuenta bancaria. |

---

### 🔒 **Validaciones para Autenticación 3D Secure (3DS)**

**Cuando `CardHolderAuth` = `verified` o `attempted`:**

| Campo | Regla de Validación |
|:--|:--|
| `Cavv` | **Obligatorio** — Cardholder Authentication Verification Value (CAVV). |
| `Xid` | **Obligatorio** — Transaction Identifier (XID). |
| `ThreeDsVersion` | **Obligatorio** — Versión del protocolo 3D Secure (por ejemplo: `2.1.0`). |
| `DirectoryServerId` | **Obligatorio** — Identificador del servidor de directorio de 3DS. |

---

### 🧩 **Validaciones para Kount Fraud Detection**

**Cuando `TransactionSessionId` es provisto:**

| Campo | Regla de Validación |
|:--|:--|
| `TransactionSessionId` | **32 caracteres alfanuméricos** — Debe tener exactamente 32 caracteres. |

---

### 🧾 **Validaciones de Información del Titular (Billing Information)**

| Campo | Regla de Validación |
|:--|:--|
| `FirstName` | **Obligatorio** — Nombre del titular de la tarjeta. |
| `LastName` | **Obligatorio** — Apellido del titular. |
| `Address1` | **Obligatorio** — Dirección (línea 1). |
| `City` | **Obligatorio** — Ciudad. |
| `State` | **Obligatorio** — Estado/Provincia. |
| `Zip` | **Obligatorio** — Código postal. |
| `Country` | **Obligatorio** — Código de país (ISO Alpha-2). |
| `Email` | **Obligatorio y debe ser válido** — Email del titular (formato de email válido). |

---

## 🚫 **Errores de Validación**

Si alguna validación falla:

- Se retorna un código **400 Bad Request**.
- En el cuerpo de la respuesta se incluyen los errores detallados.

**Ejemplo de respuesta de error:**

```json
{
  "errors": [
    {
      "field": "CreditCardNumber",
      "message": "El número de tarjeta (ccnumber) es requerido para pagos con tarjeta."
    },
    {
      "field": "Amount",
      "message": "El monto debe ser mayor que cero."
    }
  ]
}
```

---

###  Estructura de carpetas y configuración de proyecto para empaquetar como SDK modular y escalable

```
📁 MyCompany.Payments.SDK/
│
├── 📁 Abstractions/                  // Interfaces para inyección y contratos
│   ├── IPaymentGateway.cs
│   ├── ITransactionRepository.cs
│   └── IOutboxService.cs
│
├── 📁 Domain/                        // Entidades y ValueObjects
│   ├── Transaction.cs
│   ├── Currency.cs
│   ├── ... (catálogos)
│   └── AuditableEntity.cs
│
├── 📁 Infrastructure/
│   ├── 📁 Persistence/              // EF Core, Configs y DbContext
│   │   ├── PaymentsDbContext.cs
│   │   ├── TransactionConfiguration.cs
│   │   └── ...
│   ├── 📁 Gateways/                 // Integraciones externas (NMI, etc)
│   │   └── NmiGateway.cs
│   └── 📁 Services/                 // Servicios auxiliares como Logger, Outbox
│
├── 📁 Application/                  // CQRS (Commands, Queries, Handlers)
│   ├── Commands/
│   └── Queries/
│
├── 📁 Extensions/                   // Extension methods para DI, configuración
│   └── ServiceCollectionExtensions.cs
│
├── 📁 Tests/                        // Pruebas unitarias e integración
│
└── MyCompany.Payments.SDK.csproj   // Proyecto principal
```

### Contenido ejemplo de archivo .csproj

```bash
<Project Sdk="Microsoft.NET.Sdk">
<PropertyGroup>
<TargetFramework>net8.0</TargetFramework>
<Nullable>enable</Nullable>
<GeneratePackageOnBuild>true</GeneratePackageOnBuild>
<Version>1.0.0</Version>
<Authors>MyCompany</Authors>
<Company>MyCompany</Company>
<PackageId>MyCompany.Payments.SDK</PackageId>
<Description>SDK para integrar procesamiento de pagos multitenant con NMI y Clean Architecture.</Description>
<RepositoryUrl>https://github.com/MyCompany/Payments</RepositoryUrl>
</PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="8.0.0" />
    <PackageReference Include="System.Text.Json" Version="8.0.0" />
  </ItemGroup>

  <ItemGroup>
    <None Update="README.md">
      <Pack>True</Pack>
      <PackagePath>docs\</PackagePath>
    </None>
  </ItemGroup>
</Project>
```
---
### ✅ publicacion localmente con:
```bash
dotnet pack
dotnet nuget push bin/Release/*.nupkg --source <my-repo>
```

---
## 📬 Soporte
Para dudas o soporte técnico, escribe a: `fcano@3techpanama.com`

## 📄 Licencia
MIT License — libre uso empresarial y educativo.
