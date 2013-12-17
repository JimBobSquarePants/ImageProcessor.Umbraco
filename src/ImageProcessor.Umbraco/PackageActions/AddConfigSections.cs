using System;
using System.Configuration;
using System.IO;
using System.Web.Configuration;
using System.Xml;
using ImageProcessor.Web.Config;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.packager.standardPackageActions;
using umbraco.interfaces;
using umbraco.IO;

namespace ImageProcessor.Umbraco.PackageActions
{
	/// <summary>
	/// This package action will Add a new HTTP Module to the web.config file.
	/// </summary>
	/// <remarks>
	/// This package action has been customized from the PackageActionsContrib Project.
	/// http://packageactioncontrib.codeplex.com
	/// </remarks>
	public class AddConfigSections : IPackageAction
	{
		/// <summary>
		/// The alias of the action - for internal use only.
		/// </summary>
		internal static readonly string ActionAlias = "ImageProcessor_AddConfigSections";

		/// <summary>
		/// This Alias must be unique and is used as an identifier that must match the alias in the package action XML.
		/// </summary>
		/// <returns>The Alias of the package action.</returns>
		public string Alias()
		{
			return ActionAlias;
		}

		/// <summary>
		/// Executes the specified package name.
		/// </summary>
		/// <param name="packageName">Name of the package.</param>
		/// <param name="xmlData">The XML data.</param>
		/// <returns></returns>
		public bool Execute(string packageName, XmlNode xmlData)
		{
			try
			{
				var webConfig = WebConfigurationManager.OpenWebConfiguration("~/");
				if (webConfig.SectionGroups["imageProcessor"] == null)
				{
					var sectionGroup = new ConfigurationSectionGroup();

					this.AddSectionToSectionGroup(sectionGroup, "security", new ImageSecuritySection());
					this.AddSectionToSectionGroup(sectionGroup, "processing", new ImageProcessingSection());
					this.AddSectionToSectionGroup(sectionGroup, "cache", new ImageCacheSection());

					webConfig.SectionGroups.Add("imageProcessor", sectionGroup);

					webConfig.Save(ConfigurationSaveMode.Modified);
				}

				return true;
			}
			catch (Exception ex)
			{
				Log.Add(LogTypes.Error, -1, string.Format("Error at install {0} package action: {1}", ActionAlias, ex));
			}

			return false;
		}

		/// <summary>
		/// Returns a Sample XML Node
		/// </summary>
		/// <returns>The sample xml as node</returns>
		public XmlNode SampleXml()
		{
			string xml = string.Concat("<Action runat=\"install\" undo=\"true\" alias=\"", this.Alias(), "\" />");
			return helper.parseStringToXmlNode(xml);
		}

		/// <summary>
		/// Undoes the specified package name.
		/// </summary>
		/// <param name="packageName">Name of the package.</param>
		/// <param name="xmlData">The XML data.</param>
		/// <returns></returns>
		public bool Undo(string packageName, XmlNode xmlData)
		{
			try
			{
				var webConfig = WebConfigurationManager.OpenWebConfiguration("~/");
				if (webConfig.SectionGroups["imageprocessor"] != null)
				{
					webConfig.SectionGroups.Remove("imageprocessor");

					webConfig.Save(ConfigurationSaveMode.Modified);
				}

				return true;
			}
			catch (Exception ex)
			{
				string message = string.Concat("Error at undo ", this.Alias(), " package action: ", ex);
				Log.Add(LogTypes.Error, -1, message);
			}

			return false;
		}

		private void AddSectionToSectionGroup(ConfigurationSectionGroup sectionGroup, string name, ConfigurationSection section)
		{
			if (sectionGroup.Sections[name] == null)
			{
				var configPath = string.Concat("config", Path.DirectorySeparatorChar, "imageprocessor", Path.DirectorySeparatorChar, name, ".config");
				var xmlPath = IOHelper.MapPath(string.Concat("~/", configPath));
				string xml;

				using (var reader = new StreamReader(xmlPath))
				{
					xml = reader.ReadToEnd();
				}

				section.SectionInformation.SetRawXml(xml);
				section.SectionInformation.ConfigSource = configPath;

				sectionGroup.Sections.Add(name, section);
			}
		}
	}
}