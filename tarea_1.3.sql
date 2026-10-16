--tarea modulo 1.3 

CREATE OR ALTER VIEW vw_AnalisisCombate AS
SELECT 
p.IDPokemon,
p.Nombre AS Pokemon,
t.Nombre AS TipoElemental,
ISNULL(tf.Nombre, 'No tiene') AS FuerteA,
ISNULL(td.Nombre, 'No tiene') AS DebilA
FROM Pokemon p

JOIN TipoPokemon tp ON p.IDPokemon = tp.IDPokemon
JOIN Tipo t  ON tp.IDTipo = t.IDTipo

LEFT JOIN FuerteA fa ON t.IDTipo = fa.IDTipo
LEFT JOIN Tipo tf  ON fa.IDTipo = tf.IDTipo 

LEFT JOIN DebilA da ON t.IDTipo = da.IDTipo
LEFT JOIN Tipo td ON da.IDTipo = td.IDTipo; 

select * from vw_AnalisisCombate


