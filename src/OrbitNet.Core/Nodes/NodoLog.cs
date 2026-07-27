using OrbitNet.Core.Models;

namespace OrbitNet.Core.Nodes
{
    /// <summary>
    /// Representa un nodo enlazado para registrar logs de auditoría de forma manual.
    /// </summary>
    public class NodoLog
    {
        public LogRegistro Valor { get; set; }
        public NodoLog? Siguiente { get; set; }

        public NodoLog(LogRegistro log)
        {
            Valor = log;
            Siguiente = null;
        }
    }
}
