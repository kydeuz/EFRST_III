
-- SGEB - Sistema de Gestión de Emergencias de Bomberos



-- Bloque Reportante: tabla + stored procedures

-- ============================================================
-- 1. TABLA
-- ============================================================


IF OBJECT_ID('dbo.Reportante', 'U') IS NOT NULL
    DROP TABLE dbo.Reportante;
GO

CREATE TABLE dbo.Reportante (
    IdReportante        INT IDENTITY(1,1)   NOT NULL PRIMARY KEY,
    TipoDocumento       VARCHAR(20)         NOT NULL DEFAULT 'DNI',  -- DNI, CE, Pasaporte
    NumeroDocumento     VARCHAR(20)         NOT NULL,
    Nombre              VARCHAR(150)        NOT NULL,
    Telefono            VARCHAR(20)         NULL,
    Correo              VARCHAR(150)        NULL,
    FechaRegistro       DATETIME            NOT NULL DEFAULT GETDATE(),
    CONSTRAINT UQ_Reportante_NumeroDocumento UNIQUE (NumeroDocumento)
);
GO

-- ============================================================
-- 2. STORED PROCEDURES
-- ============================================================

CREATE OR ALTER PROCEDURE dbo.sp_Reportante_Registrar
    @TipoDocumento      VARCHAR(20),
    @NumeroDocumento    VARCHAR(20),
    @Nombre             VARCHAR(150),
    @Telefono           VARCHAR(20) = NULL,
    @Correo             VARCHAR(150) = NULL,
    @IdReportanteNuevo  INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Reportante
        (TipoDocumento, NumeroDocumento, Nombre, Telefono, Correo, FechaRegistro)
    VALUES
        (@TipoDocumento, @NumeroDocumento, @Nombre, @Telefono, @Correo, GETDATE());

    SET @IdReportanteNuevo = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Reportante_ObtenerPorDocumento
    @TipoDocumento   VARCHAR(20),
    @NumeroDocumento VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT IdReportante, TipoDocumento, NumeroDocumento, Nombre, Telefono, Correo, FechaRegistro
    FROM dbo.Reportante
    WHERE TipoDocumento = @TipoDocumento AND NumeroDocumento = @NumeroDocumento;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Reportante_ObtenerPorId
    @IdReportante INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT IdReportante, TipoDocumento, NumeroDocumento, Nombre, Telefono, Correo, FechaRegistro
    FROM dbo.Reportante
    WHERE IdReportante = @IdReportante;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Reportante_Listar
AS
BEGIN
    SET NOCOUNT ON;

    SELECT IdReportante, TipoDocumento, NumeroDocumento, Nombre, Telefono, Correo, FechaRegistro
    FROM dbo.Reportante
    ORDER BY Nombre;
END
GO

-- Bloque Incidente: tabla + stored procedures


-- ============================================================
-- 1. TABLA
-- ============================================================

IF OBJECT_ID('dbo.Incidente', 'U') IS NOT NULL
    DROP TABLE dbo.Incidente;
GO

CREATE TABLE dbo.Incidente (
    IdIncidente         INT IDENTITY(1,1)   NOT NULL PRIMARY KEY,
    IdReportante        INT                 NOT NULL,
    TipoIncidente       VARCHAR(50)         NOT NULL,   -- Incendio, Rescate, Accidente de tránsito, Materiales peligrosos, Otro
    Descripcion         VARCHAR(500)        NOT NULL,
    Direccion           VARCHAR(200)        NOT NULL,
    Zona                VARCHAR(100)        NOT NULL,
    Latitud             DECIMAL(9,6)        NULL,
    Longitud            DECIMAL(9,6)        NULL,
    Prioridad           VARCHAR(20)         NOT NULL DEFAULT 'Media',       -- Alta, Media, Baja
    Estado              VARCHAR(20)         NOT NULL DEFAULT 'Registrado', -- Registrado, Asignado, Atendido, Cerrado
    FechaHoraRegistro   DATETIME            NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Incidente_Reportante FOREIGN KEY (IdReportante)
        REFERENCES dbo.Reportante(IdReportante)
);
GO

CREATE INDEX IX_Incidente_Estado ON dbo.Incidente(Estado);
CREATE INDEX IX_Incidente_FechaHoraRegistro ON dbo.Incidente(FechaHoraRegistro DESC);
CREATE INDEX IX_Incidente_IdReportante ON dbo.Incidente(IdReportante);
GO

-- ============================================================
-- 2. STORED PROCEDURES
-- ============================================================

CREATE OR ALTER PROCEDURE dbo.sp_Incidente_Registrar
    @IdReportante       INT,
    @TipoIncidente      VARCHAR(50),
    @Descripcion        VARCHAR(500),
    @Direccion          VARCHAR(200),
    @Zona               VARCHAR(100),
    @Latitud            DECIMAL(9,6) = NULL,
    @Longitud           DECIMAL(9,6) = NULL,
    @Prioridad          VARCHAR(20),
    @IdIncidenteNuevo   INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Incidente
        (IdReportante, TipoIncidente, Descripcion, Direccion, Zona, Latitud, Longitud,
         Prioridad, Estado, FechaHoraRegistro)
    VALUES
        (@IdReportante, @TipoIncidente, @Descripcion, @Direccion, @Zona, @Latitud, @Longitud,
         @Prioridad, 'Registrado', GETDATE());

    SET @IdIncidenteNuevo = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Incidente_Listar
AS
BEGIN
    SET NOCOUNT ON;

    SELECT i.IdIncidente, i.IdReportante, i.TipoIncidente, i.Descripcion, i.Direccion, i.Zona,
           i.Latitud, i.Longitud, i.Prioridad, i.Estado, i.FechaHoraRegistro,
           r.Nombre AS NombreReportante, r.Telefono AS TelefonoReportante
    FROM dbo.Incidente i
    INNER JOIN dbo.Reportante r ON r.IdReportante = i.IdReportante
    ORDER BY i.FechaHoraRegistro DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Incidente_ObtenerPorId
    @IdIncidente INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT i.IdIncidente, i.IdReportante, i.TipoIncidente, i.Descripcion, i.Direccion, i.Zona,
           i.Latitud, i.Longitud, i.Prioridad, i.Estado, i.FechaHoraRegistro,
           r.Nombre AS NombreReportante, r.Telefono AS TelefonoReportante
    FROM dbo.Incidente i
    INNER JOIN dbo.Reportante r ON r.IdReportante = i.IdReportante
    WHERE i.IdIncidente = @IdIncidente;
END
GO

-- SGEB - Sistema de Gestión de Emergencias de Bomberos
-- Bloque Unidad: catálogo de unidades (autobombas, ambulancias, etc.)
-- No depende de Incidente ni Reportante, se puede ejecutar en cualquier orden
-- respecto a esos dos, pero DEBE ir antes de 04_incidente_unidad.sql.

-- ============================================================
-- 1. TABLA
-- ============================================================

IF OBJECT_ID('dbo.Unidad', 'U') IS NOT NULL
    DROP TABLE dbo.Unidad;
GO

CREATE TABLE dbo.Unidad (
    IdUnidad        INT IDENTITY(1,1)   NOT NULL PRIMARY KEY,
    CodigoUnidad    VARCHAR(20)         NOT NULL,   -- ej. "B-05"
    TipoUnidad      VARCHAR(50)         NOT NULL,   -- Autobomba, Ambulancia, Escalera, Rescate, Materiales Peligrosos
    Placa           VARCHAR(20)         NULL,
    Capacidad       INT                 NULL,       -- personal que moviliza
    Estacion        VARCHAR(100)        NULL,       -- estación/base a la que pertenece
    Estado          VARCHAR(20)         NOT NULL DEFAULT 'Disponible', -- Disponible, En servicio, Mantenimiento, Fuera de servicio
    FechaRegistro   DATETIME            NOT NULL DEFAULT GETDATE(),
    CONSTRAINT UQ_Unidad_CodigoUnidad UNIQUE (CodigoUnidad)
);
GO

CREATE INDEX IX_Unidad_Estado ON dbo.Unidad(Estado);
GO

-- ============================================================
-- 2. STORED PROCEDURES
-- ============================================================

CREATE OR ALTER PROCEDURE dbo.sp_Unidad_Registrar
    @CodigoUnidad   VARCHAR(20),
    @TipoUnidad     VARCHAR(50),
    @Placa          VARCHAR(20) = NULL,
    @Capacidad      INT = NULL,
    @Estacion       VARCHAR(100) = NULL,
    @IdUnidadNueva  INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Unidad (CodigoUnidad, TipoUnidad, Placa, Capacidad, Estacion, Estado, FechaRegistro)
    VALUES (@CodigoUnidad, @TipoUnidad, @Placa, @Capacidad, @Estacion, 'Disponible', GETDATE());

    SET @IdUnidadNueva = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Unidad_Listar
AS
BEGIN
    SET NOCOUNT ON;

    SELECT IdUnidad, CodigoUnidad, TipoUnidad, Placa, Capacidad, Estacion, Estado, FechaRegistro
    FROM dbo.Unidad
    ORDER BY CodigoUnidad;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Unidad_ListarDisponibles
AS
BEGIN
    SET NOCOUNT ON;

    SELECT IdUnidad, CodigoUnidad, TipoUnidad, Placa, Capacidad, Estacion, Estado, FechaRegistro
    FROM dbo.Unidad
    WHERE Estado = 'Disponible'
    ORDER BY CodigoUnidad;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Unidad_ObtenerPorId
    @IdUnidad INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT IdUnidad, CodigoUnidad, TipoUnidad, Placa, Capacidad, Estacion, Estado, FechaRegistro
    FROM dbo.Unidad
    WHERE IdUnidad = @IdUnidad;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Unidad_ActualizarEstado
    @IdUnidad   INT,
    @NuevoEstado VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Unidad
    SET Estado = @NuevoEstado
    WHERE IdUnidad = @IdUnidad;
END
GO


-- Bloque IncidenteUnidad: asignación de unidades a un incidente (historia "asignar unidad")

-- ============================================================
-- 1. TABLA
-- ============================================================


IF OBJECT_ID('dbo.IncidenteUnidad', 'U') IS NOT NULL
    DROP TABLE dbo.IncidenteUnidad;
GO

CREATE TABLE dbo.IncidenteUnidad (
    IdIncidenteUnidad   INT IDENTITY(1,1)   NOT NULL PRIMARY KEY,
    IdIncidente         INT                 NOT NULL,
    IdUnidad            INT                 NOT NULL,
    FechaHoraAsignacion DATETIME            NOT NULL DEFAULT GETDATE(),
    FechaHoraLlegada    DATETIME            NULL,
    Estado              VARCHAR(20)         NOT NULL DEFAULT 'Asignada', -- Asignada, En camino, En sitio, Liberada
    CONSTRAINT FK_IncidenteUnidad_Incidente FOREIGN KEY (IdIncidente)
        REFERENCES dbo.Incidente(IdIncidente),
    CONSTRAINT FK_IncidenteUnidad_Unidad FOREIGN KEY (IdUnidad)
        REFERENCES dbo.Unidad(IdUnidad)
);
GO

CREATE INDEX IX_IncidenteUnidad_IdIncidente ON dbo.IncidenteUnidad(IdIncidente);
CREATE INDEX IX_IncidenteUnidad_IdUnidad ON dbo.IncidenteUnidad(IdUnidad);
GO

-- ============================================================
-- 2. STORED PROCEDURES
-- ============================================================


GO

CREATE OR ALTER PROCEDURE dbo.sp_IncidenteUnidad_Asignar
    @IdIncidente            INT,
    @IdUnidad               INT,
    @IdIncidenteUnidadNueva INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    -- Reserva atómica: solo pasa si la unidad sigue Disponible
    UPDATE dbo.Unidad SET Estado = 'En servicio'
    WHERE IdUnidad = @IdUnidad AND Estado = 'Disponible';
    IF @@ROWCOUNT = 0 THROW 50001, 'La unidad no está disponible.', 1;

    IF EXISTS (SELECT 1 FROM dbo.Incidente WHERE IdIncidente = @IdIncidente AND Estado = 'Cerrado')
        THROW 50002, 'El incidente ya está cerrado.', 1;

    INSERT INTO dbo.IncidenteUnidad (IdIncidente, IdUnidad, FechaHoraAsignacion, Estado)
    VALUES (@IdIncidente, @IdUnidad, GETDATE(), 'Asignada');

    SET @IdIncidenteUnidadNueva = SCOPE_IDENTITY();

    UPDATE dbo.Incidente SET Estado = 'Asignado'
    WHERE IdIncidente = @IdIncidente AND Estado = 'Registrado';

    COMMIT TRANSACTION;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_IncidenteUnidad_Liberar
    @IdIncidenteUnidad INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    DECLARE @IdUnidad INT;

    -- Marca la asignación como Liberada (solo si aún no lo estaba)
    UPDATE dbo.IncidenteUnidad
    SET Estado = 'Liberada'
    WHERE IdIncidenteUnidad = @IdIncidenteUnidad
      AND Estado <> 'Liberada';

    IF @@ROWCOUNT = 0
        THROW 50003, 'La asignación no existe o ya fue liberada.', 1;

    SELECT @IdUnidad = IdUnidad
    FROM dbo.IncidenteUnidad
    WHERE IdIncidenteUnidad = @IdIncidenteUnidad;

    -- Devuelve la unidad a Disponible (solo si seguía En servicio)
    UPDATE dbo.Unidad
    SET Estado = 'Disponible'
    WHERE IdUnidad = @IdUnidad AND Estado = 'En servicio';

    COMMIT TRANSACTION;
END
GO



CREATE OR ALTER PROCEDURE dbo.sp_IncidenteUnidad_ListarPorIncidente
    @IdIncidente INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT iu.IdIncidenteUnidad, iu.IdIncidente, iu.IdUnidad,
           iu.FechaHoraAsignacion, iu.FechaHoraLlegada, iu.Estado,
           u.CodigoUnidad, u.TipoUnidad
    FROM dbo.IncidenteUnidad iu
    INNER JOIN dbo.Unidad u ON u.IdUnidad = iu.IdUnidad
    WHERE iu.IdIncidente = @IdIncidente
    ORDER BY iu.FechaHoraAsignacion;
END
GO


-- Bloque AtencionCierre: cierre de un incidente (historia "registrar atención/cierre")


-- ============================================================
-- 1. TABLA
-- ============================================================


IF OBJECT_ID('dbo.AtencionCierre', 'U') IS NOT NULL
    DROP TABLE dbo.AtencionCierre;
GO

CREATE TABLE dbo.AtencionCierre (
    IdCierre            INT IDENTITY(1,1)   NOT NULL PRIMARY KEY,
    IdIncidente         INT                 NOT NULL,
    FechaHoraCierre     DATETIME            NOT NULL DEFAULT GETDATE(),
    ResultadoCierre     VARCHAR(50)         NOT NULL,  -- Controlado, Sin novedad, Con pérdidas materiales, Con víctimas, Falsa alarma
    Observaciones       VARCHAR(500)        NULL,
    PersonalInvolucrado INT                 NULL,
    CONSTRAINT FK_AtencionCierre_Incidente FOREIGN KEY (IdIncidente)
        REFERENCES dbo.Incidente(IdIncidente),
    CONSTRAINT UQ_AtencionCierre_IdIncidente UNIQUE (IdIncidente)
);
GO

-- ============================================================
-- 2. STORED PROCEDURES
-- ============================================================

CREATE OR ALTER PROCEDURE dbo.sp_AtencionCierre_Registrar
    @IdIncidente         INT,
    @ResultadoCierre     VARCHAR(50),
    @Observaciones       VARCHAR(500) = NULL,
    @PersonalInvolucrado INT = NULL,
    @IdCierreNuevo       INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    INSERT INTO dbo.AtencionCierre
        (IdIncidente, FechaHoraCierre, ResultadoCierre, Observaciones, PersonalInvolucrado)
    VALUES
        (@IdIncidente, GETDATE(), @ResultadoCierre, @Observaciones, @PersonalInvolucrado);

    SET @IdCierreNuevo = SCOPE_IDENTITY();

    UPDATE dbo.Incidente
    SET Estado = 'Cerrado'
    WHERE IdIncidente = @IdIncidente;

    COMMIT TRANSACTION;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_AtencionCierre_ObtenerPorIncidente
    @IdIncidente INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT IdCierre, IdIncidente, FechaHoraCierre, ResultadoCierre, Observaciones, PersonalInvolucrado
    FROM dbo.AtencionCierre
    WHERE IdIncidente = @IdIncidente;
END
GO


-- Bloque Consultas/Reportes: historias "consultar historial por zona" y

CREATE OR ALTER PROCEDURE dbo.sp_Incidente_ListarPorZona
    @Zona VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT i.IdIncidente, i.IdReportante, i.TipoIncidente, i.Descripcion, i.Direccion, i.Zona,
           i.Latitud, i.Longitud, i.Prioridad, i.Estado, i.FechaHoraRegistro,
           r.Nombre AS NombreReportante, r.Telefono AS TelefonoReportante
    FROM dbo.Incidente i
    INNER JOIN dbo.Reportante r ON r.IdReportante = i.IdReportante
    WHERE i.Zona = @Zona
    ORDER BY i.FechaHoraRegistro DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Reportes_EstadisticasPorTipo
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        i.TipoIncidente,
        COUNT(*)                                                  AS TotalIncidentes,
        SUM(CASE WHEN i.Estado = 'Cerrado'   THEN 1 ELSE 0 END)   AS Cerrados,
        SUM(CASE WHEN i.Estado = 'Registrado' THEN 1 ELSE 0 END)  AS SinAsignar,
        AVG(DATEDIFF(MINUTE, i.FechaHoraRegistro, ac.FechaHoraCierre)) AS MinutosPromedioAtencion
    FROM dbo.Incidente i
    LEFT JOIN dbo.AtencionCierre ac ON ac.IdIncidente = i.IdIncidente
    GROUP BY i.TipoIncidente
    ORDER BY TotalIncidentes DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Reportes_EstadisticasPorZona
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        i.Zona,
        COUNT(*)                                                AS TotalIncidentes,
        SUM(CASE WHEN i.Estado = 'Cerrado' THEN 1 ELSE 0 END)   AS Cerrados
    FROM dbo.Incidente i
    GROUP BY i.Zona
    ORDER BY TotalIncidentes DESC;
END
GO
