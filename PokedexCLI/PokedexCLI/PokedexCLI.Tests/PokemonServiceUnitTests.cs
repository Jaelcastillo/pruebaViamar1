using Moq;
using PokedexCLI.Models;
using PokedexCLI.Repositories;
using PokedexCLI.Services;

namespace PokedexCLI.Tests;

public class PokemonServiceUnitTests
{
    [Fact]
    public void ObtenerPaginaPokemon_Pagina1_DebeRetornar20Elementos()
    {
        // Arrange
        var mockRepo = new Mock<IPokemonRepository>();

        var listaPokemon = Enumerable.Range(1, 25)
            .Select(i => new PokemonDetalle
            {
                NPokedex = i,
                Nombre = $"Pokemon{i}"
            });

        mockRepo
            .Setup(r => r.ObtenerTodos())
            .Returns(listaPokemon);

        var service = new PokemonService(mockRepo.Object);

        // Act
        var resultado =
            service.ObtenerPaginaPokemon(1, 20)
                   .ToList();

        // Assert
        Assert.Equal(20, resultado.Count);
    }

    [Fact]
    public void ObtenerPaginaPokemon_Pagina2_DebeRetornar5Elementos()
    {
        // Arrange
        var mockRepo = new Mock<IPokemonRepository>();

        var listaPokemon = Enumerable.Range(1, 25)
            .Select(i => new PokemonDetalle
            {
                NPokedex = i,
                Nombre = $"Pokemon{i}"
            });

        mockRepo
            .Setup(r => r.ObtenerTodos())
            .Returns(listaPokemon);

        var service = new PokemonService(mockRepo.Object);

        // Act
        var resultado =
            service.ObtenerPaginaPokemon(2, 20)
                   .ToList();

        // Assert
        Assert.Equal(5, resultado.Count);
    }

    [Fact]
    public void ObtenerPaginaPokemon_Pagina0_DebeLanzarArgumentOutOfRangeException()
    {
        // Arrange
        var mockRepo = new Mock<IPokemonRepository>();

        var service = new PokemonService(mockRepo.Object);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(
            () => service.ObtenerPaginaPokemon(0, 20));
    }

    [Fact]
    public void ObtenerPaginaPokemon_PaginaNegativa_DebeLanzarArgumentOutOfRangeException()
    {
        // Arrange
        var mockRepo = new Mock<IPokemonRepository>();

        var service = new PokemonService(mockRepo.Object);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(
            () => service.ObtenerPaginaPokemon(-1, 20));
    }

    [Fact]
    public void AnalizarCombateAgrupado_DebeEliminarDuplicados()
    {
        // Arrange
        var mockRepo =
            new Mock<IPokemonRepository>();

        var datos = new List<AnalisisCombate>
    {
        new AnalisisCombate
        {
            Pokemon = "Bulbasaur",
            TipoElemental = "Planta",
            FuerteA = "Agua",
            DebilA = "Fuego"
        },

        new AnalisisCombate
        {
            Pokemon = "Bulbasaur",
            TipoElemental = "Planta",
            FuerteA = "Agua",
            DebilA = "Fuego"
        },

        new AnalisisCombate
        {
            Pokemon = "Bulbasaur",
            TipoElemental = "Planta",
            FuerteA = "Tierra",
            DebilA = "Fuego"
        },

        new AnalisisCombate
        {
            Pokemon = "Bulbasaur",
            TipoElemental = "Planta",
            FuerteA = "Tierra",
            DebilA = "Hielo"
        }
    };

        mockRepo
            .Setup(r => r.ObtenerAnalisisCombate("Bulbasaur"))
            .Returns(datos);

        var service =
            new PokemonService(mockRepo.Object);

        // Act
        var resultado =
            service.AnalizarCombateAgrupado("Bulbasaur");

        // Assert

        Assert.Equal(2, resultado.Ventajas.Count);

        Assert.Contains("Agua", resultado.Ventajas);

        Assert.Contains("Tierra", resultado.Ventajas);

        Assert.Equal(2, resultado.Debilidades.Count);

        Assert.Contains("Fuego", resultado.Debilidades);

        Assert.Contains("Hielo", resultado.Debilidades);
    }

    [Theory]
    [InlineData(0, 1.5)]
    [InlineData(-5, 1.5)]
    [InlineData(10, 0)]
    [InlineData(10, -2)]
    public void RegistrarNuevoPokemon_DatosInvalidos_DebeLanzarExcepcion(
    double peso,
    double altura)
    {
        // Arrange
        var mockRepo =
            new Mock<IPokemonRepository>();

        var service =
            new PokemonService(mockRepo.Object);

        // Act + Assert
        Assert.Throws<ArgumentOutOfRangeException>(
            () => service.RegistrarNuevoPokemon(
                "Pikachu",
                peso,
                altura,
                25,
                1,
                1,
                1));

        mockRepo.Verify(
            r => r.AgregarPokemon(
                It.IsAny<string>(),
                It.IsAny<double>(),
                It.IsAny<double>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<int>()),
            Times.Never);
    }

    
}