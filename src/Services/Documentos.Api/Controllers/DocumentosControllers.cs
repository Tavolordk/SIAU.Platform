// Documentos.Api/Controllers/DocumentosController.cs
using Documentos.Api.Data;
using Documentos.Api.Models;
using Documentos.Api.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Documentos.Api.Controllers;

[ApiController]
[Route("documentos")]
public sealed class DocumentosController(
	IDocumentosRepository repo,
	IStorageProvider storage,
	IOptions<DocumentosOptions> opts) : ControllerBase
{
	[HttpPost]
	[RequestSizeLimit(200_000_000)] // 200 MB ejemplo
	public async Task<ActionResult<DocumentoItemDto>> Upload([FromForm] DocumentoUploadDto dto, CancellationToken ct)
	{
		if (dto.Archivo is null || dto.Archivo.Length == 0) return BadRequest("Archivo vacío.");

		// Guardar físico
		var saved = await storage.SaveAsync(dto.Archivo, dto.SolicitudId, dto.TipoDocumentoId, ct);

		// Persistir metadatos via SP
		var id = await repo.UpsertDocumentoAsync(
			dto.SolicitudId,
			dto.TipoDocumentoId,
			dto.Archivo.FileName,
			saved.Extension,
			saved.MimeType,
			saved.TamanoBytes,
			(opts.Value.Proveedor ?? "LOCAL").ToUpperInvariant(),
			saved.RutaRelativa,
			saved.ChecksumSha256,
			dto.UsuarioId,
			ct);

		// Devolver DTO (consultado)
		var item = await repo.GetByIdAsync(id, ct);
		return CreatedAtAction(nameof(Download), new { id }, item);
	}

	[HttpGet("solicitud/{solicitudId}")]
	public Task<IReadOnlyList<DocumentoItemDto>> GetPorSolicitud([FromRoute] int solicitudId, CancellationToken ct)
		=> repo.GetPorSolicitudAsync((uint)solicitudId, ct);

	[HttpGet("{id}/download")]
	public async Task<IActionResult> Download([FromRoute] int id, CancellationToken ct)
	{
		var doc = await repo.GetByIdAsync((uint)id, ct);
		if (doc is null) return NotFound();

		var stream = await storage.OpenReadAsync(doc.StorageRuta, ct);
		return File(stream, doc.MimeType ?? "application/octet-stream", doc.NombreOriginal);
	}

	[HttpDelete("solicitud/{solicitudId}/tipo/{tipoDocumentoId}")]
	public async Task<IActionResult> Delete([FromRoute] int solicitudId, [FromRoute] byte tipoDocumentoId, [FromQuery] uint usuarioId, CancellationToken ct)
	{
		// Rescatamos ruta antes de borrar en BD
		var ruta = await repo.GetRutaAsync((uint)solicitudId, tipoDocumentoId, ct);

		await repo.DeleteAsync((uint)solicitudId, tipoDocumentoId, usuarioId, ct);

		if (!string.IsNullOrWhiteSpace(ruta))
			await storage.DeleteAsync(ruta!, ct);

		return NoContent();
	}
}
