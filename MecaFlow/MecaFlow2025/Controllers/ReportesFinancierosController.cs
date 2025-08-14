using ClosedXML.Excel;
using MecaFlow2025.Attributes;
using MecaFlow2025.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MecaFlow2025.Controllers
{
    [AuthorizeRole("Administrador")]
    public class ReportesFinancierosController : Controller
    {
        private readonly MecaFlowContext _ctx;
        public ReportesFinancierosController(MecaFlowContext ctx) => _ctx = ctx;

        // --------------------------- INDEX ---------------------------
        public async Task<IActionResult> Index(string scope = "day")
        {
            var registros = await _ctx.ReportesFinancieros
                                      .AsNoTracking()
                                      .OrderBy(r => r.Fecha)
                                      .ToListAsync();

            string[] mesesEs = new[]
            { "Enero","Febrero","Marzo","Abril","Mayo","Junio",
              "Julio","Agosto","Septiembre","Octubre","Noviembre","Diciembre" };

            IEnumerable<ResumenVM> data;
            switch (scope)
            {
                case "year":
                    ViewBag.TituloScope = "AÑO";
                    data = registros
                           .GroupBy(r => r.Fecha.Year)
                           .Select(g => new ResumenVM
                           {
                               Periodo = g.Key.ToString(), // 2025
                               TotalIngresos = g.Sum(x => x.TotalIngresos ?? 0),
                               TotalGastos = g.Sum(x => x.TotalGastos ?? 0),
                               ReporteId = null
                           });
                    break;

                case "month":
                    ViewBag.TituloScope = "MES";
                    data = registros
                           .GroupBy(r => new { r.Fecha.Year, r.Fecha.Month })
                           .Select(g => new ResumenVM
                           {
                               Periodo = $"{mesesEs[g.Key.Month - 1]} {g.Key.Year}",
                               TotalIngresos = g.Sum(x => x.TotalIngresos ?? 0),
                               TotalGastos = g.Sum(x => x.TotalGastos ?? 0),
                               ReporteId = null
                           });
                    break;

                default: // "day"
                    ViewBag.TituloScope = "DÍA";
                    data = registros.Select(r => new ResumenVM
                    {
                        Periodo = r.Fecha.ToString("dd-MM-yyyy"),
                        TotalIngresos = r.TotalIngresos ?? 0,
                        TotalGastos = r.TotalGastos ?? 0,
                        ReporteId = r.ReporteId
                    });
                    break;
            }

            ViewBag.Scope = scope;
            return View(data.OrderBy(d => d.Periodo));
        }

        // --------------------------- CREATE ---------------------------
        // GET
        public IActionResult Create() => View();

        // POST
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string FechaString,
                                                decimal? TotalIngresos,
                                                decimal? TotalGastos,
                                                string? Observaciones)
        {
            if (!DateOnly.TryParseExact(FechaString, "yyyy-MM-dd", null,
                                        DateTimeStyles.None, out var fecha))
            {
                ModelState.AddModelError("FechaString", "Fecha inválida");
                return View();
            }

            var registro = new ReportesFinanciero
            {
                Fecha = fecha,
                TotalIngresos = TotalIngresos,
                TotalGastos = TotalGastos,
                Observaciones = Observaciones
            };

            _ctx.Add(registro);
            await _ctx.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // --------------------------- EDIT ---------------------------
        public async Task<IActionResult> Edit(int id)
        {
            var reg = await _ctx.ReportesFinancieros.FindAsync(id);
            return reg is null ? NotFound() : View(reg);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
                                              string FechaString,
                                              decimal? TotalIngresos,
                                              decimal? TotalGastos,
                                              string? Observaciones)
        {
            var reg = await _ctx.ReportesFinancieros.FindAsync(id);
            if (reg is null) return NotFound();

            if (!DateOnly.TryParseExact(FechaString, "yyyy-MM-dd", null,
                                        DateTimeStyles.None, out var fecha))
            {
                ModelState.AddModelError("FechaString", "Fecha inválida");
                return View(reg);
            }

            reg.Fecha = fecha;
            reg.TotalIngresos = TotalIngresos;
            reg.TotalGastos = TotalGastos;
            reg.Observaciones = Observaciones;

            await _ctx.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // --------------------------- DETAILS ---------------------------
        public async Task<IActionResult> Details(int id)
        {
            var reg = await _ctx.ReportesFinancieros
                                .AsNoTracking()
                                .FirstOrDefaultAsync(r => r.ReporteId == id);

            return reg is null ? NotFound() : View(reg);
        }

        // --------------------------- DELETE ---------------------------
        public async Task<IActionResult> Delete(int id)
        {
            var reg = await _ctx.ReportesFinancieros.FindAsync(id);
            return reg is null ? NotFound() : View(reg);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reg = await _ctx.ReportesFinancieros.FindAsync(id);
            if (reg is not null)
            {
                _ctx.Remove(reg);
                await _ctx.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // ---------------------- EXPORT HELPERS ------------------------
        private IEnumerable<ResumenVM> BuildResumen(string scope)
        {
            var registros = _ctx.ReportesFinancieros
                                .AsNoTracking()
                                .OrderBy(r => r.Fecha)
                                .ToList();

            string[] mesesEs = new[]
            { "Enero","Febrero","Marzo","Abril","Mayo","Junio",
              "Julio","Agosto","Septiembre","Octubre","Noviembre","Diciembre" };

            switch (scope)
            {
                case "year":
                    return registros
                           .GroupBy(r => r.Fecha.Year)
                           .Select(g => new ResumenVM
                           {
                               Periodo = g.Key.ToString(),
                               TotalIngresos = g.Sum(x => x.TotalIngresos ?? 0),
                               TotalGastos = g.Sum(x => x.TotalGastos ?? 0),
                               ReporteId = null
                           })
                           .OrderBy(x => x.Periodo);

                case "month":
                    return registros
                           .GroupBy(r => new { r.Fecha.Year, r.Fecha.Month })
                           .Select(g => new ResumenVM
                           {
                               Periodo = $"{mesesEs[g.Key.Month - 1]} {g.Key.Year}",
                               TotalIngresos = g.Sum(x => x.TotalIngresos ?? 0),
                               TotalGastos = g.Sum(x => x.TotalGastos ?? 0),
                               ReporteId = null
                           })
                           .OrderBy(x => x.Periodo);

                default: // "day"
                    return registros
                           .Select(r => new ResumenVM
                           {
                               Periodo = r.Fecha.ToString("dd-MM-yyyy"),
                               TotalIngresos = r.TotalIngresos ?? 0,
                               TotalGastos = r.TotalGastos ?? 0,
                               ReporteId = r.ReporteId
                           })
                           .OrderBy(x => x.Periodo);
            }
        }

        // --------------------------- EXPORT EXCEL ---------------------------
        [HttpGet]
        public IActionResult ExportToExcel(string scope = "day")
        {
            var data = BuildResumen(scope).ToList();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Reportes");

            // Encabezados
            ws.Cell(1, 1).Value = "Período";
            ws.Cell(1, 2).Value = "Ingresos (₡)";
            ws.Cell(1, 3).Value = "Gastos (₡)";
            ws.Cell(1, 4).Value = "Utilidad (₡)";
            ws.Range(1, 1, 1, 4).Style.Font.SetBold();

            // Datos
            int row = 2;
            foreach (var r in data)
            {
                ws.Cell(row, 1).Value = r.Periodo;
                ws.Cell(row, 2).Value = r.TotalIngresos;
                ws.Cell(row, 3).Value = r.TotalGastos;
                ws.Cell(row, 4).Value = r.Utilidad;
                row++;
            }

            // Formato números
            ws.Column(2).Style.NumberFormat.Format = "#,##0.00";
            ws.Column(3).Style.NumberFormat.Format = "#,##0.00";
            ws.Column(4).Style.NumberFormat.Format = "#,##0.00";

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            var bytes = stream.ToArray();

            var fileName = $"Reportes_{scope}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        // --------------------------- EXPORT PDF ---------------------------
        [HttpGet]
        public IActionResult ExportToPdf(string scope = "day")
        {
            var data = BuildResumen(scope).ToList();

            var titleScope = scope switch
            {
                "year" => "AÑO",
                "month" => "MES",
                _ => "DÍA"
            };

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    // HEADER con color y título grande
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("MecaFlow Taller").FontSize(16).Bold().FontColor("#1a5276");
                            col.Item().Text($"Reporte Financiero — {titleScope}").FontSize(22).Bold();
                            col.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(10).FontColor("#7d7d7d");
                        });
                        // Si quieres agregar un logo
                        // row.ConstantItem(80).Image("wwwroot/images/logo.png");
                    });

                    // ESPACIO
                    page.Content().PaddingVertical(15).Column(col =>
                    {
                        col.Item().Table(table =>
                        {
                            // Columnas
                            table.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn(3);
                                cols.RelativeColumn(2);
                                cols.RelativeColumn(2);
                                cols.RelativeColumn(2);
                            });

                            // Encabezado con fondo
                            table.Header(header =>
                            {
                                string headerBg = "#154360";
                                string headerColor = "#FFFFFF";

                                header.Cell().Background(headerBg).Padding(5).Text("Período").FontColor(headerColor).Bold();
                                header.Cell().Background(headerBg).Padding(5).Text("Ingresos (₡)").FontColor(headerColor).Bold();
                                header.Cell().Background(headerBg).Padding(5).Text("Gastos (₡)").FontColor(headerColor).Bold();
                                header.Cell().Background(headerBg).Padding(5).Text("Utilidad (₡)").FontColor(headerColor).Bold();
                            });

                            // Filas
                            bool alt = false;
                            foreach (var r in data)
                            {
                                string bgColor = alt ? "#f2f3f4" : "#ffffff";
                                alt = !alt;

                                table.Cell().Background(bgColor).Padding(5).Text(r.Periodo);
                                table.Cell().Background(bgColor).Padding(5).Text(r.TotalIngresos.ToString("N2"));
                                table.Cell().Background(bgColor).Padding(5).Text(r.TotalGastos.ToString("N2"));
                                table.Cell().Background(bgColor).Padding(5)
                                     .Text(r.Utilidad.ToString("N2"))
                                     .FontColor(r.Utilidad < 0 ? "#c0392b" : "#27ae60");
                            }

                            // Totales al final
                            table.Cell().ColumnSpan(3).AlignRight().Padding(5).Background("#d5dbdb").Text("TOTAL UTILIDAD:").Bold();
                            table.Cell().Padding(5).Background("#d5dbdb")
                                 .Text(data.Sum(x => x.Utilidad).ToString("N2"))
                                 .Bold()
                                 .FontColor(data.Sum(x => x.Utilidad) < 0 ? "#c0392b" : "#27ae60");
                        });
                    });

                    // FOOTER
                    page.Footer().AlignCenter().Text(txt =>
                    {
                        txt.DefaultTextStyle(x => x.FontSize(9).FontColor("#7d7d7d"));
                        txt.Span("Reporte generado automáticamente por ");
                        txt.Span("MecaFlow").Bold();
                    });
                });
            }).GeneratePdf();

            var fileName = $"Reportes_{scope}_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }



        /*--------------------- View-Model -----------------------------*/
        public class ResumenVM
        {
            public string Periodo { get; set; } = "";
            public decimal TotalIngresos { get; set; }
            public decimal TotalGastos { get; set; }
            public decimal Utilidad => TotalIngresos - TotalGastos;
            public int? ReporteId { get; set; }
        }
    }
}
