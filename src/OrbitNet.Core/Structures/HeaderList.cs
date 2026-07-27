using OrbitNet.Core.Nodes;

namespace OrbitNet.Core.Structures
{
    public class HeaderList
    {
        public HeaderNode? Cabeza { get; set; }
        public HeaderList()
        {
            Cabeza = null;
        }

        public HeaderNode? Buscar(int indice)
        {
            HeaderNode? auxiliar = Cabeza;

            while (auxiliar != null)
            {
                if (auxiliar.Indice == indice)
                {
                    return auxiliar;
                }
                auxiliar = auxiliar.Siguiente;
            }
            return null;
        }

        public HeaderNode ObtenerOCrear(int indice)
        {
            // 1. Intentar buscar si ya existe para no duplicar
            HeaderNode? existente = Buscar(indice);
            if (existente != null)
                return existente;

            // 2. Si no existe, creamos la nueva cabecera
            HeaderNode nuevo = new HeaderNode(indice);

            // CASO A: La lista estaba completamente vacía. Este nuevo nodo es el primero.
            if (Cabeza == null)
            {
                Cabeza = nuevo;
                return nuevo;
            }

            // CASO B: El nuevo índice es MENOR que el primero de todos. 
            // Se debe insertar al súper inicio de la lista.
            if (indice < Cabeza.Indice)
            {
                nuevo.Siguiente = Cabeza;
                Cabeza.Anterior = nuevo;
                Cabeza = nuevo; // Actualizamos la raíz de la lista
                return nuevo;
            }

            // CASO C: El nodo va en medio o al final. 
            // Recorremos la lista buscando dónde encajarlo para mantener el orden.
            HeaderNode actual = Cabeza;
            while (actual.Siguiente != null && actual.Siguiente.Indice < indice)
            {
                actual = actual.Siguiente;
            }

            // Reconexión de punteros (Clásica inserción en lista doblemente enlazada)
            nuevo.Siguiente = actual.Siguiente;

            if (actual.Siguiente != null) // Si no estamos al final de la lista
                actual.Siguiente.Anterior = nuevo;

            nuevo.Anterior = actual;
            actual.Siguiente = nuevo;

            return nuevo;
        }

        public void EliminarCabecera(int index)
        {
            if (Cabeza == null) return;

            if (Cabeza.Indice == index)
            {
                Cabeza = Cabeza.Siguiente;
                return;
            }

            HeaderNode? actual = Cabeza;
            while (actual.Siguiente != null && actual.Siguiente.Indice != index)
            {
                actual = actual.Siguiente;
            }

            if (actual.Siguiente != null)
            {
                actual.Siguiente = actual.Siguiente.Siguiente;
            }
        }
    }
}