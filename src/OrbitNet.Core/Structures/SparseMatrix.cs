using System;
using System.Text;
using OrbitNet.Core.Nodes;

namespace OrbitNet.Core.Structures
{
    public class SparseMatrix
    {
        private HeaderList filas;
        private HeaderList columnas;

        // SE AGREGA: Contador interno requerido para las propiedades de estado
        private int nodeCount;

        public SparseMatrix()
        {
            filas = new HeaderList();
            columnas = new HeaderList();
            nodeCount = 0;
        }

        // ===================================================================
        // PROPIEDADES NUEVAS (Requeridas por la lógica del auxiliar)
        // ===================================================================
        public int Count => nodeCount;
        public bool IsEmpty => nodeCount == 0;

        public MatrizNode? Buscar(int fila, int columna)
        {
            HeaderNode? cabFila = filas.Buscar(fila);

            if (cabFila == null)
                return null;

            MatrizNode? actual = cabFila.Acceso;

            while (actual != null)
            {
                if (actual.Columna == columna)
                    return actual;

                actual = actual.Derecha;
            }

            return null;
        }

        public MatrizNode? BuscarPorId(string id)
        {
            HeaderNode? cabFila = filas.Cabeza;

            while (cabFila != null)
            {
                MatrizNode? actual = cabFila.Acceso;

                while (actual != null)
                {
                    if (actual.Id.Equals(id, StringComparison.OrdinalIgnoreCase))
                        return actual;

                    actual = actual.Derecha;
                }

                cabFila = cabFila.Siguiente;
            }

            return null;
        }

        public bool Existe(int fila, int columna)
        {
            return Buscar(fila, columna) != null;
        }

        // ===================================================================
        // FUNCION INSERTAR (Modificada para lanzar la excepción del auxiliar)
        // ===================================================================
        public bool Insertar(int fila, int columna, string id, string nombre, string ip)
        {
            // El auxiliar maneja una excepción en caso de colisión en lugar de un return false
            if (Existe(fila, columna))
            {
                throw new InvalidOperationException($"Colisión detectada: ya existe un satélite en las coordenadas ({fila}, {columna}).");
            }

            MatrizNode nuevo = new MatrizNode(fila, columna, id, nombre, ip);

            HeaderNode cabFila = filas.ObtenerOCrear(fila);
            HeaderNode cabColumna = columnas.ObtenerOCrear(columna);

            InsertarFila(cabFila, nuevo);
            InsertarColumna(cabColumna, nuevo);

            nodeCount++; // Se incrementa el contador
            return true;
        }

        private void InsertarFila(HeaderNode cabecera, MatrizNode nuevo)
        {
            if (cabecera.Acceso == null)
            {
                cabecera.Acceso = nuevo;
                return;
            }

            if (nuevo.Columna < cabecera.Acceso.Columna)
            {
                nuevo.Derecha = cabecera.Acceso;
                cabecera.Acceso.Izquierda = nuevo;
                cabecera.Acceso = nuevo;
                return;
            }

            MatrizNode actual = cabecera.Acceso;

            while (actual.Derecha != null && actual.Derecha.Columna < nuevo.Columna)
            {
                actual = actual.Derecha;
            }

            nuevo.Derecha = actual.Derecha;

            if (actual.Derecha != null)
                actual.Derecha.Izquierda = nuevo;

            nuevo.Izquierda = actual;
            actual.Derecha = nuevo;
        }

        private void InsertarColumna(HeaderNode cabecera, MatrizNode nuevo)
        {
            if (cabecera.Acceso == null)
            {
                cabecera.Acceso = nuevo;
                return;
            }

            if (nuevo.Fila < cabecera.Acceso.Fila)
            {
                nuevo.Abajo = cabecera.Acceso;
                cabecera.Acceso.Arriba = nuevo;
                cabecera.Acceso = nuevo;
                return;
            }

            MatrizNode actual = cabecera.Acceso;

            while (actual.Abajo != null && actual.Abajo.Fila < nuevo.Fila)
            {
                actual = actual.Abajo;
            }

            nuevo.Abajo = actual.Abajo;

            if (actual.Abajo != null)
                actual.Abajo.Arriba = nuevo;

            nuevo.Arriba = actual;
            actual.Abajo = nuevo;
        }

        // ===================================================================
        // FUNCION ELIMINAR (Modificada con las limpiezas estrictas del auxiliar)
        // ===================================================================
        public bool Eliminar(int fila, int columna)
        {
            MatrizNode? nodo = Buscar(fila, columna);

            if (nodo == null)
                return false;

            // 1. Desconectar horizontalmente
            if (nodo.Izquierda != null)
                nodo.Izquierda.Derecha = nodo.Derecha;
            else
            {
                HeaderNode? cabFila = filas.Buscar(fila);
                if (cabFila != null)
                {
                    cabFila.Acceso = nodo.Derecha;
                }
            }

            if (nodo.Derecha != null)
                nodo.Derecha.Izquierda = nodo.Izquierda;

            // 2. Desconectar verticalmente
            if (nodo.Arriba != null)
                nodo.Arriba.Abajo = nodo.Abajo;
            else
            {
                HeaderNode? cabCol = columnas.Buscar(columna);
                if (cabCol != null) cabCol.Acceso = nodo.Abajo;
            }

            if (nodo.Abajo != null)
                nodo.Abajo.Arriba = nodo.Arriba;

            // 3. Aislamiento físico del nodo en memoria
            nodo.Izquierda = null;
            nodo.Derecha = null;
            nodo.Arriba = null;
            nodo.Abajo = null;

            // 4. NUEVO: Eliminar cabeceras si se quedaron sin nodos asignados
            HeaderNode? cabFilaActual = filas.Buscar(fila);
            if (cabFilaActual != null && cabFilaActual.Acceso == null)
            {
                filas.EliminarCabecera(fila);
            }

            HeaderNode? cabColActual = columnas.Buscar(columna);
            if (cabColActual != null && cabColActual.Acceso == null)
            {
                columnas.EliminarCabecera(columna);
            }

            nodeCount--; // Se decrementa el contador de nodos activos
            return true;
        }

        // Se conserva tu función Contar remapeada a la variable optimizada
        public int Contar()
        {
            return nodeCount;
        }

        // ===================================================================
        // FUNCION VACIAR MATRIZ (Modificada para limpiar el contador)
        // ===================================================================
        public void Vaciar()
        {
            filas = new HeaderList();
            columnas = new HeaderList();
            nodeCount = 0;
        }

        // ===================================================================
        // NUEVOS MÉTODOS COMPLEMENTARIOS (Exportación en vectores estáticos y DOT)
        // ===================================================================
        public int[] ObtenerFilas()
        {
            int count = 0;
            HeaderNode? temp = filas.Cabeza;
            while (temp != null)
            {
                count++;
                temp = temp.Siguiente;
            }

            int[] arr = new int[count];
            temp = filas.Cabeza;
            for (int i = 0; i < count; i++)
            {
                arr[i] = temp!.Indice;
                temp = temp.Siguiente;
            }
            return arr;
        }

        public int[] ObtenerColumnas()
        {
            int count = 0;
            HeaderNode? temp = columnas.Cabeza;
            while (temp != null)
            {
                count++;
                temp = temp.Siguiente;
            }

            int[] arr = new int[count];
            temp = columnas.Cabeza;
            for (int i = 0; i < count; i++)
            {
                arr[i] = temp!.Indice;
                temp = temp.Siguiente;
            }
            return arr;
        }

        public MatrizNode[] ObtenerTodosLosNodos()
        {
            MatrizNode[] arr = new MatrizNode[nodeCount];
            int idx = 0;
            HeaderNode? cabFila = filas.Cabeza;
            while (cabFila != null)
            {
                MatrizNode? actual = cabFila.Acceso;
                while (actual != null)
                {
                    if (idx < nodeCount)
                    {
                        arr[idx++] = actual;
                    }
                    actual = actual.Derecha;
                }
                cabFila = cabFila.Siguiente;
            }
            return arr;
        }

        private bool EstaEnRuta(MatrizNode nodo, MatrizNode[]? ruta)
        {
            if (ruta == null) return false;
            for (int i = 0; i < ruta.Length; i++)
            {
                if (ruta[i] == nodo) return true;
            }
            return false;
        }

        private bool EsBordeDeRuta(MatrizNode n1, MatrizNode n2, MatrizNode[]? ruta)
        {
            if (ruta == null) return false;
            for (int i = 0; i < ruta.Length - 1; i++)
            {
                if ((ruta[i] == n1 && ruta[i + 1] == n2) || (ruta[i] == n2 && ruta[i + 1] == n1))
                {
                    return true;
                }
            }
            return false;
        }

        // ESTO ES PARA NUESTRO Graphviz
        public string GenerarCodigoDot(MatrizNode[]? ruta = null)
        {
            StringBuilder dot = new StringBuilder();
            dot.AppendLine("digraph G {");
            dot.AppendLine("    rankdir=TB;");
            dot.AppendLine("    node [fontname=\"Courier New\", fontsize=9, shape=none];");
            dot.AppendLine("    edge [fontname=\"Courier New\", fontsize=8];");
            dot.AppendLine("    bg [style=invisible];");

            dot.AppendLine("    root [label=<");
            dot.AppendLine("        <TABLE BORDER=\"0\" CELLBORDER=\"1\" CELLSPACING=\"0\" BGCOLOR=\"#EBF5FB\">");
            dot.AppendLine("            <TR><TD COLSPAN=\"2\"><B>Raíz Matriz</B></TD></TR>");
            dot.AppendLine("            <TR><TD PORT=\"rows\">Filas</TD><TD PORT=\"cols\">Columnas</TD></TR>");
            dot.AppendLine("        </TABLE>");
            dot.AppendLine("    >];");

            HeaderNode? rowNode = filas.Cabeza;
            while (rowNode != null)
            {
                dot.AppendLine($"    row_{rowNode.Indice} [label=<");
                dot.AppendLine($"        <TABLE BORDER=\"0\" CELLBORDER=\"1\" CELLSPACING=\"0\" BGCOLOR=\"#FADBD8\">");
                dot.AppendLine($"            <TR><TD COLSPAN=\"2\"><B>Fila: {rowNode.Indice}</B></TD></TR>");
                dot.AppendLine($"            <TR><TD PORT=\"next\">Sig</TD><TD PORT=\"access\">Acceso</TD></TR>");
                dot.AppendLine($"        </TABLE>");
                dot.AppendLine($"    >];");
                rowNode = rowNode.Siguiente;
            }

            HeaderNode? colNode = columnas.Cabeza;
            while (colNode != null)
            {
                dot.AppendLine($"    col_{colNode.Indice} [label=<");
                dot.AppendLine($"        <TABLE BORDER=\"0\" CELLBORDER=\"1\" CELLSPACING=\"0\" BGCOLOR=\"#FCF3CF\">");
                dot.AppendLine($"            <TR><TD COLSPAN=\"2\"><B>Col: {colNode.Indice}</B></TD></TR>");
                dot.AppendLine($"            <TR><TD PORT=\"next\">Sig</TD><TD PORT=\"access\">Acceso</TD></TR>");
                dot.AppendLine($"        </TABLE>");
                dot.AppendLine($"    >];");
                colNode = colNode.Siguiente;
            }

            rowNode = filas.Cabeza;
            while (rowNode != null)
            {
                MatrizNode? node = rowNode.Acceso;
                while (node != null)
                {
                    string bgColor = EstaEnRuta(node, ruta) ? "#2ECC71" : "#D5F5E3";
                    dot.AppendLine($"    node_{node.Fila}_{node.Columna} [label=<");
                    dot.AppendLine($"        <TABLE BORDER=\"0\" CELLBORDER=\"1\" CELLSPACING=\"0\" BGCOLOR=\"{bgColor}\">");
                    dot.AppendLine($"            <TR><TD PORT=\"up\">Up</TD><TD PORT=\"down\">Down</TD></TR>");
                    dot.AppendLine($"            <TR><TD COLSPAN=\"2\"><B>Row: {node.Fila}<BR/>Col: {node.Columna}<BR/>ID: {node.Id}<BR/>IP: {node.IpAddress}</B></TD></TR>");
                    dot.AppendLine($"            <TR><TD PORT=\"left\">Left</TD><TD PORT=\"right\">Right</TD></TR>");
                    dot.AppendLine($"        </TABLE>");
                    dot.AppendLine($"    >];");
                    node = node.Derecha;
                }
                rowNode = rowNode.Siguiente;
            }

            if (filas.Cabeza != null)
            {
                dot.AppendLine("    root:rows -> row_" + filas.Cabeza.Indice + ";");
                rowNode = filas.Cabeza;
                while (rowNode.Siguiente != null)
                {
                    dot.AppendLine($"    row_{rowNode.Indice}:next -> row_{rowNode.Siguiente.Indice};");
                    rowNode = rowNode.Siguiente;
                }
            }

            if (columnas.Cabeza != null)
            {
                dot.AppendLine("    root:cols -> col_" + columnas.Cabeza.Indice + ";");
                colNode = columnas.Cabeza;
                while (colNode.Siguiente != null)
                {
                    dot.AppendLine($"    col_{colNode.Indice}:next -> col_{colNode.Siguiente.Indice};");
                    colNode = colNode.Siguiente;
                }
            }

            rowNode = filas.Cabeza;
            while (rowNode != null)
            {
                if (rowNode.Acceso != null)
                {
                    dot.AppendLine($"    row_{rowNode.Indice}:access -> node_{rowNode.Acceso.Fila}_{rowNode.Acceso.Columna}:left;");

                    MatrizNode? node = rowNode.Acceso;
                    while (node != null)
                    {
                        if (node.Derecha != null)
                        {
                            bool esRuta = EsBordeDeRuta(node, node.Derecha, ruta);
                            string color = esRuta ? "#27AE60" : "blue";
                            string styleOpts = esRuta ? ", penwidth=3.0" : "";
                            dot.AppendLine($"    node_{node.Fila}_{node.Columna}:right -> node_{node.Derecha.Fila}_{node.Derecha.Columna}:left [dir=both, color=\"{color}\"{styleOpts}];");
                        }
                        node = node.Derecha;
                    }
                }
                rowNode = rowNode.Siguiente;
            }

            colNode = columnas.Cabeza;
            while (colNode != null)
            {
                if (colNode.Acceso != null)
                {
                    dot.AppendLine($"    col_{colNode.Indice}:access -> node_{colNode.Acceso.Fila}_{colNode.Acceso.Columna}:up;");

                    MatrizNode? node = colNode.Acceso;
                    while (node != null)
                    {
                        if (node.Abajo != null)
                        {
                            bool esRuta = EsBordeDeRuta(node, node.Abajo, ruta);
                            string color = esRuta ? "#27AE60" : "red";
                            string styleOpts = esRuta ? ", penwidth=3.0" : "";
                            dot.AppendLine($"    node_{node.Fila}_{node.Columna}:down -> node_{node.Abajo.Fila}_{node.Abajo.Columna}:up [dir=both, color=\"{color}\"{styleOpts}];");
                        }
                        node = node.Abajo;
                    }
                }
                colNode = colNode.Siguiente;
            }

            if (columnas.Cabeza != null)
            {
                dot.Append("    { rank=same; root; ");
                colNode = columnas.Cabeza;
                while (colNode != null)
                {
                    dot.Append($"col_{colNode.Indice}; ");
                    colNode = colNode.Siguiente;
                }
                dot.AppendLine("}");
            }

            rowNode = filas.Cabeza;
            while (rowNode != null)
            {
                dot.Append($"    {{ rank=same; row_{rowNode.Indice}; ");
                MatrizNode? node = rowNode.Acceso;
                while (node != null)
                {
                    dot.Append($"node_{node.Fila}_{node.Columna}; ");
                    node = node.Derecha;
                }
                dot.AppendLine("}");
                rowNode = rowNode.Siguiente;
            }

            dot.AppendLine("}");
            return dot.ToString();
        }
    }
}