using Microsoft.Extensions.DependencyInjection;
using TecnoFuturo.Core.Entities;
using TecnoFuturo.Core.Repositories;
using TecnoFuturo.Core.DTOs;

public class CicloFormativoRepositoryLista : ICicloFormativoRepository
{
    private readonly IServiceProvider _serviceProvider;
    private readonly List<CicloFormativo> _ciclosFormativos = new();

    public CicloFormativoRepositoryLista(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IReadOnlyList<CicloFormativoDTO> ObtenerCiclosFormativos()
    {
        return [.. _ciclosFormativos.Select(ToMap)];
    }

    public IReadOnlyList<CicloFormativoDTO> ObtenerCiclosFormativosPorCentro(int centroId)
    {
        return [.. _ciclosFormativos.Where(c => c.CentroId == centroId).Select(ToMap)];
    }

    public CicloFormativoDTO? ObtenerCicloFormativoPorId(string id)
    {
        var ciclo = _ciclosFormativos.FirstOrDefault(c => c.CicloFormativoId == id);
        return ciclo == null ? null : ToMap(ciclo);
    }

    public CicloFormativoDTO InsertarCicloFormativo(CicloFormativo cicloFormativo)
    {
        var centroRepository = _serviceProvider.GetRequiredService<ICentroRepository>();
        var centro = centroRepository.ObtenerCentroPorId(cicloFormativo.CentroId);

        if (centro == null)
        {
            throw new ArgumentException("El centro especificado no existe", nameof(cicloFormativo));
        }

        var existe = _ciclosFormativos.Any(c => c.CicloFormativoId == cicloFormativo.CicloFormativoId);

        if (existe)
        {
            throw new InvalidOperationException("El ciclo formativo ya existe");
        }

        _ciclosFormativos.Add(cicloFormativo);
        return ToMap(cicloFormativo);
    }

    public CicloFormativoDTO ModificarCicloFormativo(CicloFormativo cicloFormativo)
    {
        var centroRepository = _serviceProvider.GetRequiredService<ICentroRepository>();
        var centro = centroRepository.ObtenerCentroPorId(cicloFormativo.CentroId);

        if (centro == null)
        {
            throw new ArgumentException("El centro especificado no existe", nameof(cicloFormativo));
        }

        var index = _ciclosFormativos.FindIndex(c => c.CicloFormativoId == cicloFormativo.CicloFormativoId);

        if (index == -1)
        {
            throw new ArgumentException("El ciclo formativo no existe", nameof(cicloFormativo));
        }

        _ciclosFormativos[index] = cicloFormativo;
        return ToMap(cicloFormativo);
    }

    public bool BorrarCicloFormativo(string id)
    {
        var cicloFormativo = _ciclosFormativos.FirstOrDefault(c => c.CicloFormativoId == id);

        if (cicloFormativo == null) return false;

        var centroRepository = _serviceProvider.GetRequiredService<ICentroRepository>();
        var centro = centroRepository.ObtenerCentroPorId(cicloFormativo.CentroId);

        if (centro == null)
        {
            throw new ArgumentException("El centro especificado no existe");
        }

        var moduloRepository = _serviceProvider.GetRequiredService<IModuloRepository>();
        var modulos = moduloRepository.ObtenerModulosPorCicloFormativo(id);

        if (modulos.Count != 0)
        {
            throw new InvalidOperationException("El ciclo formativo tiene modulos asociados");
        }

        _ciclosFormativos.Remove(cicloFormativo);
        return true;
    }
    private CicloFormativoDTO ToMap(CicloFormativo ciclo)
    {
        return new CicloFormativoDTO(
            ciclo.CentroId,
            ciclo.CicloFormativoId,
            ciclo.Nombre,
            ciclo.Turno
        );
    }
}
