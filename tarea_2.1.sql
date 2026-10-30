CREATE OR ALTER PROCEDURE sp_ReportePorRegion
    @IDRegion INT 
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Regiones WHERE IDRegion = @IDRegion)
    BEGIN
        THROW 50000, 'Error: La región especificada no existe en la base de datos.', 1;
    END

    SELECT 
        p.NPokedex,
        p.Nombre,
        p.Peso,
        p.Altura,
        r.Nombre AS Region
    FROM Pokemon p
    JOIN Regiones r ON p.IDRegion = r.IDRegion
    WHERE r.IDRegion = @IDRegion;

    SELECT 
        COUNT(p.IDPokemon) AS CantidadTotalPokemon,
        AVG(p.Peso) AS PesoPromedio,
        MAX(p.Altura) AS AlturaMaxima
    FROM Pokemon p
    WHERE p.IDRegion = @IDRegion;

END;
GO

-- Pruebas
EXEC sp_ReportePorRegion @IDRegion = 1;
GO

EXEC sp_ReportePorRegion @IDRegion = 9999;
GO