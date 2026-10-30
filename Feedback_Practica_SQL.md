# 📊 Revisión y Feedback General — Práctica SQL Server (`IntegradorPokemonFyM`)

¡Excelente trabajo completando los **3 módulos** de la práctica de SQL Server! Aquí tienes la evaluación detallada de cada uno de tus scripts, destacando lo que implementaste muy bien y las mejoras puntuales que debes aplicar.

---

## 🏆 Resumen de Calificaciones

| Módulo | Tema | Calificación |
| :--- | :--- | :---: |
| **Módulo 1** | Vistas (`Views`) | **8.5 / 10** |
| **Módulo 2** | Procedimientos Almacenados (`Stored Procedures`) | **9.8 / 10** |
| **Módulo 3** | Triggers de Auditoría y Control (`Triggers`) | **9.0 / 10** |
| **PROMEDIO FINAL** | **Práctica Completa** | **9.1 / 10** 🌟 |

---

# 📋 Evaluación del Módulo 1: Vistas (Views)

## 1️⃣ Tarea 1.1: Vista de Detalle General (`tarea_1.1.sql` — `vw_PokemonDetalle`)
**Calificación: 9.5 / 10** 🌟

### ✅ Lo que hiciste muy bien (No requiere cambios):
* **Reto Extra Completado (`STRING_AGG`):** Implementaste de forma impecable la función `STRING_AGG(t.Nombre, ' / ')` para concatenar los tipos de cada Pokémon en una sola fila y evitar registros duplicados.
* **Agrupación (`GROUP BY`):** Aplicaste correctamente la regla de agrupar por todas las columnas no agregadas en el `SELECT`.
* **Relaciones (`JOIN`):** Las uniones entre `Pokemon`, `Regiones`, `TipoPokemon` y `Tipo` son exactas.

### 🔧 Oportunidades de mejora:
* **Uso del separador de lotes `GO`:** En SQL Server, la sentencia `CREATE OR ALTER VIEW` debe ser la única instrucción dentro de su lote de ejecución. Coloca siempre la palabra `GO` después del `;` de la vista y antes del `SELECT * FROM vw_PokemonDetalle`.
* **Tip Pro (`LEFT JOIN`):** Usar `LEFT JOIN` hacia `TipoPokemon` y `Tipo` asegura que si en el futuro se registra un Pokémon sin tipo asignado aún, igual aparezca listado en la vista.

---

## 2️⃣ Tarea 1.2: Vista del Árbol Evolutivo (`tarea_1.2.sql` — `vw_CadenaEvolutiva`)
**Calificación: 9.5 / 10** 🌟

### ✅ Lo que hiciste muy bien (No requiere cambios):
* **Múltiples `LEFT JOIN` hacia la misma tabla:** Resolviste de manera excelente la relación de la tabla `Evolucion` haciendo dos `LEFT JOIN` independientes hacia la tabla `Pokemon` con alias claros (`pre` para `IDInvolucion` y `post` para `IDEvolucion`).
* **Manejo de valores `NULL`:** Usaste `ISNULL(..., 'No tiene')` tal cual se pidió en el requerimiento.

### 🔧 Oportunidades de mejora:
* **Falta el separador `GO`:** Recuerda agregar `GO` antes del `SELECT * FROM vw_CadenaEvolutiva` final.

---

## 3️⃣ Tarea 1.3: Vista de Ventajas y Debilidades (`tarea_1.3.sql` — `vw_AnalisisCombate`)
**Calificación: 6.5 / 10** ⚠️

### ✅ Lo que hiciste muy bien:
* **Estructura y uso de `LEFT JOIN`:** Muy buena decisión usar `LEFT JOIN` para `FuerteA` y `DebilA`, evitando perder tipos que no tienen ventaja contra otros (como el tipo *Normal*), además del buen uso de `ISNULL`.

### ❌ Lo que debes corregir:
1. **Error en la condición `ON` al buscar el nombre del tipo fuerte y débil:**
   En las líneas donde unes con la tabla `Tipo` (`tf` y `td`), escribiste:
   ```sql
   LEFT JOIN FuerteA fa ON t.IDTipo = fa.IDTipo
   LEFT JOIN Tipo tf  ON fa.IDTipo = tf.IDTipo  -- <-- Detalle a corregir

   LEFT JOIN DebilA da ON t.IDTipo = da.IDTipo
   LEFT JOIN Tipo td ON da.IDTipo = td.IDTipo;  -- <-- Detalle a corregir
   ```
   * **¿Qué provoca esto?** Al igualar `fa.IDTipo = tf.IDTipo` y `da.IDTipo = td.IDTipo`, estás volviendo a traer el nombre del **propio tipo del Pokémon** en lugar del tipo contra el que es fuerte (`fa.IDFuerteA`) o débil (`da.IDDebilA`). Por eso Bulbasaur (`Planta`) sale como fuerte a `Planta` y débil a `Planta`.
   * **Corrección:**
   ```sql
   LEFT JOIN FuerteA fa ON t.IDTipo = fa.IDTipo
   LEFT JOIN Tipo tf ON fa.IDFuerteA = tf.IDTipo 

   LEFT JOIN DebilA da ON t.IDTipo = da.IDTipo
   LEFT JOIN Tipo td ON da.IDDebilA = td.IDTipo;
   GO
   ```
2. **Separador de lotes `GO`:** Agregar `GO` antes del `SELECT * FROM vw_AnalisisCombate`.

---

# ⚙️ Evaluación del Módulo 2: Procedimientos Almacenados (Stored Procedures)

## 4️⃣ Tarea 2.1: Buscador y Reporte por Región (`tarea_2.1.sql` — `sp_ReportePorRegion`)
**Calificación: 10 / 10** 🌟

### ✅ Lo que hiciste muy bien (No requiere cambios):
* **Validación de existencia:** Excelente uso de `IF NOT EXISTS (SELECT 1 FROM Regiones WHERE IDRegion = @IDRegion)` junto con `THROW 50000, ..., 1;`.
* **Consultas requeridas:** Devolviste tanto el listado de Pokémon de la región como el segundo conjunto de resultados con las funciones de agregación (`COUNT`, `AVG`, `MAX`).
* **Pruebas completas:** Incluiste `GO` correctamente y dejaste pruebas tanto para un caso válido (`@IDRegion = 1`) como para forzar el error (`@IDRegion = 9999`).

---

## 5️⃣ Tarea 2.2: Alta Completa de un Pokémon (`tarea_2.2.sql` — `sp_AgregarPokemon`)
**Calificación: 9.5 / 10** 🌟

### ✅ Lo que hiciste muy bien:
* **Manejo de transacciones e identidad:** Uso impecable de `BEGIN TRY`, `BEGIN TRANSACTION`, captura del nuevo ID con `SCOPE_IDENTITY()`, inserción en las 3 tablas relacionadas (`TipoPokemon`, `HabilidadPokemon`, `Evolucion`), `COMMIT TRANSACTION` y validación segura `IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION` en el bloque `CATCH`.

### 🔧 Oportunidades de mejora:
* **Tipo de dato en `@Peso` y `@Altura`:** En la tabla `Pokemon`, las columnas `Peso` y `Altura` son de tipo `FLOAT`, mientras que en los parámetros del SP usaste `DECIMAL(5,2)`. `DECIMAL(5,2)` solo soporta números hasta `999.99`, por lo que si intentas registrar un Pokémon de `1000` kg o más, dará error de desbordamiento (*Arithmetic overflow*). Es recomendable usar `FLOAT` o `DECIMAL(10,2)`.
* **Tip en el `CATCH`:** Dentro de un bloque `BEGIN CATCH`, puedes escribir simplemente `THROW;` (sin parámetros) y SQL Server relanzará automáticamente el mensaje, número y línea exacta del error original sin necesidad de declarar `@ErrorMsg`.

---

## 6️⃣ Tarea 2.3: Eliminación Segura (`tarea_2.3.sql` — `sp_EliminarPokemon`)
**Calificación: 10 / 10** 🌟

### ✅ Lo que hiciste muy bien (No requiere cambios):
* **Validación proactiva:** Agregaste por iniciativa propia un `IF NOT EXISTS` para validar que el `@IDPokemon` exista antes de iniciar los borrados.
* **Orden exacto de integridad referencial:** Limpiaste primero `TipoPokemon` y `HabilidadPokemon`, actualizaste a `NULL` las referencias en `Evolucion` (`IDInvolucion` e `IDEvolucion`), eliminaste la fila de `Evolucion` y finalmente eliminaste el registro en `Pokemon`, todo dentro de una transacción segura.

---

# 🛡️ Evaluación del Módulo 3: Triggers de Auditoría y Control

## 7️⃣ Tarea 3.1: Auditoría de Eliminación (`tarea_3.1.sql` — `tr_AuditarPokemonEliminado`)
**Calificación: 9.0 / 10** 🌟

### ✅ Lo que hiciste muy bien:
* **Operación basada en conjuntos:** Usaste `INSERT INTO PokemonesEliminados ... SELECT ... FROM deleted d;`, lo cual garantiza que el trigger audite correctamente tanto si se borra 1 Pokémon como si se borran 100 al mismo tiempo.
* **Uso de `SET NOCOUNT ON;`:** Muy buena práctica profesional dentro de triggers.

### 🔧 Oportunidades de mejora:
* **Usa `CREATE OR ALTER TRIGGER`:** En lugar de solo `CREATE TRIGGER`, para que el script pueda re-ejecutarse sin dar error de que el objeto ya existe.
* **Detalle en la prueba final:** En la línea final pusiste `DELETE FROM Pokemon WHERE IDPokemon = 3;`. Recuerda que como el Pokémon `3` tiene claves foráneas activas en `TipoPokemon`, `HabilidadPokemon` y `Evolucion`, un `DELETE` directo fallará por Foreign Key; para probar tu trigger sin errores de FK, lo ideal es llamar al procedimiento que creaste en el punto anterior: `EXEC sp_EliminarPokemon @IDPokemon = 3;`.

---

## 8️⃣ Tarea 3.2: Control de Calidad de Datos (`tarea_3.2.sql` — `tr_ValidarDatosPokemon`)
**Calificación: 9.5 / 10** 🌟

### ✅ Lo que hiciste muy bien:
* **Validación masiva con `IF EXISTS`:** Excelente uso de `IF EXISTS (SELECT 1 FROM inserted WHERE Peso <= 0 OR Altura <= 0)` junto con `ROLLBACK TRANSACTION` y `RAISERROR`.
* **Casos de prueba:** Incluiste tanto el `INSERT` inválido (`Peso = -5.0`) como el `INSERT` válido (`Peso = 6.0`).

### 🔧 Oportunidades de mejora:
* **Usa `CREATE OR ALTER TRIGGER`** en el encabezado del trigger.

---

## 9️⃣ Tarea 3.3: Historial de Modificaciones (`tarea_3.3.sql` — `tr_AuditarCambiosPokemon`)
**Calificación: 8.5 / 10** 🌟

### ✅ Lo que hiciste muy bien:
* **Diseño de tabla y lógica del Trigger:** La creación de `AuditoriaCambiosPokemon` y el cruce `FROM inserted i INNER JOIN deleted d ON i.IDPokemon = d.IDPokemon` filtrando con `WHERE i.Peso <> d.Peso OR i.Altura <> d.Altura` están excelentes, así como el uso de `SYSTEM_USER` y `GETDATE()`.

### ❌ Lo que debes corregir:
* **Falta del separador `GO` antes y después del `CREATE TRIGGER`:**
  En tu archivo `tarea_3.3.sql` están seguidos el `CREATE TABLE`, el `CREATE TRIGGER` y el `UPDATE` sin ningún `GO` entre ellos. En SQL Server, `CREATE TRIGGER` **debe ser obligatoriamente la primera sentencia de su lote**, por lo que ejecutar todo el archivo junto arroja el error: *"'CREATE TRIGGER' must be the first statement in a query batch"*.
  * **Corrección:**
  ```sql
  CREATE TABLE AuditoriaCambiosPokemon (
      ...
  );
  GO

  CREATE OR ALTER TRIGGER tr_AuditarCambiosPokemon
  ON Pokemon
  AFTER UPDATE
  AS
  BEGIN
      ...
  END;
  GO

  UPDATE Pokemon SET Peso = 8.5 WHERE IDPokemon = 1;
  SELECT * FROM AuditoriaCambiosPokemon;
  ```
