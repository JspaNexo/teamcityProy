<#
.SYNOPSIS
    Despliega en el IIS local un sitio ya compilado (.NET Framework 4.8 o .NET 10) y comprueba que responda.

.DESCRIPTION
    1. Crea el App Pool y el sitio si no existen (el App Pool según el runtime).
    2. Detiene el App Pool para que el proceso w3wp suelte los ensamblados.
    3. Copia los archivos con robocopy /MIR (la carpeta del sitio queda igual al origen).
    4. Inicia el App Pool y espera a que /health responda.
    5. Comprueba que GET / devuelva la versión desplegada.

    En TeamCity numera el build con la versión desplegada (la del ensamblado principal).
    Requiere IIS con "Scripts y herramientas de administración" y ejecutarse como administrador
    (el agente de TeamCity como servicio de Windows corre como SYSTEM). Para .NET 10 requiere además
    el ASP.NET Core Hosting Bundle 10.

.EXAMPLE
    .\scripts\desplegar-iis.ps1 -Origen .\publish -Sitio ApplicationDemo -Puerto 8081 -Runtime NetCore -Ensamblado ApplicationDemo.dll

.EXAMPLE
    .\scripts\desplegar-iis.ps1 -Origen .\ApplicationDemo.NetFramework\publish -Sitio ApplicationDemoNetFramework -Puerto 8082 -Runtime NetFramework -Ensamblado bin\ApplicationDemo.NetFramework.dll
#>
[CmdletBinding()]
param(
    # Carpeta con el sitio compilado (debe contener el web.config)
    [Parameter(Mandatory)] [string] $Origen,
    # Nombre del sitio y de su App Pool en IIS
    [Parameter(Mandatory)] [string] $Sitio,
    [Parameter(Mandatory)] [int] $Puerto,
    # NetFramework: App Pool con CLR v4.0. NetCore: App Pool "Sin código administrado"; la app la carga el ASP.NET Core Module
    [Parameter(Mandatory)] [ValidateSet('NetFramework', 'NetCore')] [string] $Runtime,
    # Ensamblado principal, relativo a -Origen: de él se lee la versión que se despliega
    [Parameter(Mandatory)] [string] $Ensamblado,
    [string] $RutaFisica = (Join-Path $env:SystemDrive "inetpub\$Sitio")
)

$ErrorActionPreference = 'Stop'

# --- Comprobaciones previas: fallar con un mensaje claro antes de tocar IIS ---

$identidad = [Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()
if (-not $identidad.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw "Administrar IIS requiere permisos de administrador: ejecuta PowerShell como administrador o el agente de TeamCity como servicio."
}

if (-not (Get-Module -ListAvailable -Name WebAdministration)) {
    throw "IIS no está habilitado (falta el módulo WebAdministration). Ver README: sección IIS."
}

if ($Runtime -eq 'NetCore' -and -not (Test-Path "$env:ProgramFiles\IIS\Asp.Net Core Module\V2\aspnetcorev2.dll")) {
    throw "Falta el ASP.NET Core Hosting Bundle 10: sin él IIS no puede ejecutar una app .NET 10. Ver README: sección IIS."
}

$Origen = (Resolve-Path $Origen).Path

if (-not (Test-Path (Join-Path $Origen 'web.config'))) {
    throw "$Origen no tiene web.config: no es un sitio publicado."
}

$version = (Get-Item (Join-Path $Origen $Ensamblado)).VersionInfo.ProductVersion

# Mensaje de servicio de TeamCity: el build muestra la versión desplegada (ej. 1.0.8) en vez de un contador
Write-Host "##teamcity[buildNumber '$version']"
Write-Host "Desplegando $Sitio $version ($Runtime) en http://localhost:$Puerto desde $Origen"

Import-Module WebAdministration

# --- App Pool y sitio (solo se crean la primera vez) ---

$rutaPool = "IIS:\AppPools\$Sitio"
if (-not (Test-Path $rutaPool)) {
    New-WebAppPool -Name $Sitio | Out-Null
    Write-Host "App Pool $Sitio creado"
}

# '' = "Sin código administrado": ASP.NET Core no usa el CLR de .NET Framework del App Pool
$clr = if ($Runtime -eq 'NetFramework') { 'v4.0' } else { '' }
Set-ItemProperty $rutaPool -Name managedRuntimeVersion -Value $clr

New-Item -ItemType Directory -Force -Path $RutaFisica | Out-Null

if (-not (Test-Path "IIS:\Sites\$Sitio")) {
    # Id explícito: New-Website falla sin él cuando IIS no tiene ningún sitio
    $id = [int]((Get-Website | Measure-Object -Property Id -Maximum).Maximum) + 1
    New-Website -Name $Sitio -Id $id -Port $Puerto -PhysicalPath $RutaFisica -ApplicationPool $Sitio | Out-Null
    Write-Host "Sitio $Sitio creado en el puerto $Puerto ($RutaFisica)"
}

# --- Copia: con el App Pool detenido ningún w3wp tiene bloqueados los ensamblados ---

if ((Get-WebAppPoolState -Name $Sitio).Value -ne 'Stopped') {
    Stop-WebAppPool -Name $Sitio
}

$limite = (Get-Date).AddSeconds(30)
while ((Get-WebAppPoolState -Name $Sitio).Value -ne 'Stopped') {
    if ((Get-Date) -gt $limite) { throw "El App Pool $Sitio no se detuvo en 30 s." }
    Start-Sleep -Seconds 1
}

# /MIR borra del destino lo que ya no está en el origen; /R /W reintentan si un archivo sigue bloqueado
robocopy $Origen $RutaFisica /MIR /NFL /NDL /NJH /NJS /NP /R:5 /W:2
# robocopy: 0-7 es éxito (1 = archivos copiados), 8 o más es error
if ($LASTEXITCODE -ge 8) { throw "robocopy falló con el código $LASTEXITCODE." }

Start-WebAppPool -Name $Sitio
if ((Get-WebsiteState -Name $Sitio).Value -ne 'Started') {
    Start-Website -Name $Sitio
}

# --- Verificación: /health responde y GET / devuelve la versión desplegada ---

$url = "http://localhost:$Puerto"
$limite = (Get-Date).AddSeconds(60)
while ($true) {
    try {
        Invoke-WebRequest "$url/health" -UseBasicParsing -TimeoutSec 5 | Out-Null
        break
    }
    catch {
        if ((Get-Date) -gt $limite) { throw "El sitio no respondió en $url/health: $($_.Exception.Message)" }
        Start-Sleep -Seconds 2
    }
}

$info = Invoke-RestMethod "$url/" -TimeoutSec 10
Write-Host "GET $url/ -> $($info | ConvertTo-Json -Compress)"

if ($info.version -ne $version) {
    throw "El sitio responde la versión $($info.version), se esperaba $version."
}

Write-Host "$Sitio $version desplegado en $url"
exit 0
