using PokedexCLI.Models;

namespace PokedexCLI.Repositories;

public interface IPokemonRepository
{
    IEnumerable<PokemonDetalle> ObtenerTodos();

    IEnumerable<PokemonDetalle> BuscarPorNombre(string textoBusqueda);

    CadenaEvolutiva? ObtenerCadenaEvolutiva(string nombre);

    IEnumerable<AnalisisCombate> ObtenerAnalisisCombate(string nombre);

    (IEnumerable<PokemonRegion>, ResumenRegion?)
        ObtenerReporteRegion(int idRegion);

    int AgregarPokemon(
        string nombre,
        double peso,
        double altura,
        int nPokedex,
        int idRegion,
        int idTipoPrincipal,
        int idHabilidad);

    PokemonDetalle? ObtenerPokemonPorId(int idPokemon);

    int EliminarPokemon(int idPokemon);

    int ActualizarPokemon(
        int idPokemon,
        double peso,
        double altura);

    IEnumerable<PokemonEliminado>
        ObtenerPokemonesEliminados();

    IEnumerable<AuditoriaCambio>
        ObtenerAuditoriaCambios();
}

