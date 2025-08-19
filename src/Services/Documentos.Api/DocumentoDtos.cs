namespace Documentos.Api.Models;

public sealed class DocumentoUploadDto
{
	public uint SolicitudId { get; set; }
	public byte TipoDocumentoId { get; set; }
	public uint UsuarioId { get; set; } // requerido por los SP
	public IFormFile Archivo { get; set; } = default!;
}

public sealed class DocumentoItemDto
{
	public uint Id { get; set; }
	public uint SolicitudId { get; set; }
	public byte TipoDocumentoId { get; set; }
	public string NombreOriginal { get; set; } = default!;
	public string? Extension { get; set; }
	public string? MimeType { get; set; }
	public int? TamanoBytes { get; set; }
	public string StorageProveedor { get; set; } = default!;
	public string StorageRuta { get; set; } = default!;
	public string? ChecksumSha256 { get; set; }
	public DateTime FechaCarga { get; set; }
}
