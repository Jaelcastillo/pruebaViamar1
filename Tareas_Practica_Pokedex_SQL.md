# 🚀 Guía de Práctica SQL Server: Proyecto Pokédex (`IntegradorPokemonFyM`)

Bienvenido/a a esta práctica técnica de bases de datos en **SQL Server**. 
Ya cuentas con la base de datos `IntegradorPokemonFyM` creada y sus tablas cargadas (scripts del `00` al `09`). El objetivo de estas actividades es poner en práctica la creación de **Vistas (Views)**, **Procedimientos Almacenados (Stored Procedures)** y **Triggers de Auditoría y Control**.

---

## 📋 Módulo 1: Vistas (Views)
*Objetivo: Practicar relaciones (`JOIN`), manejo de valores nulos (`ISNULL` / `COALESCE`) y agrupación de texto.*

### Tarea 1.1: Vista de Detalle General (`vw_PokemonDetalle`)
Crea una vista llamada `vw_PokemonDetalle` que muestre la información legible de cada Pokémon sin depender de IDs numéricos.
* **Columnas requeridas:**
  * `NPokedex` (Número de Pokédex)
  * `Nombre` (Nombre del Pokémon)
  * `Peso` y `Altura`
  * `Region` (Nombre de la región a la que pertenece)
  * `Tipo` (Nombre del tipo o tipos del Pokémon)
* **Reto extra (Opcional):** Si un Pokémon tiene más de un tipo, investiga cómo usar `STRING_AGG` para mostrar sus tipos separados por coma o barra (ej. `Planta / Veneno`) en una sola fila por Pokémon, evitando duplicar filas.

### Tarea 1.2: Vista del Árbol Evolutivo (`vw_CadenaEvolutiva`)
Usando la tabla `Evolucion` y relacionándola con la tabla `Pokemon`, crea una vista llamada `vw_CadenaEvolutiva` que permita ver la línea evolutiva con nombres en lugar de IDs.
* **Columnas requeridas:**
  * `IDPokemon`
  * `PokemonActual` (Nombre del Pokémon)
  * `PreEvolucion` (Nombre del Pokémon en `IDInvolucion`)
  * `SiguienteEvolucion` (Nombre del Pokémon en `IDEvolucion`)
* **Condición:** Si el Pokémon no posee pre-evolución o evolución (`NULL`), la vista debe mostrar el texto `'No tiene'` en lugar de `NULL`.

### Tarea 1.3: Vista de Ventajas y Debilidades (`vw_AnalisisCombate`)
Conecta las tablas `Pokemon`, `TipoPokemon`, `Tipo`, `FuerteA` y `DebilA` para construir una vista llamada `vw_AnalisisCombate`.
* **Requerimiento:** Mostrar el nombre del Pokémon, su tipo elemental, contra qué tipos tiene ventaja (`FuerteA`) y contra qué tipos tiene desventaja (`DebilA`).

---

## ⚙️ Módulo 2: Procedimientos Almacenados (Stored Procedures)
*Objetivo: Implementar lógica de negocio, parámetros, validaciones y control de transacciones (`TRY...CATCH`, `BEGIN TRAN`, `COMMIT`, `ROLLBACK`).*

### Tarea 2.1: Buscador y Reporte por Región (`sp_ReportePorRegion`)
Crea un procedimiento almacenado que reciba como parámetro el nombre o el ID de una región.
* **Requerimientos:**
  1. Validar si la región existe; si no existe, retornar un mensaje de error claro (`RAISERROR` o `THROW`).
  2. Devolver el listado de todos los Pokémon pertenecientes a esa región (Número de Pokédex, Nombre, Peso y Altura).
  3. Devolver un segundo conjunto de resultados (o resumen) con: la **cantidad total** de Pokémon en la región, el **peso promedio** y la **altura máxima** registrada en dicha región.

### Tarea 2.2: Alta Completa de un Pokémon (`sp_AgregarPokemon`)
Crea un procedimiento almacenado que permita registrar un nuevo Pokémon junto con sus relaciones principales en un solo paso.
* **Parámetros de entrada:** `@Nombre`, `@Peso`, `@Altura`, `@NPokedex`, `@IDRegion`, `@IDTipoPrincipal`, `@IDHabilidad`.
* **Lógica requerida:**
  1. Iniciar una transacción (`BEGIN TRANSACTION`) dentro de un bloque `BEGIN TRY ... END TRY`.
  2. Insertar el nuevo registro en la tabla `Pokemon`.
  3. Capturar el `IDPokemon` recién generado (utilizando `SCOPE_IDENTITY()`).
  4. Insertar la relación en `TipoPokemon` usando el nuevo ID y `@IDTipoPrincipal`.
  5. Insertar la relación en `HabilidadPokemon` usando el nuevo ID y `@IDHabilidad`.
  6. Insertar su registro inicial en la tabla `Evolucion` (con `IDInvolucion` e `IDEvolucion` en `NULL`).
  7. Confirmar la transacción (`COMMIT`). En caso de cualquier error en el `CATCH`, revertir los cambios (`ROLLBACK`) y mostrar el error.

### Tarea 2.3: Eliminación Segura (`sp_EliminarPokemon`)
Dado que las tablas relacionales no tienen configurado `ON DELETE CASCADE`, intentar borrar un Pokémon directamente de la tabla `Pokemon` arrojará un error de clave foránea (`Foreign Key`).
* **Parámetro de entrada:** `@IDPokemon INT`
* **Lógica requerida (dentro de una transacción):**
  1. Eliminar sus registros asociados en `TipoPokemon` y `HabilidadPokemon`.
  2. Actualizar la tabla `Evolucion` para que cualquier otro Pokémon que tenga a `@IDPokemon` como `IDInvolucion` o `IDEvolucion` pase a tener `NULL` en ese campo.
  3. Eliminar el registro propio de `@IDPokemon` en la tabla `Evolucion`.
  4. Finalmente, eliminar el registro de la tabla `Pokemon`.

---

## 🛡️ Módulo 3: Triggers de Auditoría y Control
*Objetivo: Utilizar las tablas virtuales `inserted` y `deleted` para auditar operaciones y asegurar la integridad de los datos.*

### Tarea 3.1: Auditoría de Eliminación (`tr_AuditarPokemonEliminado`)
En el script inicial (`00`) ya existe una tabla llamada `PokemonesEliminados`.
* **Requerimiento:** Crear un trigger `AFTER DELETE` sobre la tabla `Pokemon`.
* **Lógica:** Cada vez que se elimine uno o más Pokémon de la tabla `Pokemon`, el trigger debe insertar automáticamente en `PokemonesEliminados` todos los datos del Pokémon borrado (`IDPokemon`, `Nombre`, `Peso`, `Altura`, `NPokedex`, `IDRegion`) obtenidos desde la tabla virtual `deleted`, asignando la fecha y hora actual (`GETDATE()`) en el campo `FechaEliminacion`.
* **Importante:** Asegúrate de que el trigger funcione correctamente incluso si se eliminan varios Pokémon en una sola sentencia `DELETE` (usa `INSERT INTO ... SELECT ... FROM deleted`).

### Tarea 3.2: Control de Calidad de Datos (`tr_ValidarDatosPokemon`)
* **Requerimiento:** Crear un trigger `AFTER INSERT, UPDATE` sobre la tabla `Pokemon`.
* **Lógica:** Revisar en la tabla virtual `inserted` que ningún registro tenga un `Peso <= 0` o una `Altura <= 0`. Si se detecta un valor inválido, el trigger debe cancelar la operación (`ROLLBACK TRANSACTION`) y lanzar un mensaje de error indicando que el peso y la altura deben ser mayores a cero.

### Tarea 3.3: Historial de Modificaciones (`tr_AuditarCambiosPokemon`)
* **Requerimiento:**
  1. Crea una nueva tabla llamada `AuditoriaCambiosPokemon` con las columnas:
     * `IDAuditoria INT IDENTITY(1,1) PRIMARY KEY`
     * `IDPokemon INT`
     * `PesoAnterior FLOAT`, `PesoNuevo FLOAT`
     * `AlturaAnterior FLOAT`, `AlturaNueva FLOAT`
     * `Usuario VARCHAR(100)`
     * `FechaCambio DATETIME`
  2. Crea un trigger `AFTER UPDATE` en la tabla `Pokemon` que compare las tablas `deleted` (valores anteriores) e `inserted` (valores nuevos).
  3. Solo si el `Peso` o la `Altura` sufrieron algún cambio, inserta un registro en `AuditoriaCambiosPokemon` guardando los valores viejos, los valores nuevos, el usuario que ejecutó el cambio (`SYSTEM_USER`) y la fecha exacta (`GETDATE()`).

---

## ✅ Entregables esperados
Para cada tarea, entrega un script `.sql` ordenado y comentado que incluya:
1. La creación del objeto (`CREATE OR ALTER VIEW / PROCEDURE / TRIGGER`).
2. Un bloque de prueba al final del script (comentado o listo para ejecutar) donde se evidencie cómo probaste que tu solución funciona (ej. `SELECT` a la vista, `EXEC` al procedimiento o pruebas de `INSERT`/`UPDATE`/`DELETE` para disparar los triggers).
