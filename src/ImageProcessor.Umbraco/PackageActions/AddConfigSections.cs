using System;
using System.Configuration;
using System.IO;
using System.Web.Configuration;
using System.Xml;
using ImageProcessor.Web.Config;
using umbraco;
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
		const string ActionAlias = "ImageProcessor_AddConfigSections";

		/// <summary>
		/// Set the web.config full path
		/// </summary>
		const string WebConfigPath = "~/Web.config";

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
				// Set result default to false
				var result = false;

				// Create a new xml document
				var document = new XmlDocument();

				// Keep current indentions format
				document.PreserveWhitespace = true;

				// Load the web.config file into the xml document
				document.Load(IOHelper.MapPath(WebConfigPath));

				// Set modified document default to false
				bool modified = false;

				var rootNode = document.SelectSingleNode("/configuration/configSections");
				if (rootNode == null)
					return result;

				// Set insert node default true
				bool insertNode = true;

				// Look for existing nodes with same name like the new node
				if (rootNode.HasChildNodes)
				{
					// Look for existing nodeType nodes
					var node = rootNode.SelectSingleNode(string.Format("sectionGroup[@name = '{0}']", "imageProcessor"));

					// If name already exists 
					if (node != null)
					{
						// Cancel insert node operation
						insertNode = false;
					}
				}

				// Check for insert flag
				if (insertNode)
				{
					// imageProcessor config sectionGroup
					var group = document.CreateElement("sectionGroup");
					group.Attributes.Append(xmlHelper.addAttribute(document, "name", "imageProcessor"));

					var security = document.CreateElement("section");
					security.Attributes.Append(xmlHelper.addAttribute(document, "name", "security"));
					security.Attributes.Append(xmlHelper.addAttribute(document, "requirePermission", "false"));
					security.Attributes.Append(xmlHelper.addAttribute(document, "type", "ImageProcessor.Web.Config.ImageSecuritySection, ImageProcessor.Web"));

					var processing = document.CreateElement("section");
					processing.Attributes.Append(xmlHelper.addAttribute(document, "name", "processing"));
					processing.Attributes.Append(xmlHelper.addAttribute(document, "requirePermission", "false"));
					processing.Attributes.Append(xmlHelper.addAttribute(document, "type", "ImageProcessor.Web.Config.ImageProcessingSection, ImageProcessor.Web"));

					var cache = document.CreateElement("section");
					cache.Attributes.Append(xmlHelper.addAttribute(document, "name", "cache"));
					cache.Attributes.Append(xmlHelper.addAttribute(document, "requirePermission", "false"));
					cache.Attributes.Append(xmlHelper.addAttribute(document, "type", "ImageProcessor.Web.Config.ImageCacheSection, ImageProcessor.Web"));

					group.AppendChild(security);
					group.AppendChild(processing);
					group.AppendChild(cache);

					rootNode.AppendChild(group);

					// imageProcessor config / configSource
					var config = document.CreateElement("imageProcessor");

					var configSecurity = document.CreateElement("security");
					configSecurity.Attributes.Append(xmlHelper.addAttribute(document, "configSource", @"config\imageprocessor\security.config"));

					var configCache = document.CreateElement("cache");
					configCache.Attributes.Append(xmlHelper.addAttribute(document, "configSource", @"config\imageprocessor\cache.config"));

					var configProcessing = document.CreateElement("processing");
					configProcessing.Attributes.Append(xmlHelper.addAttribute(document, "configSource", @"config\imageprocessor\processing.config"));

					config.AppendChild(configSecurity);
					config.AppendChild(configCache);
					config.AppendChild(configProcessing);

					// Append new node after the root node
					rootNode.ParentNode.InsertAfter(config, rootNode);

					// Mark document modified
					modified = true;
				}


				// Check for modified document
				if (modified)
				{
					// Save the Rewrite config file with the new rewerite rule
					document.Save(IOHelper.MapPath(WebConfigPath));

					// No errors so the result is true
					result = true;
				}

				return result;
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
			var xml = string.Format("<Action runat=\"install\" undo=\"true\" alias=\"{0}\" />", ActionAlias);
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
				// Set result default to false
				var result = false;

				// Create a new xml document
				var document = new XmlDocument();

				// Keep current indentions format
				document.PreserveWhitespace = true;

				// Load the web.config file into the xml document
				document.Load(IOHelper.MapPath(WebConfigPath));

				// Set modified document default to false
				var modified = false;

				// Select root node in the web.config file for insert new nodes
				var rootNode = document.SelectSingleNode("/configuration/configSections");

				// Check for rootNode exists
				if (rootNode == null)
					return result;

				// Look for existing nodes with same name of undo attribute
				if (rootNode.HasChildNodes)
				{
					// Look for existing add nodes with element/attribute name
					foreach (XmlNode existingNode in rootNode.SelectNodes(string.Format("sectionGroup[@name = '{0}']", "imageProcessor")))
					{
						// Remove existing node from root node
						rootNode.RemoveChild(existingNode);
						modified = true;
					}
				}

				rootNode = document.SelectSingleNode("/configuration");
				if (rootNode == null)
					return result;

				if (rootNode.HasChildNodes)
				{
					foreach (XmlNode existingNode in rootNode.SelectNodes(string.Format("{0}", "imageProcessor")))
					{
						rootNode.RemoveChild(existingNode);
						modified = true;
					}
				}

				if (modified)
				{
					try
					{
						// Save the Rewrite config file with the new rewerite rule
						document.Save(IOHelper.MapPath(WebConfigPath));

						// No errors so the result is true
						result = true;
					}
					catch (Exception ex)
					{
						Log.Add(LogTypes.Error, -1, string.Format("Error undoing '{0}' package action. {1}", ActionAlias, ex));
					}
				}

				return result;
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