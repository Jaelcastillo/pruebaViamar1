# 💻 Fase 2: Práctica de Desarrollo en .NET (C#) con Dapper — Proyecto `PokedexCLI`

¡Felicidades por completar la Fase 1 de SQL Server! Ahora que ya tienes construidas tus **Vistas**, **Procedimientos Almacenados** y **Triggers** en la base de datos `IntegradorPokemonFyM`, el siguiente paso es construir una **Aplicación de Consola en .NET (C#)** que consuma todo lo que creaste en el motor de base de datos.

Para el acceso a datos utilizaremos **Dapper**, el micro-ORM más rápido y utilizado en el ecosistema .NET para trabajar directamente con consultas SQL y Stored Procedures mapeando automáticamente los resultados a clases de C#. *(Más adelante, cuando pasemos a construir Web APIs, aprenderás Entity Framework Core).*

---

## 🛠️ Requisitos Técnicos y Arquitectura Base

Antes de programar las opciones del menú, tu proyecto debe cumplir con estas buenas prácticas de .NET:

1. **Versión y Paquetes NuGet:**
   * Crea un proyecto de Aplicación de Consola en .NET (`.NET 8` o `.NET 6+`).
   * Instala los siguientes paquetes NuGet:
     * `Dapper` (El micro-ORM para ejecutar consultas y mapear objetos).
     * `Microsoft.Data.SqlClient` (El proveedor de conexión a SQL Server).
     * `Microsoft.Extensions.Configuration.Json` (Para leer la configuración desde un archivo JSON).
2. **Prohibido quemar el Connection String en el código (Reto de Configuración):**
   * No debes poner tu cadena de conexión dentro de una variable `string` fija en `Program.cs`.
   * Agrega un archivo `appsettings.json` al proyecto y lee la cadena de conexión desde ahí.
3. **Separación en Capas / Carpetas (Arquitectura Limpia):**
   Organiza tu proyecto en al menos estas 3 áreas:
   * 📁 `Models/`: Clases C# (POCOs) cuyas propiedades coincidan con las columnas que devuelven tus Vistas y Stored Procedures (ej. `PokemonDetalle`, `CadenaEvolutiva`, `AnalisisCombate`, `ResumenRegion`, `AuditoriaCambio`).
   * 📁 `Repositories/` (o `Data/`): La clase `PokemonRepository.cs` donde vivirá todo el código de conexión a SQL y los métodos de Dapper (`Query`, `QueryFirstOrDefault`, `QueryMultiple`, `Execute`). **Regla de oro: No debe haber ningún `Console.WriteLine` ni `Console.ReadLine` dentro del Repositorio.**
   * 📄 `Program.cs` (o `MenuUI.cs`): Donde se dibuja el menú interactivo, se piden los datos al usuario por teclado y se muestran los resultados en pantalla.
4. **Uso obligatorio de `using`:**
   * Toda instancia de `SqlConnection` debe crearse con `using var connection = new SqlConnection(_connectionString);` para garantizar que las conexiones a SQL Server se cierren y liberen memoria automáticamente.

---

## 📋 Módulo 1: Lectura de Datos y Consumo de Vistas con Dapper
*Objetivo: Mapear resultados de SQL directamente a listas de objetos en C# (`IEnumerable<T>`) usando `connection.Query<T>()` y manipular colecciones con LINQ.*

### Tarea 1.1: Buscador y Listado General (Consumiendo `vw_PokemonDetalle`)
* **Requerimiento:** En el menú, permite al usuario:
  * **a)** Ver el listado general de Pokémon consultando tu vista `vw_PokemonDetalle` mediante `connection.Query<PokemonDetalle>("SELECT * FROM vw_PokemonDetalle")`.
  * **b)** Buscar un Pokémon por coincidencia de nombre (ej. si escribe `"char"`, debe mostrar a *Charmander*, *Charmeleon* y *Charizard*) enviando objetos anónimos como parámetros en Dapper (`new { Busqueda = $"%{texto}%" }`) para **prevenir Inyección SQL (SQL Injection)**.
* **🔥 Reto C# #1 (Paginación en Consola con LINQ):**
  Como existen más de 800 Pokémon, no los imprimas todos de golpe. Muestra **20 Pokémon por página** usando los métodos `.Skip()` y `.Take()` de **LINQ**, permitiendo al usuario presionar `S` (Siguiente página), `A` (Página anterior) o `M` (Volver al menú).

### Tarea 1.2: Consulta de Línea Evolutiva (Consumiendo `vw_CadenaEvolutiva`)
* **Requerimiento:** Pide al usuario el ID o Nombre de un Pokémon y consulta tu vista `vw_CadenaEvolutiva` usando `connection.QueryFirstOrDefault<CadenaEvolutiva>(...)`.
* **Validación:** Si el método devuelve `null` (el Pokémon no existe), muestra un aviso amigable. Si existe, muéstralo con formato claro:
  `[Pre-evolución: Charmander] ➔ [Actual: Charmeleon] ➔ [Siguiente: Charizard]`

### Tarea 1.3: Simulador de Análisis de Combate (Consumiendo `vw_AnalisisCombate`)
* *(Nota previa: Asegúrate de haber corregido el `JOIN` de tu vista `vw_AnalisisCombate` según el feedback de la Fase 1).*
* **Requerimiento:** Pide el nombre de un Pokémon, consulta `vw_AnalisisCombate` con Dapper y muestra su Tipo Elemental, la lista de tipos contra los que es **Fuerte** (en color verde usando `Console.ForegroundColor`) y la lista de tipos contra los que es **Débil** (en color rojo).
* **🔥 Reto C# #2 (Agrupación con LINQ `GroupBy` y `Distinct`):**
  Dado que la vista devuelve varias filas por Pokémon (una por cada combinación de ventaja/desventaja), utiliza `.GroupBy()`, `.Select()` y `.Distinct()` de **LINQ en C#** para agrupar los resultados en memoria y mostrar cada ventaja y debilidad sin repetirla en pantalla.

---

## ⚙️ Módulo 2: Ejecución de Stored Procedures con Dapper (CRUD)
*Objetivo: Ejecutar procedimientos almacenados con `commandType: CommandType.StoredProcedure`, usar `DynamicParameters` y leer múltiples resultados con `QueryMultiple`.*

### Tarea 2.1: Reporte Estadístico por Región (Consumiendo `sp_ReportePorRegion`)
* **Requerimiento:** Pide al usuario el `@IDRegion` y ejecuta el procedimiento `sp_ReportePorRegion`.
* **🔥 Reto C# #3 (Múltiples Result Sets en Dapper con `QueryMultiple`):**
  Recuerda que tu procedimiento `sp_ReportePorRegion` devuelve **dos `SELECT` distintos** en una sola ejecución:
  1. Primero la lista de Pokémon de la región.
  2. Segundo la fila de resumen (`CantidadTotalPokemon`, `PesoPromedio`, `AlturaMaxima`).
  * Investiga cómo utilizar el método **`connection.QueryMultiple("sp_ReportePorRegion", new { IDRegion = idRegion }, commandType: CommandType.StoredProcedure)`** de Dapper junto con `multi.Read<PokemonRegion>()` y `multi.ReadFirstOrDefault<ResumenRegion>()` para obtener ambos resultados en una sola llamada a la base de datos.

### Tarea 2.2: Registrar un Nuevo Pokémon (Consumiendo `sp_AgregarPokemon`)
* **Requerimiento:** Pide por consola los datos necesarios (`Nombre`, `Peso`, `Altura`, `NPokedex`, `IDRegion`, `IDTipoPrincipal`, `IDHabilidad`) y ejecuta `sp_AgregarPokemon` utilizando `connection.Execute("sp_AgregarPokemon", parametros, commandType: CommandType.StoredProcedure)`.
* **🔥 Reto C# #4 (Validación Defensiva de Inputs con `TryParse`):**
  Crea métodos auxiliares reutilizables en C# (por ejemplo, `LeerDouble(string mensaje)` y `LeerEntero(string mensaje)`) que usen `double.TryParse` / `int.TryParse` dentro de un ciclo `while`. Si el usuario escribe letras como `"abc"` cuando se le pide el Peso o el ID de Región, el programa **no debe cerrarse con un `FormatException`**, sino pedirle el dato nuevamente hasta que ingrese un número válido.

### Tarea 2.3: Eliminación Segura (Consumiendo `sp_EliminarPokemon`)
* **Requerimiento:** Pide el `@IDPokemon` a eliminar, busca primero su nombre con Dapper para mostrarlo en pantalla y pide confirmación (`¿Está seguro que desea eliminar a [Nombre]? S/N`). Si confirma con `S`, ejecuta `sp_EliminarPokemon` con `connection.Execute(...)`.

---

## 🛡️ Módulo 3: Interacción con Triggers, Excepciones y Asincronía
*Objetivo: Capturar errores personalizados de SQL Server (`SqlException`) en C# y consultar las tablas de auditoría con Dapper.*

### Tarea 3.1: Actualización de Peso/Altura y Prueba de Trigger de Validación
* **Requerimiento:** Crea una opción en el menú para **"Actualizar Peso y Altura de un Pokémon"** ejecutando un `UPDATE` parametrizado con `connection.Execute(...)`.
* **🔥 Reto C# #5 (Manejo de `SqlException` disparado por Triggers y SPs):**
  Envuelve las llamadas de tu menú o servicio en bloques `try / catch (SqlException ex)`.
  * Prueba ingresar a propósito un **Peso negativo (ej. `-10`)** desde tu consola de C#, o una región inexistente (`9999`) en el reporte por región.
  * Tu aplicación de C# debe capturar el `SqlException` lanzado por tu trigger `tr_ValidarDatosPokemon` (o por el `THROW` de tus SPs), mostrar en consola el mensaje exacto que viene desde SQL Server (`ex.Message`) en color amarillo/rojo, y **volver al menú principal sin que el programa se cierre ni explote**.

### Tarea 3.2: Panel de Auditoría en Consola
* **Requerimiento:** Crea una opción en el menú llamada **"Centro de Auditoría"** con dos sub-opciones mapeadas con Dapper (`connection.Query<T>`):
  1. **Ver Historial de Pokémon Eliminados:** Consulta la tabla `PokemonesEliminados` (alimentada automáticamente por tu trigger `tr_AuditarPokemonEliminado`) mostrando quién fue eliminado y en qué fecha/hora.
  2. **Ver Historial de Cambios de Peso/Altura:** Consulta la tabla `AuditoriaCambiosPokemon` (alimentada por tu trigger `tr_AuditarCambiosPokemon`) mostrando el ID del Pokémon, `PesoAnterior ➔ PesoNuevo`, `AlturaAnterior ➔ AlturaNueva`, el `Usuario` y la `FechaCambio`.

### 🌟 Reto Bonus (Opcional - Nivel Pro): Dapper Asíncrono (`async / await`)
En el desarrollo profesional en .NET, todas las operaciones de base de datos se hacen de forma asíncrona para no bloquear hilos.
* Utiliza los métodos asíncronos de Dapper en tu `PokemonRepository`:
  * `await connection.QueryAsync<T>(...)`
  * `await connection.QueryFirstOrDefaultAsync<T>(...)`
  * `await connection.QueryMultipleAsync(...)`
  * `await connection.ExecuteAsync(...)`

---

## 🖥️ Ejemplo de Menú Esperado en Consola

```text
==================================================
     🔴 POKÉDEX CLI (.NET + DAPPER) - GESTIÓN ⚪
==================================================
1. Listar Pokédex General (Paginado)
2. Buscar Pokémon por Nombre
3. Ver Cadena Evolutiva de un Pokémon
4. Analizador de Combate (Ventajas y Debilidades)
5. Reporte Estadístico por Región (QueryMultiple)
6. Registrar Nuevo Pokémon
7. Actualizar Peso / Altura de un Pokémon
8. Eliminar Pokémon
9. Centro de Auditoría (Eliminados y Modificados)
0. Salir
==================================================
Seleccione una opción: _
```

---

## ✅ Entregables Esperados
1. Sube la carpeta o solución de tu proyecto .NET (`PokedexCLI`) al repositorio de GitHub.
2. Incluye un archivo `.gitignore` estándar de Visual Studio / .NET (`dotnet new gitignore`) para **no subir** las carpetas compiladas `bin/` ni `obj/`.
3. El código debe compilar sin errores (`dotnet build`) y respetar la separación en carpetas (`Models`, `Repositories`, `Program.cs`).
