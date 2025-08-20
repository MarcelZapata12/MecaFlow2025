using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using MecaFlow2025.Models;
using MecaFlow2025.Helpers;
using MecaFlow2025.Attributes;

using ClosedXML.Excel;          // ⬅ Excel
using QuestPDF.Fluent;          // ⬅ PDF
using QuestPDF.Infrastructure;  // ⬅ PDF

namespace MecaFlow2025.Controllers
{
    public class FacturasController : Controller
    {
        private readonly MecaFlowContext _ctx;

        // Métodos de pago permitidos
        private static readonly string[] MetodosPago = new[] { "Sinpe Móvil", "Tarjeta", "Efectivo" };
        private SelectList MetodosSelect(string? seleccionado = null) => new SelectList(MetodosPago, seleccionado);

        public FacturasController(MecaFlowContext ctx) => _ctx = ctx;

        // ---------- Helper: query base respetando el rol/cliente ----------
        private IQueryable<Factura> BaseQuery()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userEmail = HttpContext.Session.GetString("UserEmail");

            var query = _ctx.Facturas
                            .Include(f => f.Cliente)
                            .Include(f => f.Vehiculo)
                            .AsQueryable();

            if (userRole == "Cliente" && !string.IsNullOrEmpty(userEmail))
            {
                // Buscar el cliente real por email
                var cliente = _ctx.Clientes.FirstOrDefault(c => c.Correo == userEmail);
                if (cliente != null)
                    query = query.Where(f => f.ClienteId == cliente.ClienteId);
                else
                    query = query.Where(_ => false); // sin resultados si no hay match
            }

            return query;
        }

        // GET: /Facturas
        public async Task<IActionResult> Index()
        {
            var userRole = HttpContext.Session.GetString("UserRole");

            var data = await BaseQuery()
                .OrderByDescending(f => f.FacturaId)
                .ToListAsync();

            // Pasar el rol a la vista para controlar la UI (botones, etc.)
            ViewBag.UserRole = userRole;

            return View(data);
        }

        // GET: /Facturas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var userRole = HttpContext.Session.GetString("UserRole");
            var userEmail = HttpContext.Session.GetString("UserEmail");

            var query = _ctx.Facturas
                .Include(f => f.Cliente)
                .Include(f => f.Vehiculo)
                .Where(m => m.FacturaId == id);

            // Si es cliente, verificar que la factura le pertenezca por email
            if (userRole == "Cliente" && !string.IsNullOrEmpty(userEmail))
                query = query.Where(f => f.Cliente.Correo == userEmail);

            var factura = await query.FirstOrDefaultAsync();
            if (factura == null) return NotFound();

            var tareas = await _ctx.TareasVehiculos
                .Where(t => t.VehiculoId == factura.VehiculoId)
                .OrderByDescending(t => t.FechaRegistro)
                .ToListAsync();

            ViewBag.Tareas = tareas;

            // Si es una llamada AJAX, devolver vista parcial
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView(factura);

            return View(factura);
        }

        // GET: /Facturas/Create - Solo para Admin y Empleados
        [AuthorizeRole("Administrador", "Empleado")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await CargarSelects();
            ViewData["Metodos"] = MetodosSelect();

            var model = new Factura
            {
                Fecha = DateOnly.FromDateTime(DateTime.Now)
            };
            ViewBag.Hoy = model.Fecha;

            // Si es una llamada AJAX, devolver vista parcial (ruta explícita por si acaso)
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("~/Views/Facturas/Create.cshtml", model);

            return View(model);
        }

        // POST: /Facturas/Create - Solo para Admin y Empleados
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Administrador", "Empleado")]
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
                await CargarSelects(factura.ClienteId, factura.VehiculoId);
                ViewData["Metodos"] = MetodosSelect(factura.Metodo);
                ViewBag.Hoy = factura.Fecha;

                // Si es AJAX, devolver vista parcial con errores
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return PartialView(factura);

                return View(factura);
            }

            _ctx.Add(factura);
            await _ctx.SaveChangesAsync();

            TempData["Ok"] = "Factura creada correctamente.";

            // Si es AJAX, devolver JSON para indicar éxito
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, message = "Factura creada correctamente." });

            return RedirectToAction(nameof(Index));
        }

        // GET: /Facturas/Edit/5 - Solo para Admin y Empleados
        [HttpGet]
        [AuthorizeRole("Administrador", "Empleado")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var factura = await _ctx.Facturas.FindAsync(id);
            if (factura == null) return NotFound();

            await CargarSelects(factura.ClienteId, factura.VehiculoId);
            ViewData["Metodos"] = MetodosSelect(factura.Metodo);

            // Si es una llamada AJAX, devolver vista parcial
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView(factura);

            return View(factura);
        }

        // POST: /Facturas/Edit/5 - Solo para Admin y Empleados
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Administrador", "Empleado")]
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

                // Si es AJAX, devolver vista parcial con errores
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return PartialView(dto);

                return View(dto);
            }

            // Actualiza solo campos editables
            factura.ClienteId = dto.ClienteId;
            factura.VehiculoId = dto.VehiculoId;
            factura.MontoTotal = dto.MontoTotal;
            factura.Metodo = dto.Metodo;

            await _ctx.SaveChangesAsync();
            TempData["Ok"] = "Factura actualizada correctamente.";

            // Si es AJAX, devolver JSON para indicar éxito
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, message = "Factura actualizada correctamente." });

            return RedirectToAction(nameof(Index));
        }

        // GET: /Facturas/Delete/5 - Solo para Admin y Empleados
        [AuthorizeRole("Administrador", "Empleado")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var factura = await _ctx.Facturas
                .Include(f => f.Cliente)
                .Include(f => f.Vehiculo)
                .FirstOrDefaultAsync(m => m.FacturaId == id);

            if (factura == null) return NotFound();

            // Si es una llamada AJAX, devolver vista parcial
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView(factura);

            return View(factura);
        }

        // POST: /Facturas/Delete/5 - Solo para Admin y Empleados
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Administrador", "Empleado")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var factura = await _ctx.Facturas.FindAsync(id);
            if (factura != null)
            {
                _ctx.Facturas.Remove(factura);
                await _ctx.SaveChangesAsync();
            }

            TempData["Ok"] = "Factura eliminada correctamente.";

            // Si es AJAX, devolver JSON para indicar éxito
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, message = "Factura eliminada correctamente." });

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

        // ======================== EXPORTS ==============================

        // GET: /Facturas/ExportToExcel
        [HttpGet]
        public async Task<IActionResult> ExportToExcel()
        {
            // CultureInfo preparado por si luego quieres formatear moneda/fecha
            var cr = new CultureInfo("es-CR");

            var data = await BaseQuery()
                .OrderByDescending(f => f.FacturaId)
                .Select(f => new
                {
                    f.FacturaId,
                    FechaDT = f.Fecha.ToDateTime(TimeOnly.MinValue), // Excel friendly
                    Cliente = f.Cliente != null ? f.Cliente.Nombre : "-",
                    Vehiculo = f.Vehiculo != null ? f.Vehiculo.Placa : "-",
                    Metodo = string.IsNullOrWhiteSpace(f.Metodo) ? "-" : f.Metodo,
                    Monto = f.MontoTotal
                })
                .ToListAsync();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Facturas");

            // Encabezados
            ws.Cell(1, 1).Value = "Factura #";
            ws.Cell(1, 2).Value = "Fecha";
            ws.Cell(1, 3).Value = "Cliente";
            ws.Cell(1, 4).Value = "Vehículo";
            ws.Cell(1, 5).Value = "Método";
            ws.Cell(1, 6).Value = "Monto (₡)";
            ws.Range(1, 1, 1, 6).Style.Font.SetBold();

            // Datos
            int row = 2;
            foreach (var r in data)
            {
                ws.Cell(row, 1).Value = r.FacturaId;
                ws.Cell(row, 2).Value = r.FechaDT; // como DateTime
                ws.Cell(row, 3).Value = r.Cliente;
                ws.Cell(row, 4).Value = r.Vehiculo;
                ws.Cell(row, 5).Value = r.Metodo;
                ws.Cell(row, 6).Value = r.Monto;
                row++;
            }

            // Formatos
            ws.Column(2).Style.DateFormat.Format = "dd/MM/yyyy";
            ws.Column(6).Style.NumberFormat.Format = "#,##0.00";
            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            var bytes = stream.ToArray();

            var fileName = $"Facturas_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        // GET: /Facturas/ExportToPdf
        [HttpGet]
        public async Task<IActionResult> ExportToPdf()
        {
            var cr = new CultureInfo("es-CR");

            var data = await BaseQuery()
                .OrderByDescending(f => f.FacturaId)
                .Select(f => new
                {
                    f.FacturaId,
                    Fecha = f.Fecha.ToString("dd/MM/yyyy"),
                    Cliente = f.Cliente != null ? f.Cliente.Nombre : "-",
                    Vehiculo = f.Vehiculo != null ? f.Vehiculo.Placa : "-",
                    Metodo = string.IsNullOrWhiteSpace(f.Metodo) ? "-" : f.Metodo,
                    Monto = f.MontoTotal
                })
                .ToListAsync();

            decimal total = data.Sum(x => x.Monto);

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    // Header
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("MecaFlow Taller").FontSize(16).Bold().FontColor("#1a5276");
                            col.Item().Text("Listado de Facturas").FontSize(22).Bold();
                            col.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}")
                                      .FontSize(10).FontColor("#7d7d7d");
                        });
                    });

                    // Tabla
                    page.Content().PaddingVertical(15).Column(col =>
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn(1);    // #
                                cols.RelativeColumn(1.3f); // Fecha
                                cols.RelativeColumn(2.2f); // Cliente
                                cols.RelativeColumn(1.6f); // Vehículo
                                cols.RelativeColumn(1.4f); // Método
                                cols.RelativeColumn(1.4f); // Monto
                            });

                            // Encabezado
                            table.Header(h =>
                            {
                                string bg = "#154360", fg = "#FFFFFF";
                                h.Cell().Background(bg).Padding(5).Text("Factura #").FontColor(fg).Bold();
                                h.Cell().Background(bg).Padding(5).Text("Fecha").FontColor(fg).Bold();
                                h.Cell().Background(bg).Padding(5).Text("Cliente").FontColor(fg).Bold();
                                h.Cell().Background(bg).Padding(5).Text("Vehículo").FontColor(fg).Bold();
                                h.Cell().Background(bg).Padding(5).Text("Método").FontColor(fg).Bold();
                                h.Cell().Background(bg).Padding(5).AlignRight().Text("Monto (₡)").FontColor(fg).Bold();
                            });

                            bool alt = false;
                            foreach (var r in data)
                            {
                                string bg = alt ? "#f2f3f4" : "#ffffff"; alt = !alt;

                                table.Cell().Background(bg).Padding(5).Text(r.FacturaId);
                                table.Cell().Background(bg).Padding(5).Text(r.Fecha);
                                table.Cell().Background(bg).Padding(5).Text(r.Cliente);
                                table.Cell().Background(bg).Padding(5).Text(r.Vehiculo);
                                table.Cell().Background(bg).Padding(5).Text(r.Metodo);
                                table.Cell().Background(bg).Padding(5)
                                     .AlignRight()
                                     .Text(r.Monto.ToString("N2"));
                            }

                            // Total
                            table.Cell().ColumnSpan(5).AlignRight().Padding(5).Background("#d5dbdb")
                                 .Text("TOTAL:").Bold();
                            table.Cell().Padding(5).Background("#d5dbdb").AlignRight()
                                 .Text(total.ToString("N2")).Bold();
                        });
                    });

                    page.Footer().AlignCenter().Text(t =>
                    {
                        t.DefaultTextStyle(x => x.FontSize(9).FontColor("#7d7d7d"));
                        t.Span("Reporte generado automáticamente por ");
                        t.Span("MecaFlow").Bold();
                    });
                });
            }).GeneratePdf();

            var fileName = $"Facturas_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        // ======================== Helpers =============================
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
