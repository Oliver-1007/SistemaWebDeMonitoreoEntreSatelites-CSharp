using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using OrbitNet.Core.Models;
using OrbitNet.Services.Data;

namespace OrbitNet.Services.Ingesta
{
    /// <summary>
    /// Resultado de una operación de ingesta XML.
    /// </summary>
    public class ResultadoIngesta
    {
        public bool Exitoso { get; set; }
        public int NodosProcesados { get; set; }
        public string CausaFallo { get; set; } = string.Empty;
    }

    /// <summary>
    /// Servicio de ingesta transaccional atómica de XML.
    /// Comparte la lógica entre el XmlController (carga por formulario web)
    /// y el ApiController (carga por endpoint REST JSON).
    /// No usa colecciones de System.Collections.
    /// </summary>
    public static class XmlIngestService
    {
        // Expresiones Regulares Oficiales del Proyecto
        private const string PatronIdSatelite = @"^SAT-(ECU|POL)-\d{4}$";
        private const string PatronIdAntena = @"^ANT-[A-Z]{3}-\d{3,4}$";
        private const string PatronIpv4 = @"^(?:(?:25[0-5]|2[0-4]\d|[01]?\d?\d)\.){3}(?:25[0-5]|2[0-4]\d|[01]?\d?\d)$";
        private const string PatronCoordenadas = @"^-?\d{1,2}\.\d{4,6},-?\d{1,3}\.\d{4,6}$";

        // -----------------------------------------------------------------------
        // Nodos temporales de lista enlazada manual (sin List<T>)
        // -----------------------------------------------------------------------

        private class NodoMatrizTemporal
        {
            public int Fila { get; }
            public int Columna { get; }
            public string Id { get; }
            public string Nombre { get; }
            public string IpAddress { get; }
            public NodoMatrizTemporal? Siguiente { get; set; }

            public NodoMatrizTemporal(int fila, int col, string id, string nombre, string ip)
            {
                Fila = fila; Columna = col; Id = id; Nombre = nombre; IpAddress = ip;
            }
        }

        private class NodoAvlTemporal
        {
            public string Id { get; }
            public string Nombre { get; }
            public double Frecuencia { get; }
            public NodoAvlTemporal? Siguiente { get; set; }

            public NodoAvlTemporal(string id, string nombre, double frecuencia)
            {
                Id = id; Nombre = nombre; Frecuencia = frecuencia;
            }
        }

        // -----------------------------------------------------------------------
        // Punto de entrada público
        // -----------------------------------------------------------------------

        /// <summary>
        /// Procesa un string XML de forma transaccional atómica.
        /// Si cualquier elemento falla la validación, se hace rollback completo
        /// y los TDAs en RAM quedan intactos.
        /// </summary>
        public static ResultadoIngesta ProcesarXml(string xmlContent)
        {
            NodoMatrizTemporal? cabezaMatriz = null;
            NodoAvlTemporal? cabezaAvl = null;
            int insertadosMatriz = 0;
            int insertadosAvl = 0;
            bool exito = true;
            string causaFallo = "";

            try
            {
                // OWASP XXE: deshabilitar resolución de entidades externas y DTD
                XmlReaderSettings settings = new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Prohibit,
                    XmlResolver = null
                };

                using (StringReader sr = new StringReader(xmlContent))
                using (XmlReader reader = XmlReader.Create(sr, settings))
                {
                    XmlDocument doc = new XmlDocument { XmlResolver = null };
                    doc.Load(reader);

                    // 1. Satélites Ecuatoriales → Matriz Dispersa (Fila = 0)
                    XmlNodeList satEcu = doc.SelectNodes("//constelaciones_ecuatoriales/satelite")!;
                    foreach (XmlNode sat in satEcu)
                    {
                        string? id = sat.Attributes?["id"]?.Value;
                        string? nombre = sat.SelectSingleNode("nombre")?.InnerText;
                        string? enlaceIp = sat.SelectSingleNode("enlace_ip")?.InnerText;

                        if (!ValidarYAgregarMatrizTemporal(
                                0, id, nombre, enlaceIp,
                                ref cabezaMatriz, ref insertadosMatriz, ref causaFallo))
                        {
                            exito = false;
                            break;
                        }
                    }

                    // 2. Satélites Polares → Catálogo AVL
                    if (exito)
                    {
                        XmlNodeList satPol = doc.SelectNodes("//orbitas_polares/polar/satelite")!;
                        foreach (XmlNode sat in satPol)
                        {
                            string? id = sat.Attributes?["id"]?.Value;
                            string? nombre = sat.SelectSingleNode("nombre")?.InnerText;
                            string? freqStr = sat.SelectSingleNode("frecuencia")?.InnerText;

                            if (!ValidarYAgregarAvlTemporal(
                                    id, nombre, freqStr,
                                    ref cabezaAvl, ref insertadosAvl, ref causaFallo))
                            {
                                exito = false;
                                break;
                            }
                        }
                    }

                    // 3. Antenas Terrestres → Matriz Dispersa (Fila = Round(Lat), Col = Round(Lon))
                    if (exito)
                    {
                        XmlNodeList antenas = doc.SelectNodes("//antenas_terrestres/antena")!;
                        foreach (XmlNode ant in antenas)
                        {
                            string? id = ant.Attributes?["id"]?.Value;
                            string? nombre = ant.SelectSingleNode("nombre")?.InnerText;
                            string? coords = ant.SelectSingleNode("coordenadas")?.InnerText;
                            string? ipNodo = ant.SelectSingleNode("ip_nodo")?.InnerText;

                            if (!ValidarYAgregarAntenaTemporal(
                                    id, nombre, coords, ipNodo,
                                    ref cabezaMatriz, ref insertadosMatriz, ref causaFallo))
                            {
                                exito = false;
                                break;
                            }
                        }
                    }
                }
            }
            catch (XmlException ex)
            {
                exito = false;
                causaFallo = $"Error de parsing XML: {ex.Message}";
            }
            catch (Exception ex)
            {
                exito = false;
                causaFallo = $"Error de procesamiento: {ex.Message}";
            }

            // -----------------------------------------------------------------------
            // COMMIT o ROLLBACK
            // -----------------------------------------------------------------------
            if (exito)
            {
                // COMMIT: traspasar los buffers temporales a los TDAs en RAM
                NodoMatrizTemporal? actualM = cabezaMatriz;
                while (actualM != null)
                {
                    MemoriaPlano.Matriz.Insertar(actualM.Fila, actualM.Columna, actualM.Id, actualM.Nombre, actualM.IpAddress);
                    actualM = actualM.Siguiente;
                }

                NodoAvlTemporal? actualA = cabezaAvl;
                while (actualA != null)
                {
                    Satelite s = new Satelite(actualA.Id, actualA.Nombre, actualA.Frecuencia);
                    MemoriaPlano.Catalogo.Insertar(s);
                    actualA = actualA.Siguiente;
                }

                return new ResultadoIngesta
                {
                    Exitoso = true,
                    NodosProcesados = insertadosMatriz + insertadosAvl,
                    CausaFallo = string.Empty
                };
            }
            else
            {
                // ROLLBACK: no se toca ningún TDA en RAM
                return new ResultadoIngesta
                {
                    Exitoso = false,
                    NodosProcesados = 0,
                    CausaFallo = causaFallo
                };
            }
        }

        // -----------------------------------------------------------------------
        // Validaciones privadas con listas temporales manuales
        // -----------------------------------------------------------------------

        private static bool ValidarYAgregarMatrizTemporal(
            int fila, string? id, string? nombre, string? enlaceIp,
            ref NodoMatrizTemporal? cabeza, ref int contador, ref string causaFallo)
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(enlaceIp))
            {
                causaFallo = "Atributos o elementos básicos incompletos para satélite ecuatorial.";
                return false;
            }

            id = id.Trim(); nombre = nombre.Trim(); enlaceIp = enlaceIp.Trim();

            if (!Regex.IsMatch(id, PatronIdSatelite))
            {
                causaFallo = $"El ID de satélite '{id}' no cumple con el formato requerido 'SAT-(ECU|POL)-0000'.";
                return false;
            }

            if (!Regex.IsMatch(enlaceIp, PatronIpv4))
            {
                causaFallo = $"La dirección IP '{enlaceIp}' para el satélite [{id}] es inválida.";
                return false;
            }

            int columna;
            try { columna = int.Parse(id.Substring(8)); }
            catch
            {
                causaFallo = $"No se pudo derivar la columna a partir del ID '{id}'.";
                return false;
            }

            if (MemoriaPlano.Matriz.Buscar(fila, columna) != null)
            {
                causaFallo = $"Colisión en Matriz RAM: ya existe un nodo en ({fila}, {columna}).";
                return false;
            }

            if (MemoriaPlano.Matriz.BuscarPorId(id) != null)
            {
                causaFallo = $"El ID '{id}' ya está registrado en el plano.";
                return false;
            }

            NodoMatrizTemporal? actual = cabeza;
            while (actual != null)
            {
                if (actual.Fila == fila && actual.Columna == columna)
                {
                    causaFallo = $"Colisión interna en XML: coordenada ({fila}, {columna}) duplicada.";
                    return false;
                }

                if (actual.Id.Equals(id, StringComparison.OrdinalIgnoreCase))
                {
                    causaFallo = $"ID duplicado en XML: '{id}'.";
                    return false;
                }

                actual = actual.Siguiente;
            }

            NodoMatrizTemporal nuevo = new NodoMatrizTemporal(fila, columna, id, nombre, enlaceIp);
            if (cabeza == null) { cabeza = nuevo; }
            else
            {
                NodoMatrizTemporal temp = cabeza;
                while (temp.Siguiente != null) temp = temp.Siguiente;
                temp.Siguiente = nuevo;
            }

            contador++;
            return true;
        }

        private static bool ValidarYAgregarAvlTemporal(
            string? id, string? nombre, string? freqStr,
            ref NodoAvlTemporal? cabeza, ref int contador, ref string causaFallo)
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(freqStr))
            {
                causaFallo = "Atributos o elementos incompletos para satélite polar.";
                return false;
            }

            id = id.Trim(); nombre = nombre.Trim(); freqStr = freqStr.Trim();

            if (!Regex.IsMatch(id, PatronIdSatelite))
            {
                causaFallo = $"El ID de satélite polar '{id}' no cumple con el formato 'SAT-(ECU|POL)-0000'.";
                return false;
            }

            if (!double.TryParse(freqStr,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out double frecuencia) || frecuencia <= 0)
            {
                causaFallo = $"La frecuencia '{freqStr}' del satélite [{id}] debe ser un número decimal positivo.";
                return false;
            }

            if (MemoriaPlano.Catalogo.Buscar(id) != null)
            {
                causaFallo = $"El satélite polar '{id}' ya existe en el Catálogo AVL.";
                return false;
            }

            NodoAvlTemporal? actual = cabeza;
            while (actual != null)
            {
                if (actual.Id.Equals(id, StringComparison.OrdinalIgnoreCase))
                {
                    causaFallo = $"ID polar duplicado en XML: '{id}'.";
                    return false;
                }

                actual = actual.Siguiente;
            }

            NodoAvlTemporal nuevo = new NodoAvlTemporal(id, nombre, frecuencia);
            if (cabeza == null) { cabeza = nuevo; }
            else
            {
                NodoAvlTemporal temp = cabeza;
                while (temp.Siguiente != null) temp = temp.Siguiente;
                temp.Siguiente = nuevo;
            }

            contador++;
            return true;
        }

        private static bool ValidarYAgregarAntenaTemporal(
            string? id, string? nombre, string? coordsStr, string? ipNodo,
            ref NodoMatrizTemporal? cabeza, ref int contador, ref string causaFallo)
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(nombre) ||
                string.IsNullOrWhiteSpace(coordsStr) || string.IsNullOrWhiteSpace(ipNodo))
            {
                causaFallo = "Atributos o elementos incompletos para antena terrestre.";
                return false;
            }

            id = id.Trim(); nombre = nombre.Trim(); coordsStr = coordsStr.Trim(); ipNodo = ipNodo.Trim();

            if (!Regex.IsMatch(id, PatronIdAntena))
            {
                causaFallo = $"El ID de antena '{id}' no cumple con el formato 'ANT-[CODIGO]-[NUMERO]'.";
                return false;
            }

            if (!Regex.IsMatch(ipNodo, PatronIpv4))
            {
                causaFallo = $"La IP de nodo '{ipNodo}' para antena [{id}] es inválida.";
                return false;
            }

            if (!Regex.IsMatch(coordsStr, PatronCoordenadas))
            {
                causaFallo = $"Las coordenadas '{coordsStr}' de la antena [{id}] no cumplen el patrón 'Lat,Lon'.";
                return false;
            }

            int fila, columna;
            try
            {
                string[] partes = coordsStr.Split(',');
                double lat = double.Parse(partes[0], System.Globalization.CultureInfo.InvariantCulture);
                double lon = double.Parse(partes[1], System.Globalization.CultureInfo.InvariantCulture);

                if (lat < -90.0 || lat > 90.0 || lon < -180.0 || lon > 180.0)
                {
                    causaFallo = $"Coordenadas de antena [{id}] fuera de límites geográficos válidos.";
                    return false;
                }

                fila = (int)Math.Round(lat);
                columna = (int)Math.Round(lon);
            }
            catch (Exception ex)
            {
                causaFallo = $"Fallo al parsear coordenadas de antena [{id}]: {ex.Message}";
                return false;
            }

            if (MemoriaPlano.Matriz.Buscar(fila, columna) != null)
            {
                causaFallo = $"Colisión en Matriz RAM para antena [{id}]: ya existe un nodo en ({fila}, {columna}).";
                return false;
            }

            if (MemoriaPlano.Matriz.BuscarPorId(id) != null)
            {
                causaFallo = $"El ID de antena '{id}' ya está registrado en el plano.";
                return false;
            }

            NodoMatrizTemporal? actual = cabeza;
            while (actual != null)
            {
                if (actual.Fila == fila && actual.Columna == columna)
                {
                    causaFallo = $"Colisión interna en XML: coordenada redondeada ({fila}, {columna}) duplicada.";
                    return false;
                }

                if (actual.Id.Equals(id, StringComparison.OrdinalIgnoreCase))
                {
                    causaFallo = $"ID de antena duplicado en XML: '{id}'.";
                    return false;
                }

                actual = actual.Siguiente;
            }

            NodoMatrizTemporal nuevo = new NodoMatrizTemporal(fila, columna, id, nombre, ipNodo);
            if (cabeza == null) { cabeza = nuevo; }
            else
            {
                NodoMatrizTemporal temp = cabeza;
                while (temp.Siguiente != null) temp = temp.Siguiente;
                temp.Siguiente = nuevo;
            }

            contador++;
            return true;
        }
    }
}
