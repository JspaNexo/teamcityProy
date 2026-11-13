import jetbrains.buildServer.configs.kotlin.*
import jetbrains.buildServer.configs.kotlin.buildFeatures.perfmon
import jetbrains.buildServer.configs.kotlin.buildSteps.dockerCommand
import jetbrains.buildServer.configs.kotlin.buildSteps.dotnetBuild
import jetbrains.buildServer.configs.kotlin.buildSteps.dotnetPublish
import jetbrains.buildServer.configs.kotlin.buildSteps.dotnetTest
import jetbrains.buildServer.configs.kotlin.buildSteps.script
import jetbrains.buildServer.configs.kotlin.triggers.vcs

/*
 * Configuración de TeamCity para ApplicationDemo (Kotlin DSL / Versioned Settings).
 *
 * Pipeline:
 *   1. BuildAndTest    -> compila la solución, ejecuta las pruebas y publica el artefacto.
 *   2. DockerImage     -> construye la imagen Docker (depende de BuildAndTest).
 *   3. DesplegarSwarm  -> despliega esa imagen en el Docker Swarm del agente (manual).
 *
 * Práctica 06 (Farmacorp POS Express):
 *   PosExpressBuildAndTest -> compila la solución N-Capas, ejecuta las pruebas y publica la consola.
 */

version = "2026.2"

project {
    description = "App demo .NET 10 para practicar CI con TeamCity"

    // sequential crea las dependencias snapshot DesplegarSwarm -> DockerImage -> BuildAndTest
    sequential {
        buildType(BuildAndTest)
        buildType(DockerImage)
        buildType(DesplegarSwarm)
    }.buildTypes().forEach { buildType(it) }

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
    artifactRules = "publish => ApplicationDemo-%build.number%.zip"

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
            branchFilter = "+:<default>"
            // Los cambios de POS Express los compila su propia configuración
            triggerRules = "-:Farmacorp.PosExpress/**"
        }
    }

    features {
        perfmon { }
    }
})

object DockerImage : BuildType({
    name = "Imagen Docker"

    buildNumberPattern = "${BuildAndTest.depParamRefs.buildNumber}"

    vcs {
        root(DslContext.settingsRoot)
    }

    steps {
        dockerCommand {
            name = "Docker build"
            commandType = build {
                source = file {
                    path = "ApplicationDemo/Dockerfile"
                }
                contextDir = "."
                namesAndTags = """
                    applicationdemo:%build.number%
                    applicationdemo:latest
                """.trimIndent()
                commandArgs = "--pull"
            }
        }
    }

    requirements {
        exists("docker.server.version")
    }
})

/*
 * Despliega en el Swarm del agente la imagen construida por DockerImage (misma versión).
 * Sin trigger: se ejecuta a mano y TeamCity corre antes Build y Test e Imagen Docker.
 * Requisito: el Docker del agente debe ser manager de un Swarm ("docker swarm init" una vez).
 * Sin registro de imágenes solo sirve si DockerImage corrió en el mismo agente.
 */
object DesplegarSwarm : BuildType({
    name = "Despliegue Swarm"

    buildNumberPattern = "${DockerImage.depParamRefs.buildNumber}"

    params {
        param("swarm.stack", "applicationdemo")
        // docker-stack.yml toma la etiqueta de la imagen de la variable de entorno TAG
        param("env.TAG", "%build.number%")
    }

    vcs {
        root(DslContext.settingsRoot)
    }

    steps {
        script {
            name = "docker stack deploy"
            // --detach=false espera a que las réplicas converjan: si el rolling update falla, el build falla
            scriptContent = "docker stack deploy --detach=false -c docker-stack.yml %swarm.stack%"
        }
    }

    requirements {
        exists("docker.server.version")
    }
})
