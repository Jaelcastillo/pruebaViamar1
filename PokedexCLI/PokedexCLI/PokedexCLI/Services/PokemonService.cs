using PokedexCLI.Models;
using PokedexCLI.Repositories;

namespace PokedexCLI.Services;

public class PokemonService
{
    private readonly IPokemonRepository _repository;

    public PokemonService(
        IPokemonRepository repository)
    {
        _repository = repository;
    }

    public IEnumerable<PokemonDetalle>
        ObtenerPaginaPokemon(
            int numeroPagina,
            int tamanoPagina)
    {
        if (numeroPagina < 1)
            throw new ArgumentOutOfRangeException(
                nameof(numeroPagina));

        if (tamanoPagina <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(tamanoPagina));

        return _repository
            .ObtenerTodos()
            .Skip((numeroPagina - 1) * tamanoPagina)
            .Take(tamanoPagina);
    }

    public ResultadoAnalisisCombate
    AnalizarCombateAgrupado(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException(
                "El nombre no puede estar vacío.");
        }

        var datos =
            _repository
            .ObtenerAnalisisCombate(nombre)
            .ToList();

        if (!datos.Any())
        {
            throw new InvalidOperationException(
                "No se encontró el Pokémon.");
        }

        var pokemon = datos.First();

        return new ResultadoAnalisisCombate
        {
            Pokemon = pokemon.Pokemon,

            TipoElemental = pokemon.TipoElemental,

            Ventajas = datos
                .Select(x => x.FuerteA)
                .Distinct()
                .ToList(),

            Debilidades = datos
                .Select(x => x.DebilA)
                .Distinct()
                .ToList()
        };
    }

    public void RegistrarNuevoPokemon(
    string nombre,
    double peso,
    double altura,
    int nPokedex,
    int idRegion,
    int idTipoPrincipal,
    int idHabilidad)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException(
                "El nombre es obligatorio.");
        }

        if (peso <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(peso));
        }

        if (altura <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(altura));
        }

        if (nPokedex <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(nPokedex));
        }

        _repository.AgregarPokemon(
            nombre,
            peso,
            altura,
            nPokedex,
            idRegion,
            idTipoPrincipal,
            idHabilidad);
    }

}