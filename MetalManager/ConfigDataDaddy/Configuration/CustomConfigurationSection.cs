using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace MetalManager.ConfigDataDaddy.Configuration
{
    public class CustomSongsConfig : ConfigurationSection
    {
        /// <summary>
        /// The name of this section in the app.config.
        /// </summary>
        public const string SectionName = "CustomSongsConfig";

        private const string CustomsongsCollection = "Customsongs";

        [ConfigurationProperty(CustomsongsCollection)]
        [ConfigurationCollection(typeof(EndpointCollection), AddItemName = "add")]
        public EndpointCollection Customsongs { get { return (EndpointCollection)base[CustomsongsCollection]; } }

    }
}
