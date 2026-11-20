using Microsoft.Extensions.Configuration;
using NBomber.CSharp;
using PokedexCLI.Repositories;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var repository = new PokemonRepository(configuration);

var lecturaPokedex = Scenario.Create(
    "lectura_pokedex_concurrente",
    context =>
    {
        try
        {
            repository.ObtenerReporteRegion(1);
            repository.ObtenerAnalisisCombate("Charizard");

            return Task.FromResult(Response.Ok());
        }
        catch
        {
            return Task.FromResult(Response.Fail());
        }
    })
.WithLoadSimulations(
    Simulation.Inject(
        rate: 30,
        interval: TimeSpan.FromSeconds(1),
        during: TimeSpan.FromSeconds(30))
);

var random = new Random();

var estresActualizacion = Scenario.Create(
    "estres_actualizacion_triggers",
    context =>
    {
        try
        {
            int idPokemon = random.Next(1, 101);

            double peso = random.NextDouble() * 100 + 1;

            repository.ActualizarPokemon(
                idPokemon,
                peso,
                1.5);

            return Task.FromResult(Response.Ok());
        }
        catch
        {
            return Task.FromResult(Response.Fail());
        }
    })
.WithLoadSimulations(
    Simulation.Inject(
        rate: 20,
        interval: TimeSpan.FromSeconds(1),
        during: TimeSpan.FromSeconds(20))
);

NBomberRunner
    .RegisterScenarios(
        lecturaPokedex,
        estresActualizacion)
    .Run();