using System.Data;
using Moq;
using SharedKernel.Abstractions;
using SharedKernel.Domain.Enums;     // EstadoSolicitud
using SharedKernel.Results;          // SpResult
using Solicitudes.Application.CreateSolicitud;
using TestDoubles;
using Xunit;

public class CreateSolicitudHandlerTests
{
	[Fact]
	public async Task Handler_configura_parametros_correctos()
	{
		var spx = new Mock<IStoredProcExecutor>(MockBehavior.Strict);

		FakeDbCommand? captured = null;

		spx.Setup(x => x.ExecAsync(
				It.Is<string>(p => string.Equals(p, StoredProcedures.CrearSolicitud, StringComparison.OrdinalIgnoreCase)),
				It.IsAny<Action<IDbCommand>>(),
				It.IsAny<CancellationToken>()))
		   .Callback<string, Action<IDbCommand>, CancellationToken>((_, cfg, __) =>
		   {
			   var fake = new FakeDbCommand
			   {
				   CommandType = CommandType.StoredProcedure,
				   CommandText = StoredProcedures.CrearSolicitud
			   };

			   // Ejecuta la configuración que hace el handler (agrega parámetros)
			   cfg(fake);

			   // IMPORTANTE: asignar aquí
			   captured = fake;
		   })
		   .ReturnsAsync(new SpResult(
			   1,
			   new Dictionary<string, object?>
			   {
				   ["@p_solicitud_id"] = (uint)42,
				   ["@p_folio"] = "PM-2025-08-0001"
			   }));

		var handler = new CreateSolicitudHandler(spx.Object);

		var res = await handler.Handle(
			new CreateSolicitudCommand(
				PersonaId: 123,
				Estado: EstadoSolicitud.Creada,
				NumeroOficio: "OF-777",
				FechaSolicitud: DateOnly.FromDateTime(DateTime.Today),
				UsuarioId: 9001),
			default);

		Assert.True(res.IsSuccess);
		Assert.NotNull(captured); // <- ya no debe ser null

		var ps = captured!.AsFakeParameters();
		Assert.Equal((uint)123, ps.GetValue<uint>("@p_persona_id"));
		Assert.Equal("CREADA", ps.GetValue<string>("@p_estado_clave"));
		Assert.Equal("OF-777", ps.GetValue<string>("@p_numero_oficio"));
		Assert.Equal(DateTime.Today, ((IDataParameter)ps["@p_fecha_solicitud"]).Value);
		Assert.Equal((uint)9001, ps.GetValue<uint>("@p_usuario_id"));

		spx.VerifyAll();
	}
}
