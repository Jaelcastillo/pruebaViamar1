
namespace PokedexCLI.Models;

public class PokemonEliminado
{
    public int IDPokemon { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public DateTime FechaEliminacion { get; set; }
}