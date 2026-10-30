CREATE OR ALTER PROCEDURE sp_EliminarPokemon
    @IDPokemon INT
AS
BEGIN
    BEGIN TRY
       
        BEGIN TRANSACTION;

       
        IF NOT EXISTS (SELECT 1 FROM Pokemon WHERE IDPokemon = @IDPokemon)
        BEGIN
            THROW 50000, 'Error: El Pokémon que intenta eliminar no existe en la base de datos.', 1;
        END


        DELETE FROM TipoPokemon WHERE IDPokemon = @IDPokemon;
        DELETE FROM HabilidadPokemon WHERE IDPokemon = @IDPokemon;

        
        UPDATE Evolucion 
        SET IDInvolucion = NULL 
        WHERE IDInvolucion = @IDPokemon;

        UPDATE Evolucion 
        SET IDEvolucion = NULL 
        WHERE IDEvolucion = @IDPokemon;

       
        DELETE FROM Evolucion WHERE IDPokemon = @IDPokemon;
        DELETE FROM Pokemon WHERE IDPokemon = @IDPokemon;

        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
      
        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRANSACTION;
        END;

        -- Capturamos y lanzamos el error real
        DECLARE @ErrorMsg NVARCHAR(4000);
        SET @ErrorMsg = ERROR_MESSAGE();

        THROW 50000, @ErrorMsg, 1;
    END CATCH
END;
GO

EXEC sp_EliminarPokemon @IDPokemon = 15; 
GO

SELECT * FROM Pokemon WHERE IDPokemon = 15;
