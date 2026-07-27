using System;
using System.Text;
using System.Text.RegularExpressions;
using OrbitNet.Core.Interfaces;
using OrbitNet.Core.Models;
using OrbitNet.Core.Nodes;

namespace OrbitNet.Core.Structures
{
    /// <summary>
    /// TDA LogAuditoria: Lista Enlazada Simple con puntero Tail para inserción O(1)
    /// Registra cronológicamente cada evento, advertencia e intrusión del simulador
    /// No usa System.Collections ni System.Collections.Generic
    /// </summary>
    public class LogAuditoria : IAbstractCollection
    {
        //---------------------------------------------------------------------------
        // PUNTEROS INTERNOS DE LA LISTA
        private NodoLog? Cabeza; // Primer nodo (evento más antiguo)
        private NodoLog? Cola; // Último nodo (evento más reciente) -> O(1) insert
        private int _count;

        //---------------------------------------------------------------------------
        // IAbstractCollection
        public int Count => _count;
        public bool IsEmpty => _count == 0;

        public LogAuditoria()
        {
            Cabeza = null;
            Cola = null;
            _count = 0;
        }

        //---------------------------------------------------------------------------
        // OPERACIONES PRINCIPALES

        /// <summary>
        /// Inserta un nuevo evento al final de la lista en tiempo O(1)
        /// usando el puntero Tail
        /// </summary>
        /// <param name="severity">INFO | ALERT | ERROR</param>
        /// <param name="message">Descripción del evento.</param>
        public void Registrar(string severity, string message)
        {
            // Instanciar el modelo de datos primero
            LogRegistro nuevoRegistro = new LogRegistro(severity, message);
            // Encapsularlo en el nodo manual
            NodoLog newNode = new NodoLog(nuevoRegistro);

            if (Cola == null) // Lista vacía. Head y tail apuntan al mismo nodo
            {
                Cabeza = newNode;
                Cola = newNode;
            }
            else  // Lista con elementos. Enlazar al final
            {
                Cola.Siguiente = newNode;
                Cola = newNode;
            }

            _count++;
        }

        /// <summary>
        /// Recorre linealmente la lista O(n) y retorna todos los logs cuyo
        /// campo Mensaje coincida con el patrón RegEx recibido
        /// </summary>
        /// <param name="pattern"> Expresión regular de búsqueda </param>
        /// <returns> String con los logs coincidentes formateados </returns>
        public string SearchLogRegex(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
                return "Patrón de búsqueda vacío.";

            Regex regex;
            try
            {
                regex = new Regex(pattern, RegexOptions.IgnoreCase);
            }
            catch (ArgumentException)
            {
                return $"ERROR: El patrón RegEx '{pattern}' no es válido.";
            }

            StringBuilder results = new StringBuilder();
            NodoLog? current = Cabeza;
            int matches = 0;

            while (current != null)
            {
                // Se accede de forma segura a través de 'Valor'
                if (current.Valor != null && regex.IsMatch(current.Valor.Mensaje))
                {
                    // Aprovecha el método nativo que definiste en LogRegistro
                    results.AppendLine(current.Valor.ObtenerLineaFormateada());
                    matches++;
                }
                current = current.Siguiente;
            }

            return matches == 0
                ? $"No se encontraron logs que coincidan con: '{pattern}'"
                : results.ToString();
        }

        /// <summary>
        /// Retorna todos los logs como string para auditoría completa
        /// Recorrido O(n)
        /// </summary>
        public string GetAllLogs()
        {
            if (IsEmpty) return "El log de auditoría está vacío.";

            StringBuilder sb = new StringBuilder();
            NodoLog? current = Cabeza;

            while (current != null)
            {
                if (current.Valor != null)
                {
                    // Aprovecha el formato limpio preestablecido
                    sb.AppendLine(current.Valor.ObtenerLineaFormateada());
                }
                current = current.Siguiente;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Retorna un arreglo estático con todos los nodos para ser procesados externamente (vistas, etc.)
        /// </summary>
        public LogRegistro[] ObtenerTodosLosNodos()
        {
            LogRegistro[] registros = new LogRegistro[_count];
            NodoLog? actual = Cabeza;
            int index = 0;

            while (actual != null)
            {
                if (actual.Valor != null)
                {
                    // Extraemos únicamente el modelo de datos puro
                    registros[index++] = actual.Valor;
                }
                actual = actual.Siguiente;
            }

            return registros;
        }

        /// <summary>
        /// Libera todas las referencias de nodos en memoria
        /// </summary>
        public void Clear()
        {
            Cabeza = null;
            Cola = null;
            _count = 0;
        }
    }
}