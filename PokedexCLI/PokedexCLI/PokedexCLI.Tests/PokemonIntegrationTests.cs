using Microsoft.Extensions.Configuration;
using PokedexCLI.Repositories;

public class PokemonIntegrationTests
{
    private readonly PokemonRepository _repository;

    public PokemonIntegrationTests()
    {
        IConfiguration configuration =
            new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile(
                "appsettings.json",
                optional: false)
            .Build();

        _repository =
            new PokemonRepository(configuration);
    }

    [Fact]
    public void BuscarPokemonPorNombre_DebeEncontrarPikachu()
    {
        var resultado =
            _repository
            .BuscarPorNombre("Pikachu")
            .FirstOrDefault();

        Assert.NotNull(resultado);
    }

    [Fact]
    public void CrearYBuscarPokemon_DebeFuncionar()
    {
        string nombre =
            $"TestMon_Auto_{Guid.NewGuid():N}";

        int numeroPokedex =
            new Random().Next(900000, 999999);

        _repository.AgregarPokemon(
            nombre,
            15.5,
            1.2,
            numeroPokedex,
            1,
            1,
            1);

        var pokemon =
            _repository
            .BuscarPorNombre(nombre)
            .FirstOrDefault();

        Assert.NotNull(pokemon);
    }

    [Fact]
    public void ActualizarPokemon_DebeGenerarRegistroDeAuditoria()
    {
        // Arrange
        string nombre =
            $"TestMon_Audit_{Guid.NewGuid():N}";

        int numeroPokedex =
            new Random().Next(900000, 999999);

        _repository.AgregarPokemon(
            nombre,
            15.5,
            1.2,
            numeroPokedex,
            1,
            1,
            1);

        var pokemon =
            _repository
            .BuscarPorNombre(nombre)
            .First();

        // Act
        _repository.ActualizarPokemon(
            pokemon.IDPokemon,
            25.0,
            1.8);

        var auditorias =
            _repository.ObtenerAuditoriaCambios();

        // Assert
        Assert.Contains(
            auditorias,
            a =>
                a.IDPokemon == pokemon.IDPokemon &&
                Math.Abs(a.PesoNuevo - 25.0) < 0.01);
    }

    [Fact]
    public void EliminarPokemon_DebeGenerarRegistroDeEliminacion()
    {
        string nombre =
            $"TestMon_Delete_{Guid.NewGuid():N}";

        int numeroPokedex =
            new Random().Next(900000, 999999);

        _repository.AgregarPokemon(
            nombre,
            10.0,
            1.0,
            numeroPokedex,
            1,
            1,
            1);

        var pokemon =
            _repository
            .BuscarPorNombre(nombre)
            .First();

        Assert.NotNull(pokemon);

        Assert.True(
            pokemon.IDPokemon > 0,
            $"IDPokemon inválido: {pokemon.IDPokemon}");

        _repository.EliminarPokemon(
            pokemon.IDPokemon);

        var eliminado =
            _repository.ObtenerPokemonPorId(
                pokemon.IDPokemon);

        Assert.Null(eliminado);

        var auditoria =
            _repository.ObtenerPokemonesEliminados();

        Assert.Contains(
            auditoria,
            p => p.Nombre == nombre);
    }

    [Fact]
    public void ActualizarPokemon_ConPesoNegativo_DebeLanzarSqlException()
    {
        var ex = Assert.Throws<Microsoft.Data.SqlClient.SqlException>(
            () => _repository.ActualizarPokemon(
                1,
                -50.0,
                1.0));

        Assert.Contains(
            "mayores",
            ex.Message,
            StringComparison.OrdinalIgnoreCase);
    }



[Fact]
    public void ObtenerReporteRegion_ConRegionInexistente_DebeLanzarSqlException()
    {
        Assert.Throws<Microsoft.Data.SqlClient.SqlException>(
            () => _repository.ObtenerReporteRegion(9999));
    }
}