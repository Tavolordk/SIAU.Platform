using System.Data;
using Moq;
using SharedKernel.Data;               
using SharedKernel.Domain.Enums;
using Solicitudes.Application.CambiarEstado;
using Solicitudes.Infrastructure;

public class CambiarEstadoHandlerTests
{
	[Fact]
	public async Task Handler_envia_parametros_correctos_al_SP()
	{
		// Arrange
		var spx = new Mock<IStoredProcExecutor>(MockBehavior.Strict);
		IDbDataParameter[]? capturedParams = null;

		spx.Setup(x => x.ExecuteAsync(
				It.Is<string>(p => p == StoredProcedures.CambiarEstadoSolicitud),
				It.IsAny<IEnumerable<IDbDataParameter>>(),
				It.IsAny<CancellationToken>()))
		   .Callback<string, IEnumerable<IDbDataParameter>, CancellationToken>((_, pars, __) =>
		   {
			   capturedParams = pars.ToArray();
		   })
		   // filas afectadas / OK
		   .ReturnsAsync(1);

		var handler = new CambiarEstadoHandler(spx.Object);

		// Act
		var res = await handler.Handle(
			new CambiarEstadoCommand(55, EstadoSolicitud.EnRevision, 7, "Se turna a revisión"),
			default);

		// Assert
		Assert.True(res.IsSuccess);
		Assert.NotNull(capturedParams);

		IDbDataParameter P(string name) =>
			capturedParams!.First(p => p.ParameterName == name);

		Assert.Equal((uint)55, Convert.ToUInt32(P("@p_solicitud_id").Value));
		Assert.Equal("EN_REVISION", Convert.ToString(P("@p_estado_clave").Value));
		Assert.Equal((uint)7, Convert.ToUInt32(P("@p_usuario_id").Value));
		Assert.Equal("Se turna a revisión", Convert.ToString(P("@p_comentario").Value));

		Assert.Equal(ParameterDirection.Input, P("@p_solicitud_id").Direction);
		Assert.Equal(ParameterDirection.Input, P("@p_estado_clave").Direction);
		Assert.Equal(ParameterDirection.Input, P("@p_usuario_id").Direction);
		Assert.Equal(ParameterDirection.Input, P("@p_comentario").Direction);

		spx.VerifyAll();
	}
}
