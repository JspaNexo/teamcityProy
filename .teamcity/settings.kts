import jetbrains.buildServer.configs.kotlin.*
import jetbrains.buildServer.configs.kotlin.buildFeatures.perfmon
import jetbrains.buildServer.configs.kotlin.buildSteps.DotnetMsBuildStep
import jetbrains.buildServer.configs.kotlin.buildSteps.PowerShellStep
import jetbrains.buildServer.configs.kotlin.buildSteps.dockerCommand
import jetbrains.buildServer.configs.kotlin.buildSteps.dotnetBuild
import jetbrains.buildServer.configs.kotlin.buildSteps.dotnetMsBuild
import jetbrains.buildServer.configs.kotlin.buildSteps.dotnetPublish
import jetbrains.buildServer.configs.kotlin.buildSteps.dotnetTest
import jetbrains.buildServer.configs.kotlin.buildSteps.powerShell
import jetbrains.buildServer.configs.kotlin.buildSteps.script
import jetbrains.buildServer.configs.kotlin.triggers.finishBuildTrigger
import jetbrains.buildServer.configs.kotlin.triggers.vcs

/*
 * Configuración de TeamCity para ApplicationDemo (Kotlin DSL / Versioned Settings).
 *
 * Pipeline:
 *   1. BuildAndTest    -> compila la solución, ejecuta las pruebas y publica el artefacto (main y develop).
 *   2. DockerImage     -> Staging, solo en develop: construye la imagen, la sube al registro local
 *                         y genera docker-stack.Temp.yml con los tokens reemplazados.
 *   3. DesplegarSwarm  -> despliega en el Docker Swarm del agente el último build correcto de Staging (manual),
 *                         con el stack y el script publicados por ese build.
 *   4. DesplegarIis    -> despliega en el IIS del agente Windows el último build correcto de BuildAndTest en develop (manual).
 *
 * ApplicationDemo.NetFramework (la misma API en .NET Framework 4.8, solo en el agente Windows):
 *   NetFrameworkBuildAndTest -> compila con el MSBuild de Visual Studio, ejecuta las pruebas y publica el sitio IIS.
 *   NetFrameworkDesplegarIis -> despliega en IIS el último build correcto en develop (manual).
 *
 * Práctica 06 (Farmacorp POS Express):
 *   PosExpressBuildAndTest -> compila la solución N-Capas, ejecuta las pruebas y publica la consola.
 */

version = "2026.2"

object Ramas {
    val STAGING = "develop"
}

// Valores de teamcity.agent.jvm.os.family: Docker/Bash corren en el agente Linux; MSBuild de .NET Framework e IIS, en el Windows
object Agentes {
    val LINUX = "Linux"
    val WINDOWS = "Windows"
}

project {
    description = "App demo .NET 10 y .NET Framework 4.8 para practicar CI/CD con TeamCity (Docker Swarm e IIS)"

    // sequential crea la dependencia snapshot DockerImage -> BuildAndTest
    sequential {
        buildType(BuildAndTest)
        buildType(DockerImage)
    }.buildTypes().forEach { buildType(it) }

    buildType(DesplegarSwarm)
    buildType(DesplegarIis)
    buildType(NetFrameworkBuildAndTest)
    buildType(NetFrameworkDesplegarIis)
    buildType(PosExpressBuildAndTest)
}

object PosExpressBuildAndTest : BuildType({
    name = "POS Express - Build y Test"

    buildNumberPattern = "1.0.%build.counter%"
    artifactRules = "Farmacorp.PosExpress/publish => Farmacorp.PosExpress-%build.number%.zip"

    params {
        param("configuration", "Release")
    }

    vcs {
        root(DslContext.settingsRoot)
        cleanCheckout = true
    }

    steps {
        dotnetBuild {
            name = "Compilar"
            projects = "Farmacorp.PosExpress/Farmacorp.PosExpress.slnx"
            configuration = "%configuration%"
            args = "-p:Version=%build.number%"
        }
        dotnetTest {
            name = "Pruebas"
            projects = "Farmacorp.PosExpress/Farmacorp.PosExpress.slnx"
            configuration = "%configuration%"
            skipBuild = true
        }
        dotnetPublish {
            name = "Publicar consola"
            projects = "Farmacorp.PosExpress/Farmacorp.PosExpress.Presentacion/Farmacorp.PosExpress.Presentacion.csproj"
            configuration = "%configuration%"
            outputDir = "Farmacorp.PosExpress/publish"
            args = "-p:Version=%build.number%"
        }
    }

    triggers {
        vcs {
            branchFilter = "+:<default>"
            triggerRules = "+:Farmacorp.PosExpress/**"
        }
    }
})

object BuildAndTest : BuildType({
    name = "Build y Test"

    buildNumberPattern = "1.0.%build.counter%"
    // El publish ya sirve para IIS (incluye el web.config); el script viaja con él para Despliegue IIS Staging
    artifactRules = """
        publish => ApplicationDemo-%build.number%.zip
        scripts/desplegar-iis.ps1 => scripts
    """.trimIndent()

    params {
        param("configuration", "Release")
    }

    vcs {
        root(DslContext.settingsRoot)
        cleanCheckout = true
    }

    steps {
        dotnetBuild {
            name = "Compilar"
            projects = "ApplicationDemo.slnx"
            configuration = "%configuration%"
            args = "-p:Version=%build.number%"
        }
        dotnetTest {
            name = "Pruebas"
            projects = "ApplicationDemo.slnx"
            configuration = "%configuration%"
            skipBuild = true
        }
        dotnetPublish {
            name = "Publicar"
            projects = "ApplicationDemo/ApplicationDemo.csproj"
            configuration = "%configuration%"
            outputDir = "publish"
            args = "-p:Version=%build.number% -p:UseAppHost=false"
        }
    }

    triggers {
        vcs {
            branchFilter = """
                +:<default>
                +:${Ramas.STAGING}
            """.trimIndent()
            // Los cambios de POS Express y de la versión .NET Framework los compila su propia configuración
            triggerRules = """
                -:Farmacorp.PosExpress/**
                -:ApplicationDemo.NetFramework/**
            """.trimIndent()
        }
    }

    features {
        perfmon { }
    }
})

/*
 * Staging: igual que el pipeline de referencia, cada paso solo corre en la rama develop.
 * Publica en el registro local del entorno TeamCity (contenedor registry:2 en localhost:5000).
 */
object DockerImage : BuildType({
    name = "Imagen Docker Staging"

    buildNumberPattern = "${BuildAndTest.depParamRefs.buildNumber}"
    // El script de despliegue viaja con el stack: Despliegue Swarm Staging usa los dos del mismo commit
    artifactRules = """
        docker-stack.Temp.yml
        scripts/desplegar-swarm.sh => scripts
    """.trimIndent()

    params {
        param("env.REGISTRO", "localhost:5000")
        param("env.IMAGEN", "%env.REGISTRO%/applicationdemo")
        param("env.TAG", "%build.number%")
        param("env.APP_IMAGE_BUILD_NUMBER", "%env.IMAGEN%:%env.TAG%")
    }

    vcs {
        root(DslContext.settingsRoot)
    }

    steps {
        dockerCommand {
            name = "Docker Build Staging"
            conditions {
                equals("teamcity.build.branch", Ramas.STAGING)
            }
            commandType = build {
                source = file {
                    path = "ApplicationDemo/Dockerfile"
                }
                contextDir = "."
                namesAndTags = "%env.APP_IMAGE_BUILD_NUMBER%"
                // VERSION: el ensamblado lleva la misma versión que el tag, así GET / muestra la versión desplegada
                commandArgs = "--pull --build-arg VERSION=%env.TAG%"
            }
        }
        dockerCommand {
            name = "Docker Push Staging"
            conditions {
                equals("teamcity.build.branch", Ramas.STAGING)
            }
            commandType = push {
                namesAndTags = "%env.APP_IMAGE_BUILD_NUMBER%"
                removeImageAfterPush = true
            }
        }
        script {
            name = "Replace Staging Swarm Tokens"
            conditions {
                equals("teamcity.build.branch", Ramas.STAGING)
            }
            // Usa las variables env.IMAGEN y env.TAG de este build
            scriptContent = "bash scripts/reemplazar-tokens-swarm.sh docker-stack.yml docker-stack.Temp.yml"
        }
    }

    triggers {
        // Al terminar bien Build y Test en develop; la dependencia snapshot reutiliza ese build, no lo repite
        finishBuildTrigger {
            buildType = "${BuildAndTest.id}"
            successfulOnly = true
            branchFilter = "+:${Ramas.STAGING}"
        }
    }

    requirements {
        exists("docker.server.version")
        equals("teamcity.agent.jvm.os.family", Agentes.LINUX)
    }
})

/*
 * Despliega en el Swarm del agente la última versión publicada en Staging: toma el docker-stack.Temp.yml
 * y el script de despliegue del último build correcto de DockerImage en develop.
 * Sin VCS root: no hace checkout, así que no hay rama que elegir al pulsar Run y todo lo que ejecuta
 * viene de ese build. Sin trigger: se ejecuta manual. Requisito: el Docker del agente debe ser manager
 * de un Swarm ("docker swarm init" una vez).
 */
object DesplegarSwarm : BuildType({
    name = "Despliegue Swarm Staging"

    params {
        param("swarm.stack", "applicationdemo")
    }

    steps {
        script {
            name = "docker stack deploy"
            // El script numera el build con la versión desplegada (lee la imagen del archivo)
            scriptContent = "bash despliegue/desplegar-swarm.sh despliegue/docker-stack.Temp.yml %swarm.stack%"
        }
    }

    dependencies {
        // Solo dependencia de artefactos, filtrada a develop: no hay dependencia snapshot,
        // así que no reconstruye nada
        artifacts(DockerImage) {
            buildRule = lastSuccessful("+:${Ramas.STAGING}")
            cleanDestination = true
            artifactRules = """
                docker-stack.Temp.yml => despliegue
                scripts/desplegar-swarm.sh => despliegue
            """.trimIndent()
        }
    }

    requirements {
        exists("docker.server.version")
        equals("teamcity.agent.jvm.os.family", Agentes.LINUX)
    }
})

/*
 * La misma API que ApplicationDemo, en ASP.NET clásico sobre .NET Framework 4.8.
 * Necesita el agente Windows: compila con el MSBuild de Visual Studio 2026 (v18), como un proyecto .NET Framework
 * tradicional, y publica Web.config + bin\ listos para IIS.
 */
object NetFrameworkBuildAndTest : BuildType({
    name = "Framework - Build y Test"

    buildNumberPattern = "1.0.%build.counter%"
    artifactRules = """
        ApplicationDemo.NetFramework/publish => ApplicationDemo.NetFramework-%build.number%.zip
        scripts/desplegar-iis.ps1 => scripts
    """.trimIndent()

    params {
        param("configuration", "Release")
    }

    vcs {
        root(DslContext.settingsRoot)
        cleanCheckout = true
    }

    steps {
        dotnetMsBuild {
            name = "Compilar (MSBuild)"
            projects = "ApplicationDemo.NetFramework/ApplicationDemo.NetFramework.slnx"
            version = DotnetMsBuildStep.MSBuildVersion.V18
            targets = "Build"
            configuration = "%configuration%"
            args = "-restore -p:Version=%build.number%"
        }
        dotnetTest {
            name = "Pruebas"
            projects = "ApplicationDemo.NetFramework/ApplicationDemo.NetFramework.slnx"
            configuration = "%configuration%"
            skipBuild = true
        }
        dotnetMsBuild {
            name = "Publicar sitio IIS"
            projects = "ApplicationDemo.NetFramework/ApplicationDemo.NetFramework/ApplicationDemo.NetFramework.csproj"
            version = DotnetMsBuildStep.MSBuildVersion.V18
            // Target del .csproj: copia Web.config y bin\*.dll a ApplicationDemo.NetFramework/publish
            targets = "PublicarSitio"
            configuration = "%configuration%"
            args = "-p:Version=%build.number%"
        }
    }

    triggers {
        vcs {
            branchFilter = """
                +:<default>
                +:${Ramas.STAGING}
            """.trimIndent()
            triggerRules = """
                +:ApplicationDemo.NetFramework/**
                +:scripts/desplegar-iis.ps1
            """.trimIndent()
        }
    }

    requirements {
        equals("teamcity.agent.jvm.os.family", Agentes.WINDOWS)
    }
})

/*
 * Despliegue en el IIS del agente Windows, igual que Despliegue Swarm Staging: sin VCS root ni trigger (manual,
 * sin rama que elegir). Descarga del último build correcto en develop el sitio compilado y scripts/desplegar-iis.ps1,
 * los dos del mismo commit, y ejecuta el script: crea App Pool y sitio si faltan, copia y comprueba /health.
 * El agente debe correr como servicio de Windows (SYSTEM) para administrar IIS.
 */
open class DespliegueIis(
    nombre: String,
    origen: BuildType,
    zip: String,
    sitio: String,
    puerto: Int,
    runtime: String,
    ensamblado: String
) : BuildType({
    name = nombre

    params {
        param("iis.sitio", sitio)
        param("iis.puerto", "$puerto")
    }

    steps {
        powerShell {
            name = "Desplegar en IIS"
            // El módulo WebAdministration de IIS es del PowerShell de Windows (5.1)
            edition = PowerShellStep.Edition.Desktop
            noProfile = true
            scriptMode = file {
                path = "despliegue/desplegar-iis.ps1"
            }
            scriptArgs = "-Origen sitio -Sitio %iis.sitio% -Puerto %iis.puerto% -Runtime $runtime -Ensamblado $ensamblado"
        }
    }

    dependencies {
        artifacts(origen) {
            buildRule = lastSuccessful("+:${Ramas.STAGING}")
            cleanDestination = true
            artifactRules = """
                $zip!** => sitio
                scripts/desplegar-iis.ps1 => despliegue
            """.trimIndent()
        }
    }

    requirements {
        equals("teamcity.agent.jvm.os.family", Agentes.WINDOWS)
    }
})

// .NET 10 en IIS: App Pool "Sin código administrado"; la app la carga el ASP.NET Core Module (Hosting Bundle)
object DesplegarIis : DespliegueIis(
    nombre = "Despliegue IIS Staging",
    origen = BuildAndTest,
    zip = "ApplicationDemo-*.zip",
    sitio = "ApplicationDemo",
    puerto = 8081,
    runtime = "NetCore",
    ensamblado = "ApplicationDemo.dll"
)

// .NET Framework 4.8 en IIS: App Pool con CLR v4.0
object NetFrameworkDesplegarIis : DespliegueIis(
    nombre = "Framework - Despliegue IIS Staging",
    origen = NetFrameworkBuildAndTest,
    zip = "ApplicationDemo.NetFramework-*.zip",
    sitio = "ApplicationDemoNetFramework",
    puerto = 8082,
    runtime = "NetFramework",
    ensamblado = "bin/ApplicationDemo.NetFramework.dll"
)
