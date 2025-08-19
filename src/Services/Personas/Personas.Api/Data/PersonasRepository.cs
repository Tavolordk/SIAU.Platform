using System.Data;
using System.Data.Common;
using Dapper;
using SharedKernel.Abstractions;

namespace Personas.Api.Data;

public sealed class PersonasRepository(IConnectionFactory factory) : IPersonasRepository
{
	private async Task<T> WithConn<T>(Func<IDbConnection, Task<T>> work, CancellationToken ct)
	{
		using var conn = factory.Create();
		if (conn is DbConnection dbc) await dbc.OpenAsync(ct); else conn.Open();
		return await work(conn);
	}

	public Task<PersonaDto?> GetById(uint id, CancellationToken ct) =>
		WithConn(conn => conn.QuerySingleOrDefaultAsync<PersonaDto>(
			@"SELECT  id,
                  nombres,
                  primer_apellido       AS PrimerApellido,
                  segundo_apellido      AS SegundoApellido,
                  sexo_id               AS SexoId,
                  fecha_nacimiento      AS FechaNacimiento,
                  nacionalidad_id       AS NacionalidadId,
                  pais_nacimiento_id    AS PaisNacimientoId,
                  entidad_nacimiento_id AS EntidadNacimientoId,
                  municipio_nacimiento_id AS MunicipioNacimientoId,
                  estado_civil_id       AS EstadoCivilId,
                  rfc,
                  curp,
                  curp_validada         AS CurpValidada
          FROM persona
          WHERE id = @id",
			new { id }), ct);

	public Task<IReadOnlyList<PersonaDto>> Search(string? texto, string? curp, string? rfc, int page, int pageSize, CancellationToken ct) =>
		WithConn<IReadOnlyList<PersonaDto>>(async conn =>
		{
			var off = (page - 1) * pageSize;
			var rows = await conn.QueryAsync<PersonaDto>(
				@"SELECT  id,
                      nombres,
                      primer_apellido       AS PrimerApellido,
                      segundo_apellido      AS SegundoApellido,
                      sexo_id               AS SexoId,
                      fecha_nacimiento      AS FechaNacimiento,
                      nacionalidad_id       AS NacionalidadId,
                      pais_nacimiento_id    AS PaisNacimientoId,
                      entidad_nacimiento_id AS EntidadNacimientoId,
                      municipio_nacimiento_id AS MunicipioNacimientoId,
                      estado_civil_id       AS EstadoCivilId,
                      rfc,
                      curp,
                      curp_validada         AS CurpValidada
              FROM persona
              WHERE (@curp IS NULL OR curp = @curp)
                AND (@rfc  IS NULL OR rfc  = @rfc)
                AND (@texto IS NULL OR (
                     nombres LIKE CONCAT('%',@texto,'%')
                     OR primer_apellido  LIKE CONCAT('%',@texto,'%')
                     OR segundo_apellido LIKE CONCAT('%',@texto,'%')
                     OR CONCAT(nombres,' ',primer_apellido,' ',IFNULL(segundo_apellido,'')) LIKE CONCAT('%',@texto,'%')
                ))
              ORDER BY id DESC
              LIMIT @pageSize OFFSET @off",
				new { texto, curp, rfc, pageSize, off });

			return rows.ToList();
		}, ct);


	public Task<uint> Create(PersonaCreateDto dto, CancellationToken ct) =>
		WithConn(async conn =>
		{
			var p = new
			{
				dto.Nombres,
				dto.PrimerApellido,
				dto.SegundoApellido,
				dto.SexoId,
				FechaNacimiento = dto.FechaNacimiento.ToDateTime(TimeOnly.MinValue), // <-- clave
				dto.NacionalidadId,
				dto.PaisNacimientoId,
				dto.EntidadNacimientoId,
				dto.MunicipioNacimientoId,
				dto.EstadoCivilId,
				dto.Rfc,
				dto.Curp
			};

			var id = await conn.ExecuteScalarAsync<uint>(
				@"INSERT INTO persona
              (nombres, primer_apellido, segundo_apellido, sexo_id, fecha_nacimiento,
               nacionalidad_id, pais_nacimiento_id, entidad_nacimiento_id, municipio_nacimiento_id,
               estado_civil_id, rfc, curp, curp_validada, curp_validada_fuente)
              VALUES (@Nombres, @PrimerApellido, @SegundoApellido, @SexoId, @FechaNacimiento,
                      @NacionalidadId, @PaisNacimientoId, @EntidadNacimientoId, @MunicipioNacimientoId,
                      @EstadoCivilId, @Rfc, @Curp, 0, 'NA');
              SELECT LAST_INSERT_ID();", p);

			return id;
		}, ct);


	public Task<IReadOnlyList<object>> GetContactos(uint personaId, CancellationToken ct) =>
		WithConn<IReadOnlyList<object>>(async conn =>
		{
			var rows = await conn.QueryAsync(
				@"SELECT id, tipo, valor, extension, es_principal AS esPrincipal, validado
              FROM persona_contacto
              WHERE persona_id=@personaId
              ORDER BY es_principal DESC, id",
				new { personaId });

			return rows.ToList();
		}, ct);

	public Task<IReadOnlyList<object>> GetAsignaciones(uint personaId, bool? soloActivas, CancellationToken ct) =>
		WithConn<IReadOnlyList<object>>(async conn =>
		{
			var rows = await conn.QueryAsync(
				@"SELECT id, estructura_id AS estructuraId, tipo_asignacion AS tipoAsignacion,
                     cargo, funciones, numero_empleado AS numeroEmpleado,
                     fecha_inicio AS fechaInicio, fecha_fin AS fechaFin, activo
              FROM persona_asignacion
              WHERE persona_id=@personaId AND (@solo IS NULL OR activo=@solo)
              ORDER BY id DESC",
				new { personaId, solo = soloActivas });

			return rows.ToList();
		}, ct);

	// ====== AQUÍ LOS SP ======

	// SP: siau_cedulas.sp_upsert_persona_contacto
	public Task AddContacto(uint personaId, ContactoCreateDto dto, CancellationToken ct) =>
		WithConn(async conn =>
		{
			// 'tipo' en BD es ENUM('correo','celular','tel_oficina')
			var tipo = (dto.Tipo ?? "").Trim().ToLowerInvariant();

			var dp = new DynamicParameters();
			dp.Add("p_persona_id", personaId, DbType.UInt32);
			dp.Add("p_tipo", tipo, DbType.String);
			dp.Add("p_valor", dto.Valor, DbType.String);
			dp.Add("p_extension", dto.Extension, DbType.String);
			dp.Add("p_es_principal", dto.EsPrincipal ? 1 : 0, DbType.Byte);
			dp.Add("p_validado", 0, DbType.Byte);
			dp.Add("p_origen", "API", DbType.String);
			dp.Add("p_contacto_id", dbType: DbType.UInt32, direction: ParameterDirection.Output);

			await conn.ExecuteAsync(
				"siau_cedulas.sp_upsert_persona_contacto",
				dp,
				commandType: CommandType.StoredProcedure);

			// Si quisieras regresar el id: var nuevoId = dp.Get<uint>("p_contacto_id");
			return 0; // el método firma Task; ignoramos el id
		}, ct);

	// SP: siau_cedulas.sp_set_adscripcion y siau_cedulas.sp_agregar_comision
	public Task AddAsignacion(uint personaId, AsignacionCreateDto dto, CancellationToken ct) =>
		WithConn(async conn =>
		{
			var fecha = dto.FechaInicio is DateOnly d
				? d.ToDateTime(TimeOnly.MinValue)
				: DateTime.UtcNow.Date;

			var dp = new DynamicParameters();
			dp.Add("p_persona_id", personaId, DbType.UInt32);
			dp.Add("p_estructura_id", dto.EstructuraId, DbType.UInt32);
			dp.Add("p_cargo", dto.Cargo ?? "", DbType.String);
			dp.Add("p_funciones", dto.Funciones ?? "", DbType.String);
			dp.Add("p_numero_empleado", dto.NumeroEmpleado ?? "", DbType.String);
			dp.Add("p_fecha_inicio", fecha, DbType.Date);
			dp.Add("p_asignacion_id", dbType: DbType.UInt32, direction: ParameterDirection.Output);

			var sp = string.Equals(dto.TipoAsignacion ?? "", "comision", StringComparison.OrdinalIgnoreCase)
				? "siau_cedulas.sp_agregar_comision"
				: "siau_cedulas.sp_set_adscripcion";

			await conn.ExecuteAsync(sp, dp, commandType: CommandType.StoredProcedure);

			// var nuevoId = dp.Get<uint?>("p_asignacion_id");
			return 0;
		}, ct);

}