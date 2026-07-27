using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrbitNet.Services.Data;
using OrbitNet.Services.Ingesta;

namespace OrbitNet.WebSur.Controllers
{
    /// <summary>
    /// Controlador especializado en el procesamiento e ingesta transaccional
    /// de archivos XML subidos desde el formulario web de carga masiva.
    /// Delega la lógica de validación y commit al servicio XmlIngestService
    /// para mantener el principio de responsabilidad única.
    /// </summary>
    public class XmlController : Controller
    {
        /// <summary>
        /// Acción POST: Recibe un archivo XML subido por el formulario web,
        /// lo lee como string y lo procesa transaccionalmente vía XmlIngestService.
        /// </summary>
        [HttpPost]
        public IActionResult CargarXml(IFormFile archivoXml)
        {
            if (archivoXml == null || archivoXml.Length == 0)
            {
                string msgErr = "Por favor, seleccione un archivo XML válido.";
                MemoriaPlano.Logs.Registrar("ALERT", msgErr);
                TempData["ErrorMessage"] = msgErr;
                return RedirectToAction("Index", "Home");
            }

            MemoriaPlano.Logs.Registrar("INFO", $"Iniciando ingesta de archivo XML: '{archivoXml.FileName}'");

            string xmlContent;
            try
            {
                using (var reader = new System.IO.StreamReader(archivoXml.OpenReadStream()))
                {
                    xmlContent = reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                string msgErr = $"No se pudo leer el archivo XML: {ex.Message}";
                MemoriaPlano.Logs.Registrar("ERROR", msgErr);
                TempData["ErrorMessage"] = msgErr;
                return RedirectToAction("Index", "Home");
            }

            // Delegar la lógica al servicio compartido (también usado por ApiController)
            ResultadoIngesta resultado = XmlIngestService.ProcesarXml(xmlContent);

            if (resultado.Exitoso)
            {
                string msgSucc = $"Carga XML exitosa (Commit). Se procesaron {resultado.NodosProcesados} elementos en los TDAs.";
                MemoriaPlano.Logs.Registrar("INFO", msgSucc);
                TempData["SuccessMessage"] = msgSucc;
            }
            else
            {
                string msgFallo = $"Transacción XML abortada (Rollback). Causa: {resultado.CausaFallo}. El plano permanece intacto.";
                MemoriaPlano.Logs.Registrar("ERROR", msgFallo);
                TempData["ErrorMessage"] = msgFallo;
            }

            return RedirectToAction("Index", "Home");
        }
    }
}