/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace MetalManager
{
    public class CustomSongsConfig: ConfigurationSection
    {
        //<summary>
        // the name of this section in the app.config
        //summary

        public const string SectionName = "CustomSongsConfig";

        private const string EndpointCollectionName = "CustomSong";

        [ConfigurationProperty(EndpointCollectionName)]
        [ConfigurationCollection(typeof(CustomSongsCollection), AddItemName = "add")]
        public CustomSongsCollection CustomSongs { get { return (CustomSongsCollection)base[EndpointCollectionName]; } }


    }

    public class CustomSongsCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new CSConfigEndpointElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((CSConfigEndpointElement)element).Name;
        }
    }

    public class CSConfigEndpointElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("path", IsRequired = true)]
        public string Path
        {
            get { return (string)this["path"]; }
            set { this["path"] = value; }
        }

        [ConfigurationProperty("lwt", IsRequired = false)]
        public string LastWriteTime
        {
            get { return (string)this["lwt"]; }
            set { this["lwt"] = value; }
        }

        [ConfigurationProperty("lvt", IsRequired = false)]
        public string LastVerificationTime
        {
            get { return (string)this["lvt"]; }
            set { this["lvt"] = value; }
        }

    }




}
*/