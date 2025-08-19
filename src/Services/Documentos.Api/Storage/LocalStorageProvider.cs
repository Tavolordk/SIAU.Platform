// Documentos.Api/Storage/LocalStorageProvider.cs
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Documentos.Api.Storage;

public sealed class LocalStorageProvider(IOptions<DocumentosOptions> opts) : IStorageProvider
{
	private readonly string _root = opts.Value.LocalRoot
		?? throw new InvalidOperationException("Config 'Documentos:LocalRoot' es requerida para LOCAL.");

	public async Task<StorageSaveResult> SaveAsync(IFormFile file, uint solicitudId, byte tipoDocumentoId, CancellationToken ct)
	{
		var subdir = Path.Combine("solicitudes", solicitudId.ToString());
		var dir = Path.Combine(_root, subdir);
		Directory.CreateDirectory(dir);

		var ext = Path.GetExtension(file.FileName);
		if (string.IsNullOrWhiteSpace(ext))
			ext = GuessExtension(file.ContentType) ?? ".bin";

		var nombre = $"tipo_{tipoDocumentoId}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{ext}";
		var rutaRel = Path.Combine(subdir, nombre);
		var rutaAbs = Path.Combine(_root, rutaRel);

		// Calcular SHA-256 y guardar
		string sha256;
		using (var src = file.OpenReadStream())
		{
			using var sha = SHA256.Create();
			var hash = await sha.ComputeHashAsync(src, ct);
			sha256 = Convert.ToHexString(hash).ToLowerInvariant();
		}

		// Guardar archivo (volver a abrir stream)
		using (var src2 = file.OpenReadStream())
		using (var dest = File.Create(rutaAbs))
		{
			await src2.CopyToAsync(dest, ct);
			await dest.FlushAsync(ct);
		}

		var tam = checked((int)Math.Min(file.Length, int.MaxValue));
		return new StorageSaveResult(
			rutaRel.Replace('\\', '/'),
			file.ContentType,
			ext.TrimStart('.'),
			tam,
			sha256);
	}

	public Task<Stream> OpenReadAsync(string rutaRelativa, CancellationToken ct)
	{
		var rutaAbs = Path.Combine(_root, rutaRelativa.Replace('/', Path.DirectorySeparatorChar));
		if (!File.Exists(rutaAbs)) throw new FileNotFoundException(rutaRelativa);
		Stream s = File.OpenRead(rutaAbs);
		return Task.FromResult(s);
	}

	public Task DeleteAsync(string rutaRelativa, CancellationToken ct)
	{
		var rutaAbs = Path.Combine(_root, rutaRelativa.Replace('/', Path.DirectorySeparatorChar));
		if (File.Exists(rutaAbs)) File.Delete(rutaAbs);
		return Task.CompletedTask;
	}

	private static string? GuessExtension(string? mime)
	{
		return mime?.ToLowerInvariant() switch
		{
			"application/pdf" => ".pdf",
			"image/png" => ".png",
			"image/jpeg" => ".jpg",
			"application/zip" => ".zip",
			"text/plain" => ".txt",
			_ => null
		};
	}
}
