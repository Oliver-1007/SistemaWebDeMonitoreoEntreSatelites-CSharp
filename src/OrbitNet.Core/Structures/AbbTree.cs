using OrbitNet.Core.Models;
using OrbitNet.Core.Nodes;

namespace OrbitNet.Core.Structures
{
    public class AbbTree
    {
        private AbbNode? Cabeza;

        public AbbTree()
        {
            Cabeza = null;
        }

        public int Contar
        {
            get { return ContarNodos(Cabeza); }
        }

        public bool IsEmpty
        {
            get { return Cabeza == null; }
        }

        public void InsertarFinal(AbbNode nodo)
        {

            if (nodo == null)
            {
                return;
            }

            Cabeza = Insertar(Cabeza, nodo);
        }

        public AbbNode? Insertar(AbbNode? actual, AbbNode nuevo)
        {
            if (actual == null)
            {
                return nuevo;
            }

            if (nuevo.Priority >= actual.Priority)
            {

                actual.Derecha = Insertar(actual.Derecha, nuevo);
            }
            else
            {
                actual.Izquierda = Insertar(actual.Izquierda, nuevo);
            }
            return actual;
        }

        public AbbNode? EliminarMax()
        {
            if (Cabeza == null)
            {
                return null;
            }

            // Caso especial: El nodo de extrema derecha es la raíz misma (no hay subárbol derecho)
            if (Cabeza.Derecha == null)
            {
                AbbNode maxNode = Cabeza;
                // La raíz se reemplaza por su subárbol izquierdo
                Cabeza = Cabeza.Izquierda;

                // Desvincular enlaces físicos para evitar retención de referencias en memoria
                maxNode.Izquierda = null;
                maxNode.Derecha = null;
                return maxNode;
            }

            // Caso general: Recorrer el árbol para encontrar el nodo más a la derecha y su padre
            AbbNode parent = Cabeza;
            AbbNode current = Cabeza.Derecha;

            while (current.Derecha != null)
            {
                parent = current;
                current = current.Derecha;
            }

            // 'current' es el nodo de extrema derecha (máxima prioridad)
            // Reconectar el posible subárbol izquierdo de 'current' a la rama derecha del padre
            parent.Derecha = current.Izquierda;

            // Desvincular referencias del nodo extraído
            current.Izquierda = null;
            current.Derecha = null;

            return current;
        }

        private int ContarNodos(AbbNode? nodo)
        {
            if (nodo == null) return 0;
            return 1 + ContarNodos(nodo.Izquierda) + ContarNodos(nodo.Derecha);
        }

        public AbbNode[] ObtenerMensajesOrdenados()
        {
            int conteo = Contar;
            AbbNode[] arr = new AbbNode[conteo];
            int index = 0;

            // Recorrido In-Order Inverso (Derecha -> Raíz -> Izquierda)
            // para rellenar el arreglo de mayor a menor prioridad.
            LlenarArregloInOrderInverso(Cabeza, arr, ref index);
            return arr;
        }

        private void LlenarArregloInOrderInverso(AbbNode? nodo, AbbNode[] arr, ref int index)
        {
            if (nodo == null) return;

            // Procesar primero el subárbol derecho (prioridades mayores)
            LlenarArregloInOrderInverso(nodo.Derecha, arr, ref index);

            // Procesar el nodo actual
            if (index < arr.Length)
            {
                arr[index++] = nodo;
            }

            // Procesar el subárbol izquierdo (prioridades menores)
            LlenarArregloInOrderInverso(nodo.Izquierda, arr, ref index);
        }
    }
}