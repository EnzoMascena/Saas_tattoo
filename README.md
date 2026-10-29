# Tato

Plataforma SaaS multi-tenant para la gestión de estudios de tatuajes.

La primera entrega es el **blog y portfolio público**: una página parecida a un perfil de Instagram para cada estudio y para cada artista. Sobre la misma base llega después el **MVP de gestión**: agenda, señas, clientes, tatuajes y sesiones.

> **Estado:** en desarrollo. Versión 1 (blog) online antes del **17 de noviembre de 2026** (D-46).
> Listo: login, estudios, usuarios, aislamiento por estudio y el backend del panel: publicaciones con fotos, tipo Trabajo o Blog, permisos por página y «Mi página». Sigue: los endpoints públicos, el wireframe y el front del panel.

---

## Qué es

Cada estudio que usa la plataforma es un *tenant* aislado: sus usuarios, contenido y datos no son visibles para ningún otro estudio. El primer estudio en usarla es el estudio piloto, que funciona como tester del producto.

### Versión 1 · Blog y portfolio

- **Páginas públicas** del estudio y de cada artista, independientes y con la misma estructura. Cada una tiene su dirección (por ejemplo, `/inkmortal`) y un encabezado como el de Instagram: foto o logo, nombre, bio y links a Instagram y WhatsApp.
- **Tres pestañas** en cada página:
  - **Trabajos:** una grilla de fotos.
  - **Blog:** publicaciones con más texto.
  - **Sobre mí / Sobre nosotros:** la del estudio muestra a sus artistas, y la de cada artista enlaza a su estudio.
- **Publicaciones** de tipo Trabajo o Blog, con hasta 10 fotos y 2200 caracteres de texto, como en Instagram.
- **Panel privado** con login, diseñado primero para el celular: publicaciones con fotos y los datos de cada página. Cada artista gestiona su página, y los Dueños, la del estudio.
- **Fotos optimizadas** al subirlas (tamaño de galería, de detalle y miniatura). El sitio nunca sirve los originales.
- **Vistas previas y SEO**: cada página pública sale con título, descripción y foto (Open Graph), más un sitemap.
- **Base del sistema**: login, estudios, usuarios y aislamiento por estudio, que después reutiliza el MVP.

El alcance completo, con las pantallas y las reglas, está en el [doc 07](docs/07_Blog_y_Portfolio_Publico.md).

### Después

- Del blog: diseños disponibles, fechas de viaje, videos, filtros de la galería y dominio propio por página.
- MVP de gestión: Agenda y Turnos con señas, Clientes, Tatuajes, Sesiones con walk-ins y notificaciones in-app.
- «Publicar en blog» desde un tatuaje finalizado, pedido de turno desde el sitio y alta de estudios por autoservicio.
- Integraciones con Mercado Pago y WhatsApp.

---

## Stack

| Capa | Tecnología |
| --- | --- |
| Backend | ASP.NET Core Web API con controladores · .NET 10 LTS · OpenAPI nativo |
| Datos | SQL Server 2025 · EF Core 10, code-first con migraciones |
| Autenticación | ASP.NET Core Identity con cookie HttpOnly · token antiforgery en los pedidos que modifican datos |
| Validación | El dominio valida los datos y las reglas de negocio; la API traduce cada error a ProblemDetails. FluentValidation se suma si la validación de entrada crece |
| Fotos | NetVips (libvips): WebP sin metadatos en tres tamaños. `NetVips.Native` trae libvips, no hay que instalar nada aparte |
| Frontend | React 19.2 + TypeScript · Vite 8 · Node 24 LTS · pnpm |
| Interfaz | Tailwind CSS + shadcn/ui |
| Front: ruteo, datos y formularios | React Router · TanStack Query · React Hook Form + Zod |
| Cliente de la API | Generado desde OpenAPI con Orval |
| Archivos | Puerto de almacenamiento: disco local en desarrollo; disco del servidor o almacenamiento de objetos en producción |
| Tests | xUnit + Testcontainers · Vitest + Testing Library · tests de arquitectura |

No se usan MediatR, AutoMapper ni Fluent Assertions: cambiaron a licencias comerciales y no hacen falta. Para mapear, a mano o con Mapperly.

---

## Arquitectura

**Clean Architecture en un monolito modular** (D-37, D-38): un solo despliegue, dividido en módulos de negocio con fronteras claras. El dominio está en el centro y las dependencias apuntan siempre hacia adentro.

```mermaid
flowchart LR
    Web["Web · React + TS"] -- "HTTPS + cookie" --> Api
    Api --> Application
    Api -. "solo para componer" .-> Infrastructure
    Infrastructure -- "implementa los puertos" --> Application
    Application --> Domain
    Infrastructure --> Domain
```

| Proyecto | Responsabilidad | Depende de |
| --- | --- | --- |
| `Tato.Domain` | Entidades, objetos de valor, estados, eventos de dominio y reglas RN. Sin paquetes externos. | Nada |
| `Tato.Application` | Casos de uso, puertos (interfaces), validación de entrada, autorización por recurso y DTOs. | Domain |
| `Tato.Infrastructure` | EF Core, Identity, almacenamiento de archivos, adaptadores externos y tareas automáticas. | Application, Domain |
| `Tato.Api` | Controladores, autenticación, middleware, OpenAPI y composición de dependencias. | Application; Infrastructure solo para componer |

### Reglas que no se rompen

- **El dominio no conoce nada externo**: ni EF Core, ni ASP.NET, ni proveedores. Habla con puertos; la infraestructura los implementa con adaptadores.
- **Un módulo no toca las tablas ni las clases internas de otro**: se comunica con casos de uso o eventos. Los tests de arquitectura lo verifican y la CI falla si alguien lo rompe.
- **Multi-tenancy** (D-42): una sola base con `EstudioId` en todas las tablas del estudio. El estudio sale de la sesión autenticada, nunca de un dato que mande el navegador. Filtros de consulta con nombre de EF Core, más un control en `SaveChanges` que impide guardar datos de otro estudio. Row-Level Security de SQL Server, como segunda línea de defensa, está pendiente.
- **Fechas en UTC** (D-43): se muestran en la zona horaria del estudio (`America/Argentina/Buenos_Aires` por defecto), en formato dd/mm/aaaa y 24 h. El reloj se obtiene siempre de `TimeProvider`, nunca de `DateTime.Now`.
- **Concurrencia optimista**: las entidades que editan varios usuarios llevan `rowversion`, a nivel de agregado. El panel recibe la `version` al abrir, la devuelve al guardar y, si alguien guardó antes, recibe un 409 en lugar de pisar su cambio.
- **Archivos y base de acuerdo**: al subir, primero los archivos y después la base; al quitar, al revés. Si algo falla en el medio, no quedan filas que apunten a archivos inexistentes.
- **Sin borrados operativos**: el contenido cambia de estado (por ejemplo, Publicada → Oculta) y queda en el historial.

### Módulos

| Módulo | Contenido | Entrega |
| --- | --- | --- |
| Estudios | Estudios (tenants), usuarios, roles, perfiles y configuración del estudio | v1 |
| Portfolio | Publicaciones (trabajos y blog), fotos y páginas públicas | v1 |
| Clientes · Agenda · Tatuajes · Notificaciones | Gestión operativa del estudio | MVP |
| Pagos | Integración con Mercado Pago | Post-MVP |

Cada módulo es una carpeta dentro de cada proyecto (`Tato.Domain/Portfolio`, `Tato.Application/Portfolio`, etc.).

### Sitio público y vistas previas

WhatsApp, Instagram y Facebook no ejecutan JavaScript para armar la vista previa de un link. Por eso la API entrega el HTML de cada página pública con su título, descripción y foto ya cargados (Open Graph) y genera el sitemap. Los endpoints públicos son de solo lectura y con caché; los del panel usan la cookie de sesión.

---

## API del panel

Todo exige sesión, y todo es del estudio de la sesión. Los pedidos que modifican datos (`POST`, `PUT`, `PATCH`, `DELETE`) exigen además el token antiforgery en el encabezado `X-XSRF-TOKEN`.

### Sesión

| Método | Ruta | Qué hace |
| --- | --- | --- |
| `GET` | `/api/auth/antiforgery` | Entrega el token antiforgery. Se pide al cargar el panel y otra vez después de iniciar o cerrar sesión |
| `POST` | `/api/auth/login` | Inicia sesión (`204`). Límite de 5 intentos por minuto por IP |
| `POST` | `/api/auth/logout` | Cierra la sesión |
| `GET` | `/api/auth/yo` | Usuario logueado, su estudio y sus roles |

### Publicaciones y fotos

| Método | Ruta | Qué hace |
| --- | --- | --- |
| `GET` | `/api/publicaciones?estado=&tipo=&pagina=&tamanio=` | «Mis publicaciones», de la modificada más recientemente a la más vieja |
| `GET` | `/api/publicaciones/{id}` | Detalle, con sus fotos en orden y su `version` |
| `POST` | `/api/publicaciones` | Crea en Borrador (`201`) |
| `PUT` | `/api/publicaciones/{id}` | Reemplaza los datos (manda el formulario completo y la `version`) |
| `POST` | `/api/publicaciones/{id}/consentimiento` | Confirma el consentimiento del cliente (RN-BLG-03) |
| `POST` | `/api/publicaciones/{id}/publicar` | La publica: necesita fotos y consentimiento. Solo Dueño o Administrativo |
| `POST` | `/api/publicaciones/{id}/ocultar` | La saca del sitio sin borrarla (RN-BLG-06). Solo Dueño o Administrativo |
| `POST` | `/api/publicaciones/{id}/fotos` | Sube una foto (`multipart/form-data`: `archivo` y `version`) |
| `POST` | `/api/publicaciones/{id}/fotos/{fotoId}/quitar` | Quita la foto y borra sus archivos |
| `PUT` | `/api/publicaciones/{id}/fotos/orden` | Reordena: la primera queda como portada |
| `POST` | `/api/publicaciones/{id}/fotos/{fotoId}/portada` | Pone esa foto primera |
| `GET` | `/api/fotos/{fotoId}/{tamanio}` | Devuelve la versión `miniatura`, `galeria` o `detalle` en WebP |

Cada cambio recibe la `version` que tenía el panel y devuelve la nueva, para seguir editando sin recargar.

### Mi página

| Método | Ruta | Qué hace |
| --- | --- | --- |
| `GET` | `/api/paginas` | Las páginas que gestiona la persona: la suya de artista y, si es Dueño, la del estudio |
| `GET` | `/api/paginas/{id}` | Detalle, con su `direccion` y su `version` |
| `POST` | `/api/paginas` | Crea la del estudio (`delEstudio: true`) o la propia de artista (`201`). Nace oculta y la dirección sale del nombre |
| `PUT` | `/api/paginas/{id}` | Reemplaza los datos. La dirección no cambia (RN-BLG-07) |
| `PUT` | `/api/paginas/{id}/direccion` | Cambia la dirección a propósito. Única en toda la plataforma |
| `POST` | `/api/paginas/{id}/mostrar` | La muestra en el sitio |
| `POST` | `/api/paginas/{id}/ocultar` | La saca del sitio sin borrarla |

### Permisos

Por página (D-51, doc 07). Cada artista gestiona su propia página, y nadie más la edita, ni siquiera un Dueño. En la página del estudio:

| Rol | Crear | Editar | Publicar y ocultar | Datos de la página |
| --- | --- | --- | --- | --- |
| Dueño | Sí | Todas | Sí | Sí |
| Administrativo | Sí | Todas | Sí | No |
| Tatuador | Sí | Solo las que creó | No | No |

### Errores

Todos los errores llegan como ProblemDetails (RFC 9457), con un `detail` que se le puede mostrar a la persona.

| Código | Cuándo |
| --- | --- |
| `400` | Datos mal formados o foto que no se puede usar |
| `401` | Sin sesión |
| `403` | El rol no permite la operación |
| `404` | No existe o es de otro estudio (las dos cosas se responden igual) |
| `409` | Alguien la modificó desde que se abrió: recargar y volver a intentar |
| `422` | Una regla de negocio no lo permite. Trae `regla` con el código, por ejemplo `"RN-BLG-03"` |

---

## Estructura del repositorio

```
tato-saas/
├── backend/
│   ├── Tato.slnx
│   ├── global.json                  # fija el SDK de .NET 10
│   ├── .config/dotnet-tools.json    # dotnet-ef como herramienta local
│   ├── src/
│   │   ├── Tato.Domain/
│   │   ├── Tato.Application/
│   │   ├── Tato.Infrastructure/
│   │   └── Tato.Api/                # archivos/: fotos guardadas en desarrollo (fuera de git)
│   └── tests/
│       ├── Tato.Domain.Tests/
│       ├── Tato.IntegrationTests/   # xUnit + Testcontainers, contra SQL Server real
│       └── Tato.ArchitectureTests/  # dependencias y fronteras entre módulos
├── frontend/
│   ├── pnpm-workspace.yaml
│   ├── .nvmrc                       # Node 24
│   ├── apps/
│   │   ├── sitio/                   # sitio público del estudio (www)
│   │   └── panel/                   # panel privado (subdominio panel); crece hasta ser la app del MVP
│   └── packages/                    # pendiente
│       ├── ui/                      # componentes compartidos (Tailwind + shadcn/ui)
│       └── api-client/              # cliente generado desde OpenAPI
├── docs/                            # requerimientos en Markdown (docs 04, 05 y 07 por ahora)
├── .github/                         # pendiente: CI y Dependabot
└── README.md
```

Lo marcado como pendiente todavía no existe en el repositorio.

En el front, el código de cada app se organiza por funcionalidad (`src/features/publicaciones`, `src/features/disenos`, …), no por tipo de archivo.

---

## Primeros pasos

### Requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Node.js 24 LTS (la versión está fijada en `.nvmrc`) y pnpm
- SQL Server 2025 (alcanza con una instancia local)
- Docker, para los tests de integración
- Recomendado: VS Code con C# Dev Kit y la extensión MSSQL

### Backend

```bash
cd backend
dotnet restore
dotnet tool restore

# Cadena de conexión local, fuera del repositorio
dotnet user-secrets set "ConnectionStrings:Tato" \
  "Server=localhost;Database=Tato_Dev;User Id=<usuario>;Password=<contraseña>;TrustServerCertificate=True" \
  --project src/Tato.Api

# Crear o actualizar la base
dotnet ef database update --project src/Tato.Infrastructure --startup-project src/Tato.Api

# Dar de alta el estudio y sus usuarios (en la versión 1 no hay registro, P-19)
dotnet run --project src/Tato.Api -- alta-estudio
dotnet run --project src/Tato.Api -- alta-usuario

# Levantar la API con HTTPS: las cookies de sesión (__Host-) solo viajan por HTTPS
dotnet run --project src/Tato.Api --launch-profile https
```
La API queda en `https://localhost:7239`. En desarrollo, el documento OpenAPI está en `/openapi/v1.json` y las fotos se guardan en `src/Tato.Api/archivos/`.

Para probarla desde el navegador, Swagger UI está en `https://localhost:7239/swagger`, solo en desarrollo. Los pedidos que modifican datos llevan el token antiforgery:

1. `GET /api/auth/antiforgery` y copiar el `token`.
2. Pegarlo en **Authorize**.
3. `POST /api/auth/login`, que responde `204`.
4. Pedir un token nuevo y cambiarlo en **Authorize** (Logout y pegar el nuevo): el de antes de iniciar sesión ya no sirve.

### Frontend

```bash
cd frontend
pnpm install

pnpm dev:sitio               # sitio público
pnpm dev:panel               # panel privado
pnpm api:generate            # pendiente: regenera el cliente tipado desde el OpenAPI de la API
```

En desarrollo, Vite hace de proxy de `/api` hacia la API, así la cookie de sesión es del mismo origen y no hace falta configurar CORS.

### Tests

```bash
cd backend && dotnet test    # requiere Docker corriendo (Testcontainers)
cd frontend && pnpm test
```

Los tests de integración levantan un SQL Server 2025 en Docker. Para usar uno ya instalado, cargá su cadena de conexión en la variable `TATO_TESTS_SQLSERVER`: cada corrida crea una base nueva con nombre único y la borra al terminar, así nunca toca la de desarrollo.

---

## Configuración

Nada hardcodeado (D-34):

- **Parámetros de negocio**: viven en la configuración de cada estudio, en la base, con valores por defecto (por ejemplo, el máximo de fotos por publicación, 10 por defecto). Nunca en el código.
- **Parámetros técnicos**: `appsettings.json` tipados con el patrón Options y validados al arrancar: con una configuración imposible, la API no levanta. Por ejemplo, `Fotos` (peso y píxeles máximos, lado de cada tamaño, calidad de WebP) y `Almacenamiento:Disco:CarpetaRaiz`.
- **Secretos**: nunca en el repositorio. `dotnet user-secrets` en desarrollo; variables de entorno en producción.
- **Front**: variables en `.env.local` (ignorado por git), con un `.env.example` versionado como referencia. Todo lo que llega al front es público: ahí no van secretos.

---

## Convenciones

**Idioma.** El dominio usa el vocabulario del glosario (doc 05): `Publicacion`, `Diseno`, `Estudio`, `Turno`, `Sena`. Sin ñ ni tildes en los identificadores.

**Ramas.** `main` siempre está desplegable y protegida. El trabajo va en ramas cortas (`feat/…`, `fix/…`, `docs/…`) y entra por pull request, para que corra la CI.

**Commits.** [Conventional Commits](https://www.conventionalcommits.org/es/) en español, con el módulo como alcance. Si el cambio implementa o modifica una regla o decisión, se cita su identificador:

```
feat(portfolio): exigir consentimiento antes de publicar (RN-BLG-03)
fix(estudios): tomar el EstudioId de la sesión y no del pedido
docs: cerrar P-18 con el hosting elegido
```

**Identificadores de la documentación.** `RN-xxx` regla de negocio · `D-xx` decisión · `P-xx` pendiente · `R-xx` punto a revisar del modelo de datos.

**Formato.** `dotnet format` en el backend y el linter de la plantilla de Vite en el front, ambos validados en la CI.

---

## Hoja de ruta de la versión 1

- [ ] **Semana 1** (5 al 11/10): repositorio, entorno, hosting y dominio (corte 0)
  - [x] Repositorio y entorno
  - [ ] Hosting y dominio (P-18)
- [ ] **Semana 2** (12 al 18/10): login, estudios, usuarios y aislamiento por estudio (corte 1) · primer despliegue
  - [x] Login, estudios, usuarios y aislamiento por estudio
  - [ ] Primer despliegue
  - [x] Backend del panel, adelantado: publicaciones, permisos, control de versión y fotos (corte 2)
- [ ] **Semana 3** (19 al 25/10): ajustes del backend al doc 07, versión 2.0, wireframe y front del panel
  - [x] Tipo de publicación, texto de 2200 caracteres y permisos por página
  - [x] Páginas públicas en el panel («Mi página»)
  - [ ] Endpoints públicos
  - [ ] Wireframe navegable del sitio y del panel
  - [ ] Front del panel
- [ ] **Semana 4** (26/10 al 1/11): sitio público: las páginas, sus pestañas y el detalle de cada publicación
- [ ] **Semana 5** (2 al 8/11): diseño visual de cada página, vistas previas y SEO
- [ ] **Semana 6** (9 al 15/11): carga del contenido real, pruebas en celulares y ajustes, sin funciones nuevas
- [ ] **16/11**: margen

Si el plan se atrasa, se recorta en este orden: el diseño visual propio de cada página (se arranca con un tema común), la pestaña Blog y la pestaña «Sobre». Lo indispensable es el encabezado con contacto, la pestaña Trabajos, el detalle y el panel de carga.

---

## Despliegue

Propuesta por defecto (P-18, ver `docs/`): un servidor virtual con SQL Server Express y Cloudflare adelante para DNS, CDN y HTTPS, con un presupuesto de hasta 30.000 ARS por mes. El dominio es de la plataforma, con una ruta por página (D-52), y el panel va en el subdominio `panel`. El primer despliegue se hace cuanto antes, para que los problemas del hosting aparezcan temprano.

---

## Documentación

| Documento | Contenido | Dónde |
| --- | --- | --- |
| 01 · Visión, alcance y roles | Qué es el producto, alcance, roles y matriz de permisos | Proyecto (PDF) |
| 02 · Reglas de negocio generales | Tatuajes, co-tatuadores, sesiones, visibilidad, clientes y notificaciones | Proyecto (PDF) |
| 03 · Módulo Agenda y Turnos | Calendario, señas, turnos, estados, reagendamientos y montos | Proyecto (PDF) |
| 04 · Modelo de datos | Entidades y relaciones: la versión 1 implementada y el MVP en borrador | [docs/](docs/04_Modelo_de_Datos_BORRADOR.md) |
| 05 · Decisiones, glosario y pendientes | Por qué se decidió cada cosa y temas abiertos | [docs/](docs/05_Decisiones_Glosario_y_Pendientes.md) |
| 06 · Stack y arquitectura | Tecnologías, arquitectura, seguridad, escalabilidad y patrones | Proyecto (PDF) |
| 07 · Blog y portfolio público | Alcance, pantallas y reglas de la primera entrega | [docs/](docs/07_Blog_y_Portfolio_Publico.md) |

Los documentos pasan a Markdown, dentro de `docs/`, a medida que se actualizan: así quedan versionados junto al código. Los que todavía no se pasaron siguen como PDF en el proyecto.

Los documentos 02, 03 y 07 son la fuente de verdad de las reglas de negocio. Si el código y la documentación no coinciden, se corrige uno de los dos en el mismo pull request.

---

## Licencia

Software privado. Todos los derechos reservados.