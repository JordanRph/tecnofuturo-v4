using Microsoft.Extensions.Options;
using System.Text.Json;
using TecnoFuturo.Core.Entities;

namespace TecnoFuturo.Data.Helpers
{
    public class JsonHelper
    {
        private readonly string _directorioTabajo;
        private readonly JsonSerializerOptions _jsonOptions;
        public JsonHelper(IOptions<DirectorioOption> option)
        {
           
                _directorioTabajo = Path.Combine(Directory.GetCurrentDirectory(), option.Value.Datos);

                if (Directory.Exists(_directorioTabajo))
                {
                    Directory.CreateDirectory(_directorioTabajo);
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
                string ruta = Path.Combine(_directorioTabajo, nombreArchivo);

                if (!File.Exists(ruta))
                {
                    return null;
                }

                var json = File.ReadAllText(ruta);
                if (string.IsNullOrEmpty(json))
                {
                    return null;
                }

                return JsonSerializer.Deserialize<List<T>>(json);
            }
            catch 
            {
              return null;
            }
        }

        public void Guardar<T>(string ruta, IEnumerable<T> datos)
        {
            var json = JsonSerializer.Serialize(datos, _jsonOptions);
            File.WriteAllText(ruta, json);
        }
    }
}
