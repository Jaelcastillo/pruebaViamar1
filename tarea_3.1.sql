CREATE TRIGGER tr_AuditarPokemonEliminado
ON Pokemon
AFTER DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO PokemonesEliminados (IDPokemon, Nombre, Peso, Altura, NPokedex, IDRegion, FechaEliminacion)
    SELECT 
        d.IDPokemon,
        d.Nombre,
        d.Peso,
        d.Altura,
        d.NPokedex,
        d.IDRegion,
        GETDATE() AS FechaEliminacion 
    FROM deleted d;
END;
GO

DELETE FROM Pokemon WHERE IDPokemon = 3;
SELECT * FROM PokemonesEliminados
