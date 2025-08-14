using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MecaFlow2025.Models;

namespace MecaFlow2025.Controllers
{
    public class TareasVehiculoController : Controller
    {
        private readonly MecaFlowContext _ctx;

        public TareasVehiculoController(MecaFlowContext ctx) => _ctx = ctx;

        // ----- Lista base de sectores -----
        private static readonly string[] SectoresBase = new[]
        {
            "Frenos", "Motor", "Transmisión", "Suspensión", "Dirección",
            "Eléctrico/Electrónica", "Aire acondicionado", "Enfriamiento",
            "Carrocería/Pintura", "Otro"
        };

        private SelectList SectoresSelect(string? seleccionado = null) =>
            new SelectList(SectoresBase.Select(s => new { Value = s, Text = s }), "Value", "Text", seleccionado);

        // GET: TareasVehiculo
        public async Task<IActionResult> Index(int? vehiculoId, bool? realizadas, string? q, string? sector)
        {
            var query = _ctx.TareasVehiculos
                            .Include(t => t.Vehiculo)
                            .AsNoTracking()
                            .AsQueryable();

            if (vehiculoId.HasValue)
                query = query.Where(t => t.VehiculoId == vehiculoId.Value);

            if (realizadas.HasValue)
                query = query.Where(t => t.Realizada == realizadas.Value);

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(t => t.Descripcion != null && t.Descripcion.Contains(q));

            if (!string.IsNullOrWhiteSpace(sector))
                query = query.Where(t => t.Sector == sector);

            var listado = await query
                .OrderByDescending(t => t.FechaRegistro)   // DateOnly? -> sin hora
                .ThenByDescending(t => t.TareaId)
                .ToListAsync();

            ViewData["VehiculoId"] = new SelectList(
                await _ctx.Vehiculos.AsNoTracking()
                    .Select(v => new { v.VehiculoId, v.Placa })
                    .ToListAsync(),
                "VehiculoId", "Placa", vehiculoId
            );

            ViewData["Sectores"] = SectoresSelect(sector);
            ViewData["Q"] = q;
            ViewData["Realizadas"] = realizadas;
            ViewData["VehiculoIdFilter"] = vehiculoId;
            ViewData["SectorFilter"] = sector;

            return View(listado);
        }

        // GET: TareasVehiculo/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var tarea = await _ctx.TareasVehiculos
                                  .Include(t => t.Vehiculo)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync(m => m.TareaId == id);
            if (tarea == null) return NotFound();

            return View(tarea);
        }

        // GET: TareasVehiculo/Create
        public async Task<IActionResult> Create(int? vehiculoId)
        {
            ViewData["VehiculoId"] = new SelectList(
                await _ctx.Vehiculos.AsNoTracking()
                    .Select(v => new { v.VehiculoId, v.Placa })
                    .ToListAsync(),
                "VehiculoId", "Placa", vehiculoId
            );

            ViewBag.Sectores = SectoresSelect();

            return View(new TareasVehiculo
            {
                VehiculoId = vehiculoId ?? 0,
                Realizada = false,
                // FechaRegistro = DateOnly.FromDateTime(DateTime.Now) // si quisieras precargarla
            });
        }

        // POST: TareasVehiculo/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VehiculoId,Descripcion,Sector,Realizada")] TareasVehiculo tarea)
        {
            // Evita validar navegación
            ModelState.Remove("Vehiculo");

            if (ModelState.IsValid)
            {
                // Solo FECHA (sin hora)
                if (!tarea.FechaRegistro.HasValue)
                    tarea.FechaRegistro = DateOnly.FromDateTime(DateTime.Now);

                _ctx.TareasVehiculos.Add(tarea);
                await _ctx.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { vehiculoId = tarea.VehiculoId });
            }

            ViewData["VehiculoId"] = new SelectList(
                await _ctx.Vehiculos.AsNoTracking()
                    .Select(v => new { v.VehiculoId, v.Placa })
                    .ToListAsync(),
                "VehiculoId", "Placa", tarea.VehiculoId
            );

            ViewBag.Sectores = SectoresSelect(tarea.Sector);
            return View(tarea);
        }

        // GET: TareasVehiculo/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var tarea = await _ctx.TareasVehiculos.FindAsync(id);
            if (tarea == null) return NotFound();

            ViewData["VehiculoId"] = new SelectList(
                await _ctx.Vehiculos.AsNoTracking()
                    .Select(v => new { v.VehiculoId, v.Placa })
                    .ToListAsync(),
                "VehiculoId", "Placa", tarea.VehiculoId
            );

            ViewBag.Sectores = SectoresSelect(tarea.Sector);
            return View(tarea);
        }

        // POST: TareasVehiculo/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TareaId,VehiculoId,Descripcion,Sector,Realizada")] TareasVehiculo form)
        {
            ModelState.Remove("Vehiculo"); // no tocamos navegación

            if (id != form.TareaId) return NotFound();

            if (ModelState.IsValid)
            {
                var tarea = await _ctx.TareasVehiculos.FirstOrDefaultAsync(t => t.TareaId == id);
                if (tarea == null) return NotFound();

                // Actualiza solo campos permitidos (no tocamos FechaRegistro)
                tarea.VehiculoId = form.VehiculoId;
                tarea.Descripcion = form.Descripcion;
                tarea.Sector = form.Sector;
                tarea.Realizada = form.Realizada;

                await _ctx.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { vehiculoId = tarea.VehiculoId });
            }

            ViewData["VehiculoId"] = new SelectList(
                await _ctx.Vehiculos.AsNoTracking()
                    .Select(v => new { v.VehiculoId, v.Placa })
                    .ToListAsync(),
                "VehiculoId", "Placa", form.VehiculoId
            );

            ViewBag.Sectores = SectoresSelect(form.Sector);
            return View(form);
        }

        // GET: TareasVehiculo/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var tarea = await _ctx.TareasVehiculos
                                  .Include(t => t.Vehiculo)
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync(m => m.TareaId == id);
            if (tarea == null) return NotFound();

            return View(tarea);
        }

        // POST: TareasVehiculo/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tarea = await _ctx.TareasVehiculos.FindAsync(id);
            if (tarea != null)
            {
                var vehiculoId = tarea.VehiculoId;
                _ctx.TareasVehiculos.Remove(tarea);
                await _ctx.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { vehiculoId });
            }

            return RedirectToAction(nameof(Index));
        }

        // Cambiar Pendiente/Realizada desde el listado (manteniendo filtros)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleRealizada(int id, int? vehiculoId, bool? realizadas, string? q, string? sector)
        {
            var tarea = await _ctx.TareasVehiculos.FindAsync(id);
            if (tarea == null) return NotFound();

            tarea.Realizada = !tarea.Realizada;
            await _ctx.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { vehiculoId, realizadas, q, sector });
        }
    }
}
