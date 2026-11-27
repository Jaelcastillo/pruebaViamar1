# 📊 Revisión y Feedback — Fase 3: Pruebas Unitarias, Automatizadas y de Carga (`PokedexCLI.Tests` & `NBomber`)

¡Felicitaciones por el excelente trabajo en la **Fase 3**! Lograste implementar una arquitectura limpia basada en interfaces (`IPokemonRepository`), separar la lógica de negocio en `PokemonService`, construir pruebas unitarias con **`xUnit` + `Moq`**, pruebas de integración contra **SQL Server** y pruebas de carga concurrente con **`NBomber`**.

Lo mejor de esta entrega es que **tus propias pruebas automatizadas de integración descubrieron un detalle oculto en tu vista SQL de la Fase 1** (abajo te explicamos exactamente por qué fallaban esos 2 tests de 15 en tu `resultado.txt`). ¡Esa es la demostración perfecta de por qué hacemos pruebas automatizadas en la vida real!

---

## 🏆 Calificación Global de la Fase 3: **9.2 / 10 (92 / 100)** 🌟

| Módulo Evaluado | Estado | Calificación |
| :--- | :---: | :---: |
| **0. Refactorización y Correcciones Fase 2 (`IPokemonRepository`, `PokemonService`, limpieza Git)** | ✅ Excelente | **9.5 / 10** |
| **1. Pruebas Unitarias (`xUnit` + `Moq` en `PokemonServiceUnitTests.cs`)** | ✅ Impecable | **10 / 10** |
| **2. Pruebas Automatizadas de Integración (`PokemonIntegrationTests.cs`)** | ✅ Muy bien (Hallazgo en `vw_PokemonDetalle`) | **8.5 / 10** |
| **3. Pruebas de Carga y Estrés (`NBomber` + Reportes HTML/Markdown)** | ✅ Excelente | **9.5 / 10** |

---

## 🌟 1. Lo que hiciste EXCELENTE

### 🏗️ Refactorización e Inyección de Dependencias
* Corregiste todos los puntos pendientes de la Fase 2: limpiaste las carpetas `bin/` y `obj/` del repositorio, completaste el `.csproj`, agregaste la propiedad `Tipo` en `PokemonDetalle`, corregiste el `Console.ReadKey()` en `RegistrarPokemon` y creaste `ObtenerPokemonPorId`.
* Extrajiste correctamente `IPokemonRepository` y creaste `PokemonService` recibiendo la interfaz por constructor.

### 🧪 Módulo 1: Pruebas Unitarias (`PokemonServiceUnitTests.cs`) — 10/10
* **Estructura AAA (`Arrange`, `Act`, `Assert`):** Código súper limpio y fácil de leer.
* **Paginación con `Moq`:** Probaste con precisión la página 1 completa (20 registros), la página 2 con el remanente (5 registros) y las excepciones `ArgumentOutOfRangeException` para página `0` y `-1`.
* **Deduplicación LINQ:** Simulaste registros duplicados en `ObtenerAnalisisCombate("Bulbasaur")` y comprobaste que `AnalizarCombateAgrupado` devuelve únicamente elementos únicos en `Ventajas` y `Debilidades`.
* **Pruebas Parametrizadas (`[Theory]` + `[InlineData]`):** Excelente implementación probando 4 combinaciones inválidas de peso/altura y verificando con **`mockRepo.Verify(..., Times.Never)`** que jamás se invoca a la base de datos cuando la validación falla en C#.

### 🚀 Módulo 3: Pruebas de Carga y Estrés con `NBomber` — 9.5/10
* Configuraste e incluiste los reportes reales de **NBomber** (`.html`, `.md`, `.csv`, `.txt`):
  * **`lectura_pokedex_concurrente` (30 req/seg durante 30s):** `900` peticiones exitosas, **`0` fallos**, latencia media de `5.11 ms` y percentil 95 (`p95`) de **`4.03 ms`** (superando ampliamente la meta de `< 100 ms`).
  * **`estres_actualizacion_triggers` (20 req/seg durante 20s):** `400` actualizaciones concurrentes disparando tu trigger `tr_AuditarCambiosPokemon`, **`0` fallos**, latencia media de `4.13 ms` y `p95` de **`2.14 ms`**.

---

## 💡 2. El Gran Hallazgo: ¿Por qué fallaron 2 de los 15 tests en `resultado.txt`?

En tu archivo `resultado.txt` vimos que **13 de 15 pruebas pasaron en verde**, y solo fallaron estas dos pruebas de integración:
1. `ActualizarPokemon_DebeGenerarRegistroDeAuditoria`
2. `EliminarPokemon_DebeGenerarRegistroDeEliminacion`

Incluso vimos que en `EliminarPokemon_DebeGenerarRegistroDeEliminacion` agregaste esta línea de diagnóstico muy inteligente porque notaste algo extraño con el ID:
```csharp
Assert.True(pokemon.IDPokemon > 0, $"IDPokemon inválido: {pokemon.IDPokemon}");
```

### 🔍 ¿Cuál era la causa raíz?
1. En ambos tests, después de insertar el Pokémon con `_repository.AgregarPokemon(...)`, lo buscas por nombre llamando a:
   ```csharp
   var pokemon = _repository.BuscarPorNombre(nombre).First();
   ```
2. Tu método `BuscarPorNombre` en `PokemonRepository.cs` consulta la vista `vw_PokemonDetalle`:
   ```sql
   SELECT * FROM vw_PokemonDetalle WHERE Nombre LIKE @Busqueda
   ```
3. Pero si revisas tu script `tarea_1.1.sql` donde creaste `vw_PokemonDetalle`:
   ```sql
   CREATE OR ALTER VIEW vw_PokemonDetalle AS
   SELECT 
       p.NPokedex,
       p.Nombre,
       p.Peso,
       p.Altura,
       r.Nombre AS Region,
       STRING_AGG(t.Nombre, ' / ') AS Tipo
   FROM Pokemon p ...
   ```
   **¡La vista `vw_PokemonDetalle` no tenía incluida la columna `p.IDPokemon`!**
4. Como la vista no devuelve ninguna columna llamada `IDPokemon`, cuando **Dapper** mapea el resultado hacia la clase `PokemonDetalle`, deja la propiedad `pokemon.IDPokemon` con su valor por defecto en C#, que es **`0`**.
5. Por eso, en el paso siguiente, tu test ejecutaba `_repository.ActualizarPokemon(0, 25.0, 1.8)` y `_repository.EliminarPokemon(0)`, fallando porque el `IDPokemon = 0` no existe en SQL Server.

### ✅ ¿Cómo se soluciona en 10 segundos?
Solo debes agregar `p.IDPokemon` al `SELECT` y al `GROUP BY` de tu vista `vw_PokemonDetalle` en SQL Server (`tarea_1.1.sql`):
```sql
CREATE OR ALTER VIEW vw_PokemonDetalle AS
SELECT 
    p.IDPokemon, -- <-- Agregamos el IDPokemon a la vista
    p.NPokedex,
    p.Nombre,
    p.Peso,
    p.Altura,
    r.Nombre AS Region,
    STRING_AGG(t.Nombre, ' / ') AS Tipo
FROM Pokemon p
JOIN Regiones r ON p.IDRegion = r.IDRegion 
JOIN TipoPokemon tp ON p.IDPokemon = tp.IDPokemon 
JOIN Tipo t ON tp.IDTipo = t.IDTipo 
GROUP BY 
    p.IDPokemon, -- <-- Y lo incluimos en el GROUP BY
    p.NPokedex, 
    p.Nombre, 
    p.Peso, 
    p.Altura, 
    r.Nombre;
GO
```
Al actualizar esa vista en tu base de datos y volver a correr `dotnet test`, **los 15 de 15 tests pasarán en verde (`Passed: 15, Failed: 0`)**.

---

## 🔧 3. Tres Pequeños Tips Técnicos de C# para Pulir

1. **Restaurar el `namespace` en `Models/PokemonDetalle.cs`:**
   * Al borrar la letra `s` suelta que estaba al final de `PokemonDetalle.cs`, se borró también la primera línea `namespace PokedexCLI.Models;`. Aunque compila porque queda en el *global namespace*, agrégale `namespace PokedexCLI.Models;` arriba para que sea consistente con los demás modelos.
2. **Quitar referencias NuGet donde no se usan (`.csproj`):**
   * En `PokedexCLI.csproj` instalaste `Moq`, cuando `Moq` solo debe instalarse en el proyecto de pruebas `PokedexCLI.Tests.csproj`.
   * En `PokedexCLI.Tests.csproj` instalaste `NBomber`, cuando `NBomber` ya tiene su propio proyecto `PokedexCLI.LoadTests.csproj`. Mantener cada paquete solo en el proyecto que lo necesita hace que tu aplicación final pese menos.
3. **Uso de `Random.Shared` en pruebas concurrentes (`PokedexCLI.LoadTests/Program.cs`):**
   * En la prueba de carga declaraste `var random = new Random();` afuera del escenario y varios hilos lo llaman al mismo tiempo (`random.Next(1, 101)`). En .NET la clase `Random` tradicional no es *thread-safe* (si dos hilos la llaman en el mismo microsegundo puede devolver ceros). En .NET 6/8/10 siempre usa **`Random.Shared.Next(1, 101)`**, que está diseñado específicamente para alta concurrencia.
