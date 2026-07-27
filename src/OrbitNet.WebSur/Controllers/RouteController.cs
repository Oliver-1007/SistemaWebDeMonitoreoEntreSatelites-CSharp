using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using OrbitNet.Core.Nodes;
using OrbitNet.Services.Algoritmos;
using OrbitNet.Services.Data;

namespace OrbitNet.WebSur.Controllers
{
    /// <summary>
    /// Controlador encargado de gestionar las operaciones de enrutamiento lógico
    /// de saltos ortogonales a través de la Matriz Dispersa.
    /// Usa el EnrutadorOrtogonal (DFS manual sin colecciones genéricas).
    /// </summary>
    public class RouteController : Controller
    {
        private readonly EnrutadorOrtogonal _enrutador = new EnrutadorOrtogonal();

        /// <summary>
        /// Acción POST: Traza la ruta de saltos ortogonales entre dos satélites
        /// usando el EnrutadorOrtogonal (DFS con backtracking manual).
        /// Guarda la ruta calculada en MemoriaPlano.RutaActiva para la vista.
        /// </summary>
        [HttpPost]
        public IActionResult Trazar(string origenId, string destinoId)
        {
            // 1. Validar selección de origen y destino
            if (string.IsNullOrWhiteSpace(origenId) || string.IsNullOrWhiteSpace(destinoId))
            {
                TempData["ErrorMessage"] = "Debe seleccionar un satélite de origen y uno de destino.";
                return RedirectToAction("Index", "Home");
            }

            if (origenId.Equals(destinoId, StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "El satélite de origen y destino no pueden ser el mismo.";
                return RedirectToAction("Index", "Home");
            }

            // 2. Calcular la ruta a través de los punteros físicos de la matriz
            MatrizNode[] ruta = _enrutador.EncontrarRuta(MemoriaPlano.Matriz, origenId, destinoId);

            // 3. Evaluar resultado
            if (ruta.Length > 0)
            {
                MemoriaPlano.RutaActiva = ruta;
                int saltos = ruta.Length - 1;

                // Construir representación textual del camino para logs
                StringBuilder pathStr = new StringBuilder();
                for (int i = 0; i < ruta.Length; i++)
                {
                    pathStr.Append(ruta[i].Id);
                    if (i < ruta.Length - 1)
                        pathStr.Append(" -> ");
                }

                MemoriaPlano.Logs.Registrar("INFO",
                    $"Enrutamiento: Ruta calculada [{pathStr}]. Saltos totales: {saltos}.");
                TempData["SuccessMessage"] = $"Ruta trazada con éxito. Saltos totales: {saltos}.";
            }
            else
            {
                MemoriaPlano.RutaActiva = null;
                string msgAlerta = $"Enrutamiento: Sin camino físico entre '{origenId}' y '{destinoId}' (malla rota).";
                MemoriaPlano.Logs.Registrar("ALERT", msgAlerta);
                TempData["ErrorMessage"] = "No se pudo trazar una ruta física entre los satélites seleccionados.";
            }

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Acción POST: Limpia el trazado de ruta activo en memoria.
        /// </summary>
        [HttpPost]
        public IActionResult Limpiar()
        {
            MemoriaPlano.RutaActiva = null;
            MemoriaPlano.Logs.Registrar("INFO", "Enrutamiento: Trazado de ruta limpiado.");
            TempData["SuccessMessage"] = "Trazado de ruta limpiado.";
            return RedirectToAction("Index", "Home");
        }
    }
}