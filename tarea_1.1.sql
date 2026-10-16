CREATE OR ALTER VIEW vw_PokemonDetalle AS
select 
p.NPokedex,
p.Nombre,
p.Peso,
p.Altura,
r.Nombre AS Region,
STRING_AGG(t.Nombre, ' / ') AS Tipo
FROM Pokemon p
JOIN Regiones r ON p.IDRegion = r.IDRegion 
JOIN TipoPokemon tp ON p.IDPokemon = tp.IDPokemon 
JOIN Tipo t ON tp.IDTipo = t.IDTipo 
GROUP BY 
p.NPokedex, 
p.Nombre, 
p.Peso, 
p.Altura, 
r.Nombre; -- Agrupamos por todo lo que no sea una función de agregación

SELECT * FROM vw_PokemonDetalle