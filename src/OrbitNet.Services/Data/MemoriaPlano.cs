using OrbitNet.Core.Nodes;
using OrbitNet.Core.Structures;

namespace OrbitNet.Services.Data
{
    /// <summary>
    /// Clase estática global que almacena en memoria el estado del plano satelital.
    /// Permite a los controladores acceder de manera unificada al estado en RAM.
    /// No usa colecciones de System.Collections.
    /// </summary>
    public static class MemoriaPlano
    {
        /// <summary>TDA principal: Matriz Dispersa Ortogonal del plano satelital.</summary>
        public static SparseMatrix Matriz { get; } = new SparseMatrix();

        /// <summary>TDA de auditoría: Lista Enlazada Simple con puntero Tail.</summary>
        public static LogAuditoria Logs { get; } = new LogAuditoria();

        /// <summary>TDA catálogo: Árbol AVL de satélites polares.</summary>
        public static AvlTree Catalogo { get; } = new AvlTree();

        /// <summary>Ruta ortogonal activa calculada por el EnrutadorOrtogonal.</summary>
        public static MatrizNode[]? RutaActiva { get; set; } = null;

        /// <summary>Contador de ticks de simulación procesados.</summary>
        public static int TickActual { get; set; } = 0;
    }
}