using System;

namespace OrbitNet.Core.Models
{
    /// <summary>
    /// Representa una entrada individual de la bitácora de auditoría.
    /// </summary>
    public class LogRegistro
    {
        public string MarcaDeTiempo { get; }
        public string Tipo { get; } // INFO, ALERT, ERROR
        public string Mensaje { get; }

        public LogRegistro(string tipo, string mensaje)
        {
            MarcaDeTiempo = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Tipo = tipo;
            Mensaje = mensaje;
        }

        public string ObtenerLineaFormateada()
        {
            string tag = Tipo == "INFO" ? "OK" : (Tipo == "ALERT" ? "WARN" : "FAIL");
            return $"{MarcaDeTiempo} | {Tipo,-5} ({tag}) | {Mensaje}";
        }
    }
}




