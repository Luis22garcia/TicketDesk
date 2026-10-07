# 🎫 TicketDesk 2.0

Sistema de gestión de tickets de soporte técnico desarrollado como proyecto de portafolio profesional con **C#, .NET 10, ASP.NET Core Web API, Entity Framework Core y SQL Server**.

TicketDesk 2.0 permite gestionar solicitudes de soporte técnico, usuarios, categorías, asignaciones, estados, comentarios, historial y notificaciones mediante una API REST protegida con **JWT y autorización basada en roles**.

Actualmente el **backend se encuentra funcional y en etapa de pruebas y mejoras**. El frontend con React se encuentra pendiente de desarrollo.

---

## 🚀 Tecnologías utilizadas

### Backend

- C#
- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- JWT Authentication
- BCrypt
- Swagger / OpenAPI

### Herramientas

- Visual Studio Community
- SQL Server Management Studio
- Swagger
- Git
- GitHub
- Postman — previsto para pruebas adicionales

### Frontend

El frontend todavía está pendiente de desarrollo.

Tecnologías previstas:

- React
- Bootstrap o Tailwind CSS

---

# 📌 Características principales

Actualmente el backend cuenta con:

- 🔐 Autenticación mediante JWT.
- 🔑 Contraseñas protegidas mediante BCrypt.
- 👥 Sistema de usuarios.
- 👑 Roles Administrador, Agente y Usuario.
- 🏷️ Gestión de categorías.
- 🎫 Gestión de tickets.
- 🛠️ Asignación de tickets a agentes.
- 🔄 Control de estados.
- 📝 Historial de tickets.
- 💬 Comentarios.
- 🔔 Notificaciones.
- 📊 Dashboard.
- 🔒 Autorización mediante roles.
- 🛡️ Restricciones de acceso según propietario y asignación.
- 🗄️ SQL Server mediante Entity Framework Core.
- 📦 Migraciones de Entity Framework Core.
- 📖 Documentación mediante Swagger/OpenAPI.

---

# 👥 Roles del sistema

El sistema contempla tres roles principales.

## 👑 Administrador

Tiene acceso a las funciones administrativas del sistema.

Puede:

- Gestionar usuarios.
- Gestionar categorías.
- Consultar tickets.
- Asignar tickets a agentes.
- Actualizar tickets.
- Eliminar tickets.
- Consultar dashboard.
- Consultar historial.
- Consultar notificaciones.

---

## 🛠️ Agente

El agente es responsable de atender los tickets que le fueron asignados.

Puede:

- Consultar tickets asignados.
- Actualizar tickets asignados.
- Cambiar estados.
- Agregar comentarios.
- Consultar historial de los tickets que puede gestionar.
- Consultar sus notificaciones.

---

## 👤 Usuario

El usuario puede crear y consultar sus solicitudes de soporte.

Puede:

- Crear tickets.
- Consultar sus propios tickets.
- Agregar comentarios a sus tickets.
- Consultar historial de sus tickets.
- Consultar sus notificaciones.

---

# 🔐 Autenticación y seguridad

El sistema utiliza autenticación basada en **JWT (JSON Web Tokens)**.

### Implementado

- Inicio de sesión mediante correo y contraseña.
- Validación de credenciales.
- Contraseñas protegidas mediante BCrypt.
- Generación de tokens JWT.
- Validación de tokens.
- Expiración de tokens.
- Issuer.
- Audience.
- Autorización mediante roles.
- Protección de endpoints mediante `[Authorize]`.
- Restricciones de acceso según el usuario autenticado.
- Validación de usuarios activos.

### Flujo de autenticación

```text
Usuario
   │
   ▼
POST /api/Auth/login
   │
   ▼
Validación de correo y contraseña
   │
   ▼
BCrypt
   │
   ▼
Generación de JWT
   │
   ▼
Cliente
   │
   ▼
Authorization: Bearer <token>
   │
   ▼
Endpoint protegido
```

---

# 🗄️ Base de datos

El proyecto utiliza **SQL Server** como sistema gestor de base de datos y **Entity Framework Core** como ORM.

## Tablas principales

```text
Usuarios
Categorias
Tickets
HistorialTickets
ComentariosTickets
Notificaciones
```

## Relaciones principales

```text
Usuario 1 ──── N Tickets

Categoria 1 ──── N Tickets

Usuario 1 ──── N Comentarios

Ticket 1 ──── N Comentarios

Ticket 1 ──── N Historial

Usuario 1 ──── N Historial

Usuario 1 ──── N Notificaciones

Ticket 1 ──── N Notificaciones
```

---

# 📦 Entity Framework Core y migraciones

El proyecto utiliza migraciones para controlar la evolución de la estructura de la base de datos.

Migraciones existentes:

```text
InitialCreate
AddUsuariosCategorias
SeedUsuariosCategorias
AddAgenteToTickets
AddHistorialTickets
AddComentariosTickets
AddNotificaciones
```

También se incluye:

```text
TicketDeskDbContextModelSnapshot.cs
```

Las migraciones permiten crear y actualizar la estructura de la base de datos de acuerdo con los modelos actuales.

---

# 🎫 Gestión de tickets

El módulo de tickets es el núcleo principal del sistema.

## Crear ticket

```http
POST /api/Tickets
```

Permite registrar un ticket con información como:

- Título.
- Descripción.
- Prioridad.
- Usuario.
- Categoría.

El estado inicial de un ticket nuevo es:

```text
Abierto
```

La creación del ticket también puede generar el registro correspondiente en el historial.

---

# 📋 Consulta de tickets

```http
GET /api/Tickets
```

El resultado depende del rol del usuario autenticado.

### Administrador

Puede consultar todos los tickets.

### Agente

Puede consultar los tickets que tiene asignados.

### Usuario

Puede consultar únicamente sus propios tickets.

---

# 🔎 Consulta de ticket

```http
GET /api/Tickets/{id}
```

El acceso también está limitado según el rol y la relación del usuario con el ticket.

---

# ✏️ Actualización de tickets

```http
PUT /api/Tickets/{id}
```

Permite modificar información del ticket de acuerdo con los permisos del usuario.

Entre los datos gestionados se encuentran:

- Título.
- Descripción.
- Estado.
- Prioridad.
- Usuario.
- Categoría.
- Agente.

Los cambios de estado son controlados mediante un servicio específico.

---

# 🔄 Estados de tickets

El sistema cuenta con un servicio dedicado:

```text
Services/TicketStatusService.cs
```

El flujo permitido actualmente es:

```text
Abierto
   ↓
En proceso
   ↓
Resuelto
   ↓
Cerrado
```

El sistema evita realizar cambios de estado inválidos.

Por ejemplo:

```text
Abierto → Resuelto
```

no se permite directamente.

También se validó que:

```text
Cerrado → En proceso
```

es rechazado por la API con respuesta:

```text
400 Bad Request
```

---

# 👨‍💻 Asignación de tickets

Endpoint:

```http
PUT /api/Tickets/{id}/asignar
```

La asignación permite que un administrador asigne un ticket a un agente.

Se valida que:

- El agente exista.
- El usuario tenga rol `Agente`.
- El agente esté activo.
- El ticket exista.
- No se realice una asignación duplicada.

Ejemplo:

```json
{
  "agenteId": 2
}
```

Cuando se realiza una asignación:

```text
Administrador
      │
      ▼
Asignación del ticket
      │
      ├──► Historial
      │
      └──► Notificación al agente
```

También se implementó la validación para evitar asignar nuevamente el mismo ticket al mismo agente.

---

# 📝 Historial de tickets

Endpoint:

```http
GET /api/HistorialTickets/{ticketId}
```

El historial permite registrar las acciones importantes realizadas sobre un ticket.

Actualmente contempla eventos como:

```text
Ticket creado
Ticket asignado
Cambio de agente
Cambio de estado
```

El historial almacena información como:

- Usuario que realizó la acción.
- Acción.
- Estado anterior.
- Estado nuevo.
- Comentario.
- Fecha.

Esto permite mantener una trazabilidad de las operaciones realizadas sobre cada ticket.

---

# 💬 Comentarios

Endpoints principales:

```http
POST /api/ComentariosTickets/{ticketId}
```

```http
GET /api/ComentariosTickets/{ticketId}
```

Los usuarios pueden agregar comentarios de acuerdo con sus permisos.

Las restricciones consideran:

```text
Usuario → sus propios tickets

Agente → tickets asignados

Administrador → acceso administrativo
```

Los comentarios almacenan información relacionada con:

- Usuario.
- Ticket.
- Contenido.
- Fecha.

---

# 🔔 Notificaciones

Endpoint:

```http
GET /api/Notificaciones
```

Permite consultar las notificaciones del usuario autenticado.

También existen:

```http
PUT /api/Notificaciones/{id}/leer
```

```http
PUT /api/Notificaciones/leer-todas
```

## Notificación por asignación

Cuando un administrador asigna un ticket a un agente, se genera una notificación para el agente.

Ejemplo:

```text
Se te ha asignado el ticket #X.
```

## Notificación por cambio de estado

Cuando cambia el estado de un ticket, se genera una notificación para el propietario del ticket.

Ejemplo:

```text
Tu ticket #X ha cambiado de estado.
```

Las notificaciones pueden:

- Consultarse.
- Marcarse individualmente como leídas.
- Marcarse todas como leídas.

---

# 📊 Dashboard

Endpoint:

```http
GET /api/Dashboard
```

El dashboard está protegido y destinado a usuarios con permisos administrativos o de agente.

La información gestionada incluye indicadores relacionados con:

- Total de tickets.
- Tickets por estado.
- Tickets por prioridad.
- Tickets por categoría.
- Usuarios activos.
- Usuarios inactivos.
- Tickets recientes.

El dashboard forma parte del backend y posteriormente será consumido por el frontend.

---

# 👤 Gestión de usuarios

Endpoint base:

```http
/api/Usuarios
```

El sistema cuenta con operaciones CRUD para la administración de usuarios.

Incluye:

- Crear usuario.
- Consultar usuarios.
- Consultar usuario por ID.
- Actualizar usuario.
- Desactivar usuario.

Las contraseñas no se almacenan directamente, sino mediante **hash BCrypt**.

La desactivación de usuarios permite conservar el registro en la base de datos.

---

# 🏷️ Gestión de categorías

Endpoint base:

```http
/api/Categorias
```

Incluye operaciones para:

- Crear categorías.
- Consultar categorías.
- Consultar categoría por ID.
- Actualizar categorías.
- Desactivar categorías.

Las categorías pueden mantenerse mediante desactivación lógica.

---

# 🧪 Pruebas realizadas

Las pruebas del backend se han realizado principalmente mediante **Swagger**.

## Autenticación

Se verificó:

- Login de Administrador.
- Login de Agente.
- Login de Usuario.
- Validación de credenciales.
- Protección de endpoints sin token.

---

## Autorización por roles

Se verificó el acceso al endpoint administrativo:

```http
GET /api/Usuarios
```

Resultados comprobados:

| Situación | Resultado |
|---|---:|
| Administrador con token | `200 OK` |
| Agente con token | `403 Forbidden` |
| Usuario con token | `403 Forbidden` |
| Sin token | `401 Unauthorized` |

Esto confirma que el endpoint cuenta con protección mediante autenticación y autorización por rol.

---

## Estados

También se probaron las siguientes transiciones:

```text
Abierto → En proceso       ✅
En proceso → Resuelto      ✅
Resuelto → Cerrado         ✅
Cerrado → En proceso       ❌ 400
```

La última transición fue rechazada correctamente por la validación de estados.

---

## Asignación

Se verificó:

- Asignación de ticket a agente.
- Registro de la asignación.
- Generación de notificación.
- Registro en historial.
- Prevención de asignaciones duplicadas.

---

## Notificaciones

Se verificó:

- Consulta de notificaciones.
- Notificación por asignación.
- Notificación por cambio de estado.
- Marcar notificación como leída.
- Marcar todas como leídas.

---

# 📁 Estructura del proyecto

```text
TicketDesk
│
├── TicketDesk.Api
│   │
│   ├── Controllers
│   │   ├── AuthController.cs
│   │   ├── CategoriasController.cs
│   │   ├── ComentariosTicketsController.cs
│   │   ├── DashboardController.cs
│   │   ├── HistorialTicketsController.cs
│   │   ├── NotificacionesController.cs
│   │   ├── TicketsController.cs
│   │   └── UsuariosController.cs
│   │
│   ├── Data
│   │   └── TicketDeskDbContext.cs
│   │
│   ├── DTOs
│   │   ├── AsignarTicketDto.cs
│   │   ├── CreateCategoriaDto.cs
│   │   ├── CreateComentarioDto.cs
│   │   ├── CreateTicketDto.cs
│   │   ├── CreateUsuarioDto.cs
│   │   ├── LoginDto.cs
│   │   ├── TicketResponseDto.cs
│   │   ├── UpdateCategoriaDto.cs
│   │   ├── UpdateTicketDto.cs
│   │   └── UpdateUsuarioDto.cs
│   │
│   ├── Migrations
│   │   ├── InitialCreate
│   │   ├── AddUsuariosCategorias
│   │   ├── SeedUsuariosCategorias
│   │   ├── AddAgenteToTickets
│   │   ├── AddHistorialTickets
│   │   ├── AddComentariosTickets
│   │   ├── AddNotificaciones
│   │   └── TicketDeskDbContextModelSnapshot.cs
│   │
│   ├── Models
│   │   ├── Categoria.cs
│   │   ├── ComentarioTicket.cs
│   │   ├── HistorialTicket.cs
│   │   ├── Notificacion.cs
│   │   ├── Ticket.cs
│   │   └── Usuario.cs
│   │
│   ├── Properties
│   │   └── launchSettings.json
│   │
│   ├── Services
│   │   ├── JwtService.cs
│   │   ├── PasswordService.cs
│   │   └── TicketStatusService.cs
│   │
│   ├── Program.cs
│   ├── TicketDesk.Api.csproj
│   ├── TicketDesk.Api.http
│   ├── appsettings.json
│   └── README.md
│
├── .gitignore
└── TicketDesk.slnx
```

---

# 📡 Endpoints principales

## Autenticación

```http
POST /api/Auth/login
```

## Usuarios

```http
GET    /api/Usuarios
GET    /api/Usuarios/{id}
POST   /api/Usuarios
PUT    /api/Usuarios/{id}
DELETE /api/Usuarios/{id}
```

## Categorías

```http
GET    /api/Categorias
GET    /api/Categorias/{id}
POST   /api/Categorias
PUT    /api/Categorias/{id}
DELETE /api/Categorias/{id}
```

## Tickets

```http
GET    /api/Tickets
GET    /api/Tickets/{id}
POST   /api/Tickets
PUT    /api/Tickets/{id}
DELETE /api/Tickets/{id}
PUT    /api/Tickets/{id}/asignar
```

## Historial

```http
GET /api/HistorialTickets/{ticketId}
```

## Comentarios

```http
GET  /api/ComentariosTickets/{ticketId}
POST /api/ComentariosTickets/{ticketId}
```

## Notificaciones

```http
GET /api/Notificaciones
PUT /api/Notificaciones/{id}/leer
PUT /api/Notificaciones/leer-todas
```

## Dashboard

```http
GET /api/Dashboard
```

---

# 📖 Swagger / OpenAPI

El proyecto utiliza Swagger/OpenAPI para probar y documentar la API durante el desarrollo.

La API se ejecuta actualmente en:

```text
https://localhost:7155
```

y:

```text
http://localhost:5203
```

Los endpoints protegidos requieren autenticación mediante:

```http
Authorization: Bearer <JWT>
```

El archivo de pruebas HTTP utiliza una variable para el token:

```http
Authorization: Bearer {{token}}
```

De esta manera no es necesario almacenar un JWT real dentro de cada petición.

---

# ⚙️ Configuración

La configuración sensible del entorno de desarrollo no forma parte del repositorio.

La aplicación utiliza la configuración de ASP.NET Core y puede trabajar con:

- User Secrets.
- Variables de entorno.
- Configuración local de desarrollo.

La cadena de conexión y la clave JWT deben mantenerse fuera del repositorio público.

El archivo `appsettings.json` incluido en el proyecto contiene únicamente configuración general de la aplicación.

---

# ▶️ Ejecución del proyecto

## Requisitos

Antes de ejecutar el proyecto se necesita:

- .NET 10 SDK.
- SQL Server.
- Visual Studio Community o un entorno compatible.
- Entity Framework Core.
- Base de datos configurada.

## Clonar el repositorio

```bash
git clone <URL_DEL_REPOSITORIO>
```

Entrar al proyecto:

```bash
cd TicketDesk
```

## Restaurar dependencias

```bash
dotnet restore
```

## Aplicar migraciones

```bash
dotnet ef database update --project TicketDesk.Api
```

## Ejecutar la API

```bash
dotnet run --project TicketDesk.Api
```

La API quedará disponible mediante las URLs configuradas por el proyecto.

---

# 🧪 Pruebas pendientes

Aunque ya se han realizado pruebas funcionales del backend, todavía quedan pruebas más completas por implementar.

Pendientes:

- [ ] Pruebas unitarias automatizadas.
- [ ] Pruebas de integración.
- [ ] Pruebas completas de todos los endpoints.
- [ ] Crear colección de Postman.
- [ ] Documentar casos de prueba.
- [ ] Probar errores y casos límite.
- [ ] Revisar completamente permisos de cada endpoint.
- [ ] Automatizar pruebas de autenticación y autorización.

---

# 🔒 Mejoras de seguridad pendientes

Antes de considerar el proyecto listo para producción:

- [ ] Revisar configuración de producción.
- [ ] Mantener la clave JWT fuera del repositorio.
- [ ] Utilizar variables de entorno o User Secrets.
- [ ] Revisar CORS.
- [ ] Revisar políticas de autorización.
- [ ] Revisar validaciones de entrada.
- [ ] Revisar manejo global de excepciones.
- [ ] Revisar información expuesta en respuestas de la API.
- [ ] Rotar cualquier secreto utilizado durante desarrollo antes de producción.

---

# 🎨 Frontend pendiente

El frontend todavía no ha sido desarrollado.

La siguiente etapa principal será crear una aplicación con:

```text
React
   │
   ▼
TicketDesk API
   │
   ▼
SQL Server
```

## Módulos previstos

### Login

- [ ] Inicio de sesión.
- [ ] Almacenamiento seguro de sesión/token.
- [ ] Redirección según rol.
- [ ] Cierre de sesión.

### Dashboard

- [ ] Indicadores.
- [ ] Tickets por estado.
- [ ] Tickets por prioridad.
- [ ] Tickets recientes.
- [ ] Información según rol.

### Tickets

- [ ] Lista de tickets.
- [ ] Crear ticket.
- [ ] Ver detalle.
- [ ] Editar.
- [ ] Cambiar estado.
- [ ] Asignar agente.
- [ ] Filtrar tickets.

### Detalle del ticket

- [ ] Información general.
- [ ] Historial.
- [ ] Comentarios.
- [ ] Estado.
- [ ] Prioridad.
- [ ] Agente asignado.

### Usuarios

- [ ] Lista.
- [ ] Crear.
- [ ] Editar.
- [ ] Activar/desactivar.

### Categorías

- [ ] Lista.
- [ ] Crear.
- [ ] Editar.
- [ ] Activar/desactivar.

### Notificaciones

- [ ] Mostrar notificaciones.
- [ ] Indicador de no leídas.
- [ ] Marcar como leída.
- [ ] Marcar todas como leídas.

---

# 🐳 Docker — pendiente

Como mejora para el proyecto se contempla agregar:

- [ ] Dockerfile.
- [ ] Docker Compose.
- [ ] Contenedor de la API.
- [ ] Configuración de base de datos.
- [ ] Variables de entorno.
- [ ] Ejecución completa mediante Docker.

---

# 📚 Mejoras futuras

Después de terminar el frontend se pueden agregar:

- [ ] Paginación.
- [ ] Búsqueda de tickets.
- [ ] Filtros avanzados.
- [ ] Ordenamiento.
- [ ] Reportes.
- [ ] Estadísticas adicionales.
- [ ] Mejor manejo global de errores.
- [ ] Logging estructurado.
- [ ] Pruebas automatizadas.
- [ ] Docker.
- [ ] CI/CD.
- [ ] Despliegue en la nube.

---

# 🗺️ Roadmap

El desarrollo continuará aproximadamente en este orden:

```text
✅ 1. Arquitectura y configuración del backend
        ↓
✅ 2. Modelos y base de datos
        ↓
✅ 3. Autenticación JWT
        ↓
✅ 4. Roles y autorización
        ↓
✅ 5. CRUD de usuarios
        ↓
✅ 6. CRUD de categorías
        ↓
✅ 7. CRUD de tickets
        ↓
✅ 8. Asignación de tickets
        ↓
✅ 9. Estados y validaciones
        ↓
✅ 10. Historial
        ↓
✅ 11. Comentarios
        ↓
✅ 12. Notificaciones
        ↓
✅ 13. Dashboard
        ↓
🔄 14. Pruebas completas del backend
        ↓
⏳ 15. Mejoras de seguridad
        ↓
⏳ 16. Frontend React
        ↓
⏳ 17. Integración Frontend + API
        ↓
⏳ 18. Pruebas automatizadas
        ↓
⏳ 19. Docker
        ↓
⏳ 20. Despliegue
```

---

# 📊 Estado actual

## Backend

| Módulo | Estado |
|---|---|
| ASP.NET Core Web API | ✅ |
| .NET 10 | ✅ |
| SQL Server | ✅ |
| Entity Framework Core | ✅ |
| Migraciones | ✅ |
| Autenticación JWT | ✅ |
| BCrypt | ✅ |
| Roles | ✅ |
| Usuarios | ✅ |
| Categorías | ✅ |
| Tickets | ✅ |
| Asignación | ✅ |
| Estados | ✅ |
| Historial | ✅ |
| Comentarios | ✅ |
| Notificaciones | ✅ |
| Dashboard | ✅ |
| Swagger/OpenAPI | ✅ |
| Pruebas funcionales iniciales | ✅ |
| Pruebas automatizadas | ⏳ |

## Frontend

| Módulo | Estado |
|---|---|
| React | ⏳ |
| Login | ⏳ |
| Dashboard | ⏳ |
| Tickets | ⏳ |
| Usuarios | ⏳ |
| Categorías | ⏳ |
| Comentarios | ⏳ |
| Historial | ⏳ |
| Notificaciones | ⏳ |

## Infraestructura

| Funcionalidad | Estado |
|---|---|
| Git | ✅ |
| GitHub | ⏳ |
| Docker | ⏳ |
| CI/CD | ⏳ |
| Despliegue | ⏳ |

---

# 🎯 Objetivo del proyecto

TicketDesk 2.0 tiene como objetivo demostrar conocimientos prácticos de desarrollo **Backend / Full Stack Junior con .NET y C#**.

El proyecto busca demostrar experiencia práctica en:

- C#.
- .NET 10.
- ASP.NET Core Web API.
- Desarrollo de APIs REST.
- Entity Framework Core.
- SQL Server.
- Diseño de modelos y relaciones.
- CRUD.
- DTOs.
- Autenticación JWT.
- BCrypt.
- Autorización basada en roles.
- Validación de permisos.
- Gestión de tickets.
- Asignación de tickets.
- Control de estados.
- Historial de operaciones.
- Comentarios.
- Notificaciones.
- Dashboard.
- Swagger/OpenAPI.
- Git y GitHub.

El proyecto se encuentra actualmente en **fase de desarrollo**, con el backend funcional y el frontend pendiente de implementación.

---

# 👨‍💻 Autor

** ING: Luis Mario Pineda García **

Proyecto desarrollado como parte del portafolio profesional para demostrar habilidades en desarrollo backend y desarrollo web con .NET.

GitHub:

`https://github.com/Luis22garcia`