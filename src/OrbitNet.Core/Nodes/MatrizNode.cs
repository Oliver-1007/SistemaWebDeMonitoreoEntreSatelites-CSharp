using OrbitNet.Core.Structures;

namespace OrbitNet.Core.Nodes
{
    public class MatrizNode
    {
        public int Fila { get; set; }
        public int Columna { get; set; }
        public string Id { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string IpAddress { get; set; } = "";

        public AbbTree Buffer { get; } = new AbbTree();

        public MatrizNode? Izquierda { get; set; }
        public MatrizNode? Derecha { get; set; }
        public MatrizNode? Arriba { get; set; }
        public MatrizNode? Abajo { get; set; }

        public MatrizNode(int fila, int columna, string id, string nombre, string ipAddress)
        {
            Fila = fila;
            Columna = columna;
            Id = id;
            Nombre = nombre;
            IpAddress = ipAddress;

            Izquierda = null;
            Derecha = null;
            Arriba = null;
            Abajo = null;
        }
    }
}