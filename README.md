# SGEB — Sistema de Gestión de Emergencias de Bomberos

Sistema web para registrar y dar seguimiento a incidentes reportados a una central de emergencias de bomberos: quién reporta, qué pasó, dónde, con qué prioridad y en qué estado se encuentra.

**Proyecto EFSRT — Equipo N°04 · Cibertec**

## Stack

| Capa | Tecnología |
|---|---|
| Presentación | ASP.NET Core MVC (Razor + Bootstrap) |
| Lógica de negocio | Servicios en C# (`SGEB.BLL`) |
| Acceso a datos | ADO.NET + Stored Procedures (`SGEB.DAL`) |
| Base de datos | SQL Server |
| Herramientas | dotnet CLI, VS Code, Git |

## Arquitectura en 3 capas

```
SGEB.Web  ──►  SGEB.BLL  ──►  SGEB.DAL  ──►  SQL Server
(MVC)         (Services)      (Repositories)   (Stored Procedures)
```

Regla de dependencias: **Web → BLL → DAL**. Ninguna capa salta a la siguiente.

| Proyecto | Responsabilidad |
|---|---|
| `SGEB.Web` | Controllers, ViewModels con validaciones (Data Annotations) y Views Razor |
| `SGEB.BLL` | Reglas de negocio y orquestación entre servicios |
| `SGEB.DAL` | Entidades y Repositories: abre conexión, ejecuta el SP y mapea filas. Sin reglas de negocio |

## Estructura de la solución

```
SGEB/
├── database/
│   └── SGEB.sql
└── src/
    ├── SGEB.Web/
    │   ├── Controllers/
    │   │   ├── IncidenteController.cs
    │   │   └── ReporteController.cs      ← acción Exportar (con datos de prueba)
    │   ├── Models/
    │   │   └── (RegistrarIncidenteViewModel, IncidenteListItemViewModel)
    │   └── Views/
    │       ├── Home/
    │       │   └── Index.cshtml          ← página principal (portal de módulos)
    │       └── Incidente/
    │           ├── Index.cshtml
    │           └── Registrar.cshtml
    ├── SGEB.BLL/
    │   ├── Exporters/                    ← patrón Strategy (PDF, Excel, CSV)
    │   │   ├── IReportExporter.cs
    │   │   ├── PdfReportExporter.cs
    │   │   ├── ExcelReportExporter.cs
    │   │   └── CsvReportExporter.cs
    │   └── Services/
    │       ├── IIncidenteService.cs / IncidenteService.cs
    │       └── IReportanteService.cs / ReportanteService.cs
    └── SGEB.DAL/
        ├── Entities/
        │   ├── Incidente.cs
        │   └── Reportante.cs
        └── Repositories/
            ├── IIncidenteRepository.cs / IncidenteRepository.cs
            └── IReportanteRepository.cs / ReportanteRepository.cs
```

## Avance actual

| Componente | Estado |
|---|---|
| Solución en 3 capas (Web, BLL, DAL) | ✅ Terminado |
| Base de datos (`SGEB.sql`) | ✅ Terminado |
| Módulo de Incidentes | ✅ Terminado |
| Página principal (Home) | ✅ Terminado |
| Exportadores de reportes (Strategy: PDF, Excel, CSV) | ✅ Terminado (QuestPDF y ClosedXML instalados) |
| Validaciones de asignación de unidades en el SP | ✅ Terminado |

---

## 1. Solución en 3 capas

Creada con dotnet CLI: `SGEB.Web`, `SGEB.BLL` y `SGEB.DAL`, con referencias entre proyectos configuradas (Web → BLL → DAL). La solución compila sin errores. El paquete `Microsoft.Data.SqlClient` está instalado en DAL.

## 2. Base de datos (`SGEB.sql`)

Script único con tablas, índices y stored procedures, ordenado por dependencias.

### Tablas

| Tabla | Descripción | Restricciones clave |
|---|---|---|
| `Reportante` | Ciudadano que reporta la emergencia | `UNIQUE (NumeroDocumento)` |
| `Incidente` | Orden central del sistema | FK → `Reportante`; índices por `Estado`, `FechaHoraRegistro`, `IdReportante` |
| `Unidad` | Catálogo de unidades (autobomba, ambulancia, escalera, rescate, materiales peligrosos) | `UNIQUE (CodigoUnidad)`; índice por `Estado` |
| `IncidenteUnidad` | Asignación de unidades a un incidente | FK → `Incidente`, FK → `Unidad` |
| `AtencionCierre` | Cierre del incidente y su resultado | FK → `Incidente`; `UNIQUE (IdIncidente)` (un cierre por incidente) |

### Estados manejados

- **Incidente:** `Registrado` → `Asignado` → `Atendido` → `Cerrado`
- **Prioridad:** `Alta`, `Media`, `Baja`
- **Unidad:** `Disponible`, `En servicio`, `Mantenimiento`, `Fuera de servicio`
- **IncidenteUnidad:** `Asignada`, `En camino`, `En sitio`, `Liberada`
- **Resultado de cierre:** `Controlado`, `Sin novedad`, `Con pérdidas materiales`, `Con víctimas`, `Falsa alarma`

### Stored procedures

| Bloque | Procedimientos |
|---|---|
| Reportante | `sp_Reportante_Registrar`, `sp_Reportante_ObtenerPorDocumento`, `sp_Reportante_ObtenerPorId`, `sp_Reportante_Listar` |
| Incidente | `sp_Incidente_Registrar`, `sp_Incidente_Listar`, `sp_Incidente_ObtenerPorId`, `sp_Incidente_ListarPorZona` |
| Unidad | `sp_Unidad_Registrar`, `sp_Unidad_Listar`, `sp_Unidad_ListarDisponibles`, `sp_Unidad_ObtenerPorId`, `sp_Unidad_ActualizarEstado` |
| IncidenteUnidad | `sp_IncidenteUnidad_Asignar`, `sp_IncidenteUnidad_Liberar`, `sp_IncidenteUnidad_ListarPorIncidente` |
| AtencionCierre | `sp_AtencionCierre_Registrar`, `sp_AtencionCierre_ObtenerPorIncidente` |
| Reportes | `sp_Reportes_EstadisticasPorTipo`, `sp_Reportes_EstadisticasPorZona` |

### Integridad transaccional

Los SP que tocan más de una tabla usan `BEGIN TRANSACTION` + `SET XACT_ABORT ON`:

- **`sp_IncidenteUnidad_Asignar`**: reserva la unidad (solo si sigue `Disponible`), rechaza incidentes `Cerrado`, inserta la asignación y pasa el incidente de `Registrado` a `Asignado`. Si una regla falla, lanza un error con `THROW` (códigos `50001` unidad no disponible, `50002` incidente cerrado) y revierte todo.
- **`sp_IncidenteUnidad_Liberar`**: marca la asignación como `Liberada` y devuelve la unidad a `Disponible` (solo si seguía `En servicio`). Si la asignación no existe o ya estaba liberada, lanza `THROW` con código `50003`.
- **`sp_AtencionCierre_Registrar`**: inserta el cierre y pasa el incidente a `Cerrado`.

### Reportes disponibles en BD

- **Por tipo:** total de incidentes, cerrados, sin asignar y minutos promedio de atención (registro → cierre).
- **Por zona:** total de incidentes y cerrados.

## 3. Módulo de Incidentes

Módulo completo de punta a punta: **registrar** y **listar** incidentes.

### DAL — Entidades

**`Incidente`**: `IdIncidente`, `IdReportante`, `TipoIncidente`, `Descripcion`, `Direccion`, `Zona`, `Latitud?`, `Longitud?`, `Prioridad` (default `Media`), `Estado` (default `Registrado`), `FechaHoraRegistro`, más `NombreReportante?` y `TelefonoReportante?` (llenados por el JOIN de los SP de lectura).

**`Reportante`**: `IdReportante`, `TipoDocumento` (default `DNI`), `NumeroDocumento`, `Nombre`, `Telefono?`, `Correo?`, `FechaRegistro`.

Las entidades son POCO puros: sin atributos de EF ni de validación. La validación de formulario vive en Web (ViewModels) y las reglas en BLL.

### DAL — Repositories

| Interfaz | Métodos |
|---|---|
| `IIncidenteRepository` | `Registrar`, `Listar`, `ObtenerPorId` |
| `IReportanteRepository` | `Registrar`, `ObtenerPorDocumento`, `ObtenerPorId`, `Listar` |

- Cadena de conexión `SGEBConnection` leída desde `appsettings.json` vía `IConfiguration`; si falta, lanza `InvalidOperationException` con mensaje claro.
- Cada método abre su `SqlConnection`/`SqlCommand` con `using`, ejecuta el SP (`CommandType.StoredProcedure`) y mapea con un método privado (`MapearIncidente`, `MapearReportante`).
- Parámetros nulos (`Latitud`, `Longitud`, `Telefono`, `Correo`) se envían como `DBNull.Value`.
- El ID generado vuelve por parámetro `OUTPUT` (`@IdIncidenteNuevo`, `@IdReportanteNuevo`).

### BLL — Servicios

**`IncidenteService`** — `RegistrarIncidente`, `ObtenerTodos`, `ObtenerPorId`

Reglas de negocio de `RegistrarIncidente`:
1. La descripción es obligatoria (`ArgumentException` si viene vacía).
2. Si la prioridad no es `Alta`, `Media` o `Baja`, se normaliza a `Media`.
3. Resuelve al reportante llamando a `IReportanteService.ObtenerOCrear` **antes** de insertar el incidente.
4. Asigna `IdReportante`, fija `Estado = "Registrado"` y `FechaHoraRegistro = DateTime.Now`.
5. Inserta y devuelve el ID del nuevo incidente.

**`ReportanteService`** — `ObtenerOCrear`, `ObtenerTodos`, `ObtenerPorId`

Patrón **find-or-create** en `ObtenerOCrear`:
1. Valida que número de documento y nombre no estén vacíos.
2. Busca por tipo + número de documento.
3. Si existe, devuelve su `IdReportante`; si no, lo registra y devuelve el nuevo ID.

> `IncidenteService` orquesta con `IReportanteService` (no con `IReportanteRepository`): resolver "quién reporta" es lógica de negocio, no responsabilidad del Repository.

### Web — ViewModels

**`RegistrarIncidenteViewModel`** — datos del incidente y del reportante en un solo formulario, con validación por Data Annotations:

| Campo | Validación |
|---|---|
| `TipoIncidente` | Requerido |
| `Descripcion` | Requerido, máx. 500 |
| `Direccion` | Requerido, máx. 200 |
| `Zona` | Requerido, máx. 100 |
| `Latitud`, `Longitud` | Opcionales |
| `Prioridad` | Requerido (default `Media`) |
| `TipoDocumento` | Requerido (default `DNI`) |
| `NumeroDocumento` | Requerido, máx. 20 |
| `NombreReportante` | Requerido, máx. 150 |
| `TelefonoReportante` | Opcional, formato `[Phone]` |
| `CorreoReportante` | Opcional, formato `[EmailAddress]` |

**`IncidenteListItemViewModel`** — proyección para la tabla del listado: `IdIncidente`, `TipoIncidente`, `Direccion`, `Zona`, `Prioridad`, `Estado`, `FechaHoraRegistro`, `NombreReportante`.

### Web — `IncidenteController`

| Ruta | Acción |
|---|---|
| `GET /Incidente` | `Index`: lista los incidentes mapeando entidad → `IncidenteListItemViewModel` |
| `GET /Incidente/Registrar` | Muestra el formulario vacío |
| `POST /Incidente/Registrar` | Valida el modelo, arma `Incidente` + `Reportante` y llama a `RegistrarIncidente` |

Detalles del `POST`:
- Protegido con `[ValidateAntiForgeryToken]`.
- Si `ModelState` es inválido, devuelve la vista con los errores.
- Captura `ArgumentException` del BLL y la muestra como error general del formulario.
- En éxito, guarda `TempData["Mensaje"]` (“Incidente #N registrado correctamente.”) y redirige al `Index`.

### Web — Vistas

- **`Incidente/Index.cshtml`**: tabla Bootstrap (`#`, Tipo, Dirección, Zona, Reportante, Prioridad, Estado, Fecha en formato `dd/MM/yyyy HH:mm`), alerta de éxito con `TempData` y botón “+ Nuevo incidente”.
- **`Incidente/Registrar.cshtml`**: formulario en dos bloques (datos del incidente y datos del reportante), con selectores para tipo de incidente (Incendio, Rescate, Accidente de tránsito, Materiales peligrosos, Otro), prioridad y tipo de documento (DNI, Carné de extranjería, Pasaporte), y validación cliente con `_ValidationScriptsPartial`.

## 4. Página principal

Vista `Home/Index.cshtml` terminada: portal de entrada con el título **SGEB** y el subtítulo “Sistema de Gestión de Emergencias de Bomberos”, más una grilla responsive de 4 tarjetas Bootstrap (1 columna en móvil, 2 en tablet, 4 en escritorio), una por módulo:

| Tarjeta | Descripción | Estado |
|---|---|---|
| Incidentes | Registrar y consultar incidentes reportados | ✅ Activa (enlaza a `Incidente/Index` con `asp-controller` / `asp-action`) |
| Unidades | Catálogo y asignación de unidades a incidentes | ⏳ “Próximamente” (botón deshabilitado) |
| Atención / Cierre | Registro del cierre y resultado de un incidente | ⏳ “Próximamente” (botón deshabilitado) |
| Reportes | Estadísticas y exportación a PDF, Excel y CSV | ⏳ “Próximamente” (botón deshabilitado) |

Las tarjetas pendientes están en gris (`text-muted`) y llevan un comentario que indica el cambio a hacer. Cuando exista el controller del módulo, quien lo desarrolle reemplaza el botón deshabilitado por:

```html
<a asp-controller="Unidad" asp-action="Index" class="btn btn-primary">Entrar</a>
```

---


## Cómo ejecutar

1. Ejecutar `database/SGEB.sql` en SQL Server (crea tablas y stored procedures; usa `DROP TABLE` previo, así que **recrea las tablas desde cero**).
2. Configurar la cadena de conexión en `src/SGEB.Web/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "SGEBConnection": "Server=TU_SERVIDOR;Database=SGEB;Trusted_Connection=True;TrustServerCertificate=True;"
     }
   }
   ```
3. Desde la raíz de la solución:
   ```bash
   dotnet build
   dotnet run --project src/SGEB.Web
   ```
4. Abrir `/Incidente` en el navegador.

## Equipo N°04

- César Jesús Burgos Villón (Coordinador)
- Giovanne Aldahir Lozano Barturen
- Cristian Jheremy Trujillo Loayza
- Giancarlo Emerson Iquise Huamani
- Franz


## Cambios Giovanne

Parte 4 del proyecto: **Reportes y exportación**. Muestra las estadísticas por tipo y por zona en pantalla y permite descargarlas en PDF, Excel y CSV con datos reales de la base de datos (antes `Exportar` usaba datos de prueba).

### Avance

| Componente | Estado |
|---|---|
| Entidades de estadísticas (DAL) | ✅ Terminado |
| `ReporteRepository` sobre los 2 SP de reportes (DAL) | ✅ Terminado |
| `ReporteService` (BLL) | ✅ Terminado |
| ViewModels de reportes (Web) | ✅ Terminado |
| `ReporteController`: pantallas + `Exportar` con datos reales | ✅ Terminado |
| Vistas `PorTipo`, `PorZona` y botones de descarga | ✅ Terminado |
| Registro en `Program.cs` | ✅ Terminado |
| Tarjeta "Reportes" activa en el Home | ✅ Terminado |

### Archivos nuevos y modificados

| Capa | Archivo | Cambio |
|---|---|---|
| DAL | `Entities/EstadisticaTipo.cs` | Nuevo |
| DAL | `Entities/EstadisticaZona.cs` | Nuevo |
| DAL | `Repositories/IReporteRepository.cs` | Nuevo |
| DAL | `Repositories/ReporteRepository.cs` | Nuevo |
| BLL | `Services/IReporteService.cs` | Nuevo |
| BLL | `Services/ReporteService.cs` | Nuevo |
| Web | `Models/ReporteViewModels.cs` | Nuevo (2 ViewModels) |
| Web | `Controllers/ReporteController.cs` | Modificado: acciones `Index`, `PorTipo`, `PorZona` y `Exportar` con datos reales |
| Web | `Views/Reporte/PorTipo.cshtml` | Nuevo |
| Web | `Views/Reporte/PorZona.cshtml` | Nuevo |
| Web | `Views/Reporte/_BotonesExportar.cshtml` | Nuevo (partial con los 3 botones) |
| Web | `Views/Home/Index.cshtml` | Modificado: tarjeta "Reportes" activa |
| Web | `Program.cs` | Modificado: registro de `IReporteRepository` e `IReporteService` |

### DAL

**Entidades** (POCO puros, una propiedad por columna que devuelve el SP):

| Entidad | Propiedades |
|---|---|
| `EstadisticaTipo` | `TipoIncidente`, `TotalIncidentes`, `Cerrados`, `SinAsignar`, `MinutosPromedioAtencion` (`int?`) |
| `EstadisticaZona` | `Zona`, `TotalIncidentes`, `Cerrados` |

> `MinutosPromedioAtencion` es `int?` porque `sp_Reportes_EstadisticasPorTipo` devuelve `NULL` cuando un tipo de incidente todavía no tiene ningún cierre. El repository lo lee con `IsDBNull`.

**Repository** (`IReporteRepository` / `ReporteRepository`), un método por SP:

| Método | Stored procedure |
|---|---|
| `ObtenerEstadisticasPorTipo()` | `sp_Reportes_EstadisticasPorTipo` |
| `ObtenerEstadisticasPorZona()` | `sp_Reportes_EstadisticasPorZona` |

Sigue el mismo patrón que `IncidenteRepository`: cadena `SGEBConnection` desde `IConfiguration`, `SqlConnection`/`SqlCommand` con `using`, `CommandType.StoredProcedure` y mapeo con `GetOrdinal`.

### BLL

`IReporteService` / `ReporteService` exponen los mismos dos métodos y delegan en el repository. No hay reglas de negocio: los SP ya calculan todo.

### Web

**ViewModels** (`Models/ReporteViewModels.cs`):

| ViewModel | Campos |
|---|---|
| `EstadisticaTipoViewModel` | `TipoIncidente`, `TotalIncidentes`, `Cerrados`, `SinAsignar`, `MinutosPromedioAtencion?`, `PorcentajeCierre` (calculado) |
| `EstadisticaZonaViewModel` | `Zona`, `TotalIncidentes`, `Cerrados`, `PorcentajeCierre` (calculado) |

**ReporteController**

| Ruta | Acción |
|---|---|
| `GET /Reporte` | `Index`: redirige a `PorTipo` |
| `GET /Reporte/PorTipo` | Tabla de estadísticas por tipo de incidente |
| `GET /Reporte/PorZona` | Tabla de estadísticas por zona |
| `GET /Reporte/Exportar?reporte=tipo\|zona&formato=pdf\|excel\|csv` | Descarga el reporte pedido en el formato pedido |

Detalles de `Exportar`:

- Recibe un parámetro nuevo, `reporte` (`tipo` o `zona`), para que **una sola acción sirva a los dos reportes**. Si el valor no es válido responde `400`.
- Sigue buscando el exportador por `formato` (Strategy); si el formato no existe responde `400`.
- Los datos salen de los mismos métodos privados (`ObtenerPorTipo`, `ObtenerPorZona`) que alimentan las pantallas, así que **lo descargado coincide con lo que se ve**.
- Convierte lo que devuelve el service a lo que pide el exportador: título, nombres de columnas y filas como texto (los números pasan a `string`; minutos sin dato salen como `-`).
- Agrega una fila **TOTAL** al final de cada reporte.
- El archivo se nombra `reporte-{tipo|zona}-{yyyyMMdd-HHmm}.{extensión}`.

Columnas exportadas:

| Reporte | Columnas |
|---|---|
| Por tipo | Tipo, Total, Cerrados, Sin asignar, Min. promedio, % cierre |
| Por zona | Zona, Total, Cerrados, % cierre |

**Vistas**

- `PorTipo.cshtml` y `PorZona.cshtml`: tabla Bootstrap con fila de totales, pestañas para cambiar entre reportes y mensaje "Todavía no hay incidentes registrados" cuando no hay datos.
- `_BotonesExportar.cshtml`: partial reutilizado por ambas vistas con los botones **PDF**, **Excel** y **CSV**, que apuntan a `Reporte/Exportar` enviando `reporte` y `formato` con `asp-route-*`.

**Home**: en la tarjeta "Reportes" se quitó `text-muted` y el botón deshabilitado se reemplazó por:

```cshtml
<a asp-controller="Reporte" asp-action="PorTipo" class="btn btn-primary">Entrar</a>
```

### Program.cs

```csharp
builder.Services.AddScoped<IReporteRepository, ReporteRepository>();
builder.Services.AddScoped<IReporteService, ReporteService>();
```

### Cómo probar

1. Tener la BD creada con `database/SGEB.sql` y algunos incidentes (con cierres, para que haya minutos promedio).
2. `dotnet run --project src/SGEB.Web` y abrir `/Reporte/PorTipo`.
3. Verificar que se ven las dos tablas (`PorTipo` y `PorZona`) con datos reales.
4. Descargar PDF, Excel y CSV de cada reporte (6 descargas) y comprobar que los datos coinciden con la pantalla.
5. Con la BD vacía, comprobar que no se cae y muestra el mensaje de "sin incidentes".

### Criterios de terminado

- [x] Se ven las dos tablas de estadísticas con datos reales.
- [x] Los tres botones descargan archivo para el reporte por tipo.
- [x] Los tres botones descargan archivo para el reporte por zona.
- [x] Los datos de los archivos coinciden con la pantalla.
- [x] Tarjeta "Reportes" activa en el Home.

---


## Cambios César v1

Integración de la rama `modulo-atencion-cierre` en `main`, estado **Atendido** en el flujo de incidentes y consistencia entre el catálogo de unidades y las asignaciones. Se reescribió `database/SGEB.sql` para que sea ejecutable de principio a fin y traiga datos de prueba.

### Avance

| Componente | Estado |
|---|---|
| Fusión de `modulo-atencion-cierre` en `main` | ✅ Terminado |
| `SGEB.sql`: orden de borrado, creación de BD y datos de prueba | ✅ Terminado |
| `sp_Incidente_MarcarAtendido` (transaccional) | ✅ Terminado |
| `sp_Unidad_ActualizarEstado` consistente con las asignaciones | ✅ Terminado |
| Traducción de errores 50020/50021/50022 en `UnidadService` | ✅ Terminado |

### 1. Fusión de la rama de Atención/Cierre

Se fusionó `origin/modulo-atencion-cierre` en `main`. Hubo un conflicto en `Program.cs`: ambas ramas agregaban registros de inyección de dependencias en el mismo punto.

- **Qué se hizo:** se conservaron los registros de las dos ramas y se eliminaron los duplicados que traía la rama (`IAtencionCierreRepository` x3, `IIncidenteService` x2 y los tres `IReportExporter` repetidos).
- **Por qué:** los exportadores se registran como `IReportExporter` por formato. Con 6 en lugar de 3 se corría el riesgo de errores de clave duplicada al buscarlos por formato.
- Se verificó con `dotnet build` (0 errores) antes de subir y se eliminó la rama remota ya fusionada.

### 2. `database/SGEB.sql`

| Cambio | Por qué |
|---|---|
| La primera línea era `- SGEB ...` (un guion) | Es un error de sintaxis en SQL Server; ahora es comentario (`--`). |
| Sección 0 de limpieza con `DROP TABLE` en orden inverso a las FK | Antes cada tabla se borraba justo antes de crearse; en la segunda ejecución `DROP TABLE Reportante` fallaba porque `Incidente` aún la referenciaba. |
| `CREATE DATABASE SGEB` si no existe + `USE SGEB` | Para que el script no caiga en `master` y pueda correrse sobre un SQL Server vacío. |
| Sección 8: datos de prueba (7 unidades, 6 incidentes, 5 reportantes) | Poder probar el flujo completo sin cargar datos a mano. Los reportantes son necesarios porque `Incidente.IdReportante` es `NOT NULL`. No se insertan asignaciones ni cierres. |

> El script borra y recrea todas las tablas. Para desplegar un cambio en un SP sobre una BD con datos, ejecutar solo ese bloque (`CREATE OR ALTER`).

### 3. Estado Atendido: `sp_Incidente_MarcarAtendido`

El estado `Atendido` existía en el modelo pero ningún flujo lo usaba. Se descartó pasarlo automáticamente al liberar una unidad: un incidente puede tener varias unidades y liberar es también una forma de corregir una asignación, por lo que "liberar" no equivale a "atendido". Se dejó como una acción explícita.

- Todo ocurre en una **transacción** y el estado se lee con `UPDLOCK`, para que dos usuarios no cambien el mismo incidente a la vez.
- Pasa de `Asignado` a `Atendido` y las unidades `Asignada`/`En camino` pasan a `En sitio`.
- Códigos de error: **50010** no existe, **50011** cerrado, **50012** sin unidad asignada, **50013** ya atendido o estado inválido, **50014** sin unidades activas.
- **50014** es nuevo: un incidente puede seguir en `Asignado` aunque todas sus unidades se hayan liberado; sin esta validación podía quedar `Atendido` sin ninguna unidad.
- `sp_IncidenteUnidad_Asignar` ahora también pasa `Atendido` a `Asignado`: si llega una unidad nueva a un incidente ya atendido, vuelve a estar en curso.

### 4. Consistencia entre Unidades e Incidente–Unidad

**Problema detectado en las pruebas:** al liberar una unidad desde el catálogo de Unidades, la pantalla del incidente seguía mostrando la asignación como activa. El estado "en servicio" vive en dos tablas (`Unidad.Estado` e `IncidenteUnidad.Estado`) y el catálogo modificaba solo una.

**Solución:** se corrigió en `sp_Unidad_ActualizarEstado`, no con un trigger ni en C#.

- **Por qué no un trigger:** `Asignar` y `Liberar` también escriben en `Unidad`, así que se dispararía desde ellos. Además, el proyecto ya maneja las reglas entre tablas dentro de los SP con transacción.
- **Por qué no en C#:** serían dos llamadas separadas, no atómicas. Si fallaba la segunda quedaba la misma inconsistencia.
- El SP actualiza la unidad y libera sus asignaciones activas **en la misma transacción**. Se toma primero el bloqueo de `Unidad`, igual que en `Asignar` y `Liberar`.
- **50020:** no se puede poner "En servicio" a mano; solo `Asignar` lo hace. Antes se podía crear una unidad ocupada sin incidente y nada la liberaba.
- **50021:** estado inválido. **50022:** la unidad no existe.

### 5. Capa de aplicación

- `UnidadService.CambiarEstado` captura `SqlException` (50020, 50021, 50022) y la convierte en `ArgumentException` con un mensaje legible. El controller no cambia: ya mostraba `ArgumentException` en `TempData["Error"]`. Así SQL se queda fuera de la capa Web, como indica la regla de dependencias.
- El dropdown de estados del catálogo ya no incluye "En servicio".

### Cómo probar

1. Ejecutar `database/SGEB.sql` (ver "Cómo ejecutar"). Debe dejar 7 unidades, 6 incidentes y 5 reportantes.
2. Asignar `A-01` al incidente #1: la unidad pasa a *En servicio* y el incidente a *Asignado*.
3. En el catálogo, cambiar `A-01` a *Disponible*: la pantalla del incidente muestra la asignación como *Liberada*.
4. Cambiar una unidad a *Mantenimiento* estando asignada: asignación *Liberada* y unidad en *Mantenimiento*.
5. Marcar atendido un incidente con unidad activa: pasa a *Atendido* y la unidad a *En sitio*.
6. Marcar atendido uno sin unidades activas: mensaje claro (50012 o 50014).
7. Llamar `EXEC dbo.sp_Unidad_ActualizarEstado @IdUnidad = 1, @NuevoEstado = 'En servicio'` directo en SQL: debe devolver el error 50020.

### Pendiente / a tener en cuenta

- Si se libera por el catálogo la única unidad de un incidente, este queda en `Asignado` sin unidades activas. No rompe nada (`MarcarAtendido` lo detecta con 50014), pero no se revierte el estado del incidente de forma automática.
- `sp_AtencionCierre_Registrar` no libera las unidades al cerrar: quedan `En servicio`. Falta confirmar con quien tiene la parte 3 si es intencional.

## Cambios César v2

Personalización visual con temática bombero. Solo se tocó el aspecto: no se modificó ninguna lógica, controller, stored procedure ni validación.

### Avance

| Componente | Estado |
|---|---|
| Layout general (navbar, footer, alertas) | ✅ Terminado |
| Hoja de estilos con paleta bombero (`site.css`) | ✅ Terminado |
| Partials compartidos de badges | ✅ Terminado |
| Home | ✅ Terminado |
| Incidente/Index e Incidente/Registrar | ✅ Terminado |
| Ajustes a la vista de Cierre (Giancarlo) | ✅ Terminado |

### Archivos nuevos y modificados

| Archivo | Cambio |
|---|---|
| `Views/Shared/_Layout.cshtml` | Modificado |
| `Views/Shared/_Alertas.cshtml` | Nuevo |
| `Views/Shared/_BadgeEstado.cshtml` | Nuevo |
| `Views/Shared/_BadgePrioridad.cshtml` | Nuevo |
| `wwwroot/css/site.css` | Modificado (reemplazado) |
| `Views/Home/Index.cshtml` | Modificado |
| `Views/Incidente/Index.cshtml` | Modificado |
| `Views/Incidente/Registrar.cshtml` | Modificado |
| `Views/AtencionCierre/Cerrar.cshtml` | Modificado (ajustes menores) |

### Layout y estilos

- **Navbar oscura** con marca SGEB, menú con íconos (Inicio, Incidentes, Unidades, Reportes), resalte de la sección activa y el número de emergencias 116.
- **Footer** con el nombre del sistema y el equipo, siempre pegado al fondo de la página.
- **Favicon** de camión de bomberos y título de pestaña `Página - SGEB`.
- **Alertas centralizadas:** `_Alertas.cshtml` muestra `TempData["Mensaje"]` (verde) y `TempData["Error"]` (rojo) desde el layout. Las vistas ya no necesitan su propio bloque.
- **Íconos:** Bootstrap Icons 1.11.3 cargado por CDN (requiere internet).
- **Paleta en `site.css`:**

| Color | Uso | Valor |
|---|---|---|
| Rojo bombero | Botones primarios, bordes, acentos | `#C8102E` |
| Carbón | Navbar, footer, encabezados de tabla | `#1F2328` |
| Ámbar | Prioridad media, detalles de la navbar | `#F5A300` |

- Clases reutilizables: `.page-header` (cabecera de pantalla), `.card-modulo` (tarjetas del Home), `.fila-alta` (borde rojo en incidentes de prioridad Alta) y `.sgeb-badge` con sus variantes de color.

### Badges compartidos

- `_BadgePrioridad`: Alta = rojo, Media = ámbar, Baja = verde.
- `_BadgeEstado`: un solo partial para todos los estados del sistema.
  - Incidente: Registrado, Asignado, Atendido, Cerrado.
  - Unidad: Disponible, En servicio, Mantenimiento, Fuera de servicio.
  - Asignación: Asignada, En camino, En sitio, Liberada.

Uso: `<partial name="_BadgeEstado" model="item.Estado" />`

### Vistas

- **Home:** tarjetas con ícono y efecto al pasar el mouse. Se limpiaron comentarios sobrantes.
- **Incidente/Index:** tabla dentro de una card, badges de prioridad y estado, borde rojo en incidentes de prioridad Alta, mensaje cuando no hay incidentes, botón "Unidades" en cada fila y botón "Cerrar" visible solo cuando el incidente está en estado Atendido (el controller de cierre solo acepta ese estado).
- **Incidente/Registrar:** formulario separado en dos tarjetas (incidente y reportante), con distribución responsive y botones Cancelar y Volver. Las validaciones y los campos no cambiaron.

### Ajustes a la vista de Cierre (parte de Giancarlo)

En `Views/AtencionCierre/Cerrar.cshtml`:

- Se eliminó `@Html.AntiForgeryToken()`, porque el formulario con `asp-action` ya genera el token y quedaba duplicado.
- El estado del incidente ahora usa el partial `_BadgeEstado`, para verse igual que en el resto del sistema.
- El contenedor `container-fluid` se cambió por un `div` simple, porque el layout ya incluye su propio `container`.

### Cómo probar

1. `dotnet run --project src/SGEB.Web` y abrir el Home: deben verse las 4 tarjetas con ícono y la navbar oscura.
2. Registrar un incidente y comprobar que aparece en la tabla con sus badges.
3. Verificar que un incidente en estado Atendido muestra el botón "Cerrar".

## Instrucciones para el equipo

Quedan **4 partes pendientes**. La base de datos (`database/SGEB.sql`), el módulo de Incidentes, el Home y los exportadores de reportes ya están hechos. Cada integrante elige una parte por el grupo de WhatsApp.

**Patrón a seguir:** copiar la estructura de `Incidente` y `Reportante`: Entity → Repository → Service → ViewModel → Controller → View. Registrar los servicios y repositorios propios en `Program.cs` (`AddScoped`) y, al terminar, activar la tarjeta del módulo en el Home.




## 1️⃣ Catálogo de Unidades

**Qué van a lograr:** una pantalla donde se vean todas las unidades (camiones, ambulancias, etc.), se pueda registrar una nueva y se pueda cambiar su estado.

### Pasos

1. Abran `SGEB.sql` y lean los 5 SPs de Unidad: vean qué parámetros recibe cada uno y qué columnas devuelve.
2. **Entity:** creen `Unidad.cs` en `SGEB.DAL/Entities` con una propiedad por cada columna de la tabla `Unidad`.
3. **Repository:** creen `IUnidadRepository` y `UnidadRepository` con 5 métodos, uno por SP: registrar, listar, listar disponibles, obtener por id y actualizar estado. Copien la forma de `IncidenteRepository`.
4. **Service:** creen `IUnidadService` y `UnidadService`. Aquí va la regla del código repetido (ver abajo).
5. **ViewModels:** uno para el formulario de registro (con validaciones: código y tipo obligatorios) y otro para la fila del listado.
6. **Controller:** `UnidadController` con las acciones: listar, registrar (mostrar formulario y guardar) y cambiar estado.
7. **Vistas:** `Index` (tabla), `Registrar` (formulario) y un formulario o botón para cambiar estado (con lista desplegable: Disponible, En servicio, Mantenimiento, Fuera de servicio).

**La regla del código repetido:** la BD no permite dos unidades con el mismo `CodigoUnidad`. Si intentan registrar una repetida, SQL Server lanza un error de "clave duplicada" (números 2627 o 2601). Hay que capturarlo y mostrar un mensaje como *"Ya existe una unidad con ese código"*, en vez de que se caiga la página.

### Cómo saben que terminaron

- [ ] Se ve el listado de unidades.
- [ ] Registran una unidad nueva y aparece en la tabla.
- [ ] Intentan registrar el mismo código otra vez y sale el mensaje claro.
- [ ] Cambian el estado de una unidad y se actualiza.
- [ ] Tarjeta "Unidades" activa en el Home.

---

## 2️⃣ Asignar / liberar unidades a un incidente

**Qué van a lograr:** desde un incidente, ver qué unidades tiene asignadas, asignarle una unidad disponible y poder liberarla.

### Pasos

1. Lean en `SGEB.sql` los SPs `sp_IncidenteUnidad_Asignar`, `_Liberar`, `_ListarPorIncidente` y `sp_Unidad_ListarDisponibles`. Anoten parámetros y columnas.
2. **Entity:** `IncidenteUnidad.cs`. Para mostrar el código y el tipo de la unidad en pantalla, agreguen esas propiedades extra, igual que `Incidente` tiene `NombreReportante` por el JOIN.
3. **Repository:** `IIncidenteUnidadRepository` y su clase con 3 métodos: asignar, liberar y listar por incidente.
4. **Service:** deja pasar las llamadas al repository. **No validen nada aquí:** el SP ya rechaza unidades no disponibles e incidentes cerrados.
5. **Listar unidades disponibles:** la pantalla necesita una lista desplegable con las unidades disponibles (`sp_Unidad_ListarDisponibles`). Eso pertenece al módulo de Unidades (parte 1). Para no esperar a nadie, pueden agregar ese método en su propio repository y avisar a quien tenga la parte 1 para no duplicar trabajo.
6. **Controller:** `IncidenteUnidadController` con `Index(int idIncidente)` (muestra la pantalla), `Asignar` (POST) y `Liberar` (POST). Después de asignar o liberar, redirigir a `Index` del mismo incidente.
7. **Vista:** una pantalla por incidente con: tabla de unidades asignadas (con botón "Liberar" en cada fila), y un formulario con lista desplegable de unidades disponibles y botón "Asignar".
8. **Enlace de entrada:** agreguen en la tabla de `Incidente/Index.cshtml` un botón "Unidades" por fila que lleve a su pantalla (es el único cambio que harán en esa vista).

**El manejo de errores (lo más importante):** cuando el SP rechaza algo, SQL Server devuelve un error con número:

- **50001** = la unidad no está disponible
- **50002** = el incidente ya está cerrado
- **50003** = la asignación no existe o ya fue liberada (solo en Liberar)

En el controller, capturen ese error, miren su número y muestren el mensaje en el formulario, como `Incidente` hace con los errores de validación.

### Cómo saben que terminaron

- [ ] Asignan una unidad disponible a un incidente y aparece en la tabla.
- [ ] Al asignarla, el incidente pasa de Registrado a Asignado.
- [ ] Liberan la unidad y vuelve a estar disponible.
- [ ] Intentan asignar una unidad ocupada y sale un mensaje claro (50001).
- [ ] Intentan asignar a un incidente cerrado y sale un mensaje claro (50002).
- [ ] Intentan liberar una asignación ya liberada y sale un mensaje claro (50003).

---

## 3️⃣ Atención/Cierre + historial por zona

**Qué van a lograr:** un formulario para cerrar un incidente, y una pantalla para ver el historial de incidentes filtrado por zona.

### Pasos para el cierre

1. Lean `sp_AtencionCierre_Registrar` y `_ObtenerPorIncidente` en `SGEB.sql`. Revisen también si el SP ya valida lo de las unidades asignadas.
2. **Entity:** `AtencionCierre.cs`.
3. **Repository:** `IAtencionCierreRepository` y su clase con 2 métodos: registrar y obtener por incidente.
4. **Service:** aquí van las reglas:
   - Solo se puede cerrar un incidente que tenga **al menos una unidad asignada**. Pueden comprobarlo con `sp_IncidenteUnidad_ListarPorIncidente` (el SP de la parte 2). Si el SP de cierre no lo valida, la validación va aquí.
   - Solo se cierra **una vez**: la BD tiene un UNIQUE por incidente, así que si intentan cerrar otra vez saltará un error. Captúrenlo y muestren *"Este incidente ya fue cerrado"*.
5. **ViewModel:** con resultado (lista desplegable: Controlado, Sin novedad, Con pérdidas materiales, Con víctimas, Falsa alarma), observaciones y personal involucrado.
6. **Controller y vista:** formulario de cierre que se abre desde un incidente. Agreguen un botón "Cerrar" por fila en `Incidente/Index.cshtml`.

### Pasos para el historial por zona

1. Abran `IncidenteRepository` e `IncidenteService` (ya existen) y agreguen un método nuevo que llame a `sp_Incidente_ListarPorZona`, recibiendo la zona. El mapeo de columnas ya está hecho; solo copien el patrón del método `Listar`.
2. Hagan una vista con un cuadro de texto o lista de zonas, un botón "Buscar" y una tabla con los resultados (pueden reutilizar `IncidenteListItemViewModel`).

### Cómo saben que terminaron

- [ ] Cierran un incidente con unidad asignada y pasa a estado Cerrado.
- [ ] Intentan cerrar uno sin unidades y sale un mensaje claro.
- [ ] Intentan cerrar el mismo dos veces y sale un mensaje claro.
- [ ] Filtran por una zona y solo aparecen los incidentes de esa zona.
- [ ] Tarjeta "Atención / Cierre" activa en el Home.

---

## 4️⃣ Reportes y exportación

**Qué van a lograr:** mostrar las estadísticas por tipo y por zona en pantalla, y que los botones PDF, Excel y CSV descarguen esos mismos datos. La exportación ya funciona; solo hay que conectarle datos reales.

### Pasos

1. Lean `sp_Reportes_EstadisticasPorTipo` y `sp_Reportes_EstadisticasPorZona` en `SGEB.sql`. Noten qué columnas devuelve cada uno (total, cerrados, sin asignar, minutos promedio, etc.).
2. **Repository y Service:** creen un repository y un service para estos dos SPs. Cada uno tiene un método. Los resultados pueden ser entidades pequeñas con las columnas que devuelven.
3. **ViewModels:** uno para las filas del reporte por tipo y otro para por zona.
4. **Controller:** en `ReporteController` ya existe la acción `Exportar`. Agreguen las acciones para mostrar los dos reportes en pantalla.
5. **Vistas:** una tabla por reporte, cada una con tres botones de descarga (PDF, Excel, CSV) que apunten a `Reporte/Exportar`.
6. **Conectar los datos reales:** hoy `Exportar` usa datos de prueba. Hay que reemplazarlos. El exportador recibe tres cosas: un **título**, los **nombres de las columnas** y las **filas como texto**. Entonces, el paso es tomar lo que devuelve el service y convertirlo a esas tres cosas (los números pasan a texto).
7. **Dos reportes, una acción:** como `Exportar` debe servir para ambos reportes, hay que agregarle un parámetro que diga cuál se pide (por ejemplo "tipo" o "zona"), y que los botones lo envíen.

### Cómo saben que terminaron

- [ ] Se ven las dos tablas de estadísticas con datos reales.
- [ ] Los tres botones descargan archivo para el reporte por tipo.
- [ ] Los tres botones descargan archivo para el reporte por zona.
- [ ] Al abrir los archivos descargados, los datos coinciden con lo que se ve en pantalla.
- [ ] Tarjeta "Reportes" activa en el Home.

### Flujo de trabajo con Git

Todos suben directo a `main`. Antes de subir: `git pull` y después `git push`. Si alguien sube algo roto, se vuelve a la versión anterior.
