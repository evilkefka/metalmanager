using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using MetalManager.ConfigDataDaddy.Configuration;

namespace MetalManager.ConfigDataDaddy
{
    /// <summary>
    /// Holds an Customsong's properties.
    /// </summary>
    public class Customsong
    {
        /// <summary>
        /// The Name of this Customsong.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The Server Info used to connect to the Customsong.
        /// </summary>
        public SongInfo SongInfo { get; set; }

        /// <summary>
        /// Returns true if this instance does not contain a valid Path, false otherwise.
        /// </summary>
        public bool IsEmpty { get { return string.IsNullOrEmpty(Name) && SongInfo.IsEmpty; } }
    }


    /// <summary>
    /// A collection of known Customsongs.
    /// </summary>
    /// 
    public static class Customsongs
    {
        /// <summary>
        /// A list of known Connection Environments.
        /// </summary>
        public static IEnumerable<Customsong> CustomsongsList { get { foreach (var endpoint in _customsongsList) { yield return endpoint; } } }
        private static readonly List<Customsong> _customsongsList = new List<Customsong>();

        /// <summary>
        /// Constructor.
        /// </summary>
        static Customsongs()
        {
            // Grab the Customsongs listed in the App.config and add them to our list.
            var customSection = ConfigurationManager.GetSection(CustomSongsConfig.SectionName) as CustomSongsConfig;
            if (customSection != null)
            {
                foreach (EndpointElement endpointElement in customSection.Customsongs)
                {
                    var endpoint = new Customsong() { Name = endpointElement.Name, SongInfo = new SongInfo() { Path = endpointElement.Path, LastWriteTime = endpointElement.LastWriteTime, LastVerifiedTime = endpointElement.LastVerifiedTime } };
                    AddCustomsong(endpoint);
                }
            }
        }

        /// <summary>
        /// Adds the given Customsong to the list of Customsongs.
        /// <para>NOTE: Null, duplicate, and Invalid values will not be added.</para>
        /// </summary>
        /// <param name="endpoint">The Customsong to add.</param>
        public static void AddCustomsong(Customsong endpoint)
        {
            if (endpoint == null)
                return;

            if (!_customsongsList.Contains(endpoint))
                _customsongsList.Add(endpoint);
        }

        /// <summary>
        /// Removes the given Customsong from the list of Customsongs.
        /// </summary>
        /// <param name="endpoint">The Customsong to remove.</param>
        public static void RemoveCustomsong(Customsong endpoint)
        {
            if (endpoint == null)
                return;

            if (_customsongsList.Contains(endpoint))
                _customsongsList.Remove(endpoint);
        }
    }
}
