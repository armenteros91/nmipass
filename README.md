##  Documentacion de proyecto ApiRest para procesar pagos con NMI 


# 💳 3TP.Payment.SDK

Este SDK proporciona una solución completa y escalable para procesar pagos con NMI Gateway en aplicaciones .NET 8, utilizando principios de Clean Architecture, DDD y buenas prácticas del sector financiero.

---

## 🚀 Características

- Soporte para operaciones de pago NMI: `sale`, `auth`, `capture`, `refund`, `void`, `update`
- Arquitectura desacoplada con MediatR, AutoMapper y FluentValidation
- Registro de logs de transacción request/response
- Persistencia basada en EF Core y patrón Unit of Work
- Pipelines configurables para validaciones y logging
- Fábricas de DbContext para multitenancy o uso en background

---
## 🏗️ Construcciòn o compilaciòn del Build 

### Configuracion del sdk 
```bash
> dotnet pack src/ThreeTP.Payment.SDK/ThreeTP.Payment.SDK.csproj --configuration Release
```

### Publicacion
```bash
> dotnet nuget push ThreeTP.Payment.SDK.1.0.0.nupkg -k YOUR_API_KEY -s https://api.nuget.org/v3/index.json
```
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
├── ThreeTP.Payment.Domain/            ← Entidades del dominio, interfaces puras
├── ThreeTP.Payment.Application/       ← DTOs, comandos, queries, validadores, casos de uso
├── ThreeTP.Payment.Infrastructure/    ← Implementación de servicios (EF Core, APIs externas)
├── ThreeTP.Payment.SDK/               ← Adaptador reusable para exponer todo como SDK
└── ThreeTP.Payment.API/               ← (opcional) Web API para pruebas locales
tests/
└── ThreeTP.Payment.Tests/             ← Unit tests
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

## 📬 Soporte
Para dudas o soporte técnico, escribe a: `soporte@3tp.dev`

---

## 📄 Licencia
MIT License — libre uso empresarial y educativo.
