using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using PokedexCLI.Models;
using Dapper;
using System.Data;



namespace PokedexCLI.Repositories
{
    public class PokemonRepository
    {
        private readonly string _connectionString;

        // Constructor: Recibe la configuración para leer el appsettings.json automáticamente
        public PokemonRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");
        }

        // Método para la Tarea 1.1: Listar todos los Pokémon desde la vista vw_PokemonDetalle
        public IEnumerable<PokemonDetalle> ObtenerTodos()
        {
            // Uso obligatorio de 'using var connection' para liberar memoria y cerrar la conexión al terminar
            using var connection = new SqlConnection(_connectionString);

            string query = "SELECT * FROM vw_PokemonDetalle";

            // Dapper ejecuta la consulta y mapea las filas directamente a una lista de objetos PokemonDetalle
            return connection.Query<PokemonDetalle>(query);
        }

        // Método para Buscar por Coincidencia de Nombre (Previene SQL Injection)
        public IEnumerable<PokemonDetalle> BuscarPorNombre(string textoBusqueda)
        {
            using var connection = new SqlConnection(_connectionString);

            string query = "SELECT * FROM vw_PokemonDetalle WHERE Nombre LIKE @Busqueda";

            // Pasamos un objeto anónimo para que Dapper limpie el parámetro de forma segura
            var parametros = new { Busqueda = $"%{textoBusqueda}%" };

            return connection.Query<PokemonDetalle>(query, parametros);
        }

        public CadenaEvolutiva? ObtenerCadenaEvolutiva(string nombre)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
        SELECT *
        FROM vw_CadenaEvolutiva
        WHERE PokemonActual = @Nombre";

            return connection.QueryFirstOrDefault<CadenaEvolutiva>(
                sql,
                new { Nombre = nombre });
        }

        public IEnumerable<AnalisisCombate>
          ObtenerAnalisisCombate(string nombre)
        {
            using var connection =
                new SqlConnection(_connectionString);

            string sql = @"
        SELECT *
        FROM vw_AnalisisCombate
        WHERE Pokemon = @Nombre";

            return connection.Query<AnalisisCombate>(
                sql,
                new { Nombre = nombre });
        }

        public (IEnumerable<PokemonRegion>, ResumenRegion?)
         ObtenerReporteRegion(int idRegion)
        {
            using var connection =
                new SqlConnection(_connectionString);

            using var multi =
                connection.QueryMultiple(
                    "sp_ReportePorRegion",
                    new { IDRegion = idRegion },
                    commandType: CommandType.StoredProcedure);

            var pokemones =
                multi.Read<PokemonRegion>();

            var resumen =
                multi.ReadFirstOrDefault<ResumenRegion>();

            return (pokemones, resumen);
        }

        public int AgregarPokemon(
          string nombre,
          double peso,
          double altura,
          int nPokedex,
          int idRegion,
          int idTipoPrincipal,
          int idHabilidad)
        {
            using var connection =
                new SqlConnection(_connectionString);

            var parametros = new
            {
                Nombre = nombre,
                Peso = peso,
                Altura = altura,
                NPokedex = nPokedex,
                IDRegion = idRegion,
                IDTipoPrincipal = idTipoPrincipal,
                IDHabilidad = idHabilidad
            };

            return connection.Execute(
                "sp_AgregarPokemon",
                parametros,
                commandType: CommandType.StoredProcedure);
        }

        public PokemonDetalle? ObtenerPokemonPorPokedex(int nPokedex)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
        SELECT
            IDPokemon,
            Nombre,
            Peso,
            Altura,
            NPokedex
        FROM Pokemon
        WHERE NPokedex = @NPokedex";

            return connection.QueryFirstOrDefault<PokemonDetalle>(
                sql,
                new { NPokedex = nPokedex });
        }
        public int EliminarPokemon(int idPokemon)
        {
            using var connection =
                new SqlConnection(_connectionString);

            return connection.Execute(
                "sp_EliminarPokemon",
                new { IDPokemon = idPokemon },
                commandType: CommandType.StoredProcedure);
        }

        public int ActualizarPokemon(
        int idPokemon,
        double peso,
        double altura)
        {
            using var connection =
                new SqlConnection(_connectionString);

            string sql = @"
        UPDATE Pokemon
        SET Peso = @Peso,
            Altura = @Altura
        WHERE IDPokemon = @IDPokemon";

            return connection.Execute(
                sql,
                new
                {
                    IDPokemon = idPokemon,
                    Peso = peso,
                    Altura = altura
                });
        }

        public IEnumerable<PokemonEliminado>
    ObtenerPokemonesEliminados()
        {
            using var connection =
                new SqlConnection(_connectionString);

            string sql = @"
        SELECT *
        FROM PokemonesEliminados
        ORDER BY FechaEliminacion DESC";

            return connection.Query<PokemonEliminado>(sql);
        }

        public IEnumerable<AuditoriaCambio>
    ObtenerAuditoriaCambios()
        {
            using var connection =
                new SqlConnection(_connectionString);

            string sql = @"
        SELECT *
        FROM AuditoriaCambiosPokemon
        ORDER BY FechaCambio DESC";

            return connection.Query<AuditoriaCambio>(sql);
        }


    }
}


