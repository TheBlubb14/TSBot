using Newtonsoft.Json;
using System;
using System.IO;

namespace TSBot
{
    public sealed class Config
    {
        public string Path { get; private set; }

        public Secret Secret { get; private set; } = new Secret();

        public Config(string path)
        {
            if (string.IsNullOrEmpty(path))
                    throw new ArgumentNullException(nameof(path));

            this.Path = new FileInfo(path).FullName;
        }

        public bool Load()
        {
            if (File.Exists(this.Path))
            {
                this.Secret = JsonConvert.DeserializeObject<Secret>(File.ReadAllText(this.Path));
                return true;
            }
            else
            {
                return false;
            }
        }


        public void Save()
        {
            File.WriteAllText(this.Path, JsonConvert.SerializeObject(this.Secret, Formatting.Indented));
        }
    }
}
