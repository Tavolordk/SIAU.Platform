using System.Data;
using System.Linq;
using Moq;
using SharedKernel.Data;                   // IStoredProcExecutor
using SharedKernel.Domain.Enums;          // EstadoSolicitud
using Solicitudes.Application.CreateSolicitud;
using TestDoubles;                        // FakeDbParameter
using Xunit;

public class CreateSolicitudHandlerTests
{
	[Fact]
	public async Task Handler_configura_parametros_correctos()
	{
		// Arrange
		var spx = new Mock<IStoredProcExecutor>(MockBehavior.Strict);
		IDbDataParameter[]? captured = null;

		// Cuando el handler pida crear parámetros, devolvemos un FakeDbParameter real
		spx.Setup(x => x.CreateParameter(
				It.IsAny<string>(),
				It.IsAny<object?>(),
				It.IsAny<DbType>(),
				It.IsAny<ParameterDirection>()))
		   .Returns((string name, object? value, DbType dbType, ParameterDirection dir) =>
		   {
			   return new FakeDbParameter
			   {
				   ParameterName = name,
				   Value = value,
				   DbType = dbType,
				   Direction = dir
			   };
		   });

		// Capturamos la lista de parámetros y seteamos los outputs como lo haría el SP
		spx.Setup(x => x.ExecuteAsync(
				It.Is<string>(p => string.Equals(p, StoredProcedures.CrearSolicitud, System.StringComparison.OrdinalIgnoreCase)),
				It.IsAny<IEnumerable<IDbDataParameter>>(),
				It.IsAny<CancellationToken>()))
		   .Callback<string, IEnumerable<IDbDataParameter>, CancellationToken>((_, pars, __) =>
		   {
			   captured = pars.ToArray();

			   // Simular salidas del SP
			   var outId = captured.First(p => p.ParameterName == "@p_solicitud_id");
			   var outFolio = captured.First(p => p.ParameterName == "@p_folio");
			   outId.Value = (uint)42;
			   outFolio.Value = "PM-2025-08-0001";
		   })
		   .ReturnsAsync(1); // filas afectadas

		var handler = new CreateSolicitudHandler(spx.Object);

		var cmd = new CreateSolicitudCommand(
			PersonaId: 123,
			Estado: EstadoSolicitud.Creada,
			NumeroOficio: "OF-777",
			FechaSolicitud: DateOnly.FromDateTime(DateTime.Today),
			UsuarioId: 9001
		);

		// Act
		var res = await handler.Handle(cmd, default);

		// Assert
		Assert.True(res.IsSuccess);
		Assert.NotNull(captured);

		IDbDataParameter P(string name) => captured!.First(p => p.ParameterName == name);

		Assert.Equal((uint)123, Convert.ToUInt32(P("@p_persona_id").Value));
		Assert.Equal("CREADA", Convert.ToString(P("@p_estado_clave").Value));
		Assert.Equal("OF-777", Convert.ToString(P("@p_numero_oficio").Value));
		Assert.Equal(DateTime.Today, Convert.ToDateTime(P("@p_fecha_solicitud").Value));
		Assert.Equal((uint)9001, Convert.ToUInt32(P("@p_usuario_id").Value));

		// Direcciones
		Assert.Equal(ParameterDirection.Input, P("@p_persona_id").Direction);
		Assert.Equal(ParameterDirection.Input, P("@p_estado_clave").Direction);
		Assert.Equal(ParameterDirection.Input, P("@p_numero_oficio").Direction);
		Assert.Equal(ParameterDirection.Input, P("@p_fecha_solicitud").Direction);
		Assert.Equal(ParameterDirection.Input, P("@p_usuario_id").Direction);
		Assert.Equal(ParameterDirection.Output, P("@p_solicitud_id").Direction);
		Assert.Equal(ParameterDirection.Output, P("@p_folio").Direction);

		// Resultado que regresa el handler (valores de salida)
		var (solicitudId, folio) = res.Value;
		Assert.Equal((uint)42, solicitudId);
		Assert.Equal("PM-2025-08-0001", folio);

		spx.VerifyAll();
	}
}
