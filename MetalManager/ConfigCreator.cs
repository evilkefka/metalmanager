using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace MetalManager
{
    class ConfigCreator
    {

        /// <summary>
        /// Comes to the rescue if no Config file can be found. Creates the file, then restarts MetalManager.exe
        /// </summary>
        public static void CreateConfig()
        {
            //because I'm tired and it's easier this way

            string newConfig = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\n";
            newConfig += "<configuration>\n";
            newConfig += "  <configSections>\n";
            newConfig += "    <section name =\"CustomSongsConfig\" type=\"MetalManager.ConfigDataDaddy.Configuration.CustomSongsConfig, MetalManager\" />\n";
            newConfig += "  </configSections>\n";
            newConfig += "  \n";
            newConfig += "    <startup> \n";
            newConfig += "        <supportedRuntime version =\"v4.0\" sku=\".NETFramework,Version=v4.6.1\" />\n";
            newConfig += "    </startup>\n";
            newConfig += "  <appSettings>\n";
            newConfig += "    <add key =\"gameDirectory\" value=\"\"/>\n";
            newConfig += "    <add key =\"modDirectory\" value=\"\"/>\n";
            newConfig += "  </appSettings>\n\n";
            newConfig += "  <CustomSongsConfig>\n";
            newConfig += "    <Customsongs>\n";
            newConfig += "      <add name =\"null\" path=\"null\" lwt=\"0\" lvt=\"\" />\n";
            newConfig += "    </Customsongs>\n";
            newConfig += "  </CustomSongsConfig>\n";
            newConfig += "  \n";
            newConfig += "</configuration>\n";

            string appsHome = AppDomain.CurrentDomain.BaseDirectory;
            

            try
            {
                
                File.WriteAllText(appsHome + "\\MetalManager.exe.config", newConfig);
                //MessageBox.Show("No config found. Making a new one and restarting Metal Manager.\n" + appsHome);
                //this isn't very welcoming to new users who just downloaded the MetalManager.exe. And there's no reason to alert people who deleted it, because screw them

                Application.Restart();
                Environment.Exit(0);
            }
            catch 
            {
                MessageBox.Show("No config found, and we experienced an error when making a new one.\nStart up was cancelled. :(");
                System.Windows.Forms.Application.Exit();
                Environment.Exit(0);
            }
        }

    }
}
