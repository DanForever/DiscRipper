using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Xml;

namespace DiscRipper
{
    internal class DiscRipperSettingsProvider : LocalFileSettingsProvider
    {
        public static string SettingsFileName => "user.config";
        public static string SettingsDirectoryPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DiscRipper");
        public static string SettingsFilePath => Path.Combine(SettingsDirectoryPath, SettingsFileName);

        public override string Name => "DiscRipperSettingsProvider";

        public override string ApplicationName => AppDomain.CurrentDomain.FriendlyName;

        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(Name, config);

            // Decide where to store settings
            Directory.CreateDirectory(SettingsDirectoryPath);
        }

        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection properties)
        {
            var values = new SettingsPropertyValueCollection();

            // Load existing values from XML file
            var xml = new XmlDocument();
            if (File.Exists(SettingsFilePath))
            {
                xml.Load(SettingsFilePath);
            }
            else
            {
                xml.AppendChild(xml.CreateElement("Settings"));
            }

            foreach (SettingsProperty prop in properties)
            {
                var value = new SettingsPropertyValue(prop);

                var node = xml.DocumentElement!.SelectSingleNode($"Setting[@Name='{prop.Name}']") as XmlElement;
                if (node != null)
                {
                    value.SerializedValue = node.InnerText;
                }
                else
                {
                    value.SerializedValue = prop.DefaultValue;
                }

                values.Add(value);
            }

            return values;
        }

        public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection values)
        {
            var xml = new XmlDocument();
            if (File.Exists(SettingsFilePath))
            {
                xml.Load(SettingsFilePath);
            }
            else
            {
                xml.AppendChild(xml.CreateElement("Settings"));
            }

            foreach (SettingsPropertyValue value in values)
            {
                var node = xml.DocumentElement!.SelectSingleNode($"Setting[@Name='{value.Name}']") as XmlElement;
                if (node == null)
                {
                    node = xml.CreateElement("Setting");
                    node.SetAttribute("Name", value.Name);
                    xml.DocumentElement.AppendChild(node);
                }
                node.InnerText = value.SerializedValue?.ToString() ?? string.Empty;
            }

            xml.Save(SettingsFilePath);
        }
    }

    [SettingsProvider(typeof(DiscRipperSettingsProvider))]
    internal sealed partial class Settings
    {
    }
}
