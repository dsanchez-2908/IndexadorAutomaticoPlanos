CREATE VIEW VW_IAP_LOTES_DETALLE
AS
SELECT 
	l.cdLote,
	l.dsNombreLote,
	l.cdEstadoLote,
	el.dsEstado AS dsEstadoLote,
	l.nuCantidadArchivos,
	l.feAlta,
	u.dsUsuario AS dsUsuarioAlta,
	l.feUltimaModificacion,
	COUNT(DISTINCT la.cdArchivo) AS nuArchivosActuales,
	SUM(a.nuCantidadPaginas) AS nuTotalPaginas
FROM IAP_TD_LOTES l
INNER JOIN IAP_TV_ESTADOS_LOTE el ON l.cdEstadoLote = el.cdEstadoLote
INNER JOIN IAP_TD_USUARIOS u ON l.cdUsuarioAlta = u.cdUsuario
LEFT JOIN IAP_TD_LOTE_ARCHIVOS la ON l.cdLote = la.cdLote
LEFT JOIN IAP_TD_ARCHIVOS a ON la.cdArchivo = a.cdArchivo
GROUP BY 
	l.cdLote, l.dsNombreLote, l.cdEstadoLote, el.dsEstado,
	l.nuCantidadArchivos, l.feAlta, u.dsUsuario, l.feUltimaModificacion;
