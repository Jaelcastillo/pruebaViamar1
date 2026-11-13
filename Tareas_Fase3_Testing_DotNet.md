# 🧪 Fase 3: Pruebas Unitarias, Automatizadas y de Carga en .NET (`PokedexCLI.Tests`)

¡Felicidades por completar y pulir el CRUD en consola con **Dapper**! 
En el desarrollo de software profesional, escribir código que "funciona cuando lo pruebo a mano" es solo la mitad del trabajo. La otra mitad es garantizar mediante **pruebas automatizadas** que tu lógica no se rompa ante casos bordes, que la integración con SQL Server (Vistas, SPs y Triggers) responda como se espera y que el sistema soporte múltiples usuarios al mismo tiempo sin colapsar.

En esta **Fase 3** aprenderás a diseñar y ejecutar los **3 pilares del Testing en .NET**:
1. **Pruebas Unitarias (Unit Tests)** con `xUnit` y `Moq` (aislando la lógica sin tocar la base de datos).
2. **Pruebas Automatizadas de Integración (Integration Tests)** contra tu SQL Server real (validando el ciclo completo de CRUD + Triggers sin intervención humana).
3. **Pruebas de Carga y Estrés (Load / Stress Tests)** con `NBomber` (midiendo cuántas consultas por segundo soporta tu repositorio Dapper + SQL Server bajo concurrencia).

---

## 🏗️ Paso Previo Obligatorio: Preparar tu Código para ser "Testable"

Si intentas hacer una prueba unitaria hoy sobre `Program.cs`, notarás que es imposible porque la lógica (paginar, agrupar tipos de combate, validar datos) está mezclada con `Console.ReadLine()` y `Console.WriteLine()`, y además `PokemonRepository` se conecta directo a SQL Server.

Antes de crear las pruebas, aplica esta refactorización profesional en tu proyecto `PokedexCLI`:

### 1. Extrae la interfaz `IPokemonRepository`
Crea una interfaz `IPokemonRepository.cs` (en tu carpeta `Repositories/`) con la firma de todos los métodos públicos de `PokemonRepository`, y haz que tu clase `PokemonRepository` la implemente:
```csharp
public interface IPokemonRepository
{
    IEnumerable<PokemonDetalle> ObtenerTodos();
    IEnumerable<PokemonDetalle> BuscarPorNombre(string textoBusqueda);
    CadenaEvolutiva? ObtenerCadenaEvolutiva(string nombre);
    IEnumerable<AnalisisCombate> ObtenerAnalisisCombate(string nombre);
    (IEnumerable<PokemonRegion>, ResumenRegion?) ObtenerReporteRegion(int idRegion);
    int AgregarPokemon(string nombre, double peso, double altura, int nPokedex, int idRegion, int idTipoPrincipal, int idHabilidad);
    PokemonDetalle? ObtenerPokemonPorId(int idPokemon);
    int EliminarPokemon(int idPokemon);
    int ActualizarPokemon(int idPokemon, double peso, double altura);
    IEnumerable<PokemonEliminado> ObtenerPokemonesEliminados();
    IEnumerable<AuditoriaCambio> ObtenerAuditoriaCambios();
}

public class PokemonRepository : IPokemonRepository
{
    // Tu código actual...
}
```

### 2. Crea la capa de Lógica de Negocio (`Services/PokemonService.cs`)
Crea una carpeta `Services/` y dentro la clase `PokemonService.cs`. Esta clase recibirá `IPokemonRepository` en su constructor (**Inyección de Dependencias**) y contendrá la lógica pura (sin `Console.WriteLine`):
* **Método `ObtenerPaginaPokemon(int numeroPagina, int tamanoPagina)`:** Valida que `numeroPagina >= 1` y `tamanoPagina > 0`, llama a `_repository.ObtenerTodos()` y aplica el `.Skip()` y `.Take()`.
* **Método `AnalizarCombateAgrupado(string nombre)`:** Valida que el nombre no esté vacío, llama a `_repository.ObtenerAnalisisCombate(nombre)` y devuelve un objeto con el nombre del Pokémon, su tipo elemental, la lista de ventajas sin duplicados (`Distinct`) y la lista de debilidades sin duplicados (`Distinct`).
* **Método `RegistrarNuevoPokemon(...)`:** Valida las reglas de negocio en C# **antes** de llamar a la base de datos:
  * Si el `nombre` está vacío o en blanco $\rightarrow$ lanza `ArgumentException`.
  * Si `peso <= 0` o `altura <= 0` $\rightarrow$ lanza `ArgumentOutOfRangeException`.
  * Si `nPokedex <= 0` $\rightarrow$ lanza `ArgumentOutOfRangeException`.
  * Si todo es válido, llama a `_repository.AgregarPokemon(...)`.

---

## 🧪 Módulo 1: Pruebas Unitarias (Unit Testing con `xUnit` y `Moq`)
*Objetivo: Probar la lógica de `PokemonService` en milisegundos, en memoria y **sin conectarse a SQL Server**, simulando el repositorio con `Moq`.*

### Configuración del Proyecto de Pruebas
1. En tu solución, crea un nuevo proyecto de pruebas **xUnit** llamado `PokedexCLI.Tests`:
   ```bash
   dotnet new xunit -n PokedexCLI.Tests
   ```
2. Agrega la referencia a tu proyecto principal `PokedexCLI` e instala el paquete NuGet **`Moq`**:
   ```bash
   dotnet add PokedexCLI.Tests reference PokedexCLI/PokedexCLI.csproj
   dotnet add PokedexCLI.Tests package Moq
   ```

### Tarea 1.1: Pruebas Unitarias de Paginación (`PokemonServiceUnitTests.cs`)
Usando `Mock<IPokemonRepository>`, configura `.Setup(r => r.ObtenerTodos()).Returns(listaFalsaDe25Pokemones)` y escribe pruebas unitarias (`[Fact]`) que verifiquen:
1. **Página 1 completa:** Que al pedir la página `1` con tamaño `20`, devuelva exactamente `20` Pokémon.
2. **Última página parcial:** Que al pedir la página `2` con tamaño `20` (de un total de 25), devuelva exactamente los `5` Pokémon restantes.
3. **Parámetros inválidos:** Que si alguien pide la página `0` o `-1`, el servicio lance `ArgumentOutOfRangeException`.

### Tarea 1.2: Pruebas Unitarias del Analizador de Combate (Deduplicación LINQ)
Simula con `Moq` que `_repository.ObtenerAnalisisCombate("Bulbasaur")` devuelve 4 filas donde `"Agua"` aparece repetido dos veces en `FuerteA` y `"Fuego"` aparece repetido dos veces en `DebilA`.
* **Requerimiento:** Escribe un `[Fact]` que llame a `AnalizarCombateAgrupado("Bulbasaur")` y compruebe con `Assert.Single(...)` o `Assert.Equal(1, ...)` que las listas finales de ventajas y debilidades **no contienen elementos duplicados**.

### Tarea 1.3: Pruebas Parametrizadas de Validación (`[Theory]` e `[InlineData]`)
Investiga cómo funcionan los atributos **`[Theory]`** e **`[InlineData]`** en `xUnit` para probar múltiples casos inválidos en un solo método de prueba:
* **Requerimiento:** Crea una prueba parametrizada que intente registrar un Pokémon con pesos y alturas inválidas (ej. `[InlineData(0, 1.5)]`, `[InlineData(-5, 1.5)]`, `[InlineData(10, 0)]`, `[InlineData(10, -2)]`).
* **Verificación doble:**
  1. Comprueba con `Assert.Throws<ArgumentOutOfRangeException>(...)` que el servicio rechaza los datos.
  2. Comprueba con **`mockRepo.Verify(r => r.AgregarPokemon(...), Times.Never)`** que el repositorio **nunca fue llamado** (ahorrando un viaje innecesario a SQL Server).

---

## 🤖 Módulo 2: Pruebas Automatizadas de Integración (SQL Server Real)
*Objetivo: Construir tests automatizados que ejecuten `PokemonRepository` contra la base de datos `IntegradorPokemonFyM` real y verifiquen que los Stored Procedures y Triggers funcionen de punta a punta.*

Crea una clase `PokemonIntegrationTests.cs` en tu proyecto de pruebas que instancie tu `PokemonRepository` real leyendo el `appsettings.json`.

### Tarea 2.1: Test Automatizado de Ciclo de Vida Completo (CRUD + Triggers de Auditoría)
Programa un test automatizado (`[Fact]`) que ejecute sin intervención humana la siguiente secuencia y valide cada paso con `Assert`:
1. **Creación (Create):** Inserta un Pokémon de prueba (ej. `Nombre = "TestMon_Auto"`, `Peso = 15.5`, `Altura = 1.2`, `NPokedex = 9999`, `IDRegion = 1`, `IDTipoPrincipal = 1`, `IDHabilidad = 1`) usando `repository.AgregarPokemon(...)`.
2. **Lectura (Read):** Busca a `"TestMon_Auto"` usando `repository.BuscarPorNombre("TestMon_Auto")`, verifica con `Assert.NotNull` que exista en la base de datos y obtén su `IDPokemon`.
3. **Actualización + Trigger 3.3 (Update & Audit):**
   * Llama a `repository.ActualizarPokemon(idCreado, 25.0, 1.8)`.
   * Inmediatamente consulta `repository.ObtenerAuditoriaCambios()` y verifica con `Assert.Contains(...)` que tu trigger `tr_AuditarCambiosPokemon` haya creado automáticamente el registro con `PesoAnterior == 15.5` y `PesoNuevo == 25.0`.
4. **Eliminación + Trigger 3.1 (Delete & Audit):**
   * Llama a `repository.EliminarPokemon(idCreado)`.
   * Verifica que `repository.ObtenerPokemonPorId(idCreado)` ahora devuelva `null`.
   * Consulta `repository.ObtenerPokemonesEliminados()` y verifica con `Assert.Contains(...)` que tu trigger `tr_AuditarPokemonEliminado` haya guardado a `"TestMon_Auto"` en la tabla de eliminados.

### Tarea 2.2: Test Automatizado de Rechazo por Trigger (`tr_ValidarDatosPokemon`) y SP (`sp_ReportePorRegion`)
Escribe dos pruebas automatizadas de integración enfocadas en errores de base de datos:
1. **`ActualizarPokemon_ConPesoNegativo_DebeLanzarSqlExceptionDelTrigger`:**
   * Usa `var ex = Assert.Throws<SqlException>(() => repository.ActualizarPokemon(1, -50.0, 1.0));`
   * Verifica con `Assert.Contains("mayores", ex.Message)` que el error capturado provenga efectivamente de tu trigger `tr_ValidarDatosPokemon`.
2. **`ObtenerReporteRegion_ConRegionInexistente_DebeLanzarSqlExceptionDelSP`:**
   * Usa `Assert.Throws<SqlException>(() => repository.ObtenerReporteRegion(9999));` para comprobar que el `THROW 50000` de tu `sp_ReportePorRegion` funciona correctamente desde .NET.

---

## 🚀 Módulo 3: Pruebas de Carga y Estrés (Load Testing con `NBomber`)
*Objetivo: Medir el rendimiento real ( Peticiones por Segundo, Latencia media, Percentil 95/99 y tasa de fallos ) de tus consultas Dapper y tus Stored Procedures bajo tráfico concurrente.*

Para pruebas de carga en .NET utilizaremos **NBomber**, el framework moderno de Load Testing para C# que genera reportes detallados en consola y en archivos HTML/Markdown.

### Configuración del Proyecto de Carga (`PokedexCLI.LoadTests`)
1. Crea un proyecto de consola independiente llamado `PokedexCLI.LoadTests`:
   ```bash
   dotnet new console -n PokedexCLI.LoadTests
   dotnet add PokedexCLI.LoadTests reference PokedexCLI/PokedexCLI.csproj
   dotnet add PokedexCLI.LoadTests package NBomber
   ```

### Tarea 3.1: Escenario de Carga de Lectura Concurrente (`Query` y `QueryMultiple`)
En `Program.cs` de `PokedexCLI.LoadTests`, configura un escenario de **NBomber** llamado `"lectura_pokedex_concurrente"` que simule tráfico constante:
* **Carga a simular:** `Simulation.KeepConstant(copies: 30, during: TimeSpan.FromSeconds(30))` (30 usuarios concurrentes ejecutando consultas sin parar durante 30 segundos).
* **Pasos del escenario:**
  1. Ejecutar `repository.ObtenerReporteRegion(1)` (probando el rendimiento de `sp_ReportePorRegion` con `QueryMultiple`).
  2. Ejecutar `repository.ObtenerAnalisisCombate("Charizard")` (probando el rendimiento de la vista `vw_AnalisisCombate`).
* **Meta de rendimiento (Assertion de Carga):** Verifica en el reporte de NBomber que haya **0 fallos (`Fail = 0`)** y que el percentil 95 de latencia (`p95`) sea menor a `100 ms`.

### Tarea 3.2: Escenario de Estrés de Escritura y Triggers Concurrentes
Crea un segundo escenario en NBomber llamado `"estres_actualizacion_triggers"`:
* **Carga a simular:** Inyectar **20 actualizaciones por segundo** (`Simulation.Inject(rate: 20, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(20))`) actualizando el peso de distintos Pokémon (`IDPokemon` aleatorio entre 1 y 100 con un peso válido aleatorio).
* **Objetivo de aprendizaje:** Observar cómo responde SQL Server cuando el trigger `tr_AuditarCambiosPokemon` se dispara cientos de veces por segundo de forma concurrente e identificar si aparecen bloqueos (*Deadlocks* o *Timeouts*).

---

## 📊 ¿Cómo Ejecutar y Entregar esta Fase?

1. **Estructura de la Solución (`.sln`):**
   Tu repositorio debe contar ahora con los 3 proyectos organizados:
   * 📁 `PokedexCLI/` (Aplicación principal con `IPokemonRepository` y `PokemonService`)
   * 📁 `PokedexCLI.Tests/` (Pruebas Unitarias con `xUnit` + `Moq` y Pruebas de Integración con SQL Server)
   * 📁 `PokedexCLI.LoadTests/` (Pruebas de Carga con `NBomber`)
2. **Asegúrate de que todos los tests pasen en verde ejecutando:**
   ```bash
   dotnet test
   ```
3. **Reporte de Carga:**
   Cuando ejecutes `dotnet run --project PokedexCLI.LoadTests`, **NBomber** generará una carpeta `reports/` con un archivo `.html` y `.md` con las gráficas y métricas de tu prueba de carga (`RPS`, `Mean`, `p50`, `p95`, `p99`). **Incluye ese reporte `.md` o `.html` en el repositorio** para revisar los números que alcanzó tu base de datos.
4. **No olvides el `.gitignore`:** Asegúrate de no subir las carpetas `bin/` ni `obj/` de ninguno de los 3 proyectos.
