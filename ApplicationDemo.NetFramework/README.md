# ApplicationDemo.NetFramework

La misma API que [ApplicationDemo](../README.md) (.NET 10), escrita en **ASP.NET clásico (`System.Web`) sobre .NET
Framework 4.8**, para comparar los dos mundos en TeamCity e IIS. Mismos endpoints, mismo `ProductoService`, mismas pruebas.

| | ApplicationDemo | ApplicationDemo.NetFramework |
|---|---|---|
| Framework | .NET 10 (ASP.NET Core) | .NET Framework 4.8 (ASP.NET `System.Web`) |
| Compila con | `dotnet` (agente Linux o Windows) | MSBuild de Visual Studio 2026 (agente Windows) |
| Corre en | Docker / Swarm / IIS | Solo IIS (Windows) |
| App Pool de IIS | *Sin código administrado* + ASP.NET Core Module | CLR *v4.0*, canalización integrada |
| Puerto en IIS | 8081 | 8082 |

## Estructura

| Archivo | Qué hace |
|---|---|
| `ApplicationDemo.NetFramework/Web.config` | Registra `ApiHandler` para todas las rutas (`path="*"`) |
| `ApplicationDemo.NetFramework/ApiHandler.cs` | Entrada de IIS: pasa la petición a `Api` y escribe la respuesta |
| `ApplicationDemo.NetFramework/Api.cs` | Las rutas (`/`, `/health`, `/api/productos`, `/api/total`), sin depender de IIS |
| `ApplicationDemo.NetFramework/Services/ProductoService.cs` | Lógica de productos, igual a la de .NET 10 |
| `ApplicationDemo.NetFramework.Tests/` | Pruebas xUnit en net48 del servicio y de las rutas |

`GET /` devuelve, además de la versión, `framework` (`.NET Framework 4.8`), `clr` (`4.0.30319.42000`) y `appPool`
(el App Pool de IIS que respondió).

## Local

Desde una *Developer PowerShell for VS 2026* (tiene `msbuild` en el PATH):

```powershell
msbuild ApplicationDemo.NetFramework\ApplicationDemo.NetFramework.slnx -restore -p:Configuration=Release
dotnet test ApplicationDemo.NetFramework\ApplicationDemo.NetFramework.slnx -c Release --no-build

# Deja Web.config + bin\ en ApplicationDemo.NetFramework\publish
msbuild ApplicationDemo.NetFramework\ApplicationDemo.NetFramework\ApplicationDemo.NetFramework.csproj -t:PublicarSitio -p:Configuration=Release -p:Version=1.0.5

# Probarlo sin instalar IIS, con IIS Express (viene con Visual Studio)
& "$env:ProgramFiles\IIS Express\iisexpress.exe" /path:"$PWD\ApplicationDemo.NetFramework\publish" /port:8082 /clr:v4.0
curl http://localhost:8082/
```

Para desplegarlo en IIS, ver la sección [IIS (Windows)](../README.md#iis-windows) del README principal.
