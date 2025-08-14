using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MecaFlow2025.Models;

namespace MecaFlow2025.Controllers
{
    public class FacturasController : Controller
    {
        private readonly MecaFlowContext _ctx;

        // Métodos de pago permitidos
        private static readonly string[] MetodosPago = new[] { "Sinpe Móvil", "Tarjeta", "Efectivo" };
        private SelectList MetodosSelect(string? seleccionado = null) => new SelectList(MetodosPago, seleccionado);

        public FacturasController(MecaFlowContext ctx) => _ctx = ctx;

        // GET: /Facturas
        public async Task<IActionResult> Index()
        {
            var data = await _ctx.Facturas
                .Include(f => f.Cliente)
                .Include(f => f.Vehiculo)
                .OrderByDescending(f => f.FacturaId)
                .ToListAsync();

            return View(data);
        }

        // GET: /Facturas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var factura = await _ctx.Facturas
                .Include(f => f.Cliente)
                .Include(f => f.Vehiculo)
                .FirstOrDefaultAsync(m => m.FacturaId == id);

            if (factura == null) return NotFound();

            // (Si ya no quieres tareas aquí, borra este bloque)
            var tareas = await _ctx.TareasVehiculos
                .Where(t => t.VehiculoId == factura.VehiculoId)
                .OrderByDescending(t => t.FechaRegistro)
                .ToListAsync();

            ViewBag.Tareas = tareas;
            return View(factura);
        }

        // GET: /Facturas/Create
        public async Task<IActionResult> Create()
        {
            await CargarSelects();
            ViewData["Metodos"] = MetodosSelect();
            ViewBag.Hoy = DateOnly.FromDateTime(DateTime.Now);
            return View();
        }

        // POST: /Facturas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClienteId,VehiculoId,MontoTotal,Metodo")] Factura factura)
        {
            // Siempre setear fecha en servidor
            factura.Fecha = DateOnly.FromDateTime(DateTime.Now);

            // Evitar que las navegaciones invaliden el ModelState
            ModelState.Remove(nameof(Factura.Cliente));
            ModelState.Remove(nameof(Factura.Vehiculo));
            ModelState.Remove(nameof(Factura.Pagos));

            // Validaciones básicas
            if (factura.ClienteId <= 0)
                ModelState.AddModelError(nameof(Factura.ClienteId), "Seleccione un cliente.");

            if (factura.VehiculoId <= 0)
                ModelState.AddModelError(nameof(Factura.VehiculoId), "Seleccione un vehículo.");

            if (string.IsNullOrWhiteSpace(factura.Metodo) || !MetodosPago.Contains(factura.Metodo))
                ModelState.AddModelError(nameof(Factura.Metodo), "Seleccione un método de pago válido.");

            // Validar existencia si Ids > 0
            if (factura.ClienteId > 0 && !await _ctx.Clientes.AnyAsync(c => c.ClienteId == factura.ClienteId))
                ModelState.AddModelError(nameof(Factura.ClienteId), "El cliente no existe.");

            if (factura.VehiculoId > 0 && !await _ctx.Vehiculos.AnyAsync(v => v.VehiculoId == factura.VehiculoId))
                ModelState.AddModelError(nameof(Factura.VehiculoId), "El vehículo no existe.");

            if (!ModelState.IsValid)
            {
                TempData["Error"] = string.Join(" | ",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

                await CargarSelects(factura.ClienteId, factura.VehiculoId);
                ViewData["Metodos"] = MetodosSelect(factura.Metodo);
                ViewBag.Hoy = factura.Fecha;
                return View(factura);
            }

            _ctx.Add(factura);
            await _ctx.SaveChangesAsync();

            TempData["Ok"] = "Factura creada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Facturas/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var factura = await _ctx.Facturas.FindAsync(id);
            if (factura == null) return NotFound();

            await CargarSelects(factura.ClienteId, factura.VehiculoId);
            ViewData["Metodos"] = MetodosSelect(factura.Metodo);
            return View(factura);
        }

        // POST: /Facturas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FacturaId,ClienteId,VehiculoId,MontoTotal,Metodo")] Factura dto)
        {
            if (id != dto.FacturaId) return NotFound();

            // Evitar validar navegaciones / fecha
            ModelState.Remove(nameof(Factura.Cliente));
            ModelState.Remove(nameof(Factura.Vehiculo));
            ModelState.Remove(nameof(Factura.Pagos));
            ModelState.Remove(nameof(Factura.Fecha)); // no se edita

            if (dto.ClienteId <= 0) ModelState.AddModelError(nameof(Factura.ClienteId), "Seleccione un cliente.");
            if (dto.VehiculoId <= 0) ModelState.AddModelError(nameof(Factura.VehiculoId), "Seleccione un vehículo.");
            if (string.IsNullOrWhiteSpace(dto.Metodo) || !MetodosPago.Contains(dto.Metodo))
                ModelState.AddModelError(nameof(Factura.Metodo), "Seleccione un método de pago válido.");

            if (dto.ClienteId > 0 && !await _ctx.Clientes.AnyAsync(c => c.ClienteId == dto.ClienteId))
                ModelState.AddModelError(nameof(Factura.ClienteId), "El cliente no existe.");
            if (dto.VehiculoId > 0 && !await _ctx.Vehiculos.AnyAsync(v => v.VehiculoId == dto.VehiculoId))
                ModelState.AddModelError(nameof(Factura.VehiculoId), "El vehículo no existe.");

            var factura = await _ctx.Facturas.FirstOrDefaultAsync(f => f.FacturaId == id);
            if (factura == null) return NotFound();

            if (!ModelState.IsValid)
            {
                // Conservar fecha original para mostrarla
                dto.Fecha = factura.Fecha;
                await CargarSelects(dto.ClienteId, dto.VehiculoId);
                ViewData["Metodos"] = MetodosSelect(dto.Metodo);
                return View(dto);
            }

            // Actualiza solo campos editables
            factura.ClienteId = dto.ClienteId;
            factura.VehiculoId = dto.VehiculoId;
            factura.MontoTotal = dto.MontoTotal;
            factura.Metodo = dto.Metodo;

            await _ctx.SaveChangesAsync();
            TempData["Ok"] = "Factura actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Facturas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var factura = await _ctx.Facturas
                .Include(f => f.Cliente)
                .Include(f => f.Vehiculo)
                .FirstOrDefaultAsync(m => m.FacturaId == id);

            if (factura == null) return NotFound();
            return View(factura);
        }

        // POST: /Facturas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var factura = await _ctx.Facturas.FindAsync(id);
            if (factura != null)
            {
                _ctx.Facturas.Remove(factura);
                await _ctx.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: /Facturas/TareasPorVehiculo?vehiculoId=123
        [HttpGet]
        public async Task<IActionResult> TareasPorVehiculo(int vehiculoId)
        {
            var tareas = await _ctx.TareasVehiculos
                .Where(t => t.VehiculoId == vehiculoId)
                .OrderByDescending(t => t.FechaRegistro)
                .ToListAsync();

            return PartialView("~/Views/Facturas/_TareasPorVehiculo.cshtml", tareas);
        }

        // Helpers
        private async Task CargarSelects(int? clienteId = null, int? vehiculoId = null)
        {
            ViewData["ClienteId"] = new SelectList(
                await _ctx.Clientes
                    .AsNoTracking()
                    .OrderBy(c => c.Nombre)
                    .ToListAsync(),
                "ClienteId", "Nombre", clienteId
            );

            var vehiculosRaw = await _ctx.Vehiculos
                .AsNoTracking()
                .Select(v => new
                {
                    v.VehiculoId,
                    v.Placa,
                    ClienteNombre = v.Cliente != null ? v.Cliente.Nombre : null
                })
                .ToListAsync();

            var vehiculos = vehiculosRaw
                .Select(v => new
                {
                    v.VehiculoId,
                    Display = v.Placa + (string.IsNullOrEmpty(v.ClienteNombre) ? "" : " — " + v.ClienteNombre)
                })
                .OrderBy(v => v.Display)
                .ToList();

            ViewData["VehiculoId"] = new SelectList(vehiculos, "VehiculoId", "Display", vehiculoId);
        }
    }
}
