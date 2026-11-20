namespace PokedexCLI.Models;

public class ResultadoAnalisisCombate
{
    public string Pokemon { get; set; } = string.Empty;

    public string TipoElemental { get; set; } = string.Empty;

    public List<string> Ventajas { get; set; } = new();

    public List<string> Debilidades { get; set; } = new();
}