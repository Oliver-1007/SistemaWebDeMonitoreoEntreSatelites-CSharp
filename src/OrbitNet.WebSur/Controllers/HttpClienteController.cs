using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OrbitNet.Services.Data;

namespace OrbitNet.WebSur.Controllers
{
    /// <summary>
    /// Controlador encargado de realizar peticiones HTTP salientes desde la interfaz web.
    /// Utiliza una instancia estática de HttpClient para evitar agotamiento de sockets.
    /// </summary>
    public class HttpClienteController : Controller
    {
        // Instancia estática de HttpClient recomendada para evitar el agotamiento de sockets
        // bajo cargas concurrentes (socket exhaustion).
        private static readonly HttpClient clienteHttp = new HttpClient();

        /// <summary>
        /// Acción POST: Realiza una petición GET asíncrona a la URL indicada.
        /// Inyecta credenciales Basic Auth en el header si el usuario las proporciona.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ConsultarApi(string urlDestino, string? usuario, string? contrasena)
        {
            if (string.IsNullOrWhiteSpace(urlDestino))
            {
                TempData["ErrorMessage"] = "La URL de destino no puede estar vacía.";
                return RedirectToAction("Index", "Home");
            }

            urlDestino = urlDestino.Trim();
            MemoriaPlano.Logs.Registrar("INFO", $"Cliente HTTP: Iniciando petición GET a '{urlDestino}'");

            try
            {
                using (var peticion = new HttpRequestMessage(HttpMethod.Get, urlDestino))
                {
                    // Inyectar Basic Auth si se proporcionaron credenciales
                    if (!string.IsNullOrEmpty(usuario) || !string.IsNullOrEmpty(contrasena))
                    {
                        string userVal = usuario ?? string.Empty;
                        string passVal = contrasena ?? string.Empty;
                        string credenciales = $"{userVal}:{passVal}";
                        string base64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(credenciales));

                        peticion.Headers.Authorization =
                            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", base64);

                        MemoriaPlano.Logs.Registrar("INFO",
                            $"Cliente HTTP: Inyectando cabecera Basic Auth para usuario '{userVal}'");
                    }

                    using (HttpResponseMessage response = await clienteHttp.SendAsync(peticion))
                    {
                        string rawJson = await response.Content.ReadAsStringAsync();

                        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            MemoriaPlano.Logs.Registrar("ALERT",
                                $"Cliente HTTP: Solicitud denegada (401 Unauthorized) en '{urlDestino}'");
                            TempData["ResultadoHttp"] = rawJson;
                            TempData["ErrorMessage"] = "Petición rechazada: 401 No Autorizado.";
                        }
                        else
                        {
                            response.EnsureSuccessStatusCode();
                            MemoriaPlano.Logs.Registrar("INFO",
                                $"Cliente HTTP: Petición exitosa. Código: {(int)response.StatusCode}");
                            TempData["ResultadoHttp"] = rawJson;
                            TempData["SuccessMessage"] = "Respuesta HTTP recibida con éxito.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"Cliente HTTP: Falló la petición a '{urlDestino}'. Detalle: {ex.Message}";
                MemoriaPlano.Logs.Registrar("ERROR", errorMsg);
                TempData["ErrorMessage"] = errorMsg;
            }

            return RedirectToAction("Index", "Home");
        }
    }
}

