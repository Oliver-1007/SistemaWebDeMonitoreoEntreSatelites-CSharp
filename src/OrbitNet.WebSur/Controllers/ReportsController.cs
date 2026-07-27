using System;
using Microsoft.AspNetCore.Mvc;
using OrbitNet.Core.Nodes;
using OrbitNet.Services.Data;
using OrbitNet.Services.Visualizacion;

namespace OrbitNet.WebSur.Controllers
{
    /// <summary>
    /// Controlador especializado en compilar y exponer los tres reportes
    /// vectoriales SVG obligatorios del simulador (Sección 8 de la
    /// especificación): Mapa de Memoria, Trazabilidad de Ruta y Matriz
    /// Unificada de Buffers.
    /// Regla de dependencia unidireccional respetada:
    /// Vista -> Controlador (este) -> Capa de Servicio (ReportGenerator /
    /// GraphvizCompilador) -> TDA (MemoriaPlano.Matriz).
    /// </summary>
    public class ReportsController : Controller
    {
        // Capacidad máxima asumida por satélite para el medidor de ocupación
        // del Reporte 3. Ajustar aquí cuando BufferMensajes exponga una
        // capacidad configurable real.
        private const int CapacidadMaximaBuffer = 5;

        private readonly ReportGenerator _reportGenerator = new ReportGenerator();

        /// <summary>
        /// GET: /Reports/MemoryLayout
        /// Reporte 1 — Mapa físico de memoria de la Matriz Dispersa Ortogonal
        /// (shape=record, punteros prev/data/next). Reutiliza el mismo método
        /// que ya consume HomeController.GetDashboardViewModel() para que
        /// ambos reportes permanezcan sincronizados.
        /// </summary>
        [HttpGet]
        public IActionResult MemoryLayout()
        {
            string dot = MemoriaPlano.Matriz.GenerarCodigoDot(MemoriaPlano.RutaActiva);
            string svg = GraphvizCompilador.CompilarDotASvg(dot);
            return Content(svg, "image/svg+xml");
        }

        /// <summary>
        /// GET: /Reports/RouteTracer
        /// Reporte 2 — Trazabilidad de Ruta de Retransmisión.
        /// Colorea en verde (#2ECC71) los nodos que forman parte de la
        /// última ruta calculada por RouteController.Trazar() y almacenada
        /// en MemoriaPlano.RutaActiva; el resto de la red se pinta en rojo
        /// discontinuo (inactivo / fuera de ruta).
        /// </summary>
        [HttpGet]
        public IActionResult RouteTracer()
        {
            MatrizNode[] todosLosNodos = MemoriaPlano.Matriz.ObtenerTodosLosNodos();
            MatrizNode[]? rutaActiva = MemoriaPlano.RutaActiva;

            string[] idsVisitados;
            string codigoPaquete;

            if (rutaActiva != null && rutaActiva.Length > 0)
            {
                idsVisitados = new string[rutaActiva.Length];
                for (int i = 0; i < rutaActiva.Length; i++)
                {
                    idsVisitados[i] = rutaActiva[i].Id;
                }

                codigoPaquete = $"{rutaActiva[0].Id} -> {rutaActiva[rutaActiva.Length - 1].Id}";
            }
            else
            {
                // Sin ruta trazada todavía: se renderiza la red completa en rojo.
                idsVisitados = Array.Empty<string>();
                codigoPaquete = "SIN-RUTA-ACTIVA";
            }

            string dot = _reportGenerator.GenerateRelayTracerDot(todosLosNodos, idsVisitados, codigoPaquete);
            string svg = GraphvizCompilador.CompilarDotASvg(dot);
            return Content(svg, "image/svg+xml");
        }

        /// <summary>
        /// GET: /Reports/BufferMatrix
        /// Reporte 3 — Matriz unificada de capacidad y estado del buffer
        /// satelital. Para cada satélite activo de la red expone su
        /// porcentaje de ocupación de cola de prioridad (TDA BufferMensajes).
        /// </summary>
        [HttpGet]
        public IActionResult BufferMatrix()
        {
            MatrizNode[] todosLosNodos = MemoriaPlano.Matriz.ObtenerTodosLosNodos();
            string dot = _reportGenerator.GenerateBufferMatrixDot(todosLosNodos, CapacidadMaximaBuffer);
            string svg = GraphvizCompilador.CompilarDotASvg(dot);
            return Content(svg, "image/svg+xml");
        }
    }
}
