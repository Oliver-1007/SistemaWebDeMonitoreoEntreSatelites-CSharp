using System.Text.RegularExpressions;

namespace OrbitNet.Services.Validaciones
{
    public static class RegexValidator
    {
        // PATRONES OFICIALES MUDADOS DEL XML CONTROLLER
        private static readonly Regex SatIdRegex = new Regex(@"^SAT-(ECU|POL)-\d{4}$", RegexOptions.Compiled);
        private static readonly Regex AntIdRegex = new Regex(@"^ANT-[A-Z]{3}-\d{3,4}$", RegexOptions.Compiled); // Ajustado a 3,4
        private static readonly Regex Ipv4Regex = new Regex(@"^(?:(?:25[0-5]|2[0-4]\d|[01]?\d?\d)\.){3}(?:25[0-5]|2[0-4]\d|[01]?\d?\d)$", RegexOptions.Compiled); // IP Oficial
        private static readonly Regex CoordenadaParRegex = new Regex(@"^-?\d{1,2}\.\d{4,6},-?\d{1,3}\.\d{4,6}$", RegexOptions.Compiled); // Coordenadas Oficiales Estrictas
        private static readonly Regex FrecuenciaRegex = new Regex(@"^\d+(\.\d+)?$", RegexOptions.Compiled);

        // Métodos de validación en español
        public static bool ValidarSateliteId(string id) => !string.IsNullOrWhiteSpace(id) && SatIdRegex.IsMatch(id);
        public static bool ValidarAntenaId(string id) => !string.IsNullOrWhiteSpace(id) && AntIdRegex.IsMatch(id);
        public static bool ValidarIPv4(string ip) => !string.IsNullOrWhiteSpace(ip) && Ipv4Regex.IsMatch(ip);
        public static bool ValidarCoordenadas(string coord) => !string.IsNullOrWhiteSpace(coord) && CoordenadaParRegex.IsMatch(coord);
        public static bool ValidarFrecuencia(string freq) => !string.IsNullOrWhiteSpace(freq) && FrecuenciaRegex.IsMatch(freq);
    }
}