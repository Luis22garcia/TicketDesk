# TicketDesk 2.0 — Backend API

API REST para la gestión de tickets de soporte técnico, desarrollada con **C#, .NET 10, ASP.NET Core Web API, Entity Framework Core y SQL Server**.

El backend de TicketDesk 2.0 permite administrar usuarios, categorías, tickets, asignaciones, estados, comentarios, historial y notificaciones. Implementa autenticación mediante JWT y autorización basada en roles para controlar el acceso a los recursos.

El proyecto también incorpora funcionalidades de recuperación de contraseña por correo electrónico.

## 🚀 Tecnologías utilizadas

### Desarrollo

- C#
- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- JWT (JSON Web Tokens)
- BCrypt
- Swagger / OpenAPI

### Herramientas

- Visual Studio Community
- SQL Server Management Studio 21
- Visual Studio HTTP files (`.http`)
- GitHub

## 📌 Funcionalidades principales

- Autenticación mediante JWT.
- Protección de contraseñas mediante BCrypt.
- Autorización por roles.
- Gestión de usuarios.
- Gestión de categorías.
- Creación y consulta de tickets.
- Actualización de tickets.
- Asignación de tickets a agentes.
- Validación de transiciones de estado.
- Historial de operaciones.
- Comentarios.
- Notificaciones.
- Dashboard.
- Recuperación de contraseña mediante correo electrónico.
- Tokens de recuperación con expiración y controles de reutilización.
- Restricciones de acceso según el usuario autenticado.
- Persistencia mediante SQL Server.
- Migraciones de Entity Framework Core.

## 🏗️ Arquitectura general

El backend organiza las responsabilidades principales en controladores, servicios, modelos, DTO y acceso a datos.

```text
TicketDesk.Api/
├── Controllers/   Endpoints y lógica de las solicitudes HTTP
├── Data/          Contexto de Entity Framework Core
├── DTOs/          Objetos de transferencia de datos
├── Migrations/    Migraciones de la base de datos
├── Models/        Entidades del sistema
├── Services/      Servicios de autenticación y lógica de negocio
├── Program.cs     Configuración de la aplicación
└── TicketDesk.Api.csproj
```

La aplicación utiliza Entity Framework Core para interactuar con SQL Server y separar el acceso a datos de la lógica de los controladores.

## 👥 Roles y autorización

El sistema contempla tres roles principales.

### Administrador

- Gestionar usuarios y categorías.
- Consultar tickets.
- Asignar tickets a agentes.
- Actualizar y eliminar tickets conforme a sus permisos.
- Consultar el dashboard.
- Consultar historial y notificaciones.
- Administrar contraseñas de usuarios desde el módulo correspondiente.

### Agente

- Consultar tickets asignados.
- Actualizar tickets que tiene autorización para gestionar.
- Cambiar estados conforme a las reglas del sistema.
- Agregar comentarios.
- Consultar historial y notificaciones.

### Usuario

- Crear tickets.
- Consultar sus propios tickets.
- Agregar comentarios conforme a sus permisos.
- Consultar el historial de sus tickets.
- Consultar sus notificaciones.

Los permisos efectivos dependen de las reglas implementadas en cada controlador y servicio.

## 🔐 Autenticación y seguridad

La API utiliza JWT para autenticar solicitudes y autorización basada en roles para proteger endpoints.

### Funcionalidades implementadas

- Inicio de sesión mediante correo electrónico y contraseña.
- Validación de credenciales.
- Hash de contraseñas mediante BCrypt.
- Generación y validación de JWT.
- Expiración de tokens.
- Configuración de issuer y audience.
- Protección de endpoints con `[Authorize]`.
- Validación de usuarios activos.
- Restricciones de acceso según el usuario autenticado.
- Recuperación de contraseña por correo electrónico.
- Servicio de envío de correo mediante `EmailService.cs`.
- Modelo `PasswordResetToken` para administrar tokens de recuperación.

### Flujo de autenticación

```text
Cliente
   |
   v
POST /api/Auth/login
   |
   v
Validación de credenciales
   |
   v
Verificación mediante BCrypt
   |
   v
Generación del JWT
   |
   v
Authorization: Bearer <token>
   |
   v
Endpoint protegido
```

Las contraseñas deben almacenarse mediante hash y nunca en texto plano.

**Importante:** la cadena de conexión, la clave JWT y las credenciales SMTP deben configurarse fuera del repositorio público mediante User Secrets, variables de entorno u otro mecanismo seguro.

## 📧 Recuperación de contraseña

El backend incluye funcionalidad para recuperar contraseñas mediante correo electrónico.

Los componentes relacionados son:

- `PasswordResetToken`.
- `EmailService.cs`.
- Configuración del servicio de correo en `Program.cs`.
- Migración de Entity Framework Core para los tokens de recuperación.

El flujo implementado utiliza tokens de recuperación con expiración y controles para evitar su reutilización.

Los nombres exactos de los endpoints de recuperación deben consultarse en `AuthController.cs` antes de documentarlos en esta sección.

La configuración SMTP debe establecerse en el entorno local. No se deben publicar contraseñas de correo, tokens ni credenciales de servicios externos.

## 🗄️ Base de datos

El proyecto utiliza SQL Server como sistema gestor de base de datos y Entity Framework Core como ORM.

### Entidades principales

- `Usuario`
- `Categoria`
- `Ticket`
- `HistorialTicket`
- `ComentarioTicket`
- `Notificacion`
- `PasswordResetToken`

### Relaciones generales

```text
Usuario   1 ─── N Ticket
Categoria 1 ─── N Ticket

Ticket    1 ─── N ComentarioTicket
Usuario   1 ─── N ComentarioTicket

Ticket    1 ─── N HistorialTicket
Usuario   1 ─── N HistorialTicket

Usuario   1 ─── N Notificacion
Usuario   1 ─── N PasswordResetToken
```

Las relaciones y restricciones definitivas están determinadas por los modelos y la configuración de `TicketDeskDbContext`.

## 📦 Migraciones de Entity Framework Core

Las migraciones permiten mantener sincronizado el esquema de la base de datos con los modelos de la aplicación.

Durante el desarrollo se han incorporado migraciones relacionadas con:

- Creación inicial de la base de datos.
- Usuarios y categorías.
- Datos iniciales.
- Asignación de agentes.
- Historial de tickets.
- Comentarios.
- Notificaciones.
- Recuperación de contraseña.

También se utiliza `TicketDeskDbContextModelSnapshot.cs`.

Para consultar los nombres exactos de las migraciones disponibles:

```bash
dotnet ef migrations list --project TicketDesk.Api
```

Para aplicar las migraciones:

```bash
dotnet ef database update --project TicketDesk.Api
```

Estos comandos requieren una cadena de conexión válida y una configuración correcta de Entity Framework Core.

## 🎫 Gestión de tickets

### Crear un ticket

```http
POST /api/Tickets
```

Permite registrar un ticket con información como título, descripción, prioridad y categoría. El propietario se determina o valida según las reglas del backend.

Los tickets nuevos comienzan en estado `Abierto`.

La creación puede generar registros de historial y notificaciones según la lógica implementada.

### Consultar tickets

```http
GET /api/Tickets
```

La consulta aplica restricciones según el rol:

- **Administrador:** consulta tickets conforme a sus permisos administrativos.
- **Agente:** consulta los tickets que tiene asignados.
- **Usuario:** consulta sus propios tickets.

### Consultar un ticket

```http
GET /api/Tickets/{id}
```

El acceso está restringido según el rol y la relación del usuario autenticado con el ticket.

### Actualizar un ticket

```http
PUT /api/Tickets/{id}
```

Permite modificar la información autorizada del ticket. Las operaciones disponibles dependen del rol y de las validaciones aplicadas por el backend.

### Eliminar un ticket

```http
DELETE /api/Tickets/{id}
```

Operación destinada al rol administrativo de acuerdo con las restricciones del controlador.

## 🔄 Estados de tickets

La validación de los estados se implementa mediante:

`Services/TicketStatusService.cs`

El flujo de estados contempla:

```text
Abierto
   |
   v
En proceso
   |
   v
Resuelto
   |
   v
Cerrado
```

El servicio valida las transiciones permitidas y rechaza cambios de estado inválidos.

Durante las pruebas funcionales se verificaron las siguientes transiciones:

| Transición            | Resultado                       |
| --------------------- | ------------------------------- |
| Abierto → En proceso  | Aceptada                        |
| En proceso → Resuelto | Aceptada                        |
| Resuelto → Cerrado    | Aceptada                        |
| Cerrado → En proceso  | Rechazada con `400 Bad Request` |

## 👨‍💻 Asignación de tickets

Endpoint:

```http
PUT /api/Tickets/{id}/asignar
```

Permite asignar tickets a agentes mediante una operación administrativa.

El backend valida aspectos como:

- Existencia del agente.
- Rol de Agente.
- Estado activo del agente.
- Existencia del ticket.
- Prevención de asignaciones duplicadas al mismo agente.

Ejemplo de solicitud:

```json
{
  "agenteId": 2
}
```

La asignación puede generar un registro de historial y una notificación para el agente asignado.

## 📝 Historial de tickets

Endpoint:

```http
GET /api/HistorialTickets/{ticketId}
```

Permite consultar los registros de actividad relacionados con un ticket.

Entre los eventos contemplados están:

- Creación del ticket.
- Asignación.
- Cambio de agente.
- Cambio de estado.

Los registros pueden contener información sobre el usuario que realizó la acción, la operación, los estados anterior y nuevo, un comentario y la fecha.

## 💬 Comentarios

Endpoints:

```http
GET  /api/ComentariosTickets/{ticketId}
POST /api/ComentariosTickets/{ticketId}
```

Permiten consultar y agregar comentarios respetando las restricciones de acceso.

Las autorizaciones consideran la relación entre el usuario y el ticket:

- Usuario: sus propios tickets.
- Agente: tickets asignados.
- Administrador: acceso administrativo autorizado.

## 🔔 Notificaciones

Endpoints:

```http
GET /api/Notificaciones
PUT /api/Notificaciones/{id}/leer
PUT /api/Notificaciones/leer-todas
```

Permiten consultar las notificaciones del usuario autenticado, marcar una notificación individual como leída y marcar todas como leídas.

Se contemplan eventos como:

- Asignación de tickets.
- Cambios de estado.

## 📊 Dashboard

Endpoint:

```http
GET /api/Dashboard
```

El dashboard está protegido para los roles Administrador y Agente.

El controlador proporciona información resumida relacionada con la gestión de tickets. Los indicadores concretos dependen de la implementación actual de `DashboardController.cs`.

## 👤 Gestión de usuarios

Endpoint base:

```text
/api/Usuarios
```

El módulo incluye operaciones para:

- Consultar usuarios.
- Consultar un usuario por identificador.
- Crear usuarios.
- Actualizar usuarios.
- Desactivar usuarios.
- Administrar contraseñas conforme a los permisos implementados.

Las contraseñas deben procesarse mediante BCrypt antes de almacenarse.

## 🏷️ Gestión de categorías

Endpoint base:

```text
/api/Categorias
```

El módulo contempla:

- Consultar categorías.
- Consultar una categoría por identificador.
- Crear categorías.
- Actualizar categorías.
- Desactivar categorías según la implementación.

Las operaciones administrativas están protegidas por autorización de rol.

## 📡 Endpoints principales

| Módulo         | Método | Ruta                                 |
| -------------- | ------ | ------------------------------------ |
| Autenticación  | POST   | `/api/Auth/login`                    |
| Usuarios       | GET    | `/api/Usuarios`                      |
| Usuarios       | GET    | `/api/Usuarios/{id}`                 |
| Usuarios       | POST   | `/api/Usuarios`                      |
| Usuarios       | PUT    | `/api/Usuarios/{id}`                 |
| Usuarios       | DELETE | `/api/Usuarios/{id}`                 |
| Categorías     | GET    | `/api/Categorias`                    |
| Categorías     | GET    | `/api/Categorias/{id}`               |
| Categorías     | POST   | `/api/Categorias`                    |
| Categorías     | PUT    | `/api/Categorias/{id}`               |
| Categorías     | DELETE | `/api/Categorias/{id}`               |
| Tickets        | GET    | `/api/Tickets`                       |
| Tickets        | GET    | `/api/Tickets/{id}`                  |
| Tickets        | POST   | `/api/Tickets`                       |
| Tickets        | PUT    | `/api/Tickets/{id}`                  |
| Tickets        | DELETE | `/api/Tickets/{id}`                  |
| Asignación     | PUT    | `/api/Tickets/{id}/asignar`          |
| Historial      | GET    | `/api/HistorialTickets/{ticketId}`   |
| Comentarios    | GET    | `/api/ComentariosTickets/{ticketId}` |
| Comentarios    | POST   | `/api/ComentariosTickets/{ticketId}` |
| Notificaciones | GET    | `/api/Notificaciones`                |
| Notificaciones | PUT    | `/api/Notificaciones/{id}/leer`      |
| Notificaciones | PUT    | `/api/Notificaciones/leer-todas`     |
| Dashboard      | GET    | `/api/Dashboard`                     |

Esta tabla contiene las rutas principales documentadas. Las rutas de recuperación de contraseña deben añadirse una vez confirmados sus nombres exactos en el controlador.

## 🧪 Pruebas de la API

Las solicitudes HTTP de prueba se encuentran en:

`TicketDesk.Api/TicketDesk.Api.http`

Las pruebas se ejecutan desde **Visual Studio Community** utilizando el archivo `.http`.

### Ejemplo de inicio de sesión

```http
POST {{TicketDesk.Api_HostAddress}}/api/auth/login
Content-Type: application/json

{
 "email": "admin@ticketdesk.com",
 "password": "Admin123"
}
```

Utiliza credenciales locales de prueba. No guardes contraseñas reales en el repositorio.

### Autorización mediante JWT

Las solicitudes protegidas utilizan:

```http
Authorization: Bearer {{token}}
```

La variable `token` debe contener un JWT válido obtenido mediante el inicio de sesión.

### Pruebas funcionales realizadas

Se han probado funcionalidades relacionadas con:

- Inicio de sesión.
- Validación de credenciales.
- Autorización por roles.
- Acceso a endpoints protegidos.
- Consulta de tickets según el usuario autenticado.
- Transiciones de estado.
- Asignación de tickets.
- Prevención de asignaciones duplicadas.
- Historial.
- Comentarios.
- Notificaciones.
- Dashboard.
- Recuperación de contraseña.

En las pruebas de autorización del endpoint `GET /api/Usuarios` se comprobaron estos resultados:

| Situación               | Resultado          |
| ----------------------- | ------------------ |
| Administrador con token | `200 OK`           |
| Agente con token        | `403 Forbidden`    |
| Usuario con token       | `403 Forbidden`    |
| Sin token               | `401 Unauthorized` |

El backend compiló correctamente sin errores en la revisión realizada. Aún deben completarse las verificaciones integrales y las pruebas automatizadas.

## ⚙️ Configuración del entorno

### Requisitos

- .NET 10 SDK.
- SQL Server.
- Visual Studio Community o entorno compatible.
- Entity Framework Core.
- Base de datos configurada.
- Configuración JWT.
- Configuración SMTP para probar el envío de correos.

### Seguridad de la configuración

Utiliza User Secrets o variables de entorno para almacenar:

- Cadena de conexión.
- Clave JWT.
- Credenciales SMTP.
- Otros valores sensibles.

Antes de publicar cambios en GitHub, verifica que `appsettings.json` no contenga contraseñas, claves privadas ni credenciales reales.

## ▶️ Instalación y ejecución

### 1. Clonar el repositorio

```bash
git clone https://github.com/Luis22garcia/TicketDesk.git
cd TicketDesk
```

### 2. Restaurar dependencias

```bash
dotnet restore
```

### 3. Configurar la base de datos

Configura la cadena de conexión a SQL Server y los valores requeridos por JWT y SMTP mediante el entorno local.

### 4. Aplicar migraciones

```bash
dotnet ef database update --project TicketDesk.Api
```

### 5. Ejecutar la API

```bash
dotnet run --project TicketDesk.Api
```

Durante el desarrollo, las direcciones configuradas son:

- `https://localhost:7155`
- `http://localhost:5203`

Las direcciones efectivas dependen de la configuración de ejecución del proyecto.

### 6. Probar los endpoints

Abre `TicketDesk.Api.http` en Visual Studio Community. Ejecuta primero el inicio de sesión y después las solicitudes que requieren autenticación.

## 🗺️ Roadmap del backend

### Implementado

- [x] Arquitectura inicial.
- [x] Modelos y persistencia con SQL Server.
- [x] Migraciones de Entity Framework Core.
- [x] Autenticación JWT.
- [x] Protección de contraseñas con BCrypt.
- [x] Roles y autorización.
- [x] CRUD de usuarios.
- [x] CRUD de categorías.
- [x] Gestión de tickets.
- [x] Asignación de tickets.
- [x] Validación de estados.
- [x] Historial.
- [x] Comentarios.
- [x] Notificaciones.
- [x] Dashboard.
- [x] Recuperación de contraseña por correo.
- [x] Solicitudes HTTP para pruebas funcionales.
- [x] Verificar integralmente todos los endpoints y permisos.
- [x] Mejorar el logging y el manejo global de excepciones.

### Pendiente o por completar

- [ ] Crear pruebas unitarias automatizadas.
- [ ] Crear pruebas de integración.
- [ ] Documentar los casos de prueba.
- [ ] Completar la revisión de seguridad.
- [ ] Revisar la configuración de producción.
- [ ] Incorporar Docker y Docker Compose.
- [ ] Configurar CI/CD.
- [ ] Desplegar la API en un entorno de producción.

## 🎯 Objetivo del proyecto

TicketDesk 2.0 busca demostrar conocimientos prácticos de desarrollo backend con C# y .NET, incluyendo:

- ASP.NET Core Web API.
- Diseño y desarrollo de APIs REST.
- Entity Framework Core.
- SQL Server.
- Modelos, relaciones y DTO.
- Operaciones CRUD.
- Autenticación JWT.
- Hash de contraseñas con BCrypt.
- Autorización basada en roles.
- Validación de permisos.
- Gestión y asignación de tickets.
- Control de estados.
- Historial de operaciones.
- Comentarios y notificaciones.
- Dashboard.
- Recuperación de contraseña por correo.
- GitHub.

## 👨‍💻 Autor

**Luis Mario Pineda García**

Proyecto de portafolio profesional orientado al desarrollo backend con C# y .NET.

**GitHub:** [https://github.com/Luis22garcia](https://github.com/Luis22garcia)
