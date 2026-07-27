
using System.Net;

namespace OrbitNet.Core.Nodes
{
    public class AbbNode
    {
        //Codigo hexadecimal unico que identiic univocamente al paquete.
        public string HexCode { get; set; } = "";
        // Identificador unico del satelite del origen que emitio el paquete.
        public string EmisorId { get; set; } = "";
        //Direccion IP del nodo terreste de destino fginal.
        public string DestIp { get; set; } = "";
        // Nivel de proridad asignado al paquete de datos
        // valores enteros admitidos de 1 a 5 con 1 siendo la minima prioridad
        public int Priority { get; set; }
        // cuerpo o conenido de testo plano del mensaje transmitido.
        public string Content { get; set; } = "";
        public AbbNode? Izquierda { get; set; }
        public AbbNode? Derecha { get; set; }

        public AbbNode(string hexCode, string emisorId, string destIp, int priority, string content)
        {
            HexCode = hexCode;
            EmisorId = emisorId;
            DestIp = destIp;
            Priority = priority;
            Content = content;
            Izquierda = null;
            Derecha = null;
        }
    }
}
