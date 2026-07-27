using Microsoft.AspNetCore.Mvc;
using OrbitNet.Services.Data;
using OrbitNet.Services.Visualizacion;
using OrbitNet.WebSur.Models;

namespace OrbitNet.WebSur.Controllers
{
    /// <summary>
    /// Controlador principal del Dashboard de OrbitNet-NetCore.
    /// Agrega el estado de todos los TDAs en un ViewModel y genera el SVG
    /// del plano satelital en tiempo real desde la RAM del servidor.
    /// </summary>
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            // Recuperar mensajes de retroalimentación de otros controladores
            if (TempData["SuccessMessage"] != null)
                ViewBag.SuccessMessage = TempData["SuccessMessage"]!.ToString();

            if (TempData["ErrorMessage"] != null)
                ViewBag.ErrorMessage = TempData["ErrorMessage"]!.ToString();

            if (TempData["ResultadoHttp"] != null)
                ViewBag.ResultadoHttp = TempData["ResultadoHttp"]!.ToString();

            // Inicializar logs del sistema si la bitácora está vacía
            if (MemoriaPlano.Logs.IsEmpty)
            {
                MemoriaPlano.Logs.Registrar("INFO", "Sistema OrbitNet-NetCore activo. TDA Matriz Dispersa Ortogonal inicializada en RAM.");
                MemoriaPlano.Logs.Registrar("INFO", "TDA Catálogo AVL inicializado. TDA BufferMensajes ABB listo por satélite.");
                MemoriaPlano.Logs.Registrar("INFO", "Cliente HTTP nativo habilitado para comunicación distribuida entre hemisferios.");
            }

            return View(GetDashboardViewModel());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // -----------------------------------------------------------------------
        // Helpers privados
        // -----------------------------------------------------------------------

        private DashBoardViewModel GetDashboardViewModel()
        {
            // Generar el SVG del Reporte 1 (Memory Layout Map) pasando la ruta activa
            // para que los nodos en ruta se pinten en verde (#2ECC71)
            string codigoDot = MemoriaPlano.Matriz.GenerarCodigoDot(MemoriaPlano.RutaActiva);
            string svgOutput = GraphvizCompilador.CompilarDotASvg(codigoDot);

            return new DashBoardViewModel
            {
                Matriz = MemoriaPlano.Matriz,
                Logs = MemoriaPlano.Logs,
                Catalogo = MemoriaPlano.Catalogo,
                SvgDiagrama = svgOutput
            };
        }
    }
}
