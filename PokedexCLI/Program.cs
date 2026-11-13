using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PokedexCLI.Models;
using PokedexCLI.Repositories;

class Program
{
    static void Main(string[] args)
    {
        // 1. Configurar el lector de appsettings.json
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // 2. Instanciar el repositorio pasándole la configuración
        PokemonRepository repository = new PokemonRepository(configuration);

        bool salir = false;

        while (!salir)
        {
            Console.Clear();
            Console.WriteLine("              --------------------------");
            Console.WriteLine("        POKÉDEX CLI (.NET + DAPPER) - GESTIÓN ");
            Console.WriteLine("            ---- >:( PONME MI 100 ERWIN :) ----");

            Console.WriteLine("1. Listar Pokédex General ");
            Console.WriteLine("2. Buscar Pokémon por Nombre");
            Console.WriteLine("3. Ver Cadena Evolutiva de un Pokémon");
            Console.WriteLine("4. Analizador de combate");
            Console.WriteLine("5. Reporte Estadístico por Región");
            Console.WriteLine("6. Registrar un nuevo Pokemon");
            Console.WriteLine("7. Eliminar Pokemon");
            Console.WriteLine("8. Actualizar Peso y Altura");
            Console.WriteLine("9. Centro de Auditoría");
            Console.WriteLine("0. Salir");
            Console.Write("Seleccione una opción: ");

            string? opcion = Console.ReadLine();

            switch (opcion)
            {
                case "1":
                    ListarPokemonGeneral(repository);
                    break;

                case "2":
                    BuscarPokemonPorNombre(repository);
                    break;

                case "3":
                    VerCadenaEvolutiva(repository);
                    break;

                case "4":
                    ObtenerAnalisisCombate(repository);
                    break;

                case "5":
                    ReportePorRegion(repository);
                    break;

                case "6":
                    RegistrarPokemon(repository);
                    break;

                case "7":
                    EliminarPokemon(repository);
                    break;

                case "8":
                    ActualizarPokemon(repository);
                    break;

                case "9":
                    CentroAuditoria(repository);
                    break;

                case "0":
                    salir = true;
                    Console.WriteLine("\nSaliendo de la Pokédex CLI. Hasta NUNCAA!");
                    break;
                default:
                    Console.WriteLine("\nOpción inválida. Presione cualquier tecla para intentar de nuevo.");
                    Console.ReadKey();
                    break;
            }
        }
    }




    // Método para la Tarea 1.1
    private static void ListarPokemonGeneral(PokemonRepository repository)
    {
        try
        {

            var pokemones = repository.ObtenerTodos().ToList();

            if (!pokemones.Any())
            {
                Console.Clear();
                Console.WriteLine("LISTADO GENERAL DE POKÉMON ");
                Console.WriteLine("No se encontraron Pokémon en la base de datos.");
                Console.WriteLine("\nPresione cualquier tecla para volver al menú...");
                Console.ReadKey();
                return;
            }

            int pageSize = 20;
            int pageIndex = 0;
            int totalPages = (int)Math.Ceiling(pokemones.Count / (double)pageSize);

            while (true)
            {
                Console.Clear();
                Console.WriteLine($" LISTADO GENERAL DE POKÉMON (Página {pageIndex + 1} de {totalPages}) ");
                Console.WriteLine("-------------");

                // mi funcion de linq para que solo me tire 20 y con los botones puedas ver 20 mas o 20 menos 
                var pokemonesPagina = pokemones.Skip(pageIndex * pageSize).Take(pageSize);

                foreach (var p in pokemonesPagina)
                {
                    Console.WriteLine($"[#{p.NPokedex}] {p.Nombre} | Peso: {p.Peso}kg | Altura: {p.Altura}m | Región: {p.Region}");
                }

                Console.WriteLine("-----------------------");
                Console.WriteLine("S Siguiente página    A Página anterior    M Volver al menú");
                Console.Write("Presione una tecla de navegación: ");


                var tecla = Console.ReadKey(true).Key;

                if (tecla == ConsoleKey.S)
                {
                    // funcionpara que si estoy en los ultimos 20 no me devuelva menos de ahi )
                    if (pageIndex < totalPages - 1)
                    {
                        pageIndex++;
                    }
                }
                else if (tecla == ConsoleKey.A)
                {

                    if (pageIndex > 0)
                    {
                        pageIndex--;
                    }
                }
                else if (tecla == ConsoleKey.M)
                {

                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.Clear();
            Console.WriteLine($"\n[ERROR]: Ocurrió un error al conectar con la base de datos: {ex.Message}");
            Console.WriteLine("\nPresione cualquier tecla para volver al menu");
            Console.ReadKey();
        }
    }
    // Método para la Tarea 1.1b: Búsqueda por Nombre
    private static void BuscarPokemonPorNombre(PokemonRepository repository)
    {
        Console.Clear();

        Console.WriteLine("=== BUSCAR POKÉMON POR NOMBRE ===");
        Console.Write("Ingrese el nombre o parte del nombre: ");

        string? texto = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(texto))
        {
            Console.WriteLine("\nDebe ingresar un texto válido.");
            Console.WriteLine("\nPresione cualquier tecla para volver al menú...");
            Console.ReadKey();
            return;
        }

        try
        {
            var resultados = repository.BuscarPorNombre(texto).ToList();

            if (!resultados.Any())
            {
                Console.WriteLine($"\n>:(");
                Console.WriteLine($"No se encontraron Pokémon que coincidan con '{texto}'.");
            }
            else
            {
                Console.WriteLine($"\nResultados encontrados: {resultados.Count}");

                foreach (var p in resultados)
                {
                    Console.WriteLine(
                        $"[#{p.NPokedex}] {p.Nombre}  {p.Region} ");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[ERROR] {ex.Message}");
        }

        Console.WriteLine("\nPresione cualquier tecla para volver al menú...");
        Console.ReadKey();
    }

    private static void VerCadenaEvolutiva(PokemonRepository repository)
    {
        Console.Clear();

        Console.WriteLine("=== CADENA EVOLUTIVA ===");
        Console.Write("Ingrese el nombre del Pokémon: ");

        string? nombre = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(nombre))
        {
            Console.WriteLine("\nDebe ingresar un nombre válido.");
            Console.WriteLine("\nPresione cualquier tecla para volver al menú...");
            Console.ReadKey();
            return;
        }

        try
        {
            var cadena = repository.ObtenerCadenaEvolutiva(nombre);

            if (cadena == null)
            {
                Console.WriteLine("\n>:(");
                Console.WriteLine("No se encontró el Pokémon.");
            }
            else
            {
                Console.WriteLine("\n RESULTADO ");

                Console.WriteLine(
                    $"  Pre Evolucion  [{cadena.PreEvolucion}] -> " +
                    $"  Pokemon Actual [{cadena.PokemonActual}] -> " +
                    $"  Siguiente Evolucion [{cadena.SiguienteEvolucion}]");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[ERROR] {ex.Message}");
        }

        Console.WriteLine("\nPresione cualquier tecla para volver al menú...");
        Console.ReadKey();


    }    //tarea 1.3 Analisis de combate 
    private static void ObtenerAnalisisCombate(PokemonRepository repository)
    {
        Console.Clear();

        Console.WriteLine("=== ANALISIS DE COMBATE ===");
        Console.Write("Ingrese el nombre del Pokémon: ");

        string? nombre = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(nombre))
        {
            Console.WriteLine("\nDebe ingresar un nombre válido.");
            Console.WriteLine("\nPresione cualquier tecla para volver al menú...");
            Console.ReadKey();
            return;
        }

        try
        {
            var datos = repository.ObtenerAnalisisCombate(nombre).ToList();

            if (!datos.Any())
            {
                Console.WriteLine("\n>:( No se encontró ese Pokémon.");
            }
            else
            {
                var pokemon = datos.First();

                Console.WriteLine($"\nPokémon: {pokemon.Pokemon}");
                Console.WriteLine($"Tipo Elemental: {pokemon.TipoElemental}");

                var fuertes = datos
                    .GroupBy(x => x.FuerteA)
                    .Select(g => g.Key)
                    .Distinct()
                    .ToList();

                var debiles = datos
                    .GroupBy(x => x.DebilA)
                    .Select(g => g.Key)
                    .Distinct()
                    .ToList();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nFUERTE CONTRA:");

                foreach (var fuerte in fuertes)
                {
                    Console.WriteLine($" + {fuerte}");
                }

                Console.ResetColor();

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nDEBIL CONTRA:");

                foreach (var debil in debiles)
                {
                    Console.WriteLine($" - {debil}");
                }

                Console.ResetColor();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[ERROR] {ex.Message}");
        }

        Console.WriteLine("\nPresione cualquier tecla para volver al menú...");
        Console.ReadKey();
    }
    //Módulo 2: Ejecución de Stored Procedures con Dapper


    //Tarea 2.1 resumen por region 
    private static void ReportePorRegion(PokemonRepository repository)
    {
        Console.Clear();

        Console.WriteLine("=== REPORTE POR REGION ===");

        Console.Write("Ingrese el ID de la región: ");

        if (!int.TryParse(Console.ReadLine(), out int idRegion))
        {
            Console.WriteLine("\nDebe ingresar un número válido.");
            Console.WriteLine("\nPresione cualquier tecla para volver al menú...");
            Console.ReadKey();
            return;
        }

        try
        {
            var resultado = repository.ObtenerReporteRegion(idRegion);

            var pokemones = resultado.Item1.ToList();

            var resumen = resultado.Item2;

            Console.WriteLine("\nPOKÉMON DE LA REGIÓN:\n");

            foreach (var pokemon in pokemones)
            {
                Console.WriteLine(
                    $"#{pokemon.NPokedex} - {pokemon.Nombre} | Peso: {pokemon.Peso} | Altura: {pokemon.Altura}");
            }

            if (resumen != null)
            {
                Console.WriteLine("\n=== RESUMEN ===");

                Console.WriteLine($"Cantidad Total: {resumen.CantidadTotalPokemon}");
                Console.WriteLine($"Peso Promedio: {resumen.PesoPromedio}");
                Console.WriteLine($"Altura Máxima: {resumen.AlturaMaxima}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[ERROR] {ex.Message}");
        }

        Console.WriteLine("\nPresione cualquier tecla para volver al menú...");
        Console.ReadKey();
    }
    //tarea 2.2  Registrar un Nuevo Pokémon

    private static int LeerEntero(string mensaje)
    {
        while (true)
        {
            Console.Write(mensaje);

            if (int.TryParse(Console.ReadLine(), out int valor))
                return valor;

            Console.WriteLine("Debe ingresar un número válido.");
        }
    }

    private static double LeerDouble(string mensaje)
    {
        while (true)
        {
            Console.Write(mensaje);

            if (double.TryParse(Console.ReadLine(), out double valor))
                return valor;

            Console.WriteLine("Debe ingresar un numero valido.");
        }
    }

    private static void RegistrarPokemon(
    PokemonRepository repository)
    {
        Console.Clear();

        Console.WriteLine(" REGISTRAR NUEVO POKEMON ");

        string nombre;

        Console.Write("Nombre: ");
        nombre = Console.ReadLine() ?? "";

        double peso = LeerDouble("Peso: ");

        double altura = LeerDouble("Altura: ");

        int nPokedex = LeerEntero("Numero Pokedex: ");

        int idRegion = LeerEntero("ID Región: ");

        int idTipoPrincipal = LeerEntero("ID Tipo Principal: ");

        int idHabilidad = LeerEntero("ID Habilidad: ");

        try
        {
            repository.AgregarPokemon(
                nombre,
                peso,
                altura,
                nPokedex,
                idRegion,
                idTipoPrincipal,
                idHabilidad);

            Console.ForegroundColor =
                ConsoleColor.Green;

            Console.WriteLine(
                "\n[OK] Pokemon registrado correctamente.");

            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor =
                ConsoleColor.Red;

            Console.WriteLine(
                $"\n[ERROR] {ex.Message}");

            Console.ResetColor();
        }

        Console.WriteLine(
            "\nPresione cualquier tecla para volver al menu");

        Console.ReadKey();
    }

    //tarea 2.3 Eliminación Segura

private static void EliminarPokemon(
    PokemonRepository repository)
{
    Console.Clear();

    Console.WriteLine("=== ELIMINAR POKÉMON ===");

    int idPokemon =
        LeerEntero("Ingrese el ID del Pokémon: ");

    try
    {
        var pokemon =
            repository.ObtenerPokemonPorId(idPokemon);

        if (pokemon == null)
        {
            Console.WriteLine(
                "\n>:( No existe un Pokémon con ese ID.");
        }
        else
        {
            Console.WriteLine(
                $"\nPokémon encontrado: {pokemon.Nombre}");

            Console.Write(
                "\n¿Está seguro que desea eliminarlo? (S/N): ");

            string? respuesta = Console.ReadLine();

            if (respuesta?.ToUpper() == "S")
            {
                repository.EliminarPokemon(idPokemon);

                Console.ForegroundColor =
                    ConsoleColor.Green;

                Console.WriteLine(
                    "\n[OK] Pokémon eliminado correctamente.");

                Console.ResetColor();
            }
            else
            {
                Console.WriteLine(
                    "\nOperación cancelada.");
            }
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor =
            ConsoleColor.Red;

        Console.WriteLine(
            $"\n[ERROR] {ex.Message}");

        Console.ResetColor();
    }

    Console.WriteLine(
        "\nPresione cualquier tecla para volver al menú...");

    Console.ReadKey();
}

    private static void ActualizarPokemon(
    PokemonRepository repository)
    {
        Console.Clear();

        Console.WriteLine("=== ACTUALIZAR PESO Y ALTURA ===");

        int idPokemon =
            LeerEntero("Ingrese el ID del Pokémon: ");

        double peso =
            LeerDouble("Nuevo Peso: ");

        double altura =
            LeerDouble("Nueva Altura: ");

        try
        {
            int filas =
                repository.ActualizarPokemon(
                    idPokemon,
                    peso,
                    altura);

            if (filas > 0)
            {
                Console.ForegroundColor =
                    ConsoleColor.Green;

                Console.WriteLine(
                    "\n[OK] Pokémon actualizado correctamente.");

                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor =
                    ConsoleColor.Yellow;

                Console.WriteLine(
                    "\n[!] No existe un Pokémon con ese ID.");

                Console.ResetColor();
            }
        }
        catch (SqlException ex)
        {
            Console.ForegroundColor =
                ConsoleColor.Red;

            Console.WriteLine(
                $"\n[ERROR SQL] {ex.Message}");

            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor =
                ConsoleColor.Red;

            Console.WriteLine(
                $"\n[ERROR] {ex.Message}");

            Console.ResetColor();
        }

        Console.WriteLine(
            "\nPresione cualquier tecla para volver al menú...");

        Console.ReadKey();
    }

    private static void CentroAuditoria(
    PokemonRepository repository)
    {
        bool volver = false;

        while (!volver)
        {
            Console.Clear();

            Console.WriteLine("=== CENTRO DE AUDITORIA ");
            Console.WriteLine("1. Ver Pokémon Eliminados");
            Console.WriteLine("2. Ver Cambios de Peso y Altura");
            Console.WriteLine("0. Volver");

            Console.Write("Seleccione: ");

            string? opcion = Console.ReadLine();

            switch (opcion)
            {
                case "1":
                    VerPokemonesEliminados(repository);
                    break;

                case "2":
                    VerCambios(repository);
                    break;

                case "0":
                    volver = true;
                    break;
            }
        }
    }

    private static void VerPokemonesEliminados(
    PokemonRepository repository)
    {
        Console.Clear();

        Console.WriteLine("HISTORIAL DE POKÉMON ELIMINADOS ");

        try
        {
            var eliminados =
                repository.ObtenerPokemonesEliminados();

            foreach (var p in eliminados)
            {
                Console.WriteLine(
                    $"{p.IDPokemon} - {p.Nombre}");

                Console.WriteLine(
                    $"Fecha: {p.FechaEliminacion}");

            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"\n[ERROR] {ex.Message}");
        }

        Console.WriteLine(
            "\nPresione cualquier tecla para continuar...");

        Console.ReadKey();
    }

    private static void VerCambios(
PokemonRepository repository)
    {
        Console.Clear();

        Console.WriteLine(
            "=== HISTORIAL DE CAMBIOS ===");

        try
        {
            var cambios =
                repository.ObtenerAuditoriaCambios();

            foreach (var cambio in cambios)
            {
                Console.WriteLine(
                    $"Pokemon ID: {cambio.IDPokemon}");

                Console.WriteLine(
                    $"Peso: {cambio.PesoAnterior} -> {cambio.PesoNuevo}");

                Console.WriteLine(
                    $"Altura: {cambio.AlturaAnterior} -> {cambio.AlturaNueva}");

                Console.WriteLine(
                    $"Usuario: {cambio.Usuario}");

                Console.WriteLine(
                    $"Fecha: {cambio.FechaCambio}");

            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"\n[ERROR] {ex.Message}");
        }

        Console.WriteLine(
            "\nPresione una tecla para continuar...");

        Console.ReadKey();
    }

}
