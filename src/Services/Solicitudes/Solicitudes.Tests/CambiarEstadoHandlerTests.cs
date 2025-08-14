using System.Data;
using Moq;
using SharedKernel.Abstractions;
using SharedKernel.Domain.Enums;
using SharedKernel.Results;
using Solicitudes.Application.CambiarEstado;
using Solicitudes.Infrastructure;
using TestDoubles;
using Xunit;

public class CambiarEstadoHandlerTests
{
	[Fact]
	public async Task Handler_envia_parametros_correctos_al_SP()
	{
		var spx = new Mock<IStoredProcExecutor>(MockBehavior.Strict);
		FakeDbCommand? captured = null;

		spx.Setup(x => x.ExecAsync(
				It.Is<string>(p => p == StoredProcedures.CambiarEstadoSolicitud),
				It.IsAny<Action<IDbCommand>>(),
				It.IsAny<CancellationToken>()))
		   .Callback<string, Action<IDbCommand>, CancellationToken>((_, cfg, __) =>
		   {
			   var fake = new FakeDbCommand { CommandType = CommandType.StoredProcedure, CommandText = StoredProcedures.CambiarEstadoSolicitud };
			   cfg(fake);
			   captured = fake;
		   })
		   .ReturnsAsync(new SpResult(1, new Dictionary<string, object?>()));

		var handler = new CambiarEstadoHandler(spx.Object);

		var res = await handler.Handle(new CambiarEstadoCommand(55, EstadoSolicitud.EnRevision, 7, "Se turna a revisión"), default);

		Assert.True(res.IsSuccess);
		var p = captured!.AsFakeParameters();
		Assert.Equal((uint)55, p.GetValue<uint>("@p_solicitud_id"));
		Assert.Equal("EN_REVISION", p.GetValue<string>("@p_estado_clave"));
		Assert.Equal((uint)7, p.GetValue<uint>("@p_usuario_id"));
		Assert.Equal("Se turna a revisión", p.GetValue<string>("@p_comentario"));

		spx.VerifyAll();
	}
}
