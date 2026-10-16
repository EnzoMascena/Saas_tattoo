namespace Tato.Domain.Tests.Portfolio;

using Tato.Domain.Portfolio;

/// <summary>
/// Arma publicaciones para los tests con lo mínimo que cada uno necesita (patrón Builder, doc 06, 9).
/// Usa los métodos del dominio, así una publicación de prueba nunca saltea una regla.
/// </summary>
internal sealed class PublicacionBuilder
{
    /// <summary>
    /// Hora fija para los tests, en UTC (D-43).
    /// </summary>
    public static readonly DateTimeOffset Ahora = new(2026, 10, 11, 15, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// El máximo de fotos por defecto de RN-BLG-04.
    /// </summary>
    public const int MaxFotos = 10;

    private Guid _estudioId = Guid.NewGuid();
    private Guid _creadaPor = Guid.NewGuid();
    private DatosPublicacion _datos = new("Rosa en el antebrazo");
    private int _fotos;
    private bool _consentimiento;
    private EstadoPublicacion _estado = EstadoPublicacion.Borrador;

    public PublicacionBuilder DelEstudio(Guid estudioId)
    {
        _estudioId = estudioId;
        return this;
    }

    public PublicacionBuilder CreadaPor(Guid usuarioId)
    {
        _creadaPor = usuarioId;
        return this;
    }

    public PublicacionBuilder ConDatos(DatosPublicacion datos)
    {
        _datos = datos;
        return this;
    }

    public PublicacionBuilder ConFotos(int cantidad)
    {
        _fotos = cantidad;
        return this;
    }

    public PublicacionBuilder ConConsentimiento()
    {
        _consentimiento = true;
        return this;
    }

    /// <summary>
    /// Publicada con lo que exigen las reglas: al menos una foto y el consentimiento.
    /// </summary>
    public PublicacionBuilder Publicada()
    {
        _fotos = Math.Max(_fotos, 1);
        _consentimiento = true;
        _estado = EstadoPublicacion.Publicada;
        return this;
    }

    public PublicacionBuilder Oculta()
    {
        _estado = EstadoPublicacion.Oculta;
        return this;
    }

    public Publicacion Build()
    {
        var publicacion = Publicacion.Crear(_estudioId, _creadaPor, _datos, Ahora);

        for (var i = 0; i < _fotos; i++)
            publicacion.AgregarFoto(1600, 1200, MaxFotos, Ahora);

        if (_consentimiento)
            publicacion.ConfirmarConsentimiento(_creadaPor, Ahora);

        if (_estado == EstadoPublicacion.Publicada)
            publicacion.Publicar(Ahora);
        else if (_estado == EstadoPublicacion.Oculta)
            publicacion.Ocultar(Ahora);

        return publicacion;
    }
}
