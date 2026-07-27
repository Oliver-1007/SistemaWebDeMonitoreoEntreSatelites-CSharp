using OrbitNet.Core.Models;

namespace OrbitNet.Core.Nodes
{
    public class AvlNode
    {
        // contenido del satelite encapsulado
        public Satelite Valor { get; set; }
        //refeencia al subarbol izquierdo
        public AvlNode? Izquierda { get; set; }
        public AvlNode? Derecha { get; set; }
        // altura del nodo en el arbol para el calculo de factores de balanceo
        public int Altura { get; set; }

        //constructor
        public AvlNode(Satelite satelite)
        {
            Valor = satelite ?? throw new ArgumentNullException(nameof(satelite));
            Izquierda = null;
            Derecha = null;
            Altura = 1; // un nodo recien creado tiene altura inicial de 1
        }
    }
}