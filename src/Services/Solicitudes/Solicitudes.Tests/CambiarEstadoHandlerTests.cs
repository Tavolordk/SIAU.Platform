using System.Data;
using Moq;
using SharedKernel.Data;               // IStoredProcExecutor
using SharedKernel.Domain.Enums;
using Solicitudes.Application.CambiarEstado;
using Solicitudes.Infrastructure;
using TestDoubles;                     // FakeDbParameter

public class CambiarEstadoHandlerTests
{
	[Fact]
	public async Task Handler_envia_parametros_correctos_al_SP()
	{
		// Arrange
		var spx = new Mock<IStoredProcExecutor>(MockBehavior.Strict);
		IDbDataParameter[]? capturedParams = null;

		// Mock de CreateParameter (nuevo contrato del executor)
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

		// Capturamos los parámetros enviados al SP
		spx.Setup(x => x.ExecuteAsync(
				It.Is<string>(p => p == StoredProcedures.CambiarEstadoSolicitud),
				It.IsAny<IEnumerable<IDbDataParameter>>(),
				It.IsAny<CancellationToken>()))
		   .Callback<string, IEnumerable<IDbDataParameter>, CancellationToken>((_, pars, __) =>
		   {
			   capturedParams = pars.ToArray();
		   })
		   .ReturnsAsync(1);

		var handler = new CambiarEstadoHandler(spx.Object);

		// Act
		var res = await handler.Handle(
			new CambiarEstadoCommand(55, EstadoSolicitud.EnRevision, 7, "Se turna a revisión"),
			default);

		// Assert
		Assert.True(res.IsSuccess);
		Assert.NotNull(capturedParams);

		IDbDataParameter P(string name) => capturedParams!.First(p => p.ParameterName == name);

		Assert.Equal((uint)55, Convert.ToUInt32(P("@p_solicitud_id").Value));
		Assert.Equal("EN_REVISION", Convert.ToString(P("@p_estado_clave").Value));
		Assert.Equal((uint)7, Convert.ToUInt32(P("@p_usuario_id").Value));
		Assert.Equal("Se turna a revisión", Convert.ToString(P("@p_comentario").Value));

		// Opcionales: tipos y tamaños
		Assert.Equal(DbType.UInt32, P("@p_solicitud_id").DbType);
		Assert.Equal(DbType.String, P("@p_estado_clave").DbType);
		Assert.Equal(20, P("@p_estado_clave").Size);
		Assert.Equal(DbType.String, P("@p_comentario").DbType);
		Assert.Equal(500, P("@p_comentario").Size);

		Assert.Equal(ParameterDirection.Input, P("@p_solicitud_id").Direction);
		Assert.Equal(ParameterDirection.Input, P("@p_estado_clave").Direction);
		Assert.Equal(ParameterDirection.Input, P("@p_usuario_id").Direction);
		Assert.Equal(ParameterDirection.Input, P("@p_comentario").Direction);

		spx.VerifyAll();
	}
}
