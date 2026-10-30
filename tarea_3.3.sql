--CREACION DE LA TABLA 
CREATE TABLE AuditoriaCambiosPokemon (
        IDAuditoria INT IDENTITY(1,1) PRIMARY KEY,
        IDPokemon INT,
        PesoAnterior FLOAT,
        PesoNuevo FLOAT,
        AlturaAnterior FLOAT,
        AlturaNueva FLOAT,
        Usuario VARCHAR(100),
        FechaCambio DATETIME
    );

    --CREACION DEL TRIGGER 3.3 

    CREATE TRIGGER tr_AuditarCambiosPokemon
ON Pokemon
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO AuditoriaCambiosPokemon (IDPokemon, PesoAnterior, PesoNuevo, AlturaAnterior, AlturaNueva, Usuario, FechaCambio)
    SELECT 
        i.IDPokemon,
        d.Peso AS PesoAnterior,
        i.Peso AS PesoNuevo,
        d.Altura AS AlturaAnterior,
        i.Altura AS AlturaNueva,
        SYSTEM_USER AS Usuario, 
        GETDATE() AS FechaCambio
    FROM inserted i
    INNER JOIN deleted d ON i.IDPokemon = d.IDPokemon
    WHERE i.Peso <> d.Peso OR i.Altura <> d.Altura;
END;

UPDATE Pokemon 
SET Peso = 8.5 
WHERE IDPokemon = 1; 

SELECT * FROM AuditoriaCambiosPokemon

