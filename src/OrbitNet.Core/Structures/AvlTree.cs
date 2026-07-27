using OrbitNet.Core.Models;
using OrbitNet.Core.Nodes;

namespace OrbitNet.Core.Structures
{
    public class AvlTree
    {
        private AvlNode? Cabeza;
        private int conteo;

        public AvlTree()
        {
            Cabeza = null;
            conteo = 0;
        }

        public int Conteo => conteo;
        public bool EstaVacio => Cabeza == null;

        public void Limpiar()
        {
            Cabeza = null;
            conteo = 0;
        }

        public void Insertar(Satelite satelite)
        {
            if (satelite == null) return;
            Cabeza = InsertarRecursivo(Cabeza, satelite);
        }

        public Satelite? Buscar(string id)
        {
            return BuscarRecursivo(Cabeza, id);
        }

        private int ObtenerAltura(AvlNode? nodo)
        {
            return nodo?.Altura ?? 0;
        }

        private int ObtenerFactorBalance(AvlNode? nodo)
        {
            if (nodo == null) return 0;
            return ObtenerAltura(nodo.Izquierda) - ObtenerAltura(nodo.Derecha);
        }

        private void ActualizarAltura(AvlNode nodo)
        {
            nodo.Altura = Math.Max(ObtenerAltura(nodo.Izquierda), ObtenerAltura(nodo.Derecha)) + 1;
        }

        // Rotacion Simple a la Derecha (LL Rotation)
        private AvlNode RotarDerecha(AvlNode y)
        {
            AvlNode? x = y.Izquierda!;
            AvlNode? T2 = x.Derecha;

            // Ejecutar la rotacion de punteros
            x.Derecha = y;
            y.Izquierda = T2;

            // Actualizar alturas calculadas de abajo hacia arriba
            ActualizarAltura(y);
            ActualizarAltura(x);

            return x;
        }

        // Rotacion Simple a la Izquierda (RR Rotation)
        private AvlNode RotarIzquierda(AvlNode x)
        {
            AvlNode? y = x.Derecha!;
            AvlNode? T2 = y.Izquierda;

            // Ejecutar la rotacion de punteros
            y.Izquierda = x;
            x.Derecha = T2;

            // Actualizar alturas calculadas de abajo hacia arriba
            ActualizarAltura(x);
            ActualizarAltura(y);

            return y;
        }

        private AvlNode InsertarRecursivo(AvlNode? nodo, Satelite satelite)
        {
            // 1. Insercion estandar de Arbol Binario de Busqueda (BST)
            if (nodo == null)
            {
                conteo++;
                return new AvlNode(satelite);
            }

            int comparacion = string.Compare(satelite.Id, nodo.Valor.Id, StringComparison.OrdinalIgnoreCase);

            if (comparacion < 0)
            {
                nodo.Izquierda = InsertarRecursivo(nodo.Izquierda, satelite);
            }
            else if (comparacion > 0)
            {
                nodo.Derecha = InsertarRecursivo(nodo.Derecha, satelite);
            }
            else
            {
                // Clave duplicada: no se realiza la insercion
                return nodo;
            }

            // 2. Actualizar altura del ancestro actual
            ActualizarAltura(nodo);

            // 3. Obtener el factor de balanceo para determinar si hay desequilibrio
            int balance = ObtenerFactorBalance(nodo);

            // 4. Evaluar desbalanceo y aplicar rotaciones correspondientes

            // Caso Izquierda-Izquierda (LL)
            if (balance > 1 && string.Compare(satelite.Id, nodo.Izquierda!.Valor.Id, StringComparison.OrdinalIgnoreCase) < 0)
            {
                return RotarDerecha(nodo);
            }

            // Caso Derecha-Derecha (RR)
            if (balance < -1 && string.Compare(satelite.Id, nodo.Derecha!.Valor.Id, StringComparison.OrdinalIgnoreCase) > 0)
            {
                return RotarIzquierda(nodo);
            }

            // Caso Izquierda-Derecha (LR)
            if (balance > 1 && string.Compare(satelite.Id, nodo.Izquierda!.Valor.Id, StringComparison.OrdinalIgnoreCase) > 0)
            {
                nodo.Izquierda = RotarIzquierda(nodo.Izquierda);
                return RotarDerecha(nodo);
            }

            // Caso Derecha-Izquierda (RL)
            if (balance < -1 && string.Compare(satelite.Id, nodo.Derecha!.Valor.Id, StringComparison.OrdinalIgnoreCase) < 0)
            {
                nodo.Derecha = RotarDerecha(nodo.Derecha);
                return RotarIzquierda(nodo);
            }

            return nodo;
        }

        private Satelite? BuscarRecursivo(AvlNode? nodo, string id)
        {
            if (nodo == null) return null;

            int comparacion = string.Compare(id, nodo.Valor.Id, StringComparison.OrdinalIgnoreCase);

            if (comparacion == 0) return nodo.Valor;

            if (comparacion < 0)
            {
                return BuscarRecursivo(nodo.Izquierda, id);
            }

            return BuscarRecursivo(nodo.Derecha, id);
        }

        // --- Recorrido In-Order en un Arreglo Nativo C# ---

        public Satelite[] ObtenerTodos()
        {
            Satelite[] arrayResult = new Satelite[conteo];
            int index = 0;
            LlenarArregloInOrden(Cabeza, arrayResult, ref index);
            return arrayResult;
        }

        private void LlenarArregloInOrden(AvlNode? nodo, Satelite[] arr, ref int idx)
        {
            if (nodo == null) return;

            // 1. Recorrer subarbol izquierdo
            LlenarArregloInOrden(nodo.Izquierda, arr, ref idx);

            // 2. Procesar nodo actual
            if (idx < arr.Length)
            {
                arr[idx++] = nodo.Valor;
            }

            // 3. Recorrer subarbol derecho
            LlenarArregloInOrden(nodo.Derecha, arr, ref idx);
        }
    }
}