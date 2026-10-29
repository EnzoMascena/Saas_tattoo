# Documento 05 · Decisiones, glosario y pendientes

Por qué se decidió cada cosa, vocabulario del dominio y temas abiertos.

| Versión | Fecha | Estado | Relacionados |
| --- | --- | --- | --- |
| 1.4 | 29 de octubre de 2026 | Documento vivo | Docs 01 a 04, 06 y 07 |

> **Novedades de la versión 1.3:** se cierran las preguntas del documento 07 (blog y portfolio). Se suman las decisiones D-46 a D-56 y los pendientes P-19 a P-30, y el glosario incorpora el vocabulario del blog. Pasa a Markdown, en `docs/` del repositorio.

---

## 1. Registro de decisiones

Las reglas vigentes están en los documentos 02, 03 y 07. Este registro explica el porqué y qué alternativas se descartaron.

| ID | Decisión | Motivo o alternativa descartada | Ver |
| --- | --- | --- | --- |
| D-01 | SaaS multi-tenant desde el día uno; el estudio de la hermana del desarrollador es el piloto. | El objetivo siempre fue un SaaS; el piloto funciona como tester. | Doc 01 |
| D-02 | Facturación por usuario (asiento). Los roles acumulados del Dueño ocupan un solo asiento. | | Doc 01 |
| D-03 | Roles: Dueño (acumula Tatuador y Administrativo), Tatuador exclusivo, Administrativo exclusivo. | Quien paga un asiento de Administrativo no puede ser Tatuador, y viceversa. | Doc 01 |
| D-04 | Un estudio puede tener varios Dueños. | Los estudios grandes suelen ser sociedades. | Doc 01 |
| D-05 | MVP: Agenda y Turnos, Clientes, Tatuajes, Sesiones con walk-ins, roles y notificaciones in-app. | Blog, Finanzas, email/WhatsApp y reserva online quedan para después. El blog pasa a ser la primera entrega (D-46). | Doc 01 |
| D-06 | Estados del tatuaje: En Proceso, Finalizado, Cancelado. | Se descartó «En espera»: trabajar sobre tatuajes prometidos complica todo, y la promo de regalo se hacía en el momento. | RN-TAT-02 |
| D-07 | Se agrega Cancelado para tatuajes. | Hay proyectos que se abandonan a mitad de camino. | RN-TAT-05 |
| D-08 | Completitud de la ficha independiente del estado, con 8 campos obligatorios. | | RN-TAT-06 |
| D-09 | Pestaña fija «Tatuajes incompletos» para el Administrativo. | Los tatuadores cargan lo mínimo y el Administrativo completa. | RN-TAT-07 |
| D-10 | Finalizados visibles para todos los tatuadores; En Proceso y Cancelados solo para asignados y roles superiores. | | RN-VIS-02, 03 |
| D-11 | Opción «Publicar en blog» al finalizar; el blog se precargará con tatuajes finalizados. | Post-MVP. | RN-TAT-11 |
| D-12 | Notificaciones del MVP solo in-app. | Email y WhatsApp más adelante; WhatsApp es el objetivo final. | RN-NOT-01, 03 |
| D-13 | Tatuadores y Administrativos pueden crear clientes. | | RN-CLI-02 |
| D-14 | Jerarquía Cliente → Tatuaje → Sesión; la sesión viene de un turno o es walk-in. | | RN-SES-02, 03 |
| D-15 | Walk-ins desde el MVP. | Se evaluó dejarlos para después; se prefirió que queden resueltos desde el inicio. | RN-SES-05 |
| D-16 | Un turno puede producir varias sesiones (varios tatuajes en una visita). | Caso raro, pero el sistema es para muchos estudios. | RN-SES-04 |
| D-17 | Tatuador responsable más co-tatuadores; el responsable define los porcentajes. | Se descartaron «mitad y mitad automático» y «cada uno anota lo suyo». | RN-COT-01 a 04 |
| D-18 | Bloqueo de calendario del co-tatuador para turnos nuevos, respetando los preexistentes. | | RN-COT-05 |
| D-19 | La seña es entidad de primer nivel, prerrequisito del turno y asociada al turno. | Se descartó asociarla al tatuaje completo. | RN-SEN-01 |
| D-20 | Seña de pago único, no reembolsable, transferible hasta 2 veces. | Se descartaron pagos parciales. | RN-SEN-02 a 04 |
| D-21 | Vencimiento de la seña a los 14 días del pago. | Evita señas pagadas para tatuarse dentro de un año. | RN-SEN-05 |
| D-22 | Estados del turno: En espera, En proceso, Finalizado, Cancelado e Indefinido. Cancelados e indefinidos se conservan. | | Doc 03, 3.2 |
| D-23 | Cancelado es manual; Indefinido es automático (ausencia a las 00:00 del día siguiente o seña vencida). | El día siguiente da margen a llegadas tarde y olvidos. | RN-TUR-05, 06 |
| D-24 | Reagendamiento sobre el mismo registro con contador (opción A). | Se descartó crear un turno nuevo por reagendamiento (opción B). Si hiciera falta auditoría, se agrega una tabla de historial más adelante. | RN-TUR-04 |
| D-25 | Monto acordado en el turno (congelado), monto total en el tatuaje (editable) y saldo calculado en vivo. | Se descartó guardar saldos precalculados y guardar el restante en la seña. | RN-MON-01 a 04 |
| D-26 | Un Tatuador que hace clic en un turno ajeno ve un panel reducido, sin cliente ni montos. | | RN-CAL-04 |
| D-27 | Calendario mensual estilo Google Calendar, igual para todos, con nombre e imagen del tatuador y hora. | | RN-CAL-01 a 03 |
| D-28 | Los tatuajes se crean desde su propio apartado, eligiendo walk-in o un turno (excepto Cancelado e Indefinido). | | RN-TAT-09 |
| D-29 | Numeración legible: nº de seña, nº de turno del estudio y nº de turno del tatuador. | | RN-TUR-02 |
| D-30 | Cualquier usuario puede registrar una seña; se guarda quién la registró. | El orden es responsabilidad del Administrativo. | RN-SEN-06 |
| D-31 | Inclinación por desarrollo propio (no definitiva). | Tras relevar competidores y plantillas. | Doc 01 |
| D-32 | WhatsApp y Mercado Pago son indispensables para el mercado argentino (post-MVP). | | Doc 01 |
| D-33 | Stack base: ASP.NET Core sobre .NET 10, SQL Server 2025 y React con TypeScript. | Elección del equipo. Confirma el desarrollo propio (D-31). | Doc 06 |
| D-34 | Plazo de vencimiento de la seña y máximo de reagendamientos configurables por estudio, con 14 días y 2 reagendamientos por defecto. | Principio «nada hardcodeado». Reemplaza los valores fijos de D-20 y D-21 y cierra P-10. | RN-SEN-04, 05 |
| D-35 | Principio rector: calidad, escalabilidad y mantenibilidad por sobre la velocidad de desarrollo. | Se descarta optimizar para terminar lo antes posible. | Doc 06 |
| D-36 | Patrones de diseño donde resuelvan un problema concreto del dominio (catálogo en el doc 06). | Se descarta aplicar patrones «por cumplir». | Doc 06, 9 |
| D-37 | Clean Architecture como estilo arquitectónico base. | Facilita integrar APIs externas y mantener el sistema. Se descarta la arquitectura en capas tradicional, donde el negocio depende del acceso a datos. | Doc 06, 3 |
| D-38 | Monolito modular como variante de Clean Architecture: un solo despliegue dividido en módulos de negocio. | Se descartan los microservicios por ahora: multiplican la complejidad sin beneficio real a este tamaño. | Doc 06, 3 |
| D-39 | Sesión con cookie HttpOnly de ASP.NET Core Identity. | JWT solo si se suman apps móviles o una API pública; ambos esquemas pueden convivir. | Doc 06, 7.1 |
| D-40 | Interfaz con Tailwind CSS y shadcn/ui. | Se descarta Bootstrap. | Doc 06, 2 |
| D-41 | Doble factor fuera del MVP; se activa cuando la app crezca, empezando por los Dueños. | Identity ya lo soporta: activarlo es configuración, no rediseño. | Doc 06, 7.1 |
| D-42 | Multi-tenancy: base compartida con EstudioId en todas las tablas del estudio, filtros con nombre de EF Core y Row-Level Security como segunda línea de defensa. | Cierra P-16. | Doc 06, 4 |
| D-43 | Fechas guardadas en UTC; en pantalla, hora y formato argentinos; zona horaria por estudio con Argentina por defecto. | Evita que la regla de las 00:00 se dispare a las 21:00 si el servidor corre en UTC, y permite estudios de otros países. | Doc 06, 5 |
| D-44 | Metas medibles iniciales de capacidad, rendimiento, disponibilidad y backups. | Convierten «escalable» en criterios que se verifican con pruebas de carga. Cierra P-14. | Doc 06, 8.1 |
| D-45 | Se aprueban el resto del stack y la arquitectura del documento 06 tal como se propusieron. | Cierra la validación del documento 06 (P-17). | Doc 06 |
| D-46 | El blog es la primera entrega, antes del 17/11/2026; el MVP de gestión continúa después. | Viaje de trabajo de la tatuadora a Bolivia: quiere un portfolio profesional para compartir. Modifica D-05. | Doc 07 |
| D-47 | El blog usa la misma tecnología que el MVP: React con TypeScript, ASP.NET Core y SQL Server. | Un solo stack para aprender y mantener. | Doc 07 |
| D-48 | Alcance de la versión 1 del blog: un portfolio parecido a Instagram, con trabajos, blog y «Sobre». Diseños, fechas de viaje, filtros, botones de WhatsApp por trabajo y videos quedan para después. | Llegar al 17/11 con lo que el estudio va a usar. El formato de Instagram ya lo conocen el estudio y sus clientes. | Doc 07, 3 |
| D-49 | Una página pública por estudio y una por cada artista, independientes y con la misma estructura. El diseño visual de cada una se configura por página. | Cada artista tiene su propia marca, y el estudio la suya. Una sola estructura se construye y se mantiene una vez. | Doc 07, 4 |
| D-50 | Una sola entidad Publicación, con tipo Trabajo o Blog, de 1 a 10 fotos y texto de hasta 2200 caracteres. | Los dos tipos tienen las mismas reglas. Se descarta una entidad aparte para el blog. | Doc 07, 6 |
| D-51 | Permisos por página: cada artista gestiona la suya y nadie más la edita; en la página del estudio, Dueño y Administrativo gestionan todo, y el Tatuador crea y edita lo suyo sin publicar. | Cada página es de quien la representa. Cierra P-21. | Doc 07, 5 |
| D-52 | Un dominio de la plataforma con una ruta por página, que sale del nombre que elige cada uno. | Es lo más simple para empezar y no impide dominios propios después. Cierra P-24. | RN-BLG-07 |
| D-53 | El contacto son links a Instagram y WhatsApp en el encabezado de cada página. | Es como los contactan hoy. Cierra P-27. | Doc 07, 7.1 |
| D-54 | El consentimiento del cliente se pide para cualquier publicación, sea trabajo o blog. | Ya está implementado, es una sola casilla en el panel y protege al estudio. | RN-BLG-03 |
| D-55 | En la versión 1, los estudios y los usuarios los da de alta el desarrollador por consola. | Para un solo estudio piloto no hace falta registro. Cierra P-19. Aplicada en el corte 1. | Doc 07 |
| D-56 | Dos aplicaciones de front, el sitio y el panel, en el mismo repositorio. | El sitio queda liviano y sin login; el panel crece hasta ser la app del MVP. Cierra P-28. Aplicada en el corte 0. | Doc 07, 10 |
| D-57 | Caché del sitio público: la caché de salida de ASP.NET Core, en memoria, guarda las páginas y las publicaciones y se vacía entera con cualquier cambio desde el panel. Para el navegador, los datos se guardan un minuto (`public, max-age=60`) y las fotos una hora (`public, max-age=3600`), que también guarda Cloudflare. Algo que se oculta deja de verse en el sitio en hasta un minuto, y su foto, por link directo, hasta una hora. | Con una sola instancia alcanza la memoria: Redis queda para cuando haya varias (doc 06). A esta escala, vaciar todo es lo más simple y nunca deja nada viejo. Los tiempos equilibran lo que tarda en desaparecer algo oculto con no pedirle todo al servidor en cada visita. Se invalida en un filtro de la API, así Application no depende de ASP.NET. | Doc 07, 10 |

---

## 2. Glosario

> **Atención:** «En proceso», «Finalizado» y «Cancelado» existen como estados del turno y del tatuaje, con significados distintos. Indicar siempre a cuál se refiere.

| Término | Definición |
| --- | --- |
| Estudio | Cliente de la plataforma (tenant). Sus datos están aislados de los de otros estudios. |
| Usuario | Cuenta de acceso de una persona del estudio. Ocupa un asiento. |
| Asiento | Unidad de facturación: un usuario. |
| Dueño | Rol superusuario; puede acumular los perfiles de Tatuador y Administrativo. |
| Tatuador | Rol exclusivo de quien tatúa. |
| Administrativo | Rol exclusivo de gestión operativa, sin acceso a acumulados financieros. |
| Artista | En el blog, un usuario con perfil de Tatuador que tiene su propia página pública. |
| Visitante | Persona sin cuenta que ve las páginas públicas. Existe desde la versión 1 del blog. |
| Cliente | Persona que se tatúa o consulta. Pertenece al estudio, no a un tatuador. |
| Tatuaje | Proyecto sobre un cliente; puede requerir varias sesiones. Estados: En Proceso, Finalizado, Cancelado. |
| Sesión | Instancia concreta de trabajo sobre un tatuaje, con fecha real, monto cobrado y fotos de avance. |
| Turno | Bloque agendado en el calendario, originado por una seña. Estados: En espera, En proceso, Finalizado, Cancelado, Indefinido. |
| Seña | Pago anticipado, único y no reembolsable que origina un turno. Es un porcentaje del monto total, vence según el plazo del estudio (por defecto, 14 días) y se puede transferir hasta el máximo configurado (por defecto, 2 veces). |
| Walk-in | Sesión sin turno previo ni seña: el cliente llega sin agendar. |
| Reagendamiento | Cambio de fecha u horario del mismo turno. Consume una de las transferencias permitidas de la seña. |
| Indefinido | Estado automático del turno que indica que la seña se perdió. |
| Tatuador principal | Tatuador al que pertenece un turno. |
| Tatuador responsable | Tatuador a cargo de un tatuaje; define los porcentajes de los co-tatuadores. |
| Co-tatuador | Tatuador que colabora en el turno o el tatuaje de otro y recibe un porcentaje. |
| Bloqueo de co-tatuación | Franja del calendario del co-tatuador en la que no puede agendar turnos nuevos. |
| Ficha completa | Tatuaje con todos los campos obligatorios cargados (RN-TAT-06). |
| Tatuajes incompletos | Pestaña fija del Administrativo con las fichas que faltan completar. |
| Monto acordado | Precio pactado al agendar. Vive en el turno y no cambia. |
| Monto total | Precio vigente del tatuaje. Se hereda del turno y puede renegociarse. |
| Monto cobrado | Lo que se cobra en cada sesión, aparte de la seña. |
| Saldo pendiente | Monto total − seña − montos cobrados. Se calcula, no se guarda. |
| Configuración del estudio | Parámetros de negocio propios de cada estudio, como el plazo de la seña, el máximo de reagendamientos y el máximo de fotos por publicación, con valores por defecto. |
| Página pública | Página del sitio de un estudio o de un artista, con su encabezado, sus publicaciones y su «Sobre». |
| Dirección pública | La parte de la dirección web que identifica una página, por ejemplo `/inkmortal`. Es única en toda la plataforma. |
| Publicación | Contenido de una página pública, con fotos y texto. Tipos: Trabajo (pestaña Trabajos) y Blog (pestaña Blog). Estados: Borrador, Publicada y Oculta. |
| Portada | La primera foto de una publicación. Es la que se ve en la grilla y en las vistas previas. |
| Fijada | Publicación que aparece primero en su página, como las publicaciones fijadas de Instagram. |
| Consentimiento de difusión | Confirmación de que el cliente aceptó que se publiquen las fotos. Se registra quién la confirmó y cuándo. |
| Versión | Marca que cambia cada vez que se guarda una publicación. El panel la manda al guardar: si alguien guardó antes, el cambio se rechaza en lugar de pisar el otro. |
| Puerto | Interfaz que define el sistema para comunicarse con algo externo: pagos, mensajería o archivos. |
| Adaptador | Implementación de un puerto para un proveedor concreto, como Mercado Pago o WhatsApp. |
| Outbox | Tabla donde se guardan los mensajes salientes en la misma transacción, para enviarlos después con reintentos. |
| Webhook | Aviso que un proveedor externo envía al sistema cuando ocurre algo, por ejemplo un pago aprobado. |
| Row-Level Security | Filtro del propio SQL Server que limita las filas que ve cada consulta al estudio del usuario. Es la segunda línea de defensa del aislamiento. |
| UTC | Hora universal en la que se guardan las fechas; se convierte a la hora del estudio para mostrarla. |

---

## 3. Pendientes

| ID | Tema | Detalle | Estado |
| --- | --- | --- | --- |
| P-01 | Modelo de datos | Revisión general: el modelo actual no convence. Ver R-01 a R-16 (doc 04). | Abierto |
| P-02 | Cancelado y reagendamiento | Confirmar que un turno Cancelado se reactiva reagendándolo (consume un reagendamiento) y que, si no se reactiva, pasa a Indefinido al vencer la seña. Surge de combinar D-23 con D-24. | Abierto |
| P-03 | Plazo de la seña frente a la fecha del turno | ¿La fecha del turno y la de cada reagendamiento deben caer dentro del plazo de la seña? Si no, el turno vencería antes de realizarse. | Abierto |
| P-04 | Disparadores de estado del turno | Confirmar que En espera → En proceso y En proceso → Finalizado son manuales (la detección de ausencias depende de eso) y quién los marca. | Abierto |
| P-05 | Horarios del turno | Definir si la hora de fin es estimada (inicio + duración aproximada) o real (se registra al terminar). | Abierto |
| P-06 | Señas en tatuajes de varias sesiones | ¿Cada turno lleva seña? ¿El porcentaje se calcula sobre el total del tatuaje o sobre el valor de la sesión? Generalizar la fórmula del saldo para sumar todas las señas. | Abierto |
| P-07 | Continuación de un tatuaje | Flujo para agregar sesiones a un tatuaje En Proceso desde un turno nuevo o un walk-in. | Abierto |
| P-08 | Disponibilidad del co-tatuador | Confirmar la interpretación de RN-COT-03: libre al inicio del turno, respetando sus turnos preexistentes. | Abierto |
| P-09 | Matriz de permisos | Completar las celdas «A definir» (doc 01), incluido qué puede editar un co-tatuador. | Abierto |
| P-10 | Parametrización por estudio | Plazo de la seña y máximo de reagendamientos configurables por estudio, con 14 días y 2 por defecto. | Resuelto (D-34) |
| P-11 | Interfaz de Agenda | Pantalla de registro de seña y creación de turno, detalle del turno, cómo se ven los bloqueos y los walk-ins en la vista mensual, filtros. | Abierto |
| P-12 | Validaciones de agenda | Superposición de turnos, a prueba de dos usuarios agendando a la vez, y demás validaciones del calendario. | Abierto |
| P-13 | Módulo Clientes | Campos de la ficha, datos médicos, consentimiento informado, qué clientes ve un tatuador, búsqueda y filtros. Los datos de salud son sensibles según la Ley 25.326: definir qué se guarda y con qué consentimiento (doc 06, 7.3). | Abierto |
| P-14 | Requisitos no funcionales | Metas iniciales de capacidad, rendimiento, disponibilidad y backups definidas en el doc 06 (8.1). | Resuelto (D-44) |
| P-15 | Arquitectura y stack | Stack (D-33) y Clean Architecture como base (D-37). Detalle en el doc 06. | Resuelto |
| P-16 | Aislamiento multi-tenant | Base compartida con EstudioId, filtros de EF Core y Row-Level Security (doc 06, sección 4). | Resuelto (D-42) |
| P-17 | Puntos a confirmar del doc 06 | Validación completa: monolito modular, sesión, interfaz, doble factor, Row-Level Security, fechas y metas (doc 06, sección 11). | Resuelto (D-38 a D-45) |
| P-18 | Hosting y dominio | Urgente: sin esto, el blog no está online el 17/11. Presupuesto: hasta 30.000 ARS por mes. Opciones: servidor alquilado (VPS) con SQL Server Express, más barato y con mantenimiento propio, o servicios gestionados, más cómodos y más caros. La cuota gratuita de Azure SQL no alcanza para el uso diario (ver planilla). El dominio es de la plataforma, con una ruta por página (D-52): falta elegir el nombre. | Abierto |
| P-19 | Alta de usuarios en la versión 1 | ¿Alcanza con que el desarrollador cree el estudio piloto y sus usuarios? | Resuelto (D-55) |
| P-20 | Panel para el celular | El doc 07 1.0 lo citaba sin listarlo en sus preguntas. Queda confirmado: el panel se diseña primero para el celular. | Resuelto (doc 07, 8) |
| P-21 | ¿Quién publica en el blog? | Solo los Dueños, o cada tatuador sus propios trabajos, directo o con aprobación. | Resuelto (D-51) |
| P-22 | Diseños disponibles | ¿Muestran precio? ¿En qué moneda? ¿Son únicos o se pueden tatuar varias veces? | Postergado: fuera de la versión 1 (D-48) |
| P-23 | Próximas fechas | Sección para anunciar viajes, con ciudad y fechas. | Postergado: fuera de la versión 1 (D-48) |
| P-24 | Dirección del sitio | ¿Ruta dentro de la plataforma o dominio propio desde el inicio? | Resuelto (D-52) |
| P-25 | Marca de agua | ¿Las fotos llevan marca de agua? | Abierto |
| P-26 | Permiso de los clientes | ¿Cómo se registra hoy el permiso de los clientes para publicar sus fotos? ¿Alcanza con lo que ya publican en Instagram? Mientras tanto, el panel pide confirmarlo con una casilla (RN-BLG-03). | Abierto |
| P-27 | Contacto | ¿Solo WhatsApp, o también Instagram y un formulario? | Resuelto (D-53) |
| P-28 | Aplicaciones de front | ¿Dos aplicaciones (sitio y panel) o una sola? | Resuelto (D-56) |
| P-29 | Artista sin estudio | Cómo se modela un artista independiente cuando haya alta por autoservicio. Hoy sería un estudio de una sola persona. | Abierto (después de la versión 1) |
| P-30 | Moderación de páginas de artistas | ¿Un Dueño puede ocultar contenido de la página de un artista de su estudio? Hoy no: cada página es solo de su artista (D-51). | Abierto (cuando haya artistas que no sean Dueños) |

---

## 4. Próximos pasos sugeridos

1. Ajustar el backend al alcance del doc 07, versión 2.0: tipo de publicación, texto de 2200 caracteres, permisos por página, páginas públicas y endpoints públicos.
2. Armar un wireframe navegable del sitio y del panel. Con eso aprobado, hacer el diseño visual de cada página.
3. Construir el panel y el sitio público.
4. Definir el hosting y el dominio (P-18) y hacer el primer despliegue.
5. Actualizar los documentos 01 y 06 con el alcance del blog.
6. Después del 17/11: retomar el MVP. Con las respuestas del estudio piloto, cerrar P-02 a P-08 y P-13, revisar el modelo de datos (P-01) y completar la matriz de permisos (P-09).

---

## 5. Historial de versiones

| Versión | Fecha | Cambios |
| --- | --- | --- |
| 1.4 | 29/10/2026 | Decisión D-57: la caché del sitio público y cuánto puede seguir viéndose algo después de ocultarlo. |
| 1.3 | 16/10/2026 | Cierre de las preguntas del documento 07: decisiones D-46 a D-56 (blog como primera entrega, alcance tipo Instagram, páginas por estudio y por artista, una sola entidad Publicación, permisos por página, dirección por página, contacto, consentimiento, alta por consola y dos aplicaciones de front). Se suman P-19 a P-30: se resuelven P-19, P-20, P-21, P-24, P-27 y P-28, se postergan P-22 y P-23, y quedan abiertos P-25, P-26, P-29 y P-30. P-18 suma el presupuesto. Glosario del blog. Pasa a Markdown, en `docs/` del repositorio. |
| 1.2 | 4/10/2026 | Validación del documento 06: decisiones D-38 a D-45 (monolito modular, sesión con cookie, Tailwind con shadcn/ui, doble factor fuera del MVP, multi-tenancy con Row-Level Security, fechas en UTC con hora argentina, metas medibles y aprobación del resto del stack). Se cierran P-14, P-16 y P-17. Se actualizan los documentos 01, 04 y 06 y la planilla. |
| 1.1 | 4/10/2026 | Se suma el documento 06 (stack y arquitectura). Decisiones D-33 a D-37: stack, parámetros configurables por estudio, calidad sobre velocidad, patrones de diseño y Clean Architecture. Se cierran P-10 y P-15, P-16 pasa a propuesta y se suman P-17 y P-18. Se actualizan los documentos 01, 03 y 04. |
| 1.0 | 3/10/2026 | Consolidación inicial en cinco documentos: visión y roles, reglas generales, módulo Agenda y Turnos (incluye las cuatro definiciones aprobadas: Cancelado frente a Indefinido, reagendamiento con contador, ubicación de los montos y visibilidad al hacer clic), modelo de datos en borrador, decisiones, glosario y pendientes. Reemplaza a especificacion_saas_estudio_tatuajes.md y al resumen en Word. |
