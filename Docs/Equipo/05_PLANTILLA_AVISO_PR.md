# Comunicación obligatoria tras push y PR

## Mensaje que cada integrante envía al grupo y a Vicente

**Campus Glitch — PR listo para revisión / Draft (elegir uno)**

- **Integrante:** [Nombre] (@usuarioGitHub)
- **Issue:** #NN [título]
- **Rama:** `fix|feature|docs|test/usuario/NN-resumen`
- **Commits propios:** `SHA1` — [resumen], `SHA2` — [resumen]
- **URL del PR a `develop`:** [enlace real]
- **Qué cambié:** [2–4 frases, archivos claves]
- **Qué debe probar Vicente:** [pasos reproducibles N1/N2 y criterio esperado]
- **Pruebas que ya ejecuté:** [Unity versión, escenario, resultado, capturas/logs]
- **No probado / riesgos:** [p. ej. Android, scripts compartidos, visual]
- **Revisor solicitado:** @usuario
- **Estado:** Draft / Listo para revisión / Cambios solicitados / Aprobado (no fusionado)

**Importante: el push es a mi rama y NO está todavía integrado a `develop` o `main`. No fusionar sin revisión y QA.**

## Respuesta de Vicente

**RECIBIDO:** PR #N registrado; revisaré alcance y pruebas.

**EN QA:** PR #N en comprobación sobre `develop`/rama; pendiente de aprobación y pruebas.

**REQUIERE CAMBIOS:** PR #N, observaciones [concretas]; mantener en la misma rama, subir correcciones y avisar nuevo SHA.

**INTEGRADO EN DEVELOP:** PR #N fusionado tras aprobación de @revisor y pruebas [cuáles]; merge SHA [valor]. El Issue queda [abierto/validado/cerrado] según seguimiento.

**ENTREGADO EN MAIN:** PR de release #N fusionado en `main` y tag [versión]; las contribuciones individuales se comprobarán por autor/correo/commits.
