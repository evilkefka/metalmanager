using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace MetalManager.ConfigDataDaddy.Configuration
{
    public class EndpointCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new EndpointElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((EndpointElement)element).Name;
        }
    }

    public class EndpointElement : ConfigurationElement
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

        [ConfigurationProperty("lwt", IsRequired = false, DefaultValue = "")]
        public string LastWriteTime
        {
            get { return (string)this["lwt"]; }
            set { this["lwt"] = value; }
        }

        [ConfigurationProperty("lvt", IsRequired = false, DefaultValue = "")]
        public string LastVerifiedTime
        {
            get { return (string)this["lvt"]; }
            set { this["lvt"] = value; }
        }
    }
}
