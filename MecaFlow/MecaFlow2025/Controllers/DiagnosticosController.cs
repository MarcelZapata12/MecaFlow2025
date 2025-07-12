using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MecaFlow2025.Models;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MecaFlow2025.Controllers
{
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
            var diagnosticos = _context.Diagnosticos
                .Include(d => d.Vehiculo)
                .Include(d => d.Empleado)
                .AsQueryable();

            // Busqueda
            if (!string.IsNullOrEmpty(search))
            {
                diagnosticos = diagnosticos.Where(d =>
                    d.Detalle.Contains(search) ||
                    d.Vehiculo.Placa.Contains(search) ||
                    d.Empleado.Nombre.Contains(search));
                ViewBag.Search = search;
            }

            // Ordenamiento
            ViewBag.Sort = sort;
            ViewBag.Dir = dir;
            switch (sort)
            {
                case "vehiculo":
                    diagnosticos = dir == "desc" ? diagnosticos.OrderByDescending(d => d.Vehiculo.Placa) : diagnosticos.OrderBy(d => d.Vehiculo.Placa);
                    break;
                case "fecha":
                    diagnosticos = dir == "desc" ? diagnosticos.OrderByDescending(d => d.Fecha) : diagnosticos.OrderBy(d => d.Fecha);
                    break;
                case "detalle":
                    diagnosticos = dir == "desc" ? diagnosticos.OrderByDescending(d => d.Detalle) : diagnosticos.OrderBy(d => d.Detalle);
                    break;
                case "empleado":
                    diagnosticos = dir == "desc" ? diagnosticos.OrderByDescending(d => d.Empleado.Nombre) : diagnosticos.OrderBy(d => d.Empleado.Nombre);
                    break;
                default:
                    diagnosticos = diagnosticos.OrderByDescending(d => d.Fecha);
                    break;
            }

            return View(await diagnosticos.ToListAsync());
        }

        // GET: Diagnosticos/Create
        public IActionResult Create()
        {
            ViewBag.Vehiculos = new SelectList(_context.Vehiculos.OrderBy(v => v.Placa), "VehiculoId", "Placa");
            ViewBag.Empleados = new SelectList(_context.Empleados.OrderBy(e => e.Nombre), "EmpleadoId", "Nombre");
            return View();
        }

        // POST: Diagnosticos/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VehiculoId,Fecha,Detalle,EmpleadoId")] Diagnostico model)
        {
            if (ModelState.IsValid)
            {
                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Vehiculos = new SelectList(_context.Vehiculos, "VehiculoId", "Placa", model.VehiculoId);
            ViewBag.Empleados = new SelectList(_context.Empleados, "EmpleadoId", "Nombre", model.EmpleadoId);
            return View(model);
        }

        // GET: Diagnosticos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var diag = await _context.Diagnosticos.FindAsync(id);
            if (diag == null) return NotFound();

            ViewBag.Vehiculos = new SelectList(_context.Vehiculos, "VehiculoId", "Placa", diag.VehiculoId);
            ViewBag.Empleados = new SelectList(_context.Empleados, "EmpleadoId", "Nombre", diag.EmpleadoId);
            return View(diag);
        }

        // POST: Diagnosticos/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
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
                    if (!_context.Diagnosticos.Any(d => d.DiagnosticoId == id)) return NotFound();
                    throw;
                }
            }
            ViewBag.Vehiculos = new SelectList(_context.Vehiculos, "VehiculoId", "Placa", model.VehiculoId);
            ViewBag.Empleados = new SelectList(_context.Empleados, "EmpleadoId", "Nombre", model.EmpleadoId);
            return View(model);
        }

        // GET: Diagnosticos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var diag = await _context.Diagnosticos
                .Include(d => d.Vehiculo)
                .Include(d => d.Empleado)
                .FirstOrDefaultAsync(d => d.DiagnosticoId == id);

            if (diag == null) return NotFound();

            return View(diag);
        }

        // POST: Diagnosticos/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
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

        // Exportar a Excel
        public IActionResult ExportToExcel()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Diagnósticos");
                worksheet.Cell(1, 1).Value = "ID";
                worksheet.Cell(1, 2).Value = "Vehículo";
                worksheet.Cell(1, 3).Value = "Fecha";
                worksheet.Cell(1, 4).Value = "Detalle";
                worksheet.Cell(1, 5).Value = "Empleado";

                var data = _context.Diagnosticos
                    .Include(d => d.Vehiculo)
                    .Include(d => d.Empleado)
                    .OrderByDescending(d => d.Fecha)
                    .ToList();

                int row = 2;
                foreach (var d in data)
                {
                    worksheet.Cell(row, 1).Value = d.DiagnosticoId;
                    worksheet.Cell(row, 2).Value = d.Vehiculo?.Placa ?? "";
                    worksheet.Cell(row, 3).Value = d.Fecha.ToString("yyyy-MM-dd");
                    worksheet.Cell(row, 4).Value = d.Detalle;
                    worksheet.Cell(row, 5).Value = d.Empleado?.Nombre ?? "";
                    row++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "Diagnosticos.xlsx");
                }
            }
        }

        // Exportar a PDF (con fix AlignCenter)
        public IActionResult ExportToPdf()
        {
            var data = _context.Diagnosticos
                .Include(d => d.Vehiculo)
                .Include(d => d.Empleado)
                .OrderByDescending(d => d.Fecha)
                .ToList();

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);

                    page.Header().Element(header =>
                    {
                        header.AlignCenter().Text("Reporte de Diagnósticos").FontSize(20).Bold();
                    });

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(40);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("#").Bold();
                            header.Cell().Text("Vehículo").Bold();
                            header.Cell().Text("Fecha").Bold();
                            header.Cell().Text("Detalle").Bold();
                            header.Cell().Text("Empleado").Bold();
                        });

                        foreach (var d in data)
                        {
                            table.Cell().Text(d.DiagnosticoId.ToString());
                            table.Cell().Text(d.Vehiculo?.Placa ?? "");
                            table.Cell().Text(d.Fecha.ToString("yyyy-MM-dd"));
                            table.Cell().Text(d.Detalle ?? "");
                            table.Cell().Text(d.Empleado?.Nombre ?? "");
                        }
                    });
                });
            }).GeneratePdf();

            return File(pdfBytes, "application/pdf", "Diagnosticos.pdf");
        }
    }
}
