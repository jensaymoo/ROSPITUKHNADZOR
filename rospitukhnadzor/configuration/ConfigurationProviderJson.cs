using FluentValidation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace RosPitukhNadzor
{
    internal class ConfigurationProviderJson: IConfigurationProvider
    {
        private static Configuration instance;

        public ConfigurationProviderJson() 
        {
            try
            {
                var asm_path = Directory.GetCurrentDirectory();
                instance = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(Path.Combine(asm_path, "config.json")))!;

                var validator = new ConfigurationValidator();
                validator.ValidateAndThrow(instance);
            }
            catch
            {
                throw;
            }     
        }

        public Configuration GetConfiguration()
        {
            return instance;
        }

        public bool SaveConfiguration()
        {
            var asm_path = Directory.GetCurrentDirectory();
            File.WriteAllText(Path.Combine(asm_path, "config.json"), JsonConvert.SerializeObject(instance, Formatting.Indented));
            return true;
        }
    }

}
