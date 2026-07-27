using OrbitNet.Core.Structures;

namespace OrbitNet.WebSur.Models
{
    public class DashBoardViewModel
    {
        public SparseMatrix Matriz { get; set; } = null!;
        public LogAuditoria Logs { get; set; } = null!;
        public AvlTree Catalogo { get; set; } = null!;
        public string SvgDiagrama { get; set; } = string.Empty;
    }
}