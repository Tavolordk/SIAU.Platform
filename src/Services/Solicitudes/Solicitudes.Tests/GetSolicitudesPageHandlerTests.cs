using Moq;
using SharedKernel.Abstractions;
using Solicitudes.Application.GetSolicitudes;
using Solicitudes.Infrastructure;
namespace Solicitudes.Tests;

public class GetSolicitudesPageHandlerTests
{
	[Fact]
	public async Task Devuelve_items_mapeados_y_paginacion_correcta()
	{
		var qx = new Mock<IQueryExecutor>(MockBehavior.Strict);

		qx.Setup(x => x.ExecuteCountAsync(
			It.IsAny<string>(),
			It.IsAny<IReadOnlyDictionary<string, object?>>(),
			It.IsAny<CancellationToken>()))
		  .ReturnsAsync(2);

		qx.Setup(x => x.QueryAsync(
			It.IsAny<string>(),
			It.IsAny<IReadOnlyDictionary<string, object?>>(),
			It.IsAny<CancellationToken>()))
		  .ReturnsAsync(new List<IReadOnlyDictionary<string, object?>>
		  {
			  new Dictionary<string, object?> {
				  ["id"] = (uint)1, ["folio"] = "F-001",
				  ["persona_nombre"] = "JUAN PEREZ",
				  ["estado_clave"] = "CREADA",
				  ["fecha_solicitud"] = DateTime.Today,
				  ["numero_oficio"] = "OF-1",
				  ["created_at"] = DateTime.Today.AddHours(1)
			  },
			  new Dictionary<string, object?> {
				  ["id"] = (uint)2, ["folio"] = "F-002",
				  ["persona_nombre"] = "ANA LOPEZ",
				  ["estado_clave"] = "EN_REVISION",
				  ["fecha_solicitud"] = DateTime.Today,
				  ["numero_oficio"] = "OF-2",
				  ["created_at"] = DateTime.Today.AddHours(2)
			  }
		  });

		var handler = new GetSolicitudesPageHandler(qx.Object);

		var r = await handler.Handle(new GetSolicitudesPageQuery(1, 10), default);
		Assert.True(r.IsSuccess);
		Assert.Equal(2, r.Value?.TotalItems);
		Assert.Equal(1, r.Value?.CurrentPage);
		Assert.Equal(2, r.Value?.Items.Count);
		Assert.Equal("F-001", r.Value?.Items[0].Folio);

		qx.VerifyAll();
	}
}
