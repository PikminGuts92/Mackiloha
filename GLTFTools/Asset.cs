using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GLTFTools
{
    public class Asset
    {
        private static string _versionPattern = "^[0-9]+.[0-9]+$";
        private static Regex _versionRegex = new Regex(_versionPattern);

        private string _version = "2.0";
        private string _minVersion = "2.0";

        /// <summary>
        /// Metadata about the glTF asset
        /// </summary>
        public Asset() { }

        /// <summary>
        /// A copyright message suitable for display to credit the content creator
        /// </summary>
        [JsonProperty("copyright")]
        public string Copyright { get; set; }

        /// <summary>
        /// Tool that generated this glTF model (Useful for debugging)
        /// </summary>
        [JsonProperty("generator")]
        public string Generator { get; set; }

        /// <summary>
        /// The glTF version that this asset targets
        /// </summary>
        [JsonProperty("version")]
        public string Version
        {
            get => _version;
            set
            {
                if (_versionRegex.IsMatch(value))
                    _version = value;
                else
                    throw new Exception($"\"{value}\" does not match pattern \"{_versionPattern}\"");
            }
        }

        /// <summary>
        /// The minimum glTF version that this asset targets
        /// </summary>
        [JsonProperty("minVersion")]
        public string MinVersion
        {
            get => _minVersion;
            set
            {
                if (_versionRegex.IsMatch(value))
                    _minVersion = value;
                else
                    throw new Exception($"\"{value}\" does not match pattern \"{_versionPattern}\"");
            }
        }
    }
}
