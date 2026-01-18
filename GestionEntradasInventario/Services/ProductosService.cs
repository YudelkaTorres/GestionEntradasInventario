using GestionEntradasInventario.Data;
using Microsoft.EntityFrameworkCore;
using GestionEntradasInventario.Models;
using Microsoft.AspNetCore.Components.Web;
using System.Linq.Expressions;

namespace GestionEntradasInventario.Services;
public class ProductosService(IDbContextFactory<ApplicationDbContext> DbContext)
{
    private async Task<bool> Existe(int productoId)
    {
        await using var context = await DbContext.CreateDbContextAsync();
        return await context.Productos
            .AnyAsync(p => p.ProductoId == productoId);
    }       

    private async Task<bool> Insertar(Productos producto)
    {
        await using var context = await DbContext.CreateDbContextAsync();
        context.Productos.Add(producto);
        return await context.SaveChangesAsync() > 0;
    }

    private async Task<bool> Modificar(Productos producto)
    {
        await using var contexto = await DbContext.CreateDbContextAsync();
        contexto.Productos.Update(producto);
        return await contexto
            .SaveChangesAsync() > 0;
    }

    public async Task<bool> Guardar(Productos producto)
    {
        if (!await Existe(producto.ProductoId))
        {
            return await Insertar(producto);
        }
        else
        {
            return await Modificar(producto);
        }
    }

    public async Task<Productos?> Buscar(int productoId)
    {
        await using var contexto = await DbContext.CreateDbContextAsync();
        return await contexto.Productos
            .FirstOrDefaultAsync(p => p.ProductoId == productoId);
    }

    public async Task<bool> Eliminar(int productosId)
    {
        await using var contexto = await DbContext.CreateDbContextAsync();
        return await contexto.Productos
            .Where(p => p.ProductoId == productosId)
            .ExecuteDeleteAsync() > 0;
    }

    public async Task<List<Productos>> Listar(Expression<Func<Productos, bool>> criterio)
    {
        await using var contexto = await DbContext.CreateDbContextAsync();
        return await contexto.Productos
            .Where(criterio)
            .AsNoTracking()
            .ToListAsync();
    }
}
