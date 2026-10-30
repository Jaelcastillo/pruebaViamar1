CREATE TRIGGER tr_ValidarDatosPokemon
ON Pokemon
AFTER INSERT, UPDATE
AS
BEGIN
    
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM inserted WHERE Peso <= 0 OR Altura <= 0)
    BEGIN
        
        ROLLBACK TRANSACTION;
        
        RAISERROR('Error de validación: El peso y la altura de un Pokémon deben ser mayores estricto a cero (0).', 16, 1);
        RETURN;
    END
END;
GO

-- prueba de que me de el error 
INSERT INTO Pokemon (Nombre, Peso, Altura, NPokedex, IDRegion) 
VALUES ('Pikachu ', -5.0, 0.4, 25, 1);

-- prueba valida
INSERT INTO Pokemon (Nombre, Peso, Altura, NPokedex, IDRegion) 
VALUES ('Pikachu ', 6.0, 0.4, 25, 1);
