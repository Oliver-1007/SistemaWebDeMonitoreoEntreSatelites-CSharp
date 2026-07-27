using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using OrbitNet.Core.Models;
using OrbitNet.Core.Nodes;
using OrbitNet.Services;
using OrbitNet.Services.Data;
using OrbitNet.Services.DTOs;
using OrbitNet.WebSur.Attributes;

namespace OrbitNet.WebSur.Controllers
{
    /// <summary>
    /// Controlador especializado en exponer endpoints REST en formato JSON
    /// según la especificación oficial del Proyecto Único OrbitNet-NetCore.
    /// Rutas base: /api/v1/space/
    /// </summary>
    [ApiController]
    [Route("api/v1/space")]
    public class ApiController : Controller
    {
        // -----------------------------------------------------------------------
        // Endpoint 1: POST /api/v1/space/config
        // Ingesta de configuración XML enviada como JSON payload.
        // -----------------------------------------------------------------------

        /// <summary>
        /// Endpoint POST: /api/v1/space/config (Público)
        /// Recibe un payload JSON con el campo "xml_data" y lo procesa
        /// transaccionalmente en las estructuras TDA en memoria RAM.
        /// </summary>
        /// 
        private readonly OrbitNet.Services.DistributedRoutingService _routingService;
        private readonly int _puertoHermano;

        public ApiController(
            OrbitNet.Services.DistributedRoutingService routingService,
            IConfiguration config)
        {
            _routingService = routingService;
            _puertoHermano = config.GetValue<int>("OrbitNet:PuertoHermano");
        }

        [HttpPost("config")]
        public IActionResult CargarConfiguracion([FromBody] JsonElement payload)
        {
            if (payload.ValueKind == JsonValueKind.Undefined)
            {
                MemoriaPlano.Logs.Registrar("ERROR", "POST /config: Payload JSON vacío o corrupto.");
                return BadRequest(new
                {
                    status = "Error",
                    error_code = "PAYLOAD_INVALID",
                    details = "El cuerpo de la petición está vacío o no es JSON válido."
                });
            }

            // Extraer el campo xml_data del payload
            string xmlData;
            try
            {
                xmlData = payload.GetProperty("xml_data").GetString() ?? "";
            }
            catch
            {
                MemoriaPlano.Logs.Registrar("ERROR", "POST /config: Campo 'xml_data' ausente en payload.");
                return BadRequest(new
                {
                    status = "Error",
                    error_code = "MISSING_XML_DATA",
                    details = "El campo requerido 'xml_data' no está presente en el payload JSON."
                });
            }

            if (string.IsNullOrWhiteSpace(xmlData))
            {
                MemoriaPlano.Logs.Registrar("ALERT", "POST /config: xml_data vacío.");
                return BadRequest(new
                {
                    status = "Error",
                    error_code = "EMPTY_XML_DATA",
                    details = "El campo 'xml_data' no puede estar vacío."
                });
            }

            // Delegar el procesamiento al servicio de ingesta XML
            var resultado = OrbitNet.Services.Ingesta.XmlIngestService.ProcesarXml(xmlData);

            if (resultado.Exitoso)
            {
                MemoriaPlano.Logs.Registrar("INFO", $"POST /config exitoso. Nodos procesados: {resultado.NodosProcesados}.");
                return Ok(new
                {
                    status = "Success",
                    message = $"Configuración cargada exitosamente en RAM. Nodos procesados: {resultado.NodosProcesados}.",
                    timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
                });
            }
            else
            {
                MemoriaPlano.Logs.Registrar("ERROR", $"POST /config fallido. Causa: {resultado.CausaFallo}");
                return BadRequest(new
                {
                    status = "Error",
                    error_code = "XML_SCHEMA_VIOLATION",
                    details = resultado.CausaFallo
                });
            }
        }

        // -----------------------------------------------------------------------
        // Endpoint 2: POST /api/v1/space/relay  (Protegido con HTTP Basic Auth)
        // Transferencia y enrutamiento inter-satelital entre hemisferios.
        // -----------------------------------------------------------------------

        /// <summary>
        /// Endpoint POST: /api/v1/space/relay (Protegido con HTTP Basic Auth)
        /// Recibe un paquete de datos del hemisferio opuesto, valida credenciales
        /// y lo inserta en el buffer de prioridad del satélite receptor local.
        /// </summary>
        [BasicAuthorize]
        [HttpPost("relay")]
        public IActionResult RecibirPaqueteDistribuido([FromBody] JsonElement paquetePayload)
        {
            if (paquetePayload.ValueKind == JsonValueKind.Undefined)
            {
                MemoriaPlano.Logs.Registrar("ALERT", "POST /relay rechazado: Payload JSON vacío o corrupto.");
                return BadRequest(new
                {
                    status = "Error",
                    details = "Payload inválido o ausente."
                });
            }

            // Extraer campos del payload según la especificación oficial
            string hexCode, emisorId, destinoIp, contenido;
            int prioridad;

            try
            {
                hexCode = paquetePayload.GetProperty("codigo_hex").GetString()?.Trim().ToUpper() ?? "";
                emisorId = paquetePayload.GetProperty("emisor_id").GetString()?.Trim().ToUpper() ?? "";
                destinoIp = paquetePayload.GetProperty("destino_ip").GetString()?.Trim() ?? "";
                prioridad = paquetePayload.GetProperty("prioridad").GetInt32();
                contenido = paquetePayload.GetProperty("contenido").GetString() ?? "";
            }
            catch (Exception ex)
            {
                MemoriaPlano.Logs.Registrar("ERROR", $"POST /relay: Error al parsear payload — {ex.Message}");
                return BadRequest(new
                {
                    status = "Error",
                    details = $"Estructura del payload incorrecta: {ex.Message}"
                });
            }

            // Validar rango de prioridad (1-5)
            if (prioridad < 1 || prioridad > 5)
            {
                MemoriaPlano.Logs.Registrar("ALERT", $"POST /relay: Prioridad fuera de rango ({prioridad}).");
                return BadRequest(new { status = "Error", details = "La prioridad debe estar entre 1 y 5." });
            }

            // Buscar el satélite receptor en la matriz local por IP de destino
            MatrizNode[] nodos = MemoriaPlano.Matriz.ObtenerTodosLosNodos();
            MatrizNode? receptor = null;

            for (int i = 0; i < nodos.Length; i++)
            {
                if (nodos[i].IpAddress.Equals(destinoIp, StringComparison.OrdinalIgnoreCase))
                {
                    receptor = nodos[i];
                    break;
                }
            }

            // Si no hay coincidencia exacta por IP, usar el primer nodo disponible
            if (receptor == null && nodos.Length > 0)
            {
                receptor = nodos[0];
            }

            if (receptor == null)
            {
                MemoriaPlano.Logs.Registrar("ALERT", $"POST /relay: No hay satélites en la red local para recibir el paquete {hexCode}.");
                return StatusCode(503, new
                {
                    status = "Error",
                    details = "No existen satélites en este hemisferio para recibir el paquete."
                });
            }

            // Insertar el paquete en el buffer de prioridad ABB del satélite receptor
            AbbNode nuevoPaquete = new AbbNode(hexCode, emisorId, destinoIp, prioridad, contenido);
            receptor.Buffer.InsertarFinal(nuevoPaquete);

            int ocupados = receptor.Buffer.Contar;
            double pct = Math.Min(100.0, (ocupados / 5.0) * 100.0);

            string msgInfo = $"[RELAY] Paquete {hexCode} (Prioridad {prioridad}) insertado en buffer de {receptor.Id}.";
            MemoriaPlano.Logs.Registrar("INFO", msgInfo);

            return StatusCode(201, new
            {
                status = "Routed",
                message = $"Mensaje insertado con éxito en el buffer de prioridad del satélite receptor {receptor.Id}.",
                queue_occupancy_percentage = Math.Round(pct, 1)
            });
        }

        // -----------------------------------------------------------------------
        // Endpoint 3: POST /api/v1/space/simulation/step
        // Avance de simulación por ticks.
        // -----------------------------------------------------------------------

        /// <summary>
        /// Endpoint POST: /api/v1/space/simulation/step (Público)
        /// Avanza la simulación el número de ticks indicado en el payload,
        /// rotando los buffers y procesando los mensajes de máxima prioridad.
        /// </summary>
        [HttpPost("simulation/step")]
        public async Task<IActionResult> AvanzarSimulacion([FromBody] JsonElement payload)
        {
            int ticks = 1;
            try
            {
                if (payload.ValueKind != JsonValueKind.Undefined &&
                    payload.TryGetProperty("ticks", out JsonElement ticksEl))
                {
                    ticks = ticksEl.GetInt32();
                }
            }
            catch { ticks = 1; }

            if (ticks < 1) ticks = 1;

            MatrizNode[] nodos = MemoriaPlano.Matriz.ObtenerTodosLosNodos();
            int eventosTotal = 0;
            int saltosLogicos = 0;
            int saltosRemotos = 0;

            for (int t = 0; t < ticks; t++)
            {
                for (int i = 0; i < nodos.Length; i++)
                {
                    MatrizNode nodo = nodos[i];

                    if (!nodo.Buffer.IsEmpty)
                    {
                        AbbNode? paquete = nodo.Buffer.EliminarMax();
                        if (paquete != null)
                        {
                            eventosTotal++;
                            saltosLogicos++;

                            bool esLocal = EsDestinoLocal(paquete.DestIp, nodos);

                            if (esLocal)
                            {
                                MemoriaPlano.Logs.Registrar("INFO",
                                    $"[TICK {MemoriaPlano.TickActual + t + 1}] Paquete {paquete.HexCode} " +
                                    $"(Prioridad {paquete.Priority}) entregado localmente desde {nodo.Id} hacia IP {paquete.DestIp}.");
                            }
                            else
                            {
                                var payloadRelay = new
                                {
                                    codigo_hex = paquete.HexCode,
                                    emisor_id = paquete.EmisorId,
                                    destino_ip = paquete.DestIp,
                                    prioridad = paquete.Priority,
                                    contenido = paquete.Content
                                };

                                bool enviado = await _routingService.ReenviarPaqueteAInterHemisferioAsync(
                                    paquete.DestIp, payloadRelay, _puertoHermano);

                                if (enviado) saltosRemotos++;
                                // Los logs de éxito/error ya los registra el propio DistributedRoutingService
                            }
                        }
                    }
                }
            }

            MemoriaPlano.TickActual += ticks;

            return Ok(new
            {
                status = "Simulated",
                current_tick = MemoriaPlano.TickActual,
                events_processed = eventosTotal,
                details = $"Órbitas rotadas exitosamente. Se ejecutaron {saltosLogicos} saltos lógicos " +
                          $"({saltosRemotos} hacia la instancia hermana)."
            });
        }

        private bool EsDestinoLocal(string destIp, MatrizNode[] nodos)
        {
            for (int i = 0; i < nodos.Length; i++)
            {
                if (nodos[i].IpAddress.Equals(destIp, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        // -----------------------------------------------------------------------
        // Endpoints auxiliares de consulta (Públicos y Protegidos)
        // -----------------------------------------------------------------------

        /// <summary>
        /// Endpoint GET: /api/v1/space/satelites (Público)
        /// Retorna el listado completo de satélites en la Matriz Dispersa.
        /// </summary>
        [HttpGet("satelites")]
        public IActionResult ObtenerSatelites()
        {
            MatrizNode[] nodos = MemoriaPlano.Matriz.ObtenerTodosLosNodos();
            SateliteDto[] dtos = new SateliteDto[nodos.Length];

            for (int i = 0; i < nodos.Length; i++)
            {
                dtos[i] = new SateliteDto
                {
                    Fila = nodos[i].Fila,
                    Columna = nodos[i].Columna,
                    Id = nodos[i].Id,
                    Nombre = nodos[i].Nombre,
                    IpAddress = nodos[i].IpAddress
                };
            }

            return Json(dtos);
        }

        /// <summary>
        /// Endpoint GET: /api/v1/space/satelites/seguro (Protegido con HTTP Basic Auth)
        /// Igual al anterior pero requiere credenciales válidas.
        /// </summary>
        [BasicAuthorize]
        [HttpGet("satelites/seguro")]
        public IActionResult ObtenerSatelitesSeguro()
        {
            MatrizNode[] nodos = MemoriaPlano.Matriz.ObtenerTodosLosNodos();
            SateliteDto[] dtos = new SateliteDto[nodos.Length];

            for (int i = 0; i < nodos.Length; i++)
            {
                dtos[i] = new SateliteDto
                {
                    Fila = nodos[i].Fila,
                    Columna = nodos[i].Columna,
                    Id = nodos[i].Id,
                    Nombre = nodos[i].Nombre,
                    IpAddress = nodos[i].IpAddress
                };
            }

            return Json(dtos);
        }

        /// <summary>
        /// Endpoint GET: /api/v1/space/logs (Público)
        /// Retorna la bitácora de auditoría completa en formato JSON.
        /// </summary>
        [HttpGet("logs")]
        public IActionResult ObtenerLogs()
        {
            LogRegistro[] registros = MemoriaPlano.Logs.ObtenerTodosLosNodos();
            return Json(registros);
        }
    }
}