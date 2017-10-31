namespace DownloaderLibrary.DTO
{
    public class PriceDTO
    {
        public string Ciudad { get; set; }
        public string Magna { get; set; }
        public string Premium { get; set; }
        public string Diesel { get; set; }

        private string _entidad;
        public string Entidad
        {
            get
            {
                switch (_entidad)
                {
                    case "COAHUILA DE ZARAGOZA":
                        return "COAHUILA";
                    case "MICHOACÁN DE OCAMPO":
                        return "MICHOACÁN";
                    case "VERACRUZ DE IGNACIO DE LA LLAVE":
                        return "VERACRUZ";
                    default:
                        return _entidad;
                }
            }
            set
            {
                _entidad = value;
            }
        }
    }
}