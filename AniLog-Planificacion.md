# AniLog — Planificacion del Proyecto

> API RESTful en .NET 8 + React | Proyecto portfolio para puesto Jr

---

## Que es AniLog?

AniLog es una aplicacion web para llevar un historial personal de animes. El usuario puede buscar animes, agregarlos a su lista, registrar su progreso y calificarlos. Los datos del anime (titulo, generos, imagen, score) se obtienen automaticamente desde **Jikan API** (wrapper oficial de MyAnimeList, gratuita y sin API key).

### Por que este proyecto funciona para una entrevista Jr?

- Demuestra consumo de **API externa real** (no datos inventados)
- Muestra un **diseno de endpoints REST limpio y semantico**
- Usa **PostgreSQL con Entity Framework Core** (ORM profesional)
- Tiene una razon de ser real, no es un CRUD generico
- El stack (.NET 8 + React) es moderno y muy demandado
- El alcance es realista: suficiente para demostrar habilidad, sin sobreingenieria

### Filosofia del proyecto

**Simple pero solido.** No necesita ser perfecto ni ultra-profesional. Necesita:
- Funcionar de punta a punta (buscar, agregar, editar, eliminar)
- Estar bien estructurado (separacion de responsabilidades)
- Tener codigo legible y consistente
- Poder explicar cada decision tomada en una entrevista

---

## Stack tecnologico

| Capa | Tecnologia | Justificacion |
|---|---|---|
| Backend | ASP.NET Core 8 Web API | Framework moderno, alto rendimiento |
| ORM | Entity Framework Core 8 | Estandar en el ecosistema .NET |
| Base de datos | PostgreSQL | Robusta, profesional, muy usada en la industria |
| API externa | Jikan v4 (MyAnimeList) | Gratuita, sin key, REST pura |
| HTTP Client | IHttpClientFactory | Patron recomendado en .NET para HTTP |
| Documentacion API | Swagger / Scalar | Explorar y probar endpoints visualmente |
| Frontend | React + Vite | Rapido, moderno, ampliamente usado |
| Estilos | Tailwind CSS | Productividad y diseno consistente |

---

## Estructura del proyecto

```
AniLog/
├── AniLog.API/                        <- Backend .NET 8
│   ├── Controllers/
│   │   ├── AnimeController.cs         <- CRUD del historial personal
│   │   └── SearchController.cs        <- Busqueda en Jikan API
│   ├── Models/
│   │   ├── AnimeLog.cs                <- Entidad de la base de datos
│   │   └── AnimeStatus.cs             <- Enum: Watching, Completed, Dropped, PlanToWatch
│   ├── DTOs/
│   │   ├── AddAnimeDto.cs             <- Lo que recibe la API al crear
│   │   ├── UpdateAnimeDto.cs          <- Lo que recibe la API al actualizar
│   │   └── AnimeResponseDto.cs        <- Lo que devuelve la API
│   ├── Services/
│   │   ├── AnimeLogService.cs         <- Logica de negocio
│   │   └── JikanService.cs            <- Consumo de Jikan API
│   ├── Data/
│   │   └── AppDbContext.cs            <- EF Core DbContext
│   ├── Migrations/                    <- Generadas automaticamente con EF
│   ├── appsettings.json               <- Connection string PostgreSQL
│   └── Program.cs                     <- Configuracion de la app
│
├── AniLog.Frontend/                   <- React + Vite
│   ├── src/
│   │   ├── components/
│   │   │   ├── AnimeCard.jsx          <- Tarjeta individual de anime
│   │   │   ├── AnimeList.jsx          <- Lista con filtros por status
│   │   │   ├── SearchBar.jsx          <- Busqueda con debounce
│   │   │   └── AddAnimeModal.jsx      <- Modal para agregar anime
│   │   ├── services/
│   │   │   └── api.js                 <- Todas las llamadas al backend
│   │   ├── App.jsx
│   │   └── main.jsx
│   └── package.json
│
├── AniLog-Planificacion.md
└── README.md
```

**Nota sobre el frontend:** No usamos React Router. Una sola pagina (SPA sin rutas) es suficiente para este alcance. No complicar con routing innecesario.

**Nota sobre estado:** `useState` + props es suficiente. No necesitamos Context, Redux ni ninguna libreria de estado global.

---

## Modelo de datos

### Tabla `anime_logs` en PostgreSQL

| Campo | Tipo | Descripcion | Restricciones |
|---|---|---|---|
| `Id` | int (PK) | Identificador autoincremental | Auto-generado |
| `MalId` | int | ID del anime en MyAnimeList / Jikan | **UNIQUE** (evita duplicados) |
| `Title` | string | Titulo en ingles | Required |
| `TitleJapanese` | string | Titulo en japones | Nullable |
| `Genres` | string | Ej: "Action, Shounen, Adventure" | Nullable |
| `Episodes` | int? | Total de episodios | Null si esta en emision |
| `MalScore` | decimal | Score oficial de MyAnimeList | |
| `ImageUrl` | string | URL del poster del anime | |
| `MyScore` | decimal | Calificacion personal | Rango 0-10 |
| `MyStatus` | enum (string) | Watching / Completed / Dropped / PlanToWatch | Required, guardado como string en DB |
| `EpisodesWatched` | int | Cuantos episodios llevas | Default 0, no puede superar Episodes |
| `MyNotes` | string? | Notas personales opcionales | Nullable |
| `AddedAt` | DateTime | Fecha en que se agrego | Auto-generado (DateTime.UtcNow) |

### Decisiones de diseno del modelo

- **`MalId` es UNIQUE:** Si el usuario intenta agregar un anime que ya esta en su lista, la API devuelve error 409 Conflict. Esto es mas limpio que permitir duplicados.
- **`Genres` como string comma-separated:** Para este alcance es suficiente. En produccion se normalizaria a una tabla M:N. Si preguntan en entrevista, esa es la respuesta.
- **`MyStatus` como string en DB:** Se configura con `.HasConversion<string>()` en EF Core. Asi los datos son legibles directamente en PostgreSQL (en vez de ver numeros 0, 1, 2, 3).

### Enum AnimeStatus

```csharp
public enum AnimeStatus
{
    Watching,
    Completed,
    Dropped,
    PlanToWatch
}
```

---

## Endpoints de la API

### Historial personal — `/api/anime`

| Metodo | Ruta | Descripcion | Request Body | Response |
|---|---|---|---|---|
| `GET` | `/api/anime` | Obtiene todo el historial | — | `AnimeResponseDto[]` |
| `GET` | `/api/anime?status=watching` | Filtra por estado | — | `AnimeResponseDto[]` |
| `GET` | `/api/anime/{id}` | Detalle de una entrada | — | `AnimeResponseDto` |
| `POST` | `/api/anime` | Agrega un anime al historial | `AddAnimeDto` | `AnimeResponseDto` (201) |
| `PUT` | `/api/anime/{id}` | Actualiza progreso, score o notas | `UpdateAnimeDto` | `AnimeResponseDto` |
| `DELETE` | `/api/anime/{id}` | Elimina del historial | — | 204 No Content |

### Busqueda — `/api/search`

| Metodo | Ruta | Descripcion | Response |
|---|---|---|---|
| `GET` | `/api/search?q=naruto` | Busca animes en Jikan API | Lista simplificada de resultados |

### Decisiones sobre los endpoints

- **PUT en vez de PATCH:** PATCH requiere JsonPatchDocument en .NET, lo cual agrega complejidad innecesaria. PUT con UpdateAnimeDto (todos los campos opcionales) logra lo mismo de forma mas simple. Nadie va a cuestionar esto en una entrevista Jr.
- **201 en POST:** Devolver 201 Created con el objeto creado es la practica REST correcta.
- **204 en DELETE:** No hace falta devolver body al eliminar.
- **Filtro por query param:** `?status=watching` es mas REST-friendly que un endpoint separado.

### Codigos de error que maneja la API

| Codigo | Cuando |
|---|---|
| 400 Bad Request | Datos invalidos (score fuera de rango, etc.) |
| 404 Not Found | Anime no existe en la lista o no se encuentra en Jikan |
| 409 Conflict | Intentar agregar un anime que ya esta en la lista (MalId duplicado) |
| 503 Service Unavailable | Jikan API no responde o devuelve rate limit (429) |

---

## Flujo principal: agregar un anime

```
Usuario escribe "Attack on Titan" en SearchBar
        |
        v
Frontend llama GET /api/search?q=attack+on+titan
        |
        v
SearchController -> JikanService.SearchAnimeAsync()
        |
        v
JikanService -> GET https://api.jikan.moe/v4/anime?q=attack+on+titan
        |
        v
Se devuelven resultados simplificados al frontend
        |
        v
Usuario selecciona un anime -> se abre AddAnimeModal
Usuario elige: status = "Completed", score = 9.5
        |
        v
Frontend llama POST /api/anime { "malId": 16498, "myStatus": "Completed", "myScore": 9.5 }
        |
        v
AnimeController -> AnimeLogService.AddAnimeAsync()
        |
        v
1. Verifica que MalId no exista ya en DB (si existe -> 409 Conflict)
2. JikanService.GetAnimeByIdAsync(16498) -> obtiene titulo, generos, imagen, episodios, score oficial
3. Combina datos de Jikan + datos del usuario en entidad AnimeLog
4. Guarda en PostgreSQL via EF Core
        |
        v
API responde 201 con AnimeResponseDto completo
        |
        v
Frontend agrega el anime a la lista visible
```

---

## Jikan API — Lo que necesitas saber

### Base URL
```
https://api.jikan.moe/v4/
```

### Endpoints que usamos

**Buscar anime:**
```
GET https://api.jikan.moe/v4/anime?q=naruto&limit=10
```

**Obtener anime por ID:**
```
GET https://api.jikan.moe/v4/anime/16498
```

### Estructura de respuesta de Jikan (simplificada)

```json
{
  "data": [
    {
      "mal_id": 16498,
      "title": "Shingeki no Kyojin",
      "title_english": "Attack on Titan",
      "title_japanese": "進撃の巨人",
      "episodes": 25,
      "score": 8.54,
      "images": {
        "jpg": {
          "image_url": "https://cdn.myanimelist.net/images/anime/10/47347.jpg"
        }
      },
      "genres": [
        { "mal_id": 1, "name": "Action" },
        { "mal_id": 8, "name": "Drama" }
      ]
    }
  ]
}
```

### Rate Limiting

Jikan tiene rate limit: **3 requests/segundo, 60 requests/minuto.**

**Como lo manejamos:** En JikanService, si Jikan devuelve HTTP 429, nuestro API devuelve 503 con un mensaje claro. El frontend muestra "Servicio temporalmente no disponible, intenta en unos segundos." No implementamos retry automatico — es innecesario para este alcance.

### Clases para deserializar Jikan

Necesitas estas clases en el proyecto (pueden ir en un archivo `Models/JikanResponses.cs` o en una carpeta `Models/Jikan/`):

```csharp
public class JikanSearchResponse
{
    [JsonPropertyName("data")]
    public List<JikanAnimeData> Data { get; set; }
}

public class JikanSingleResponse
{
    [JsonPropertyName("data")]
    public JikanAnimeData Data { get; set; }
}

public class JikanAnimeData
{
    [JsonPropertyName("mal_id")]
    public int MalId { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("title_english")]
    public string? TitleEnglish { get; set; }

    [JsonPropertyName("title_japanese")]
    public string? TitleJapanese { get; set; }

    [JsonPropertyName("episodes")]
    public int? Episodes { get; set; }

    [JsonPropertyName("score")]
    public decimal? Score { get; set; }

    [JsonPropertyName("images")]
    public JikanImages Images { get; set; }

    [JsonPropertyName("genres")]
    public List<JikanGenre> Genres { get; set; }
}

public class JikanImages
{
    [JsonPropertyName("jpg")]
    public JikanJpg Jpg { get; set; }
}

public class JikanJpg
{
    [JsonPropertyName("image_url")]
    public string ImageUrl { get; set; }
}

public class JikanGenre
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
}
```

**Por que tantas clases?** Jikan devuelve JSON anidado. Necesitas una clase por cada nivel de anidamiento para que `System.Text.Json` pueda deserializar. Esto es normal y esperado — no intentes usar `dynamic` o `JsonElement` manual, es mas sucio y fragil.

---

## Implementacion paso a paso

### FASE 1: Setup y Base de Datos

**Objetivo:** Tener el proyecto .NET corriendo con PostgreSQL conectada y la tabla creada.

**Prerequisitos:**
- [ ] .NET 8 SDK instalado (`dotnet --version`)
- [ ] PostgreSQL instalado y corriendo (`psql --version`, `systemctl status postgresql`)
- [ ] EF Core tools instalado (`dotnet tool install --global dotnet-ef`)

**Pasos:**

**1.1 — Inicializar repositorio git**
```bash
cd AniLog
git init
```
Crear `.gitignore` con las exclusiones de .NET y Node (usar `dotnet new gitignore` o template de GitHub).

**1.2 — Crear proyecto .NET**
```bash
dotnet new webapi -n AniLog.API
cd AniLog.API
```
Borrar `WeatherForecast.cs` y `Controllers/WeatherForecastController.cs` (son de ejemplo).

**1.3 — Instalar paquetes NuGet**
```bash
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.Design
```
Nota: Swashbuckle ya viene incluido en el template de .NET 8 webapi.

**1.4 — Crear el enum AnimeStatus**
Archivo: `Models/AnimeStatus.cs`

**1.5 — Crear el modelo AnimeLog**
Archivo: `Models/AnimeLog.cs`
- Incluir todos los campos de la tabla
- Data Annotations basicas: `[Required]`, `[Range(0, 10)]` en MyScore

**1.6 — Crear AppDbContext**
Archivo: `Data/AppDbContext.cs`
- Registrar `DbSet<AnimeLog>`
- En `OnModelCreating`: configurar MalId como UNIQUE, MyStatus como string
```csharp
modelBuilder.Entity<AnimeLog>(entity =>
{
    entity.HasIndex(e => e.MalId).IsUnique();
    entity.Property(e => e.MyStatus).HasConversion<string>();
});
```

**1.7 — Configurar Program.cs**
- Registrar DbContext con connection string de PostgreSQL
- Verificar que la connection string en `appsettings.json` sea correcta

**1.8 — Crear y aplicar migracion**
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

**Verificacion:** Conectarte a PostgreSQL con `psql` o pgAdmin y confirmar que la tabla `anime_logs` existe con todas las columnas.

**Posibles problemas:**
- PostgreSQL no esta corriendo -> `sudo systemctl start postgresql`
- Password incorrecto en connection string -> verificar con `psql -U postgres`
- EF tools no instalado -> `dotnet tool install --global dotnet-ef`

**Commit:** `feat: initial project setup with PostgreSQL and EF Core`

---

### FASE 2: Jikan Service y Endpoint de Busqueda

**Objetivo:** Poder buscar animes desde Swagger y ver resultados de Jikan.

**2.1 — Crear clases de deserializacion de Jikan**
Archivo: `Models/JikanResponses.cs`
Todas las clases listadas arriba (JikanSearchResponse, JikanAnimeData, etc.)

**2.2 — Crear JikanService**
Archivo: `Services/JikanService.cs`

```csharp
public class JikanService
{
    private readonly HttpClient _httpClient;

    public JikanService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<JikanAnimeData>> SearchAnimeAsync(string query)
    {
        // GET anime?q={query}&limit=10
        // Deserializar JikanSearchResponse
        // Retornar la lista Data
    }

    public async Task<JikanAnimeData?> GetAnimeByIdAsync(int malId)
    {
        // GET anime/{malId}
        // Deserializar JikanSingleResponse
        // Retornar Data
    }
}
```

Manejar el caso de respuesta no exitosa (especialmente 429 rate limit).

**2.3 — Registrar HttpClient en Program.cs**
```csharp
builder.Services.AddHttpClient<JikanService>(client =>
{
    client.BaseAddress = new Uri("https://api.jikan.moe/v4/");
});
```

**2.4 — Crear SearchController**
Archivo: `Controllers/SearchController.cs`
- Un solo endpoint: `GET /api/search?q=texto`
- Inyecta JikanService
- Devuelve la lista de resultados

**2.5 — Probar en Swagger**
- `dotnet run`
- Abrir Swagger en el navegador
- Probar `GET /api/search?q=naruto`
- Verificar que devuelve datos reales de Jikan

**Posibles problemas:**
- Deserializacion falla (propiedades null) -> Verificar que los `[JsonPropertyName]` coincidan exactamente con el JSON de Jikan
- Timeout en la request -> Jikan puede tardar 1-3 segundos, es normal
- 429 Too Many Requests -> Esperar unos segundos y reintentar manualmente

**Commit:** `feat: add Jikan API integration and search endpoint`

---

### FASE 3: DTOs y CRUD Completo

**Objetivo:** API completamente funcional con todos los endpoints.

**3.1 — Crear AddAnimeDto**
Archivo: `DTOs/AddAnimeDto.cs`
```csharp
public class AddAnimeDto
{
    [Required]
    public int MalId { get; set; }

    [Required]
    public AnimeStatus MyStatus { get; set; }

    [Range(0, 10)]
    public decimal MyScore { get; set; } = 0;

    [Range(0, int.MaxValue)]
    public int EpisodesWatched { get; set; } = 0;

    public string? MyNotes { get; set; }
}
```

**3.2 — Crear UpdateAnimeDto**
Archivo: `DTOs/UpdateAnimeDto.cs`
Todos los campos opcionales (nullable). El service solo actualiza los que vienen con valor.

**3.3 — Crear AnimeResponseDto**
Archivo: `DTOs/AnimeResponseDto.cs`
Todos los campos que el frontend necesita mostrar. Nunca devolver la entidad directamente.

**3.4 — Crear AnimeLogService (la pieza mas importante)**
Archivo: `Services/AnimeLogService.cs`

Metodos:
- `GetAllAsync(AnimeStatus? status)` — query a DB, opcionalmente filtrado
- `GetByIdAsync(int id)` — buscar por PK
- `AddAnimeAsync(AddAnimeDto dto)` — **este es el mas complejo:**
  1. Verificar que MalId no exista en DB -> si existe, lanzar excepcion/retornar error
  2. Llamar a JikanService.GetAnimeByIdAsync(dto.MalId)
  3. Combinar datos de Jikan + datos del usuario
  4. Crear entidad AnimeLog y guardar en DB
  5. Retornar AnimeResponseDto
- `UpdateAnimeAsync(int id, UpdateAnimeDto dto)` — buscar entidad, actualizar campos, guardar
- `DeleteAnimeAsync(int id)` — buscar y eliminar

**3.5 — Crear AnimeController**
Archivo: `Controllers/AnimeController.cs`
- `[ApiController]` y `[Route("api/anime")]`
- Inyecta AnimeLogService
- Cada endpoint delega al service y retorna el ActionResult apropiado

**3.6 — Configurar CORS en Program.cs**
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// MAS ABAJO, despues de var app = builder.Build();
app.UseCors("AllowFrontend");  // ANTES de app.MapControllers()
```

**Importante:** El orden en el pipeline importa. `UseCors` debe ir ANTES de `MapControllers`.

**3.7 — Probar TODOS los endpoints en Swagger**

Flujo de prueba completo:
1. `GET /api/anime` -> deberia devolver lista vacia `[]`
2. `GET /api/search?q=naruto` -> verificar que trae resultados
3. `POST /api/anime` con `{ "malId": 20, "myStatus": "Completed", "myScore": 9 }` -> deberia devolver 201 con datos completos
4. `GET /api/anime` -> ahora deberia tener 1 elemento
5. `GET /api/anime/1` -> detalle del anime
6. `PUT /api/anime/1` con `{ "myScore": 8.5, "episodesWatched": 10 }` -> verificar que actualiza
7. `POST /api/anime` con el mismo MalId -> deberia devolver 409 Conflict
8. `DELETE /api/anime/1` -> 204
9. `GET /api/anime` -> lista vacia de nuevo
10. `GET /api/anime?status=watching` -> verificar filtro

**Posibles problemas:**
- CORS bloqueando requests -> verificar orden en Program.cs
- MalId unique constraint violation no manejada -> agregar try-catch o verificacion previa
- Jikan devuelve title_english como null para algunos animes -> usar title como fallback
- EpisodesWatched mayor que Episodes -> agregar validacion en el service

**Commit:** `feat: add CRUD endpoints for anime log`

---

### FASE 4: Frontend React

**Objetivo:** UI funcional que se conecta al backend. Priorizar funcionalidad sobre estetica.

**4.1 — Crear proyecto React + Vite**
```bash
cd AniLog  # volver a la raiz
npm create vite@latest AniLog.Frontend -- --template react
cd AniLog.Frontend
npm install
```

**4.2 — Instalar dependencias**
```bash
npm install axios
npm install -D tailwindcss @tailwindcss/vite
```
Configurar Tailwind segun docs oficiales para Vite:
- Agregar plugin en `vite.config.js`
- Agregar `@import "tailwindcss"` en `src/index.css`

**4.3 — Crear api.js (service layer)**
Archivo: `src/services/api.js`

```javascript
import axios from 'axios';

const API_URL = 'http://localhost:5062/api'; // ajustar puerto

export const searchAnime = (query) =>
    axios.get(`${API_URL}/search?q=${query}`);

export const getAllAnime = (status) =>
    axios.get(`${API_URL}/anime`, { params: status ? { status } : {} });

export const addAnime = (data) =>
    axios.post(`${API_URL}/anime`, data);

export const updateAnime = (id, data) =>
    axios.put(`${API_URL}/anime/${id}`, data);

export const deleteAnime = (id) =>
    axios.delete(`${API_URL}/anime/${id}`);
```

**4.4 — Crear SearchBar.jsx**
- Input de texto controlado
- Debounce con setTimeout en useEffect (300-500ms)
- Muestra resultados en un dropdown debajo del input
- Cada resultado tiene boton "Agregar" que abre el modal
- Si no hay resultados: "No se encontraron animes"
- Si esta cargando: mostrar texto "Buscando..."

**4.5 — Crear AddAnimeModal.jsx**
- Recibe el anime seleccionado (datos de Jikan) como prop
- Formulario con:
  - Select para MyStatus (Watching, Completed, Dropped, Plan to Watch)
  - Input numerico para MyScore (0-10)
  - Input numerico para EpisodesWatched
  - Textarea para MyNotes (opcional)
- Boton "Agregar" que llama a `addAnime()`
- Boton "Cancelar" que cierra el modal
- Mostrar imagen y titulo del anime seleccionado en el modal

**4.6 — Crear AnimeCard.jsx**
- Muestra: imagen (poster), titulo, generos, status (con color), score personal, episodios vistos/total
- Botones: "Editar" (abre modal de edicion inline o similar), "Eliminar" (con confirmacion)
- Diseno tipo tarjeta con Tailwind

**4.7 — Crear AnimeList.jsx**
- Botones de filtro arriba: All, Watching, Completed, Dropped, Plan to Watch
- Renderiza AnimeCards en grid
- Si la lista esta vacia: "No tienes animes en tu lista. Busca uno para empezar!"
- Loading state mientras carga

**4.8 — Armar App.jsx**
- Componer: SearchBar arriba, AnimeList abajo
- Estado principal en App: `animeList`, `selectedAnime`, `showModal`, `activeFilter`
- Funcion para refrescar la lista despues de agregar/editar/eliminar

**Flujo de datos:**
```
App.jsx (estado principal)
  ├── SearchBar -> busca -> selecciona anime -> abre AddAnimeModal
  ├── AddAnimeModal -> agrega -> refresca lista
  └── AnimeList -> muestra tarjetas -> editar/eliminar -> refresca lista
```

**Posibles problemas:**
- Tailwind no aplica estilos -> verificar configuracion en vite.config.js e index.css
- CORS error en el navegador -> verificar que el backend este corriendo y CORS configurado
- Puerto del backend diferente al esperado -> verificar en `Properties/launchSettings.json` del proyecto .NET
- Axios error no manejado -> agregar try-catch basico en cada llamada

**Commits incrementales por componente.**

---

### FASE 5: Pulido y Entrega

**Objetivo:** Proyecto presentable en GitHub.

**5.1 — Probar flujo completo end-to-end**
1. Abrir frontend y backend simultaneamente
2. Buscar un anime
3. Agregarlo con status y score
4. Verificar que aparece en la lista
5. Filtrar por status
6. Editar score y episodios
7. Eliminar un anime
8. Verificar que se refleja en la lista

**5.2 — Agregar estados de loading y error en el frontend**
- Spinner o texto "Cargando..." mientras se hacen requests
- Mensajes de error claros cuando algo falla
- No es necesario un sistema complejo: un simple estado `loading` y `error` por componente

**5.3 — Escribir README.md**

Estructura del README:
```markdown
# AniLog

Breve descripcion (2-3 lineas).

## Screenshots
(capturas de la app funcionando)

## Tech Stack
(lista con badges o tabla)

## Como correr el proyecto

### Prerequisitos
- .NET 8 SDK
- PostgreSQL
- Node.js 18+

### Backend
(comandos paso a paso)

### Frontend
(comandos paso a paso)

## Endpoints de la API
(tabla resumida)

## Arquitectura
(breve explicacion de la estructura)
```

**5.4 — Limpieza final**
- Quitar `console.log` del frontend
- Verificar que no haya passwords hardcodeados (usar variables de entorno o al menos documentar)
- Verificar que `.gitignore` excluye `node_modules`, `bin`, `obj`, `appsettings.Development.json`

**5.5 — (OPCIONAL) Docker Compose**
Solo si te sobra tiempo. Un `docker-compose.yml` que levante PostgreSQL + backend + frontend es un plus, pero no es necesario para Jr.

**Commit final:** `docs: add README with setup instructions`

---

## Prioridades si el tiempo es limitado

Si no puedes completar todo, este es el orden de importancia:

| Prioridad | Que | Por que |
|---|---|---|
| 1 | Backend completo funcionando (Fases 1-3) | Es lo que mas van a evaluar en una entrevista .NET |
| 2 | Frontend funcional (aunque sea basico) | Demuestra que sabes conectar las piezas |
| 3 | README bien escrito | Un reclutador mira el README antes que el codigo |
| 4 | UI bonita y pulida | Nice to have, no critico para Jr |
| 5 | Docker | Bonus, solo si sobra tiempo |

---

## Problemas comunes y soluciones rapidas

| Problema | Solucion |
|---|---|
| PostgreSQL no esta corriendo | `sudo systemctl start postgresql` |
| CORS bloquea requests desde React | Verificar que `app.UseCors()` va ANTES de `app.MapControllers()` |
| Jikan devuelve 429 (rate limit) | Devolver 503 al cliente, mostrar mensaje en frontend |
| Deserializacion de Jikan falla | Verificar `[JsonPropertyName]` coincida con el JSON real |
| EF Core migracion falla | Verificar connection string y que PostgreSQL este corriendo |
| Tailwind no aplica estilos | Verificar plugin en vite.config.js y `@import "tailwindcss"` en index.css |
| Puerto del backend cambia | Verificar `Properties/launchSettings.json` y actualizar api.js |
| title_english es null en Jikan | Usar `TitleEnglish ?? Title` como fallback |
| Estado del frontend no se actualiza | Despues de POST/PUT/DELETE, hacer GET para refrescar la lista |

---

## Que decir en la entrevista sobre este proyecto

**Si preguntan por que no usas AutoMapper:**
"Para el alcance del proyecto, el mapeo manual es mas explicito y facil de debuggear. AutoMapper lo usaria en proyectos mas grandes con muchas entidades."

**Si preguntan por que Genres es un string y no una tabla:**
"Es una decision de alcance. Para un proyecto personal es suficiente. En produccion lo normalizaria a una tabla muchos-a-muchos con un join table."

**Si preguntan por que no hay autenticacion:**
"Es una app single-user para uso personal. Agregar Identity/JWT seria sobreingenieria para este caso. Pero se como implementarlo si fuera necesario."

**Si preguntan por el rate limit de Jikan:**
"Lo manejo devolviendo un 503 al cliente con un mensaje claro. En produccion consideraria implementar caching de las busquedas o un rate limiter propio."

**Si preguntan por que PUT y no PATCH:**
"PUT es mas simple de implementar y para este caso donde el DTO tiene pocos campos, cumple el mismo objetivo. PATCH con JsonPatchDocument agregaria complejidad sin beneficio real aqui."

---

*Stack: .NET 8 · Entity Framework Core · PostgreSQL · Jikan API · React · Vite · Tailwind CSS*
