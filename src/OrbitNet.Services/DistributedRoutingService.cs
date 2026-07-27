using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using OrbitNet.Services.Data;

namespace OrbitNet.Services
{
    /// <summary>
    /// Servicio de enrutamiento distribuido inter-hemisférico.
    /// Serializa un paquete a JSON y lo envía vía HTTP POST al servidor hermano
    /// usando HTTP Basic Authentication con las credenciales oficiales del proyecto.
    /// No usa colecciones de System.Collections.
    /// </summary>
    public class DistributedRoutingService
    {
        private readonly IHttpClientFactory _clientFactory;

        // Credenciales oficiales del protocolo de seguridad OrbitNet
        private const string Usuario = "orbitnet_admin";
        private const string Contrasena = "USAC_ECYS_2026";

        public DistributedRoutingService(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        /// <summary>
        /// Evalúa el destino de un paquete y lo despacha al endpoint REST seguro
        /// del servidor hermano en el puerto indicado.
        /// </summary>
        /// <param name="destinoIp">IP de la antena de destino para registro en logs.</param>
        /// <param name="payloadPaquete">Objeto a serializar como JSON body.</param>
        /// <param name="puertoDestino">Puerto del servidor hermano (ej. 5001).</param>
        /// <returns>True si el servidor remoto respondió 2xx; false en caso contrario.</returns>
        public async Task<bool> ReenviarPaqueteAInterHemisferioAsync(
            string destinoIp,
            object payloadPaquete,
            int puertoDestino)
        {
            var client = _clientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(5); // <-- agregar esto

            // URL oficial del endpoint de relay según la especificación del proyecto
            string urlRemota = $"http://localhost:{puertoDestino}/api/v1/space/relay";

            // Serializar el payload a JSON
            string jsonBody = JsonSerializer.Serialize(payloadPaquete);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            // Construir y codificar las credenciales Basic Auth
            string rawCredentials = $"{Usuario}:{Contrasena}";
            string base64Credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(rawCredentials));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);

            try
            {
                MemoriaPlano.Logs.Registrar("INFO",
                    $"[RELAY] Iniciando transmisión HTTP POST hacia puerto {puertoDestino} (IP destino: {destinoIp}).");

                HttpResponseMessage response = await client.PostAsync(urlRemota, content);

                if (response.IsSuccessStatusCode)
                {
                    MemoriaPlano.Logs.Registrar("INFO",
                        $"[RELAY] Paquete entregado exitosamente al servidor del otro hemisferio. HTTP {(int)response.StatusCode}.");
                    return true;
                }

                MemoriaPlano.Logs.Registrar("ERROR",
                    $"[RELAY] El servidor remoto rechazó la petición. HTTP {(int)response.StatusCode}.");
                return false;
            }
            catch (Exception ex)
            {
                MemoriaPlano.Logs.Registrar("ERROR",
                    $"[RELAY] Fallo crítico en el canal de red: {ex.Message}");
                return false;
            }
        }
    }
}
