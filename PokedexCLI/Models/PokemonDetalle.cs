namespace PokedexCLI.Models
{
    public class PokemonDetalle
    {
        public int IDPokemon { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public double Peso { get; set; }
        public double Altura { get; set; }
        public int NPokedex { get; set; }
        public string Region { get; set; } = string.Empty;
    }
}
