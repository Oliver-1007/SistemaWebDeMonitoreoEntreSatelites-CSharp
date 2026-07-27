using System;
using Microsoft.AspNetCore.Mvc;
using OrbitNet.Core.Nodes;
using OrbitNet.Services.Data;

namespace OrbitNet.WebSur.Controllers
{
    /// <summary>
    /// Controlador especializado en administrar las operaciones del TDA BufferMensajes (ABB)
    /// asociado a cada satélite de la red.
    /// Permite encolar y desencolar mensajes de forma prioritaria en caliente.
    /// No usa colecciones de System.Collections.
    /// </summary>
    public class BufferController : Controller
    {
        /// <summary>
        /// Acción POST: Encola un paquete de datos en el buffer ABB de un satélite específico.
        /// </summary>
        [HttpPost]
        public IActionResult Encolar(
            string sateliteId,
            string hexCode,
            string emisorId,
            string destIp,
            int priority,
            string content)
        {
            // 1. Validar identificador del satélite
            if (string.IsNullOrWhiteSpace(sateliteId))
            {
                TempData["ErrorMessage"] = "Debe especificar un satélite válido.";
                return RedirectToAction("Index", "Home");
            }

            // 2. Validar rango de prioridad (1-5)
            if (priority < 1 || priority > 5)
            {
                TempData["ErrorMessage"] = "La prioridad debe estar en un rango de 1 a 5.";
                return RedirectToAction("Index", "Home", new { sateliteId });
            }

            // 3. Validar campos obligatorios del paquete
            if (string.IsNullOrWhiteSpace(hexCode) || string.IsNullOrWhiteSpace(destIp))
            {
                TempData["ErrorMessage"] = "El código de paquete y la IP de destino son obligatorios.";
                return RedirectToAction("Index", "Home", new { sateliteId });
            }

            // 4. Buscar el nodo del satélite en la matriz dispersa (sin colecciones genéricas)
            MatrizNode[] nodos = MemoriaPlano.Matriz.ObtenerTodosLosNodos();
            MatrizNode? satelite = null;

            for (int i = 0; i < nodos.Length; i++)
            {
                if (nodos[i].Id.Equals(sateliteId, StringComparison.OrdinalIgnoreCase))
                {
                    satelite = nodos[i];
                    break;
                }
            }

            // 5. Reportar error si no se localiza el satélite
            if (satelite == null)
            {
                string msgError = $"Buffer: No existe el nodo satelital '{sateliteId}'.";
                MemoriaPlano.Logs.Registrar("ERROR", msgError);
                TempData["ErrorMessage"] = msgError;
                return RedirectToAction("Index", "Home");
            }

            // 6. Crear el nodo AbbNode e insertarlo en el ABB de prioridad del satélite
            AbbNode nuevoPaquete = new AbbNode(
                hexCode.Trim().ToUpper(),
                emisorId?.Trim().ToUpper() ?? string.Empty,
                destIp.Trim(),
                priority,
                content ?? string.Empty);

            satelite.Buffer.InsertarFinal(nuevoPaquete);

            // 7. Registrar evento en la bitácora
            string msgSucc = $"Buffer: Paquete {nuevoPaquete.HexCode} (Prioridad {priority}) encolado en '{sateliteId}'.";
            MemoriaPlano.Logs.Registrar("INFO", msgSucc);
            TempData["SuccessMessage"] = $"Paquete {nuevoPaquete.HexCode} encolado en el buffer de {satelite.Nombre}.";

            return RedirectToAction("Index", "Home", new { sateliteId });
        }

        /// <summary>
        /// Acción POST: Desencola el mensaje de máxima prioridad del buffer de un satélite.
        /// El nodo de máxima prioridad es el ubicado más a la derecha del ABB.
        /// </summary>
        [HttpPost]
        public IActionResult Desencolar(string sateliteId)
        {
            // 1. Validar identificador
            if (string.IsNullOrWhiteSpace(sateliteId))
            {
                TempData["ErrorMessage"] = "Debe especificar un satélite válido.";
                return RedirectToAction("Index", "Home");
            }

            // 2. Buscar el satélite en la matriz
            MatrizNode[] nodos = MemoriaPlano.Matriz.ObtenerTodosLosNodos();
            MatrizNode? satelite = null;

            for (int i = 0; i < nodos.Length; i++)
            {
                if (nodos[i].Id.Equals(sateliteId, StringComparison.OrdinalIgnoreCase))
                {
                    satelite = nodos[i];
                    break;
                }
            }

            if (satelite == null)
            {
                TempData["ErrorMessage"] = $"No se encontró el satélite '{sateliteId}' en la matriz.";
                return RedirectToAction("Index", "Home");
            }

            // 3. Extraer el nodo de máxima prioridad (extremo derecho del ABB)
            AbbNode? despachado = satelite.Buffer.EliminarMax();

            if (despachado == null)
            {
                string msgAlerta = $"Buffer: Intento de desencolado fallido en '{sateliteId}' — buffer vacío.";
                MemoriaPlano.Logs.Registrar("ALERT", msgAlerta);
                TempData["ErrorMessage"] = $"El buffer del satélite {sateliteId} está vacío.";
            }
            else
            {
                string msgInfo = $"Buffer: Paquete {despachado.HexCode} (Prioridad {despachado.Priority}) " +
                                 $"extraído de '{sateliteId}' hacia IP {despachado.DestIp}.";
                MemoriaPlano.Logs.Registrar("INFO", msgInfo);
                TempData["SuccessMessage"] = $"Desencolado paquete {despachado.HexCode} " +
                                             $"(Prioridad: {despachado.Priority}) del satélite {sateliteId}.";
            }

            return RedirectToAction("Index", "Home", new { sateliteId });
        }
    }
}