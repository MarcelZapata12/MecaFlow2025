using ClosedXML.Excel;
using MecaFlow2025.Attributes;
using MecaFlow2025.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MecaFlow2025.Controllers
{
    [AuthorizeRole("Administrador", "Empleado", "Cliente")]
    public class DiagnosticosController : Controller
    {
        private readonly MecaFlowContext _context;

        public DiagnosticosController(MecaFlowContext context)
        {
            _context = context;
        }

        // GET: Diagnosticos
        public async Task<IActionResult> Index(string search, string sort, string dir)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userEmail = HttpContext.Session.GetString("UserEmail");

            IQueryable<Diagnostico> diagnosticos = _context.Diagnosticos
                .Include(d => d.Vehiculo).ThenInclude(v => v.Marca)
                .Include(d => d.Vehiculo).ThenInclude(v => v.Modelo)
                .Include(d => d.Vehiculo).ThenInclude(v => v.Cliente)
                .Include(d => d.Empleado);

            // Si es cliente, filtrar solo sus diagnósticos
            if (userRole == "Cliente" && !string.IsNullOrEmpty(userEmail))
            {
                diagnosticos = diagnosticos.Where(d => d.Vehiculo.Cliente.Correo == userEmail);
            }

            // Búsqueda
            if (!string.IsNullOrEmpty(search))
            {
                diagnosticos = diagnosticos.Where(d =>
                    d.Detalle.Contains(search) ||
                    d.Vehiculo.Placa.Contains(search) ||
                    d.Vehiculo.Marca.Nombre.Contains(search) ||
                    d.Vehiculo.Modelo.Nombre.Contains(search) ||
                    d.Empleado.Nombre.Contains(search)
                );
                ViewBag.Search = search;
            }

            // Ordenamiento
            ViewBag.Sort = sort;
            ViewBag.Dir = dir;
            diagnosticos = sort switch
            {
                "vehiculo" => dir == "desc" ? diagnosticos.OrderByDescending(d => d.Vehiculo.Placa) : diagnosticos.OrderBy(d => d.Vehiculo.Placa),
                "fecha" => dir == "desc" ? diagnosticos.OrderByDescending(d => d.Fecha) : diagnosticos.OrderBy(d => d.Fecha),
                "detalle" => dir == "desc" ? diagnosticos.OrderByDescending(d => d.Detalle) : diagnosticos.OrderBy(d => d.Detalle),
                "empleado" => dir == "desc" ? diagnosticos.OrderByDescending(d => d.Empleado.Nombre) : diagnosticos.OrderBy(d => d.Empleado.Nombre),
                _ => diagnosticos.OrderByDescending(d => d.Fecha),
            };

            // Pasar el rol a la vista
            ViewBag.UserRole = userRole;

            return View(await diagnosticos.ToListAsync());
        }

        // GET: Diagnosticos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var userRole = HttpContext.Session.GetString("UserRole");
            var userEmail = HttpContext.Session.GetString("UserEmail");

            var query = _context.Diagnosticos
                .Include(d => d.Vehiculo).ThenInclude(v => v.Marca)
                .Include(d => d.Vehiculo).ThenInclude(v => v.Modelo)
                .Include(d => d.Vehiculo).ThenInclude(v => v.Cliente)
                .Include(d => d.Empleado)
                .Where(d => d.DiagnosticoId == id);

            // Si es cliente, verificar que el diagnóstico sea de su vehículo
            if (userRole == "Cliente" && !string.IsNullOrEmpty(userEmail))
            {
                query = query.Where(d => d.Vehiculo.Cliente.Correo == userEmail);
            }

            var diagnostico = await query.FirstOrDefaultAsync();

            if (diagnostico == null) return NotFound();

            // Si es una llamada AJAX, devolver vista parcial
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView(diagnostico);
            }

            return View(diagnostico);
        }

        // GET: Diagnosticos/Create - Solo para Admin y Empleados
        [AuthorizeRole("Administrador", "Empleado")]
        public IActionResult Create()
        {
            ViewBag.Vehiculos = new SelectList(_context.Vehiculos
                .Include(v => v.Marca)
                .Include(v => v.Modelo)
                .Select(v => new
                {
                    v.VehiculoId,
                    Display = v.Placa + " - " + v.Marca.Nombre + " " + v.Modelo.Nombre
                }),
                "VehiculoId", "Display");

            ViewBag.Empleados = new SelectList(_context.Empleados.OrderBy(e => e.Nombre), "EmpleadoId", "Nombre");
            return View();
        }

        // POST: Diagnosticos/Create - Solo para Admin y Empleados
        [HttpPost, ValidateAntiForgeryToken]
        [AuthorizeRole("Administrador", "Empleado")]
        public async Task<IActionResult> Create([Bind("VehiculoId,Fecha,Detalle,EmpleadoId")] Diagnostico model)
        {
            if (ModelState.IsValid)
            {
                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Vehiculos = new SelectList(_context.Vehiculos
                .Include(v => v.Marca)
                .Include(v => v.Modelo)
                .Select(v => new
                {
                    v.VehiculoId,
                    Display = v.Placa + " - " + v.Marca.Nombre + " " + v.Modelo.Nombre
                }),
                "VehiculoId", "Display", model.VehiculoId);

            ViewBag.Empleados = new SelectList(_context.Empleados, "EmpleadoId", "Nombre", model.EmpleadoId);
            return View(model);
        }

        // GET: Diagnosticos/Edit/5 - Solo para Admin y Empleados
        [AuthorizeRole("Administrador", "Empleado")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var diag = await _context.Diagnosticos.FindAsync(id);
            if (diag == null) return NotFound();

            ViewBag.Vehiculos = new SelectList(_context.Vehiculos
                .Include(v => v.Marca)
                .Include(v => v.Modelo)
                .Select(v => new
                {
                    v.VehiculoId,
                    Display = v.Placa + " - " + v.Marca.Nombre + " " + v.Modelo.Nombre
                }),
                "VehiculoId", "Display", diag.VehiculoId);

            ViewBag.Empleados = new SelectList(_context.Empleados, "EmpleadoId", "Nombre", diag.EmpleadoId);
            return View(diag);
        }

        // POST: Diagnosticos/Edit/5 - Solo para Admin y Empleados
        [HttpPost, ValidateAntiForgeryToken]
        [AuthorizeRole("Administrador", "Empleado")]
        public async Task<IActionResult> Edit(int id, [Bind("DiagnosticoId,VehiculoId,Fecha,Detalle,EmpleadoId")] Diagnostico model)
        {
            if (id != model.DiagnosticoId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Diagnosticos.Any(e => e.DiagnosticoId == id)) return NotFound();
                    else throw;
                }
            }

            ViewBag.Vehiculos = new SelectList(_context.Vehiculos
                .Include(v => v.Marca)
                .Include(v => v.Modelo)
                .Select(v => new
                {
                    v.VehiculoId,
                    Display = v.Placa + " - " + v.Marca.Nombre + " " + v.Modelo.Nombre
                }),
                "VehiculoId", "Display", model.VehiculoId);

            ViewBag.Empleados = new SelectList(_context.Empleados, "EmpleadoId", "Nombre", model.EmpleadoId);
            return View(model);
        }

        // GET: Diagnosticos/Delete/5 - Solo para Admin y Empleados
        [AuthorizeRole("Administrador", "Empleado")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var diag = await _context.Diagnosticos
                .Include(d => d.Vehiculo).ThenInclude(v => v.Marca)
                .Include(d => d.Vehiculo).ThenInclude(v => v.Modelo)
                .Include(d => d.Empleado)
                .FirstOrDefaultAsync(d => d.DiagnosticoId == id);

            if (diag == null) return NotFound();

            return View(diag);
        }

        // POST: Diagnosticos/Delete/5 - Solo para Admin y Empleados
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        [AuthorizeRole("Administrador", "Empleado")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var diag = await _context.Diagnosticos.FindAsync(id);
            if (diag != null)
            {
                _context.Diagnosticos.Remove(diag);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Exportar a Excel - Solo para Admin y Empleados
        [AuthorizeRole("Administrador", "Empleado")]
        public IActionResult ExportToExcel()
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Diagnósticos");
            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Vehículo";
            worksheet.Cell(1, 3).Value = "Fecha";
            worksheet.Cell(1, 4).Value = "Detalle";
            worksheet.Cell(1, 5).Value = "Empleado";

            var data = _context.Diagnosticos
                .Include(d => d.Vehiculo).ThenInclude(v => v.Marca)
                .Include(d => d.Vehiculo).ThenInclude(v => v.Modelo)
                .Include(d => d.Empleado)
                .OrderByDescending(d => d.Fecha)
                .ToList();

            int row = 2;
            foreach (var d in data)
            {
                worksheet.Cell(row, 1).Value = d.DiagnosticoId;
                worksheet.Cell(row, 2).Value = $"{d.Vehiculo?.Placa ?? "N/A"} - {d.Vehiculo?.Marca?.Nombre ?? "N/A"} {d.Vehiculo?.Modelo?.Nombre ?? "N/A"}";
                worksheet.Cell(row, 3).Value = d.Fecha.ToString("yyyy-MM-dd");
                worksheet.Cell(row, 4).Value = d.Detalle ?? "";
                worksheet.Cell(row, 5).Value = d.Empleado?.Nombre ?? "N/A";
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Diagnosticos.xlsx");
        }

        // Exportar a PDF - Solo para Admin y Empleados
        [HttpGet]
        [AuthorizeRole("Administrador", "Empleado")]
        public IActionResult ExportToPdf()
        {
            var data = _context.Diagnosticos
                .Include(d => d.Vehiculo).ThenInclude(v => v.Marca)
                .Include(d => d.Vehiculo).ThenInclude(v => v.Modelo)
                .Include(d => d.Empleado)
                .OrderByDescending(d => d.Fecha)
                .ToList();

            var headerBg = "#154360";
            var headerFg = "#FFFFFF";
            var altRow = "#F2F3F4";
            var normRow = "#FFFFFF";

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    // HEADER con título y fecha
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("MecaFlow Taller")
                                      .FontSize(16).Bold().FontColor("#1A5276");
                            col.Item().Text("Reporte de Diagnósticos")
                                      .FontSize(22).Bold();
                            col.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}")
                                      .FontSize(10).FontColor("#7D7D7D");
                        });

                        // Si quieres logo, descomenta y pon ruta válida
                        // row.ConstantItem(80).Image("wwwroot/images/logo.png");
                    });

                    page.Content().PaddingVertical(15).Column(col =>
                    {
                        // Resumen arriba (opcional)
                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Text($"Total diagnósticos: {data.Count}")
                                            .SemiBold().FontColor("#2E86C1");
                            if (data.Any())
                                r.RelativeItem().AlignRight()
                                    .Text($"Rango: {data.Min(x => x.Fecha):yyyy-MM-dd} a {data.Max(x => x.Fecha):yyyy-MM-dd}")
                                    .FontColor("#7D7D7D");
                        });

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.ConstantColumn(40); // #
                                c.RelativeColumn(3);  // Vehículo
                                c.RelativeColumn(2);  // Fecha
                                c.RelativeColumn(4);  // Detalle
                                c.RelativeColumn(2);  // Empleado
                            });

                            // Encabezado con fondo
                            table.Header(h =>
                            {
                                h.Cell().Background(headerBg).Padding(6).Text("#").FontColor(headerFg).Bold();
                                h.Cell().Background(headerBg).Padding(6).Text("Vehículo").FontColor(headerFg).Bold();
                                h.Cell().Background(headerBg).Padding(6).Text("Fecha").FontColor(headerFg).Bold();
                                h.Cell().Background(headerBg).Padding(6).Text("Detalle").FontColor(headerFg).Bold();
                                h.Cell().Background(headerBg).Padding(6).Text("Empleado").FontColor(headerFg).Bold();
                            });

                            bool alt = false;
                            foreach (var d in data)
                            {
                                var bg = (alt = !alt) ? altRow : normRow;
                                var veh = $"{d.Vehiculo?.Placa ?? "N/A"} - {d.Vehiculo?.Marca?.Nombre ?? "N/A"} {d.Vehiculo?.Modelo?.Nombre ?? ""}".Trim();

                                table.Cell().Background(bg).Padding(6).Text(d.DiagnosticoId.ToString());
                                table.Cell().Background(bg).Padding(6).Text(veh);
                                table.Cell().Background(bg).Padding(6).Text(d.Fecha.ToString("yyyy-MM-dd"));
                                table.Cell().Background(bg).Padding(6).Text(d.Detalle ?? "").WrapAnywhere();
                                table.Cell().Background(bg).Padding(6).Text(d.Empleado?.Nombre ?? "N/A");
                            }
                        });
                    });

                    // FOOTER con tipografía y paginación
                    page.Footer().Row(r =>
                    {
                        r.RelativeItem().AlignLeft().Text(t =>
                        {
                            t.DefaultTextStyle(x => x.FontSize(9).FontColor("#7D7D7D"));
                            t.Span("MecaFlow • Reporte de Diagnósticos");
                        });

                        r.RelativeItem().AlignRight().Text(t =>
                        {
                            t.DefaultTextStyle(x => x.FontSize(9).FontColor("#7D7D7D"));
                            t.Span("Página ");
                            t.CurrentPageNumber();
                            t.Span(" / ");
                            t.TotalPages();
                        });
                    });
                });
            }).GeneratePdf();

            var fileName = $"Diagnosticos_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
    }
}