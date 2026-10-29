# Documento 07 · Blog y portfolio público

Requerimientos de la primera entrega: las páginas públicas del estudio y de sus artistas, y el panel para publicar.

| Versión | Fecha | Estado | Relacionados |
| --- | --- | --- | --- |
| 2.4 | 29 de octubre de 2026 | Aprobado para la versión 1 | Docs 01, 04, 05 y 06 |

> **Prioridad (D-46):** el blog es la primera entrega y tiene que estar online antes del **17 de noviembre de 2026**, cuando la tatuadora del estudio piloto viaja a tatuar a Bolivia. El MVP de gestión sigue después.

> **Novedades de la versión 2.0:** se cierran las preguntas de la versión 1.0 y se achica el alcance a un portfolio parecido a Instagram (D-48). Cada estudio y cada artista tiene su propia página, con sus trabajos, su blog y una sección «Sobre». Los diseños disponibles, las fechas de viaje, los filtros de la galería y los botones de WhatsApp por trabajo quedan para después de la versión 1.

---

## 1. Decisiones

| ID | Decisión | Motivo |
| --- | --- | --- |
| D-46 | El blog es la primera entrega, antes del 17/11/2026. El MVP de gestión continúa después. | Viaje de trabajo de la tatuadora a Bolivia: quiere un portfolio profesional para compartir. |
| D-47 | El blog usa la misma tecnología que el MVP: React con TypeScript, ASP.NET Core y SQL Server (doc 06). | Un solo stack para aprender y mantener. |
| D-48 | Alcance de la versión 1: un portfolio parecido a Instagram, con trabajos, blog y «Sobre». | Llegar al 17/11 con lo que el estudio va a usar. El formato de Instagram ya lo conocen el estudio y sus clientes. |
| D-49 | Una página pública por estudio y una por cada artista, independientes y con la misma estructura. El diseño visual de cada una se define después y se configura por página. | Cada artista tiene su propia marca, y el estudio la suya. Una sola estructura se construye y se mantiene una vez. |
| D-50 | Una sola entidad Publicación, con tipo Trabajo o Blog. | Los dos tipos tienen las mismas reglas: fotos, texto, estados y consentimiento. Una entidad aparte para el blog repetiría todo. |
| D-51 | Permisos por página (sección 5). Cierra P-21. | Cada página es de quien la representa. |
| D-52 | Un dominio de la plataforma con una ruta por página. Cierra P-24. | Es lo más simple para empezar y no impide sumar dominios propios después. |
| D-53 | El contacto son links a Instagram y WhatsApp en el encabezado de cada página. Cierra P-27. | Es como los contactan hoy. |
| D-54 | El consentimiento del cliente (RN-BLG-03) se pide para publicar cualquier publicación, sea trabajo o blog. | Ya está implementado, en el panel es una sola casilla y protege al estudio. |
| D-55 | En la versión 1, los estudios y los usuarios los da de alta el desarrollador por consola. Cierra P-19. | Para un solo estudio piloto no hace falta registro. Aplicada en el corte 1. |
| D-56 | Dos aplicaciones de front, el sitio y el panel, en el mismo repositorio. Cierra P-28. | El sitio queda liviano y sin login; el panel crece hasta ser la app del MVP. Aplicada en el corte 0. |
| D-57 | Caché del sitio público: la caché de salida de ASP.NET Core, en memoria, guarda las páginas y las publicaciones y se vacía entera con cualquier cambio desde el panel. Para el navegador, los datos se guardan un minuto (`public, max-age=60`) y las fotos una hora (`public, max-age=3600`), que también guarda Cloudflare. Algo que se oculta deja de verse en el sitio en hasta un minuto, y su foto, por link directo, hasta una hora. | Con una sola instancia alcanza la memoria: Redis queda para cuando haya varias (doc 06). A esta escala, vaciar todo es lo más simple y nunca deja nada viejo. Los tiempos equilibran lo que tarda en desaparecer algo oculto con no pedirle todo al servidor en cada visita. Se invalida en un filtro de la API, así Application no depende de ASP.NET. |

---

## 2. Objetivo

Una página pública por estudio y por artista, parecida a un perfil de Instagram: muestra sus trabajos, permite escribir en un blog y presenta a cada uno. Para el viaje, el objetivo concreto es tener un link profesional para compartir en Instagram y WhatsApp, que muestre foto y título al compartirlo.

---

## 3. Alcance

### 3.1 Versión 1: antes del 17 de noviembre

- **Páginas públicas** del estudio y de cada artista, con tres pestañas: Trabajos, Blog y «Sobre» (sección 7).
- **Panel privado** con login para cargar y publicar, pensado primero para el celular (sección 8).
- **Fotos optimizadas** al subirlas: tres tamaños en WebP, sin metadatos (RN-BLG-05).
- **Vistas previas y SEO básico:** título, descripción y foto por página, y un sitemap.
- **Online:** dominio, servidor, HTTPS y despliegue (P-18).
- **Base del sistema:** login, estudios, usuarios y aislamiento por estudio (D-42), que después reutiliza el MVP.

### 3.2 Después de la versión 1

- Diseños disponibles (P-22) y fechas de viaje (P-23).
- Videos.
- Filtros de la galería.
- Botones de WhatsApp con un mensaje que identifica el trabajo (RN-BLG-08).
- Dominio propio por página.
- «Publicar en blog» desde un tatuaje Finalizado del MVP (RN-BLG-10).
- Pedido o reserva de turno integrado con la Agenda.
- Alta de estudios y artistas por autoservicio, e invitación de usuarios.
- Estadísticas de visitas.

---

## 4. Páginas públicas

- **Una página por estudio y una por cada artista** que trabaja en él. Son independientes: cada una muestra solo su propio contenido (RN-BLG-12).
- **La misma estructura para todas** (sección 7). Cada una va a tener su diseño visual propio (colores, tipografías, ornamentos), configurado por página y no en el código.
- **Dirección:** un dominio de la plataforma con una ruta por página, por ejemplo `plataforma.com/alkimya` y `plataforma.com/inkmortal`. La ruta sale del nombre que elige cada uno (RN-BLG-07). En la raíz del dominio hay una portada simple de la plataforma.
- **Relación entre páginas:** la del estudio muestra a sus artistas, y la de cada artista enlaza al estudio donde trabaja (sección 7.4).

---

## 5. Usuarios y permisos

| Quién | Dónde | Qué puede hacer |
| --- | --- | --- |
| Visitante | Todas las páginas públicas | Verlas sin registrarse. |
| Artista | Su propia página | Crear, editar, publicar y ocultar. Nadie más edita su página. |
| Dueño y Administrativo | Página del estudio | Crear, editar, publicar y ocultar todo. |
| Tatuador | Página del estudio | Crear y editar lo que creó, sin publicar ni ocultar. |

Un artista es un usuario con perfil de Tatuador. En el estudio piloto, los dos usuarios son Dueños y tatuadores: cada uno gestiona su página, y los dos gestionan la del estudio.

Los datos de cada página (nombre, bio, foto, contacto y «Sobre») los edita su artista. Los del estudio, los Dueños.

---

## 6. Contenido

| Entidad | Datos principales | Notas |
| --- | --- | --- |
| Publicación | Tipo (Trabajo o Blog), título, texto de hasta 2200 caracteres, de 1 a 10 fotos (la primera es la portada), fijada, estado (Borrador, Publicada u Oculta) y consentimiento (quién lo confirmó y cuándo). | Va a la página de su artista; sin artista, a la del estudio (RN-BLG-12). Nunca guarda datos del cliente. Estilo, zona del cuerpo y fecha del trabajo ya existen en el modelo, pero el front de la versión 1 no los usa. |
| Foto | Tres versiones optimizadas (miniatura, galería y detalle), orden, ancho y alto. | Implementada en el corte 2 (doc 04). |
| Página pública | Dirección, nombre, bio corta, foto o logo, Instagram, WhatsApp, y el texto y la foto de «Sobre». En la del estudio, además, la ubicación y los horarios. | Una por estudio y una por artista. Implementada, con la foto o logo y la foto de «Sobre» (R-16, doc 04). |

---

## 7. Sitio público

Casi todas las visitas van a llegar desde el link de la bio de Instagram, en el celular: todo se diseña primero para el celular.

### 7.1 Encabezado

Igual en todas las páginas, como un perfil de Instagram: foto o logo, nombre, bio corta y links a Instagram y WhatsApp.

### 7.2 Pestañas

| Pestaña | Qué muestra |
| --- | --- |
| Trabajos | Una grilla de tres columnas con la portada de cada trabajo, en el orden de RN-BLG-13. Al final, **Ver más**. |
| Blog | Una lista de las publicaciones del blog, con portada, título, fecha y el comienzo del texto, en el mismo orden. |
| Sobre mí / Sobre nosotros | «Sobre mí» en la página de un artista y «Sobre nosotros» en la del estudio (sección 7.4). |

Al tocar una publicación, se abre su detalle.

### 7.3 Detalle de una publicación

Las fotos para deslizar, el título, el texto y la fecha, con los botones **Compartir** y **Volver**.

### 7.4 «Sobre»

| Página | Qué muestra |
| --- | --- |
| Artista | Un texto libre y una foto. Si trabaja en un estudio, un link a la página del estudio. |
| Estudio | Un texto libre y una foto, la ubicación y los horarios, y la foto y el nombre de cada artista, con un link a su página. |

### 7.5 Vistas previas y SEO

WhatsApp, Instagram y Facebook no ejecutan JavaScript para armar la vista previa de un link. Por eso el backend entrega el HTML de cada página y de cada publicación con su título, descripción y foto ya cargados (metadatos Open Graph), y genera el sitemap.

---

## 8. Panel privado

Todo el panel se diseña primero para el celular: es donde se sacan las fotos y desde donde se carga el contenido.

| Pantalla | Qué permite |
| --- | --- |
| Inicio de sesión | Email y contraseña, con la sesión en cookie (D-39). En la versión 1 no hay «Olvidé mi contraseña»: se resuelve a mano. |
| Mis publicaciones | Elegir la página, si la persona gestiona más de una; filtrar por estado (Todas, Borradores, Publicadas u Ocultas) y crear una publicación nueva, de tipo Trabajo o Blog. |
| Crear o editar publicación | Agregar hasta 10 fotos desde la cámara o la galería, elegir la portada, ordenarlas y quitarlas. Completar el título y el texto, fijarla y confirmar el consentimiento. Guardar, publicar, ocultar y verla en el sitio. |
| Mi página | Nombre (define la dirección), bio, foto o logo, Instagram, WhatsApp y el «Sobre». En la del estudio, además, la ubicación y los horarios. La dirección se puede cambiar a propósito. |

Si otra persona modificó la publicación mientras se editaba, el panel muestra un aviso con el botón **Recargar**, en lugar de pisar sus cambios.

---

## 9. Reglas de negocio

| ID | Regla | Estado |
| --- | --- | --- |
| RN-BLG-01 | El sitio público solo muestra publicaciones en estado Publicada. | Implementada |
| RN-BLG-02 | Ninguna página pública muestra datos de clientes. | Implementada: los endpoints públicos no devuelven datos de clientes ni internos |
| RN-BLG-03 | Para publicar hay que confirmar que el cliente dio su consentimiento para difundir las fotos. Se registra quién lo confirmó y cuándo. Vale para trabajos y para el blog (D-54). | Implementada |
| RN-BLG-04 | Para publicar hace falta al menos una foto, y la primera es la portada. Una publicación publicada no puede quedarse sin fotos. El máximo es configurable por estudio, con 10 por defecto. | Implementada |
| RN-BLG-05 | Las fotos se optimizan al subirlas: miniatura, galería y detalle, en WebP y sin metadatos. El sitio nunca sirve los originales. | Implementada |
| RN-BLG-06 | Ocultar no borra: la publicación cambia de estado, como en el resto del sistema (RN-VIS-06). | Implementada |
| RN-BLG-07 | Cada página pública tiene una dirección única en toda la plataforma. Se arma con el nombre que elige cada uno, en minúsculas, sin tildes ni espacios. Si después cambia el nombre, la dirección no cambia sola, para no romper los links ya compartidos: se cambia a propósito desde el panel. | Ampliada en la 2.0 (antes era una por estudio) · Implementada |
| RN-BLG-08 | Los botones de contacto abren WhatsApp con un mensaje que identifica el trabajo. | Postergada: después de la versión 1 |
| RN-BLG-09 | Quién publica. | Reemplazada por los permisos por página (D-51) |
| RN-BLG-10 | Versión 2: «Publicar en blog» desde un tatuaje Finalizado del MVP crea una publicación en borrador con sus fotos y datos, sin datos del cliente (RN-TAT-11). | Versión 2 |
| RN-BLG-11 | El texto de una publicación tiene hasta 2200 caracteres, el mismo límite que Instagram. | Implementada |
| RN-BLG-12 | Una publicación va a la página de su artista; sin artista, a la del estudio. Cada página muestra solo sus propias publicaciones. | Implementada |
| RN-BLG-13 | Las publicaciones fijadas aparecen primero; las demás, de la publicada más recientemente a la más vieja. | Implementada |

---

## 10. Tecnología y arquitectura

La misma que el MVP (D-47).

- **Front:** dos aplicaciones en el mismo repositorio (D-56): el sitio público, liviano y sin login, y el panel. React, TypeScript, Vite, Tailwind CSS con shadcn/ui, React Router, TanStack Query y React Hook Form con Zod (doc 06). El cliente de la API se genera desde el OpenAPI con Orval.
- **Backend:** el módulo Portfolio, más la base de Estudios e identidad que después usa el MVP.
  - Los endpoints del panel usan la cookie de sesión y están implementados desde el corte 2 (ver el README).
  - Los endpoints públicos son de solo lectura, con caché (D-57), y están implementados (ver el README).
- **Vistas previas:** el backend entrega el HTML con los metadatos (sección 7.5). Google sí ejecuta JavaScript al indexar, aunque puede tardar más; si el posicionamiento orgánico se vuelve crítico, se suma prerenderizado sin cambiar el stack.
- **Fotos:** se procesan con NetVips al subirlas y se guardan a través del puerto de almacenamiento (doc 06, 6.1): en el disco del servidor o en un almacenamiento de objetos, según el hosting (P-18).
- **Multi-tenancy:** aislamiento por estudio desde el primer día (D-42), con el estudio piloto como primer estudio.
- **Concurrencia:** el panel manda la versión de la publicación al guardar. Si alguien guardó antes, recibe un error 409 en lugar de pisar el cambio.

---

## 11. Plan hasta el 17 de noviembre

Plan actualizado el 29/10: el backend se termina primero y el front va después, de una vez.

| Fechas | Objetivo | Estado al 29/10 |
| --- | --- | --- |
| 5 al 25 de octubre | Repositorio y entorno (corte 0). Login, estudios, usuarios y aislamiento (corte 1). Backend del panel (corte 2). Ajustes a esta versión: tipo de publicación, texto de 2200 caracteres, permisos por página y «Mi página». | Listo. |
| 26 de octubre al 2 de noviembre | Terminar el backend: fotos de cada página (R-16), endpoints públicos, preparación para producción, y vistas previas y SEO. | En curso: las fotos de cada página y los endpoints públicos, listos. |
| 3 al 9 de noviembre | El panel, con su wireframe, y el primer despliegue (P-18). | — |
| 10 al 16 de noviembre | El sitio público y las pruebas en celulares. | — |
| 17 de noviembre | Online. | — |

### Riesgos y recortes

- **El cuello de botella suele ser el contenido:** las fotos elegidas con permiso de los clientes y los textos. Conviene que el estudio piloto empiece a juntarlo ya.
- **Orden de recorte propuesto**, si el plan se atrasa:
  1. El diseño visual propio de cada página: se arranca con un tema común. **Aplicado:** las tres páginas arrancan con un tema común, y el diseño propio de cada una (D-49) queda para después.
  2. La pestaña Blog.
  3. La pestaña «Sobre».

  Lo indispensable es el encabezado con contacto, la pestaña Trabajos, el detalle y el panel de carga.
- **El primer despliegue tiene que hacerse cuanto antes,** para que los problemas del hosting aparezcan temprano y no la última semana.

---

## 12. Preguntas de la versión 1.0

El registro completo de pendientes está en el doc 05.

| ID | Pregunta | Estado |
| --- | --- | --- |
| P-18 | Hosting y dominio. | Abierto. El presupuesto es de hasta 30.000 ARS por mes. El dominio es de la plataforma (D-52): falta elegir el nombre y el hosting. |
| P-19 | Alta de usuarios en la versión 1. | Resuelto (D-55): por consola, con los comandos `alta-estudio` y `alta-usuario`. |
| P-21 | ¿Quién publica? | Resuelto (D-51): permisos por página. |
| P-22 | Diseños disponibles: precio, moneda, y si son únicos o repetibles. | Postergado: los diseños quedan fuera de la versión 1. |
| P-23 | Sección de próximas fechas. | Postergado: fuera de la versión 1. |
| P-24 | ¿Ruta dentro de la plataforma o dominio propio? | Resuelto (D-52): ruta dentro del dominio de la plataforma. El dominio propio, después. |
| P-25 | ¿Las fotos llevan marca de agua? | Abierto. |
| P-26 | ¿Cómo se registra hoy el permiso de los clientes para publicar sus fotos? ¿Alcanza con lo que ya publican en Instagram? | Abierto. Mientras tanto, el panel pide confirmarlo con una casilla (RN-BLG-03). |
| P-27 | ¿Contacto solo por WhatsApp, o también por Instagram y un formulario? | Resuelto (D-53): links a Instagram y WhatsApp. |
| P-28 | ¿Una o dos aplicaciones de front? | Resuelto (D-56): dos. |
| P-29 | Artista sin estudio: cómo se modela cuando haya alta por autoservicio. Hoy sería un estudio de una sola persona. | Abierto, para después de la versión 1. |
| P-30 | ¿Un Dueño puede ocultar contenido de la página de un artista de su estudio? Hoy no: cada página es solo de su artista. | Abierto, para cuando haya artistas que no sean Dueños. |

---

## 13. Notas para el diseño visual

Salen del Instagram de cada uno. Se usan en la etapa de diseño, después de aprobar el wireframe.

- **Para todas las páginas:**
  - El protagonista es el trabajo: el diseño tiene que ser sobrio, con fotos grandes y mucho aire.
  - Las fotos son mayormente verticales, y la grilla no las recorta.
- **Ink Mortal** (`@ink.mortall`, marca INKM◆RTAL, la tatuadora del estudio piloto):
  - Blackwork, ornamental y freehand, con piezas grandes, influencia japonesa y formas neotribales.
  - Paleta casi monocromática: negro, blanco y grises, con el verde de las plantas y algún dorado como acento.
  - Motivo recurrente: la espiral.
  - Le gusta Tool. Como inspiración sirven la geometría sagrada, la proporción áurea y la espiral, sin usar los logos de la banda ni obras con derechos de autor.
- **Emma:** pendiente; falta ver su Instagram.
- **El estudio** (`@alkimya.tattoo`): pendiente.

---

## 14. Impacto en los demás documentos

- **Doc 01:** el blog pasa de «fuera del MVP» a primera entrega, y se suman los permisos por página. Pendiente.
- **Doc 04:** actualizado a la versión 1.3, con las entidades de la versión 1 y la propuesta de Página pública.
- **Doc 05:** actualizado a la versión 1.3, con las decisiones D-46 a D-56, los pendientes P-19 a P-30 y el glosario del blog.
- **Doc 06:** sumar las páginas públicas, la dirección por página y la entrega del HTML con metadatos. Pendiente.

---

## Historial de versiones

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 2.4 | 29/10/2026 | Endpoints públicos implementados: RN-BLG-01, RN-BLG-02, RN-BLG-12 y RN-BLG-13. Decisión D-57: la caché del sitio. |
| 2.3 | 29/10/2026 | La Página pública, implementada con sus dos fotos (R-16): el panel las sube, las reemplaza y las quita. |
| 2.2 | 29/10/2026 | Plan nuevo hasta el 17/11 (sección 11): el backend se termina hasta el 2/11; el panel y el primer despliegue van del 3 al 9/11, y el sitio, del 10 al 16/11. Se aplica el primer recorte: tema visual común para las tres páginas. |
| 2.1 | 16/10/2026 | Estado de las reglas: RN-BLG-07 y RN-BLG-11 implementadas; RN-BLG-12, en parte. La Página pública, implementada en el backend sin sus fotos. |
| 2.0 | 16/10/2026 | Se cierran las preguntas de la 1.0 y el alcance se achica a un portfolio parecido a Instagram, con páginas por estudio y por artista. Decisiones D-48 a D-56. Reglas RN-BLG-11 a RN-BLG-13; RN-BLG-07 ampliada, RN-BLG-08 postergada y RN-BLG-09 reemplazada. Pasa a Markdown, en `docs/` del repositorio. |
| 1.0 | 4/10/2026 | Borrador inicial para discutir. |
