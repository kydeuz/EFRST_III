-- ============================================================
-- SGEB - Sistema de Gestión de Emergencias de Bomberos
-- Script único: tablas, índices y stored procedures
-- ============================================================

IF DB_ID('SGEB') IS NULL
    CREATE DATABASE SGEB;
GO

USE SGEB;
GO

-- ============================================================
-- 0. LIMPIEZA (orden inverso a las dependencias / FK)
-- ============================================================

IF OBJECT_ID('dbo.AtencionCierre', 'U')  IS NOT NULL DROP TABLE dbo.AtencionCierre;
IF OBJECT_ID('dbo.IncidenteUnidad', 'U') IS NOT NULL DROP TABLE dbo.IncidenteUnidad;
IF OBJECT_ID('dbo.Incidente', 'U')       IS NOT NULL DROP TABLE dbo.Incidente;
IF OBJECT_ID('dbo.Unidad', 'U')          IS NOT NULL DROP TABLE dbo.Unidad;
IF OBJECT_ID('dbo.Reportante', 'U')      IS NOT NULL DROP TABLE dbo.Reportante;
GO

-- ============================================================
-- 1. BLOQUE REPORTANTE
-- ============================================================

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

-- ============================================================
-- 2. BLOQUE INCIDENTE
-- ============================================================

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

-- ============================================================
-- 3. BLOQUE UNIDAD
-- ============================================================

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

-- Cambio manual de estado desde el catálogo de unidades.
-- Errores: 50020 intento de poner 'En servicio' a mano (solo lo hace Asignar),
--          50021 estado inválido, 50022 la unidad no existe.
-- Si la unidad tenía asignaciones activas, se liberan en la misma transacción
-- para que el catálogo y la pantalla del incidente no se contradigan.
CREATE OR ALTER PROCEDURE dbo.sp_Unidad_ActualizarEstado
    @IdUnidad    INT,
    @NuevoEstado VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @NuevoEstado NOT IN ('Disponible', 'En servicio', 'Mantenimiento', 'Fuera de servicio')
        THROW 50021, 'Estado de unidad inválido.', 1;

    IF @NuevoEstado = 'En servicio'
        THROW 50020, 'Una unidad solo pasa a En servicio al asignarla a un incidente.', 1;

    BEGIN TRANSACTION;

    -- Primero la fila de Unidad (mismo orden de bloqueo que Asignar y Liberar)
    UPDATE dbo.Unidad
    SET Estado = @NuevoEstado
    WHERE IdUnidad = @IdUnidad;

    IF @@ROWCOUNT = 0
        THROW 50022, 'La unidad no existe.', 1;

    UPDATE dbo.IncidenteUnidad
    SET Estado = 'Liberada'
    WHERE IdUnidad = @IdUnidad
      AND Estado <> 'Liberada';

    COMMIT TRANSACTION;
END
GO

-- ============================================================
-- 4. BLOQUE INCIDENTEUNIDAD (asignar / liberar)
-- ============================================================

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

-- Errores: 50001 unidad no disponible, 50002 incidente cerrado
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

    -- Registrado -> Asignado. Atendido -> Asignado: si llega una unidad nueva a un
    -- incidente ya atendido, vuelve a estar en curso (evita Atendido con unidad en camino).
    UPDATE dbo.Incidente SET Estado = 'Asignado'
    WHERE IdIncidente = @IdIncidente AND Estado IN ('Registrado', 'Atendido');

    COMMIT TRANSACTION;
END
GO

-- Error: 50003 asignación no existe o ya liberada
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

-- ============================================================
-- 5. MARCAR INCIDENTE COMO ATENDIDO
-- Errores: 50010 no existe, 50011 cerrado, 50012 sin unidad asignada (Registrado),
--          50013 ya atendido / estado inválido, 50014 sin unidades activas
-- ============================================================

CREATE OR ALTER PROCEDURE dbo.sp_Incidente_MarcarAtendido
    @IdIncidente INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    DECLARE @Estado VARCHAR(20);

    -- UPDLOCK: evita que otra sesión cambie el estado entre la lectura y el UPDATE
    SELECT @Estado = Estado
    FROM dbo.Incidente WITH (UPDLOCK, ROWLOCK)
    WHERE IdIncidente = @IdIncidente;

    IF @Estado IS NULL
        THROW 50010, 'El incidente no existe.', 1;

    IF @Estado = 'Cerrado'
        THROW 50011, 'El incidente ya está cerrado.', 1;

    IF @Estado = 'Registrado'
        THROW 50012, 'El incidente debe tener una unidad asignada antes de ser atendido.', 1;

    IF @Estado <> 'Asignado'
        THROW 50013, 'El incidente ya fue atendido o no tiene un estado válido.', 1;

    -- Estado = Asignado, pero puede que todas sus unidades ya se hayan liberado
    IF NOT EXISTS (
        SELECT 1 FROM dbo.IncidenteUnidad
        WHERE IdIncidente = @IdIncidente AND Estado <> 'Liberada'
    )
        THROW 50014, 'El incidente no tiene unidades activas asignadas.', 1;

    UPDATE dbo.Incidente
    SET Estado = 'Atendido'
    WHERE IdIncidente = @IdIncidente;

    UPDATE dbo.IncidenteUnidad
    SET Estado = 'En sitio',
        FechaHoraLlegada = COALESCE(FechaHoraLlegada, GETDATE())
    WHERE IdIncidente = @IdIncidente
      AND Estado IN ('Asignada', 'En camino');

    COMMIT TRANSACTION;
END
GO

-- ============================================================
-- 6. BLOQUE ATENCIONCIERRE
-- ============================================================

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

-- ============================================================
-- 7. BLOQUE REPORTES
-- ============================================================

CREATE OR ALTER PROCEDURE dbo.sp_Reportes_EstadisticasPorTipo
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        i.TipoIncidente,
        COUNT(*)                                                       AS TotalIncidentes,
        SUM(CASE WHEN i.Estado = 'Cerrado'    THEN 1 ELSE 0 END)       AS Cerrados,
        SUM(CASE WHEN i.Estado = 'Registrado' THEN 1 ELSE 0 END)       AS SinAsignar,
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

-- ============================================================
-- 8. DATOS DE PRUEBA (solo reportantes, incidentes y unidades)
-- ============================================================

INSERT INTO dbo.Reportante (TipoDocumento, NumeroDocumento, Nombre, Telefono, Correo)
VALUES
    ('DNI', '70000001', 'Luis Ramírez Quispe',   '987654321', 'luis.ramirez@example.com'),
    ('DNI', '70000002', 'María Torres Huamán',   '987654322', NULL),
    ('DNI', '70000003', 'Carlos Mendoza Rojas',  '987654323', 'carlos.mendoza@example.com'),
    ('CE',  '001234567', 'Ana Gómez Pérez',      '987654324', NULL),
    ('DNI', '70000005', 'Jorge Salazar Vega',    NULL,        NULL);
GO

INSERT INTO dbo.Unidad (CodigoUnidad, TipoUnidad, Placa, Capacidad, Estacion)
VALUES
    ('B-01', 'Autobomba',            'BMB-101', 6, 'Compañía Central'),
    ('B-02', 'Autobomba',            'BMB-102', 6, 'Compañía Norte'),
    ('A-01', 'Ambulancia',           'AMB-201', 3, 'Compañía Central'),
    ('A-02', 'Ambulancia',           'AMB-202', 3, 'Compañía Norte'),
    ('E-01', 'Escalera',             'ESC-301', 4, 'Compañía Central'),
    ('R-01', 'Rescate',              'RES-401', 5, 'Compañía Norte'),
    ('M-01', 'Materiales Peligrosos','MAT-501', 5, 'Compañía Central');
GO

INSERT INTO dbo.Incidente
    (IdReportante, TipoIncidente, Descripcion, Direccion, Zona, Latitud, Longitud, Prioridad, FechaHoraRegistro)
VALUES
    ((SELECT IdReportante FROM dbo.Reportante WHERE NumeroDocumento = '70000001'),
     'Incendio', 'Fuego en vivienda de dos pisos, se observa humo denso desde la azotea.',
     'Av. Universitaria 1250', 'Puente Piedra', -11.866700, -77.075000, 'Alta', DATEADD(MINUTE, -180, GETDATE())),

    ((SELECT IdReportante FROM dbo.Reportante WHERE NumeroDocumento = '70000002'),
     'Accidente de tránsito', 'Choque entre un bus y un automóvil, hay personas atrapadas.',
     'Panamericana Norte Km 28', 'Puente Piedra', -11.860000, -77.070000, 'Alta', DATEADD(MINUTE, -150, GETDATE())),

    ((SELECT IdReportante FROM dbo.Reportante WHERE NumeroDocumento = '70000003'),
     'Rescate', 'Persona atrapada en el ascensor de un edificio desde hace una hora.',
     'Jr. Los Pinos 340', 'Comas', NULL, NULL, 'Media', DATEADD(MINUTE, -120, GETDATE())),

    ((SELECT IdReportante FROM dbo.Reportante WHERE NumeroDocumento = '001234567'),
     'Materiales peligrosos', 'Fuga de gas en un almacén, fuerte olor a químico en la zona.',
     'Av. Naranjal 890', 'Los Olivos', -11.990000, -77.070000, 'Alta', DATEADD(MINUTE, -90, GETDATE())),

    ((SELECT IdReportante FROM dbo.Reportante WHERE NumeroDocumento = '70000005'),
     'Otro', 'Árbol caído sobre la vereda obstruyendo el paso peatonal.',
     'Calle Las Magnolias 125', 'Independencia', NULL, NULL, 'Baja', DATEADD(MINUTE, -60, GETDATE())),

    ((SELECT IdReportante FROM dbo.Reportante WHERE NumeroDocumento = '70000001'),
     'Incendio', 'Cortocircuito con llamas en un puesto del mercado.',
     'Mercado Modelo, Puesto 45', 'Comas', -11.935000, -77.050000, 'Media', DATEADD(MINUTE, -30, GETDATE()));
GO