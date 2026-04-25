using Microsoft.Extensions.Options;
using System.Text.Json;
using TecnoFuturo.Core.Entities;

namespace TecnoFuturo.Data.Helpers
{
    public class JsonHelper
    {
        private readonly string _directorioTrabajo;
        private readonly JsonSerializerOptions _jsonOptions;
        public JsonHelper(IOptions<DirectorioOption> option)
        {
           
                _directorioTrabajo = Path.Combine(Directory.GetCurrentDirectory(), option.Value.Datos);

                if (!Directory.Exists(_directorioTrabajo))
                {
                    Directory.CreateDirectory(_directorioTrabajo);
                }

                _jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
           
        }
        public List<T>? LeerDatos<T>(string nombreArchivo)
        {
            try
            {
                string ruta = Path.Combine(_directorioTrabajo, nombreArchivo);

                if (!File.Exists(ruta))
                {
                    return new List<T>();
                }

                var json = File.ReadAllText(ruta);
                if (string.IsNullOrEmpty(json))
                {
                    return new List<T>();
                }

                return JsonSerializer.Deserialize<List<T>>(json);
            }
            catch 
            {
                return new List<T>();
            }
        }

        public void Guardar<T>(string nombreArchivo, IEnumerable<T> datos)
        {
            string ruta = Path.Combine(_directorioTrabajo, nombreArchivo);

            var json = JsonSerializer.Serialize(datos, _jsonOptions);
            File.WriteAllText(ruta, json);
        }
    }
}
