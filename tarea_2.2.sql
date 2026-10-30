CREATE OR ALTER PROCEDURE sp_AgregarPokemon
    @Nombre VARCHAR(100),
    @Peso DECIMAL(5,2),
    @Altura DECIMAL(5,2),
    @NPokedex INT,
    @IDRegion INT,
    @IDTipoPrincipal INT,
    @IDHabilidad INT
AS
BEGIN
    BEGIN TRY
       
        BEGIN TRANSACTION;

        INSERT INTO Pokemon (Nombre, Peso, Altura, NPokedex, IDRegion)
        VALUES (@Nombre, @Peso, @Altura, @NPokedex, @IDRegion);

        DECLARE @NuevoIDPokemon INT;
        SET @NuevoIDPokemon = SCOPE_IDENTITY();

        INSERT INTO TipoPokemon (IDPokemon, IDTipo)
        VALUES (@NuevoIDPokemon, @IDTipoPrincipal);

        INSERT INTO HabilidadPokemon (IDPokemon, IDHabilidad)
        VALUES (@NuevoIDPokemon, @IDHabilidad);

        INSERT INTO Evolucion (IDPokemon, IDInvolucion, IDEvolucion)
        VALUES (@NuevoIDPokemon, NULL, NULL);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
       
        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRANSACTION;
        END;

        DECLARE @ErrorMsg NVARCHAR(4000);
        SET @ErrorMsg = ERROR_MESSAGE();

        THROW 50000, @ErrorMsg, 1;
    END CATCH
END;
GO

EXEC sp_AgregarPokemon 
    @Nombre = 'Pika', 
    @Peso = 6.0, 
    @Altura = 0.5, 
    @NPokedex = 20, 
    @IDRegion = 4,          
    @IDTipoPrincipal = 3,  
    @IDHabilidad = 2;       
