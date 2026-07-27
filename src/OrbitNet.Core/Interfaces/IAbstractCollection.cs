namespace OrbitNet.Core.Interfaces
{
    /// <summary>
    /// Firma base abstracta que implementan todos los TDA para el control de auditoría.
    /// No permitido el uso de System.Collections o System.Collections.Generic.
    /// </summary>
    public interface IAbstractCollection
    {
        /// <summary> Cantidad de elementos actualmente almacenados en la estructura </summary>
        int Count { get; }

        /// <summary> Elimina todos los nodos de la estructura y libera las referencias en memoria. </summary>
        void Clear();

        /// <summary> Retorna true si la estructura no contiene ningun elemento </summary>
        bool IsEmpty { get; }
    }
}