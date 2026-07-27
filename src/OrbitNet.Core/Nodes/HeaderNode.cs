
namespace OrbitNet.Core.Nodes
{
    public class HeaderNode
    {
        public int Indice { get; set; }
        public HeaderNode? Siguiente { get; set; }
        public HeaderNode? Anterior { get; set; }
        public MatrizNode? Acceso { get; set; }

        public HeaderNode(int indice)
        {
            Indice = indice;
            Siguiente = null;
            Anterior = null;
            Acceso = null;
        }
    }
}