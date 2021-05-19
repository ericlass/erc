using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.IO;

namespace erc
{
    public class Config
    {
        public string FasmPath { get; set; }

        public static Config Load()
        {
            var json = File.ReadAllText("config.json");
            var config = JsonConvert.DeserializeObject<Config>(json);
            return config;
        }
    }
}
