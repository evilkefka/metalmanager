using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetalManager.ConfigDataDaddy
{
    /// <summary>
    /// Info about the server to talk to.
    /// This is used by the Customsong class.
    /// </summary>
    public class SongInfo
    {
        /// <summary>
        /// The path to the .json
        /// </summary>
        public string Path;

        /// <summary>
        /// Last-modified date of .json
        /// </summary>
        public string LastWriteTime;

        /// <summary>
        /// Last time .json was scanned for errors
        /// </summary>
        public string LastVerifiedTime;

        /// <summary>
        /// Explicit constructor.
        /// </summary>
        /// <param name="path">The path of the server.</param>
        /// <param name="lWt">Whether SSL should be used when talking to the server or not.</param>
        /// <param name="lVt">List of security groups that should be allowed to save changes.</param>
        public SongInfo(string patH = "", string lWt = "", string lVt = "")
        {
            Path = patH;
            LastWriteTime = lWt;
            LastVerifiedTime = lVt;
        }

        /// <summary>
        /// Returns true if this instance is empty, false otherwise.
        /// </summary>
        public bool IsEmpty { get { return this.Equals(_empty); } }

        /// <summary>
        /// Gets an empty ConnectionManagerServerInfo instance.
        /// </summary>
        public static SongInfo Empty { get { return _empty; } }
        private static readonly SongInfo _empty = new SongInfo();
    }
}
