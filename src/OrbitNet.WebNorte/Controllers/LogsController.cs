using Microsoft.AspNetCore.Mvc;
using OrbitNet.Services.Data;

namespace OrbitNet.WebNorte.Controllers
{
    /// <summary>
    /// Controlador especializado en administrar los registros de auditoría.
    /// </summary>
    public class LogsController : Controller
    {
        /// <summary>
        /// Acción POST: Purga completamente la bitácora de auditoría en RAM.
        /// </summary>
        [HttpPost]
        public IActionResult LimpiarLogs()
        {
            MemoriaPlano.Logs.Clear();
            MemoriaPlano.Logs.Registrar("INFO", "Se purgó la bitácora de auditoría.");
            TempData["SuccessMessage"] = "Bitácora de auditoría reiniciada.";
            return RedirectToAction("Index", "Home");
        }
    }
}


