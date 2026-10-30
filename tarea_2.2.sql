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
        -- 1. Abrimos la transacción
        BEGIN TRANSACTION;

        -- 2. Insertar el nuevo registro en la tabla principal 'Pokemon'
        INSERT INTO Pokemon (Nombre, Peso, Altura, NPokedex, IDRegion)
        VALUES (@Nombre, @Peso, @Altura, @NPokedex, @IDRegion);

        -- 3. Capturamos el ID recién generado
        DECLARE @NuevoIDPokemon INT;
        SET @NuevoIDPokemon = SCOPE_IDENTITY();

        -- 4. Insertar la relación en 'TipoPokemon'
        INSERT INTO TipoPokemon (IDPokemon, IDTipo)
        VALUES (@NuevoIDPokemon, @IDTipoPrincipal);

        -- 5. Insertar la relación en 'HabilidadPokemon'
        INSERT INTO HabilidadPokemon (IDPokemon, IDHabilidad)
        VALUES (@NuevoIDPokemon, @IDHabilidad);

        -- 6. Insertar registro inicial en 'Evolucion'
        INSERT INTO Evolucion (IDPokemon, IDInvolucion, IDEvolucion)
        VALUES (@NuevoIDPokemon, NULL, NULL);

        -- 7. Si todo salió bien, confirmamos los cambios
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        -- Si hay una transacción abierta, la revertimos
        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRANSACTION;
        END;

        -- SOLUCIÓN: Guardamos el error en una variable y luego lo lanzamos con THROW
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
    @IDRegion = 4,          -- El ID de la región (asegúrate de que exista en tu tabla Regiones)
    @IDTipoPrincipal = 3,   -- El ID del tipo (ej. Eléctrico)
    @IDHabilidad = 2;       -- El ID de la habilidad (ej. Electricidad Estática)