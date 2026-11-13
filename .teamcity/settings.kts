import jetbrains.buildServer.configs.kotlin.*
import jetbrains.buildServer.configs.kotlin.buildFeatures.perfmon
import jetbrains.buildServer.configs.kotlin.buildSteps.dockerCommand
import jetbrains.buildServer.configs.kotlin.buildSteps.dotnetBuild
import jetbrains.buildServer.configs.kotlin.buildSteps.dotnetPublish
import jetbrains.buildServer.configs.kotlin.buildSteps.dotnetTest
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
 *   3. DesplegarSwarm  -> despliega en el Docker Swarm del agente el último build correcto de Staging (manual).
 *
 * Práctica 06 (Farmacorp POS Express):
 *   PosExpressBuildAndTest -> compila la solución N-Capas, ejecuta las pruebas y publica la consola.
 */

version = "2026.2"

object Ramas {
    val STAGING = "develop"
}

project {
    description = "App demo .NET 10 para practicar CI con TeamCity"

    // sequential crea la dependencia snapshot DockerImage -> BuildAndTest
    sequential {
        buildType(BuildAndTest)
        buildType(DockerImage)
    }.buildTypes().forEach { buildType(it) }

    buildType(DesplegarSwarm)
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
            branchFilter = """
                +:<default>
                +:${Ramas.STAGING}
            """.trimIndent()
            // Los cambios de POS Express los compila su propia configuración
            triggerRules = "-:Farmacorp.PosExpress/**"
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
    artifactRules = "docker-stack.Temp.yml"

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
                commandArgs = "--pull"
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
    }
})

/*
 * Despliega en el Swarm del agente la última versión publicada en Staging: toma el docker-stack.Temp.yml
 * del último build correcto de DockerImage en develop, sin importar la rama elegida al pulsar Run.
 * Sin trigger: se ejecuta a mano. Requisito: el Docker del agente debe ser manager de un Swarm
 * ("docker swarm init" una vez).
 */
object DesplegarSwarm : BuildType({
    name = "Despliegue Swarm Staging"

    params {
        param("swarm.stack", "applicationdemo")
    }

    vcs {
        root(DslContext.settingsRoot)
    }

    steps {
        script {
            name = "docker stack deploy"
            // El script numera el build con la versión desplegada (lee la imagen del archivo)
            scriptContent = "bash scripts/desplegar-swarm.sh despliegue/docker-stack.Temp.yml %swarm.stack%"
        }
    }

    dependencies {
        // Solo dependencia de artefactos, filtrada a develop: no hay dependencia snapshot,
        // así que no reconstruye nada ni depende de la rama con la que se lance
        artifacts(DockerImage) {
            buildRule = lastSuccessful("+:${Ramas.STAGING}")
            cleanDestination = true
            artifactRules = "docker-stack.Temp.yml => despliegue"
        }
    }

    requirements {
        exists("docker.server.version")
    }
})
