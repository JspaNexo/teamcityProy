import jetbrains.buildServer.configs.kotlin.*
import jetbrains.buildServer.configs.kotlin.buildFeatures.perfmon
import jetbrains.buildServer.configs.kotlin.buildSteps.dockerCommand
import jetbrains.buildServer.configs.kotlin.buildSteps.dotnetBuild
import jetbrains.buildServer.configs.kotlin.buildSteps.dotnetPublish
import jetbrains.buildServer.configs.kotlin.buildSteps.dotnetTest
import jetbrains.buildServer.configs.kotlin.triggers.vcs

/*
 * Configuración de TeamCity para ApplicationDemo (Kotlin DSL / Versioned Settings).
 *
 * Pipeline:
 *   1. BuildAndTest  -> compila la solución, ejecuta las pruebas y publica el artefacto.
 *   2. DockerImage   -> construye la imagen Docker (depende de BuildAndTest).
 *
 * Práctica 06 (Farmacorp POS Express):
 *   PosExpressBuildAndTest -> compila la solución N-Capas, ejecuta las pruebas y publica la consola.
 */

version = "2026.2"

project {
    description = "App demo .NET 10 para practicar CI con TeamCity"

    // sequential crea la dependencia snapshot DockerImage -> BuildAndTest
    sequential {
        buildType(BuildAndTest)
        buildType(DockerImage)
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
