using OrbitNet.Core.Nodes;

namespace OrbitNet.Services.Visualizacion
{
    //-----------------------------------------------------------------------
    // Genera el código DOT para los reportes 2 y 3.
    public class ReportGenerator
    {
        //-----------------------------------------------------------------------
        // REPORTE 2 — Trazabilidad de Ruta de Retransmisión

        public string GenerateRelayTracerDot(
            MatrizNode[] allNodes,
            string[] visitedIds,
            string packetCode)
        {
            var dot = new System.Text.StringBuilder();

            dot.AppendLine("digraph RelayTracer {");
            dot.AppendLine("rankdir=LR;");
            dot.AppendLine("node [fontname=\"Consolas\", fontsize=11, style=\"filled\"];");
            dot.AppendLine("edge [fontname=\"Consolas\", fontsize=9];");
            dot.AppendLine($"label=\"Trazabilidad del Paquete: {EscapeDot(packetCode)}\";");
            dot.AppendLine("labelloc=t;");
            dot.AppendLine("fontsize=14;");
            dot.AppendLine();

            // Declaración de nodos con color dinámico
            for (int i = 0; i < allNodes.Length; i++)
            {
                MatrizNode node = allNodes[i];
                bool visited = ContainsId(visitedIds, node.Id);

                string fillColor;
                string fontColor;
                string style;

                if (visited)
                {
                    fillColor = "#2ECC71";
                    fontColor = "#000000";
                    style = "filled";
                }
                else
                {
                    fillColor = "#E74C3C";
                    fontColor = "#FFFFFF";
                    style = "dashed, filled";
                }

                string label = EscapeDot(node.Id) +
                   "\\n(" + node.Fila + "," + node.Columna + ")" +
                   "\\n" + EscapeDot(node.IpAddress);

                dot.AppendLine(
                    $"    \"{EscapeDot(node.Id)}\" " +
                    $"[label=\"{label}\", " +
                    $"fillcolor=\"{fillColor}\", " +
                    $"fontcolor=\"{fontColor}\", " +
                    $"style=\"{style}\", " +
                    $"shape=ellipse];"
                );
            }

            dot.AppendLine();

            for (int i = 1; i < allNodes.Length; i++)
            {
                MatrizNode prev = allNodes[i - 1];
                MatrizNode curr = allNodes[i];

                bool bothVisited = ContainsId(visitedIds, prev.Id) && ContainsId(visitedIds, curr.Id);

                if (bothVisited)
                {
                    dot.AppendLine(
                        $"    \"{EscapeDot(prev.Id)}\" -> \"{EscapeDot(curr.Id)}\" " +
                        $"[penwidth=3.0, color=\"#27AE60\", arrowhead=vee];"
                    );
                }
                else
                {
                    dot.AppendLine(
                        $"    \"{EscapeDot(prev.Id)}\" -> \"{EscapeDot(curr.Id)}\" " +
                        $"[penwidth=1.0, color=\"#95A5A6\", style=dashed];"
                    );
                }
            }

            dot.AppendLine("}");
            return dot.ToString();
        }


        //--------------------------------------------------------------------
        // REPORTE 3 — Matriz Unificada de Capacidad y Estado del Buffer

        public string GenerateBufferMatrixDot(
            MatrizNode[] allNodes,
            int maxCapacity = 5)
        {

            var dot = new System.Text.StringBuilder();

            dot.AppendLine("digraph BufferMatrix {");
            dot.AppendLine("rankdir=TB;");
            dot.AppendLine("node [shape=plaintext, fontname=\"Consolas\", fontsize=10];");
            dot.AppendLine("label=\"Estado de Buffers Satelitales\";");
            dot.AppendLine("labelloc=t;");
            dot.AppendLine("fontsize=14;");
            dot.AppendLine();

            for (int i = 0; i < allNodes.Length; i++)
            {
                MatrizNode node = allNodes[i];

                int occupied = node.Buffer.Contar;
                int free = maxCapacity - occupied;
                double pct = 0.0;

                if (maxCapacity > 0)
                {
                    pct = (occupied / (double)maxCapacity) * 100.0;
                }

                string barColor = "#2ECC71";

                if (pct >= 80)
                {
                    barColor = "#E74C3C";
                }
                else if (pct >= 50)
                {
                    barColor = "#F39C12";
                }

                // Recorrido in-order inverso del ABB (mayor a menor prioridad)
                // ya entregado por AbbTree.ObtenerMensajesOrdenados().
                AbbNode[] mensajesOrdenados = node.Buffer.ObtenerMensajesOrdenados();
                string mensajesEnCola = BuildMensajesEnCola(mensajesOrdenados);

                string nodeId = "buf_" + EscapeDotId(node.Id);

                string label = BuildBufferHtmlLabel(
                    node.Id,
                    node.Fila,
                    node.Columna,
                    occupied,
                    maxCapacity,
                    pct,
                    barColor,
                    mensajesEnCola
                );

                dot.AppendLine($"    {nodeId} [label={label}];");
                dot.AppendLine();
            }

            dot.AppendLine("}");
            return dot.ToString();
        }


        //---------------------------------------------------------
        // Helpers privados

        // Construye el listado textual de mensajes en cola a partir del
        // recorrido in-order inverso del ABB (AbbTree.ObtenerMensajesOrdenados),
        // mostrando primero los de mayor prioridad. Sin colecciones genéricas.
        private static string BuildMensajesEnCola(AbbNode[] mensajes)
        {
            if (mensajes == null || mensajes.Length == 0)
            {
                return "(buffer vacio)";
            }

            var sb = new System.Text.StringBuilder();

            for (int i = 0; i < mensajes.Length; i++)
            {
                sb.Append(mensajes[i].HexCode);
                sb.Append("(P");
                sb.Append(mensajes[i].Priority);
                sb.Append(")");

                if (i < mensajes.Length - 1)
                {
                    sb.Append(", ");
                }
            }

            return sb.ToString();
        }

        // Búsqueda lineal manual sobre arreglo de IDs.
        // Reemplaza el uso de HashSet o Contains de colecciones nativas.

        private static bool ContainsId(string[] ids, string target)
        {
            if (ids == null || target == null)
            {
                return false;
            }
            for (int i = 0; i < ids.Length; i++)
            {
                if (ids[i] == target) return true;
            }
            return false;
        }

        // Construye la etiqueta HTML de Graphviz para una celda del Buffer Matrix
        private string BuildBufferHtmlLabel(
            string id, int row, int col,
            int occupied, int total, double pct,
            string barColor, string mensajes)
        {
            var sb = new System.Text.StringBuilder();

            sb.AppendLine("<<table border=\"1\" cellborder=\"0\" cellspacing=\"2\" bgcolor=\"#FDFEFE\">");

            sb.AppendLine(
                $"  <tr><td colspan=\"2\" bgcolor=\"#2C3E50\">" +
                $"<font color=\"white\"><b>{EscapeHtml(id)}</b></font></td></tr>");

            sb.AppendLine(
                $"  <tr><td align=\"left\"><font point-size=\"9\">Pos:</font></td>" +
                $"<td align=\"right\"><font point-size=\"9\">({row}, {col})</font></td></tr>");

            sb.AppendLine(
                $"  <tr><td align=\"left\"><font point-size=\"9\">Buffer:</font></td>" +
                $"<td align=\"right\"><font point-size=\"9\">{occupied}/{total} ({pct:F0}%)</font></td></tr>");

            sb.AppendLine("  <tr><td colspan=\"2\">");
            sb.AppendLine("    <table border=\"0\" cellborder=\"0\" cellspacing=\"1\"><tr>");

            int filledCells = (int)(pct / 10.0);

            for (int i = 0; i < 10; i++)
            {
                string cellBg = "#D5DBDB";

                if (i < filledCells)
                {
                    cellBg = barColor;
                }

                sb.Append($"<td width=\"12\" height=\"8\" bgcolor=\"{cellBg}\"> </td>");
            }

            sb.AppendLine("</tr></table>");
            sb.AppendLine("  </td></tr>");

            sb.AppendLine(
                $"  <tr><td colspan=\"2\" align=\"left\">" +
                $"<font point-size=\"8\">Cola: {EscapeHtml(mensajes)}</font></td></tr>");

            sb.AppendLine("</table>>");

            return sb.ToString();
        }

        // Escapa caracteres especiales para etiquetas DOT entre comillas 
        private static string EscapeDot(string value)
        {
            if (value == null)
            {
                return "";
            }
            return value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n");
        }

        //Convierte un ID a formato seguro para nombre de nodo DOT
        private static string EscapeDotId(string value)
        {
            if (value == null)
            {
                return "";
            }
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (c == '-' || c == '.' || c == ' ')
                    sb.Append('_');
                else
                    sb.Append(c);
            }
            return sb.ToString();
        }

        //Escapa caracteres especiales en etiquetas HTML de Graphviz
        private static string EscapeHtml(string value)
        {
            if (value == null)
            {
                return "";
            }
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                switch (value[i])
                {
                    case '&':
                        {
                            sb.Append("&amp;");
                        }
                        break;
                    case '<':
                        {
                            sb.Append("&lt;");
                        }
                        break;
                    case '>':
                        {
                            sb.Append("&gt;");
                        }
                        break;
                    case '"':
                        {
                            sb.Append("&quot;");
                        }
                        break;
                    default:
                        sb.Append(value[i]);
                        break;
                }
            }
            return sb.ToString();
        }
    }
}