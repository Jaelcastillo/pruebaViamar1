# 📊 Revisión y Feedback — Fase 2: CRUD en Consola con .NET y Dapper (`PokedexCLI`)

¡Excelente trabajo con la construcción de **`PokedexCLI`**! Se nota un gran avance en la estructuración del código en C#, el manejo de **Dapper** y el uso de **LINQ**. 

Vimos tu mensaje en el menú principal (`---- >:( PONME MI 100 ERWIN :) ----` 😂) y estuviste **muy cerca** de lograrlo. Abajo encontrarás qué cosas te quedaron de 10 y los pequeños detalles por los que se escaparon esos últimos puntos.

---

## 🏆 Calificación de la Fase 2 (.NET + Dapper): **8.8 / 10 (88 / 100)** 🌟

| Criterio Evaluado | Estado | Puntuación |
| :--- | :---: | :---: |
| **Arquitectura y Separación de Responsabilidades (`Models` / `Repository` / `UI`)** | ✅ Excelente | 10 / 10 |
| **Uso de Dapper (`Query`, `QueryFirstOrDefault`, `QueryMultiple`, `Execute`)** | ✅ Excelente | 10 / 10 |
| **Retos de C# (Paginación LINQ, `GroupBy`, `TryParse`, Colores, `SqlException`)** | ✅ Excelente | 9.5 / 10 |
| **Compilación Limpia, Configuración (`.csproj`) y Git (`.gitignore`)** | ⚠️ Con detalles | 6.0 / 10 |

---

## 🌟 1. Lo que hiciste EXCELENTE (Puntos a destacar)

1. **Separación Limpia en `PokemonRepository.cs`:**
   * Cumpliste la regla de oro al 100%: no pusiste ni un solo `Console.WriteLine` o `Console.ReadLine` dentro del repositorio.
   * Recibiste `IConfiguration` por constructor y validaste con el operador `?? throw new InvalidOperationException(...)` que la cadena de conexión exista.
   * Usaste `using var connection = new SqlConnection(_connectionString);` en todos los métodos y pasaste todos los parámetros con objetos anónimos (`new { ... }`), protegiendo la aplicación contra **SQL Injection**.
2. **Reto #3 Superado — Uso de `QueryMultiple` y Tuplas:**
   * ¡Tu método `ObtenerReporteRegion(int idRegion)` quedó impecable! Usaste `connection.QueryMultiple("sp_ReportePorRegion", ...)` para leer primero la lista con `multi.Read<PokemonRegion>()` y luego el resumen con `multi.ReadFirstOrDefault<ResumenRegion>()`, retornando ambos en una tupla `(IEnumerable<PokemonRegion>, ResumenRegion?)`.
3. **Reto #1 Superado — Paginador en Consola con LINQ:**
   * Muy buena lógica calculando `Math.Ceiling(pokemones.Count / (double)pageSize)`, aplicando `.Skip(pageIndex * pageSize).Take(pageSize)` y leyendo las teclas con `Console.ReadKey(true).Key` (`S`, `A`, `M`) cuidando los bordes de la primera y última página.
4. **Reto #2 Superado — Agrupación con LINQ y Colores en `ObtenerAnalisisCombate`:**
   * Excelente uso de `.GroupBy().Select().Distinct().ToList()` y el manejo de `Console.ForegroundColor` (`Green` / `Red`) restaurando siempre el color original con `Console.ResetColor()`.
5. **Retos #4 y #5 Superados — Validación con `TryParse` y Captura de `SqlException`:**
   * Tus métodos auxiliares `LeerEntero(string mensaje)` y `LeerDouble(string mensaje)` evitan que el programa se rompa si el usuario ingresa letras.
   * En `ActualizarPokemon` separaste correctamente `catch (SqlException ex)` para distinguir los errores que vienen desde tus Triggers de SQL Server.

---

## 🚨 2. Errores Críticos a Corregir (Por qué no tuviste el 100)

Si alguien clona tu repositorio y ejecuta `dotnet build` o `dotnet run`, el proyecto falla por estos 3 motivos:

### ❌ Error 1: Una letra `s` suelta al final de `Models/PokemonDetalle.cs`
En la línea 12 de `PokemonDetalle.cs`, justo después de cerrar la llave `}` del namespace, se quedó escrita una letra `s` por accidente:
```csharp
namespace PokedexCLI.Models
{
    public class PokemonDetalle
    {
        ...
    }
}
s // <-- ¡Esta 's' rompe la compilación (Error CS0116)!
```
* **Impacto:** El compilador de C# no permite texto suelto fuera de una clase o namespace, por lo que el proyecto no compila hasta borrar esa `s`.

### ❌ Error 2: El archivo `PokedexCLI.csproj` se subió incompleto
El archivo `PokedexCLI.csproj` que subiste a GitHub solo tiene las 10 líneas por defecto y **no incluye los `<PackageReference>`** de los paquetes NuGet que usaste (`Dapper`, `Microsoft.Data.SqlClient`, `Microsoft.Extensions.Configuration.Json`), ni tampoco la instrucción para copiar `appsettings.json` a la carpeta de salida.
* **¿Por qué pasó?** Muy probablemente instalaste los paquetes en Visual Studio, pero subiste el archivo `.csproj` a GitHub antes de guardar toda la solución (`Ctrl + Shift + S`).
* **Cómo debe quedar tu `PokedexCLI.csproj`:**
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework> <!-- o net10.0 -->
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Dapper" Version="2.1.35" />
    <PackageReference Include="Microsoft.Data.SqlClient" Version="5.2.2" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.1" />
  </ItemGroup>

  <ItemGroup>
    <None Update="appsettings.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>

</Project>
```
*(Nota: Sin el bloque `<CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>`, cuando el programa corre busca `appsettings.json` dentro de `bin/Debug/...` y lanza `FileNotFoundException`).*

### ❌ Error 3: Subiste la carpeta `obj/` y faltó el `.gitignore`
En los entregables se pidió agregar un `.gitignore` para no subir archivos temporales de compilación (`bin/` y `obj/`), pero la carpeta `PokedexCLI/obj/` fue subida al repositorio.
* **Tip:** Antes de tu primer commit en un proyecto .NET, ejecuta en la terminal:
  ```bash
  dotnet new gitignore
  ```
  Eso crea automáticamente el archivo `.gitignore` oficial de .NET.

---

## 🔧 3. Detalles Lógicos en C# para Mejorar

### 1. En `RegistrarPokemon` (`Program.cs`, líneas 458-480) el mensaje de éxito desaparece al instante
Mira dónde quedaron las llaves del `try` y del `catch` en tu método `RegistrarPokemon`:
```csharp
        try
        {
            repository.AgregarPokemon(...);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n[OK] Pokémon registrado correctamente.");
            Console.ResetColor();
        } // <-- Aquí cerraste el try, ¡pero no hay ReadKey() afuera!
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[ERROR] {ex.Message}");
            Console.ResetColor();

            Console.WriteLine("\nPresione cualquier tecla para volver al menú...");
            Console.ReadKey(); // <-- El ReadKey quedó SOLO dentro del catch
        }
```
* **¿Qué pasa al ejecutarlo?** Cuando el Pokémon se registra **con éxito**, imprime el mensaje en verde, pero como no entra al `catch`, termina el método inmediatamente, vuelve al `while (!salir)` del menú principal y ejecuta `Console.Clear()`. El usuario nunca alcanza a leer `"[OK] Pokémon registrado correctamente."`.
* **Solución:** Saca el `Console.WriteLine("\nPresione cualquier tecla...")` y el `Console.ReadKey()` **afuera** del bloque `catch` (al final del método), tal como sí lo hiciste en `EliminarPokemon` y `ActualizarPokemon`.

### 2. Confusión entre `IDPokemon` y `NPokedex` en `EliminarPokemon` (`Program.cs`, líneas 492-517)
En `EliminarPokemon` haces lo siguiente:
1. Pides: `int idPokemon = LeerEntero("Ingrese el ID del Pokémon: ");`
2. Buscas el Pokémon llamando a `repository.ObtenerPokemonPorPokedex(idPokemon)` (el cual busca en SQL con `WHERE NPokedex = @NPokedex`).
3. Pero luego lo eliminas llamando a `repository.EliminarPokemon(idPokemon)` (el cual borra en SQL con `WHERE IDPokemon = @IDPokemon`).
* **¿Por qué es peligroso?** En los 809 Pokémon originales el `IDPokemon` y el `NPokedex` coinciden, pero cuando agregas un Pokémon nuevo (como el que probaste en la Tarea 2.2 con `@NPokedex = 20`), su `IDPokemon` autogenerado será `810` mientras su `NPokedex` es `20`. Si el usuario pone `810`, `ObtenerPokemonPorPokedex(810)` dirá que no existe; y si pone `20`, le mostrará a *Raticate* (`NPokedex = 20`) o al nuevo y terminará borrando el registro equivocado.
* **Solución:** Cambia el método del repositorio a `ObtenerPokemonPorId(int idPokemon)` filtrando con `WHERE IDPokemon = @IDPokemon`.

### 3. Columna `Tipo` en `PokemonDetalle.cs`
En tu vista `vw_PokemonDetalle` hiciste un gran trabajo concatenando los tipos con `STRING_AGG(t.Nombre, ' / ') AS Tipo`, pero olvidaste agregar la propiedad `public string Tipo { get; set; } = string.Empty;` en tu clase `PokemonDetalle.cs` para mostrar el tipo en el listado general.

### 4. Limpieza de `using` innecesario en `Program.cs`
En la línea 3 de `Program.cs` se coló un `using Microsoft.IdentityModel.Tokens;` (que Visual Studio suele autocompletar por error). Como no se usa en el archivo, es buena práctica eliminarlo.

---
