namespace SharedKernel.Domain.Enums;
using System.Diagnostics.CodeAnalysis;
[ExcludeFromCodeCoverage]
public static class StoredProcedures
{
	public const string CrearSolicitud = "sp_crear_solicitud";
	public const string CambiarEstadoSolicitud = "sp_cambiar_estado_solicitud";
	public const string AdjuntarDocumento = "sp_adjuntar_documento";
}
