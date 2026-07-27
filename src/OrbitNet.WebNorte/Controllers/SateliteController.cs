using System;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using OrbitNet.Core.Nodes;
using OrbitNet.Services.Data;

namespace OrbitNet.WebNorte.Controllers
{
    /// <summary>
    /// Controlador especializado en operaciones manuales sobre los nodos de satélite
    /// en la Matriz Dispersa Ortogonal: insertar, eliminar y limpiar el plano.
    /// No usa colecciones de System.Collections.
    /// </summary>
    public class SateliteController : Controller
    {
        private const string PatronIdSatelite = @"^SAT-(ECU|POL)-\d{4}$";
        private const string PatronIpv4 = @"^(?:(?:25[0-5]|2[0-4]\d|[01]?\d?\d)\.){3}(?:25[0-5]|2[0-4]\d|[01]?\d?\d)$";

        /// <summary>
        /// Acción POST: Inserta un nuevo nodo satelital en la Matriz Dispersa.
        /// Valida formato de ID (Regex), IPv4 (Regex), colisiones de coordenada e ID duplicado.
        /// </summary>
        [HttpPost]
        public IActionResult InsertarNodo(int row, int col, string id, string nombre, string ip)
        {
            // 1. Validar campos obligatorios
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(ip))
            {
                string msgErr = "Error al insertar: Todos los campos del satélite son obligatorios.";
                MemoriaPlano.Logs.Registrar("ALERT", msgErr);
                TempData["ErrorMessage"] = msgErr;
                return RedirectToAction("Index", "Home");
            }

            id = id.Trim();
            nombre = nombre.Trim();
            ip = ip.Trim();

            // 2. Validar ID con Regex oficial
            if (!Regex.IsMatch(id, PatronIdSatelite))
            {
                string msgErr = $"Error sintáctico en ID '{id}': Debe cumplir 'SAT-(ECU|POL)-0000'.";
                MemoriaPlano.Logs.Registrar("ALERT", msgErr);
                TempData["ErrorMessage"] = msgErr;
                return RedirectToAction("Index", "Home");
            }

            // 3. Validar IPv4 con Regex oficial
            if (!Regex.IsMatch(ip, PatronIpv4))
            {
                string msgErr = $"Error sintáctico en IP '{ip}': Debe ser IPv4 válida.";
                MemoriaPlano.Logs.Registrar("ALERT", msgErr);
                TempData["ErrorMessage"] = msgErr;
                return RedirectToAction("Index", "Home");
            }

            // 4. Validar colisión de coordenadas
            if (MemoriaPlano.Matriz.Buscar(row, col) != null)
            {
                string msgErr = $"Colisión detectada: ya existe un nodo en las coordenadas ({row}, {col}).";
                MemoriaPlano.Logs.Registrar("ERROR", msgErr);
                TempData["ErrorMessage"] = msgErr;
                return RedirectToAction("Index", "Home");
            }

            // 5. Validar ID duplicado
            if (MemoriaPlano.Matriz.BuscarPorId(id) != null)
            {
                string msgErr = $"El identificador de satélite '{id}' ya existe en el plano.";
                MemoriaPlano.Logs.Registrar("ERROR", msgErr);
                TempData["ErrorMessage"] = msgErr;
                return RedirectToAction("Index", "Home");
            }

            try
            {
                MemoriaPlano.Matriz.Insertar(row, col, id, nombre, ip);
                string msgSucc = $"Nodo satelital [{id}] insertado con éxito en ({row}, {col}). Enlaces ortogonales actualizados.";
                MemoriaPlano.Logs.Registrar("INFO", msgSucc);
                TempData["SuccessMessage"] = msgSucc;
            }
            catch (Exception ex)
            {
                string msgErr = $"Error de inserción física: {ex.Message}";
                MemoriaPlano.Logs.Registrar("ERROR", msgErr);
                TempData["ErrorMessage"] = msgErr;
            }

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Acción POST: Elimina el nodo en la coordenada (row, col) y reconecta
        /// quirúrgicamente los cuatro punteros ortogonales adyacentes.
        /// </summary>
        [HttpPost]
        public IActionResult EliminarNodo(int row, int col)
        {
            MatrizNode? target = MemoriaPlano.Matriz.Buscar(row, col);

            if (target == null)
            {
                string msgErr = $"No existe ningún nodo en la coordenada ({row}, {col}) para eliminar.";
                MemoriaPlano.Logs.Registrar("ALERT", msgErr);
                TempData["ErrorMessage"] = msgErr;
                return RedirectToAction("Index", "Home");
            }

            try
            {
                string idEliminado = target.Id;
                MemoriaPlano.Matriz.Eliminar(row, col);
                string msgSucc = $"Nodo [{idEliminado}] eliminado de ({row}, {col}). Vecinos reconectados.";
                MemoriaPlano.Logs.Registrar("INFO", msgSucc);
                TempData["SuccessMessage"] = msgSucc;
            }
            catch (Exception ex)
            {
                string msgErr = $"Error al eliminar nodo: {ex.Message}";
                MemoriaPlano.Logs.Registrar("ERROR", msgErr);
                TempData["ErrorMessage"] = msgErr;
            }

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Acción POST: Vacía completamente la Matriz Dispersa y el Catálogo AVL
        /// liberando todas las referencias de nodos en memoria RAM.
        /// </summary>
        [HttpPost]
        public IActionResult LimpiarMatriz()
        {
            MemoriaPlano.Matriz.Vaciar();
            MemoriaPlano.Catalogo.Limpiar();
            MemoriaPlano.RutaActiva = null;
            MemoriaPlano.TickActual = 0;

            MemoriaPlano.Logs.Registrar("INFO", "Se purgaron todos los nodos de la Matriz Dispersa y el Catálogo AVL. Tick reiniciado.");
            TempData["SuccessMessage"] = "Se ha limpiado el plano espacial (Matriz, AVL y ruta activa).";
            return RedirectToAction("Index", "Home");
        }
    }
}




