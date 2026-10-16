--tarea modulo 1.2 

CREATE OR ALTER VIEW vw_CadenaEvolutiva AS
SELECT
p.IDPokemon,
p.Nombre AS PokemonActual,
ISNULL(pre.Nombre, 'No tiene') AS PreEvolucion,
ISNULL(post.Nombre, 'No tiene') AS SiguienteEvolucion
FROM Pokemon p
LEFT JOIN Evolucion e ON p.IDPokemon = e.IDPokemon
LEFT JOIN Pokemon pre ON e.IDInvolucion = pre.IDPokemon
LEFT JOIN Pokemon post ON e.IDEvolucion = post.IDPokemon;

SELECT * FROM vw_CadenaEvolutiva
