using GestionEntradasInventario.Data;
using Microsoft.EntityFrameworkCore;
using GestionEntradasInventario.Models;
using System.Linq.Expressions;

namespace GestionEntradasInventario.Services;
public class EntradasService(IDbContextFactory<ApplicationDbContext> DbContext)
{
    private async Task<bool> Existe(int entradaId)
    {
        await using var contexto = await DbContext.CreateDbContextAsync();
        return await contexto.Entradas.AnyAsync(e => e.EntradaId == entradaId);
    }

    private async Task<bool> Insertar(Entradas entrada)
    {
        await using var contexto = await DbContext.CreateDbContextAsync();
        contexto.Entradas.Add(entrada);

        await AfectarExistencia(entrada.EntradasDetalle.ToArray(), TipoOperacion.Suma);

        return await contexto.SaveChangesAsync() > 0;
    }
    public async Task AfectarExistencia(EntradasDetalle[] entradasDetalle, TipoOperacion tipoOperacion)
    {  
        await using var contexto = await DbContext.CreateDbContextAsync();
        foreach (var item in entradasDetalle)
        { 
            var producto = await contexto.Productos.SingleAsync(p => p.ProductoId == item.ProductoId);
            if (tipoOperacion == TipoOperacion.Suma)
                producto.Existencia += item.Cantidad;
            else
                producto.Existencia -= item.Cantidad;
            await contexto.SaveChangesAsync();
        }
    }

    private async Task<bool> Modificar(Entradas entrada)
    {
        await using var contexto = await DbContext.CreateDbContextAsync();

        var original = await contexto.Entradas
            .Include(e => e.EntradasDetalle)
            .AsNoTracking()
            .SingleOrDefaultAsync(e => e.EntradaId == entrada.EntradaId);

        if (original is null) return false;

        await AfectarExistencia(original.EntradasDetalle.ToArray(), TipoOperacion.Resta);

        contexto.EntradasDetalle.RemoveRange(original.EntradasDetalle);

        contexto.Entradas.Update(entrada);

        await AfectarExistencia(entrada.EntradasDetalle.ToArray(), TipoOperacion.Suma);

        return await contexto.SaveChangesAsync() > 0;
    }

    public async Task<bool> Guardar(Entradas entrada)
    {
        if (!await Existe(entrada.EntradaId))
        {
            return await Insertar(entrada);
        }
        else
        {
            return await Modificar(entrada);
        }     
    }
    public async Task<Entradas?> Buscar(int entradaId)
    {
        await using var contexto = await DbContext.CreateDbContextAsync();
        return await contexto.Entradas
            .Include(e => e.EntradasDetalle)
            .FirstOrDefaultAsync(e => e.EntradaId == entradaId);
    }

    public async Task<bool> Eliminar(int entradaId)
    {
       await using var contexto = await DbContext.CreateDbContextAsync();
        var entrada = await contexto.Entradas
            .Include(e => e.EntradasDetalle)
            .FirstOrDefaultAsync(e => e.EntradaId == entradaId);

        if (entrada == null) return false;

        await AfectarExistencia(entrada.EntradasDetalle.ToArray(), TipoOperacion.Resta);

        contexto.EntradasDetalle.RemoveRange(entrada.EntradasDetalle);
        contexto.Entradas.Remove(entrada);
        var cantidad = await contexto.SaveChangesAsync();
        return cantidad > 0;
    }

    public async Task<List<Entradas>> Listar(Expression<Func<Entradas, bool>> criterio)
    {
        await using var contexto = await DbContext.CreateDbContextAsync();
        return await contexto.Entradas
            .Include(e => e.EntradasDetalle)
            .Where(criterio)
            .AsNoTracking()
            .ToListAsync();
    }
}
public enum TipoOperacion
{
    Suma = 1,
    Resta = 2
}