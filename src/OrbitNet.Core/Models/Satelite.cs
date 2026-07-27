
namespace OrbitNet.Core.Models
{
    public class Satelite
    {
        private string id;
        private string nombre = "";
        private string enlaceIp = "";
        private double frecuencia = 0.0;

        //constructor
        public Satelite(string id, string nombre, string enlaceIp)
        {
            this.id = id;
            Nombre = nombre;
            EnlaceIp = enlaceIp;
            Frecuencia = 0.0;
        }

        public Satelite(string id, string nombre, double frecuencia)
        {
            this.id = id;
            Nombre = nombre;
            EnlaceIp = ""; // Los satelites polares no tienen una direccion IP en el xml de configuracion
            Frecuencia = frecuencia;
        }

        public string Id
        {
            get { return id; }
        }

        public string Nombre
        {
            get { return nombre; }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) // Devuelve verdadero si obtiene null, un "" o "  "
                {
                    throw new ArgumentNullException("El nombre del satelite no puede estar vacio");
                }
                nombre = value; // si todo esta bien se guarda el nombre
            }
        }

        public string EnlaceIp
        {
            get { return enlaceIp; }
            set
            {
                // Solo validar formato si la IP no esta en blanco 
                // (para dar soporte a satelites polares)
                if (!string.IsNullOrEmpty(value)) // Devuelve verdadero si el texto devuelve null o ""
                {
                    if (!value.Contains(".")) // Si no contiene un punto lan
                    {
                        throw new ArgumentException("La direccion Ip debe tener un formato IPv4 valido");
                    }
                }
                enlaceIp = value ?? ""; // si todo esta bien se guarda el enlace Ip
            }
        }

        public double Frecuencia
        {
            get { return frecuencia; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("La frecuencia de operacion no puede ser negatva");
                }
                frecuencia = value;
            }
        }

        public string ObtenerDescripcion()
        {
            if (frecuencia > 0)
            {
                return $"Satelite: {Nombre} (ID: {Id}) -> Freq: {Frecuencia} Mhz";
            }
            return $"Satelite: {Nombre} (ID: {Id}) -> IP: {EnlaceIp}";
        }
    }
}
