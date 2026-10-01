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
- **`sp_IncidenteUnidad_Liberar`**: marca la asignación como `Liberada` y devuelve la unidad a `Disponible`.
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

---

## Instrucciones para el equipo

Quedan **4 partes pendientes**. La base de datos (`database/SGEB.sql`), el módulo de Incidentes, el Home y los exportadores de reportes ya están hechos. Cada integrante elige una parte por el grupo de WhatsApp.

**Patrón a seguir:** copiar la estructura de `Incidente` y `Reportante`: Entity → Repository → Service → ViewModel → Controller → View. Registrar los servicios y repositorios propios en `Program.cs` (`AddScoped`) y, al terminar, activar la tarjeta del módulo en el Home.

### 1️⃣ Catálogo de Unidades (CRUD de camiones/ambulancias)

- Entidad `Unidad`, `IUnidadRepository`, `IUnidadService`, `UnidadController`, ViewModels y vistas (listar, registrar, cambiar estado).
- SPs ya hechos: `sp_Unidad_Registrar`, `_Listar`, `_ListarDisponibles`, `_ObtenerPorId`, `_ActualizarEstado`.
- Regla de negocio: el `CodigoUnidad` no puede repetirse (hay `UNIQUE`, así que capturen ese error y muestren un mensaje claro).

### 2️⃣ Asignar / liberar unidades a un incidente

**Ya está resuelto:** las reglas de negocio viven en `sp_IncidenteUnidad_Asignar`. El SP solo asigna unidades en estado `Disponible` y rechaza incidentes `Cerrado`. No hay que validar eso en el service.

**Lo que falta hacer:**
- Entidad `IncidenteUnidad`, repository, service y controller.
- Vista donde, desde un incidente, se elige una unidad disponible (`sp_Unidad_ListarDisponibles`) y se asigna. Botón para liberar una asignación.
- Mostrar las unidades asignadas a un incidente (`sp_IncidenteUnidad_ListarPorIncidente`).
- Cuando el SP rechaza una asignación, lanza un error de SQL Server con los números `50001` (unidad no disponible) y `50002` (incidente cerrado). Hay que capturarlo en el controller y mostrar su mensaje en el formulario.
- Depende del punto 1 solo para listar unidades; pueden arrancar en paralelo.

### 3️⃣ Atención/Cierre + historial por zona

- Entidad `AtencionCierre`, repo, service, controller y vista: formulario para cerrar un incidente con resultado, observaciones y personal involucrado.
- Regla: solo se puede cerrar un incidente que tenga al menos una unidad asignada, y una sola vez (hay `UNIQUE` por incidente).
- Historial por zona: `sp_Incidente_ListarPorZona` con filtro por zona (agregar el método en `IncidenteRepository` / `IncidenteService`, que ya existen y ya mapean todas las columnas que devuelve el SP).
- SPs ya hechos: `sp_AtencionCierre_Registrar`, `_ObtenerPorIncidente`, `sp_Incidente_ListarPorZona`.

### 4️⃣ Reportes y exportación

**Ya está resuelto:** la exportación completa. El patrón Strategy está implementado en `SGEB.BLL/Exporters` (`IReportExporter` con exportadores PDF, Excel y CSV), los paquetes QuestPDF y ClosedXML están instalados y los tres exportadores están registrados en `Program.cs`. `ReporteController` ya tiene la acción `Exportar`, que descarga el archivo según el parámetro `formato` (`pdf`, `excel` o `csv`). Hoy usa datos de prueba.

**Lo que falta hacer:**
- Repository y service para `sp_Reportes_EstadisticasPorTipo` y `sp_Reportes_EstadisticasPorZona`.
- ViewModels y vistas con las estadísticas por tipo y por zona.
- Botones de descarga (PDF, Excel, CSV) que apunten a `Reporte/Exportar`.
- Reemplazar los datos de prueba de `Exportar` por los datos reales del service. El exportador recibe un título, los nombres de las columnas y las filas como texto, así que basta con convertir el resultado del service a ese formato. Si quieren exportar los dos reportes (tipo y zona), la acción debe recibir además cuál reporte se pide.

### Flujo de trabajo con Git

Todos suben directo a `main`. Antes de subir: `git pull` y después `git push`. Si alguien sube algo roto, se vuelve a la versión anterior.
