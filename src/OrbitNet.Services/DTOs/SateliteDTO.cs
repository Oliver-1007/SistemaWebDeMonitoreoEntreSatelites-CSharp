using System;

namespace OrbitNet.Services.DTOs
{
    /// <summary>
    /// Objeto de Transferencia de Datos (DTO) para la entidad Satelite.
    /// Esta clase es plana y no contiene punteros autorreferenciados (como Up, Down, Left, Right).
    /// Su propósito exclusivo es ser serializada a formato JSON de forma segura, evitando 
    /// excepciones de referencia circular (ciclos de serialización infinitos) en el framework.
    /// </summary>
    public class SateliteDto
    {
        /// <summary>
        /// Indice de la coordenada vertical (fila) en la que se ubica el satelite.
        /// </summary>
        public int Fila { get; set; }

        /// <summary>
        /// Indice de la coordenada horizontal (columna) en la que se ubica el satelite.
        /// </summary>
        public int Columna { get; set; }

        /// <summary>
        /// Identificador unico del satelite (ejemplo: SAT-ECU-0001).
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Nombre descriptivo del satelite.
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Direccion IP de conexion del satelite en formato IPv4.
        /// </summary>
        public string IpAddress { get; set; } = string.Empty;
    }
}


