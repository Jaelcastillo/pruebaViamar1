namespace PokedexCLI.Models;

public class AuditoriaCambio
{
    public int IDPokemon { get; set; }

    public double PesoAnterior { get; set; }

    public double PesoNuevo { get; set; }

    public double AlturaAnterior { get; set; }

    public double AlturaNueva { get; set; }

    public string Usuario { get; set; } = string.Empty;

    public DateTime FechaCambio { get; set; }
}