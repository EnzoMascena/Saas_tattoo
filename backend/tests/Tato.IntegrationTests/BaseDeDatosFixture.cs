using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Tato.Application.Estudios;
using Tato.Infrastructure.Persistencia;
using Testcontainers.MsSql;

namespace Tato.IntegrationTests;

/// <summary>
/// Base de datos para los tests de integración, con las migraciones reales aplicadas. Cada corrida usa una base
/// nueva, con nombre único, que se borra al terminar. Lo comparten todos los tests de una clase.
/// <list type="bullet">
/// <item>Si existe la variable de entorno TATO_TESTS_SQLSERVER, usa ese SQL Server (por ejemplo, el instalado en la máquina).</item>
/// <item>Si no, levanta en Docker un SQL Server 2025, la misma versión que producción (doc 06).</item>
/// </list>
/// </summary>
public sealed class BaseDeDatosFixture : IAsyncLifetime
{
    /// <summary>
    /// Cadena de conexión a un SQL Server existente. La base la elige el fixture: nunca usa una que ya exista.
    /// </summary>
    public const string VariableServidor = "TATO_TESTS_SQLSERVER";

    private MsSqlContainer? _contenedor;
    private string _cadenaConexion = string.Empty;

    /// <summary>
    /// La de la base de esta corrida, para levantar la API contra ella (ver ApiFixture).
    /// </summary>
    internal string CadenaConexion => _cadenaConexion;

    public async Task InitializeAsync()
    {
        var servidor = Environment.GetEnvironmentVariable(VariableServidor);

        if (string.IsNullOrWhiteSpace(servidor))
        {
            _contenedor = CrearContenedor();
            await _contenedor.StartAsync();
            servidor = _contenedor.GetConnectionString();
        }

        // Aunque la cadena traiga Database=TatoDev, se reemplaza: los tests nunca tocan una base existente.
        _cadenaConexion = new SqlConnectionStringBuilder(servidor)
        {
            InitialCatalog = $"TatoTests_{Guid.NewGuid():N}",
        }.ConnectionString;

        await using var contexto = CrearContexto(estudioId: null);
        await contexto.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (_cadenaConexion.Length > 0)
        {
            await using var contexto = CrearContexto(estudioId: null);
            await contexto.Database.EnsureDeletedAsync();
        }

        if (_contenedor is not null)
            await _contenedor.DisposeAsync();
    }

    /// <summary>
    /// Crea un contexto como si la sesión fuera de ese estudio. Sin estudio (null) es como un comando de consola.
    /// </summary>
    public TatoDbContext CrearContexto(Guid? estudioId)
    {
        var opciones = new DbContextOptionsBuilder<TatoDbContext>()
            .UseSqlServer(_cadenaConexion)
            .Options;

        return new TatoDbContext(opciones, new EstudioFijo(estudioId));
    }

    // El módulo espera a SQL Server con sqlcmd, que la imagen 2025 no trae: se reemplaza por una espera que se
    // conecta desde acá.
    private static MsSqlContainer CrearContenedor() =>
        new MsSqlBuilder("mcr.microsoft.com/mssql/server:2025-latest")
            .WithWaitStrategy(Wait.ForUnixContainer().AddCustomWaitStrategy(
                new AceptaConexiones(),
                espera => espera.WithTimeout(TimeSpan.FromMinutes(2))))
            .Build();

    /// <summary>
    /// Reemplaza a la sesión en los tests: siempre devuelve el estudio que recibe.
    /// </summary>
    private sealed class EstudioFijo : IEstudioActual
    {
        public EstudioFijo(Guid? estudioId)
        {
            EstudioId = estudioId;
        }

        public Guid? EstudioId { get; }
    }

    /// <summary>
    /// SQL Server está listo cuando acepta una conexión. Mientras arranca la conexión falla, y Testcontainers
    /// vuelve a probar cada un segundo hasta el tiempo máximo.
    /// </summary>
    private sealed class AceptaConexiones : IWaitUntil
    {
        public async Task<bool> UntilAsync(IContainer container)
        {
            var cadena = new SqlConnectionStringBuilder(((MsSqlContainer)container).GetConnectionString())
            {
                ConnectTimeout = 5,
                Pooling = false,
            }.ConnectionString;

            try
            {
                await using var conexion = new SqlConnection(cadena);
                await conexion.OpenAsync();
                return true;
            }
            catch (SqlException)
            {
                return false;
            }
        }
    }
}
