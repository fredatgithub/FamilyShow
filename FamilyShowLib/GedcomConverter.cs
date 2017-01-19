/*
 * GedcomConverter class
 * Converts a GEDCOM file to an XML file.
 * 
 * GedcomLine class
 * Parses one line in a GEDCOM file.
*/

using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Microsoft.FamilyShowLib
{
    /// <summary>
    /// Converts a GEDCOM file to an XML file.
    /// </summary>
    static class GedcomConverter
    {
        /// Create an xml file that contains the same hierarchy specified in 
        /// the GEDCOM file. GEDCOM lines are limited to 255 characters, 
        /// combineSplitValues indicates if the split lines should be combined 
        /// into a single XML element.
        static public void ConvertToXml(string gedcomFilePath, 
            string xmlFilePath, bool combineSplitValues)
        {
            // Store the previous level so can determine when need
            // to close xml element tags.
            int prevLevel = -1;

            // Used to create the .xml file, XmlWriterSettings. Indent is 
            // specified if you want to examine the xml file, otherwise
            // it should be removed.
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            using (XmlWriter writer = XmlWriter.Create(xmlFilePath, settings))
            {
                // Root element that contains all of the other elements.
                writer.WriteStartElement("root");

                // Convert each line of the gedcom file to an xml element.
                using (StreamReader sr = new StreamReader(gedcomFilePath))
                {
                    string text;
                    GedcomLine line = new GedcomLine();
                    while ((text = sr.ReadLine()) != null)
                    {
                        // Some GEDCOM files indent each line with whitespace, delete any
                        // whitespace from the beginning and end of the line.
                        text = text.Trim();

                        // Parse gedcome line into Level, Tag and Value fields.
                        if (line.Parse(text))
                        {
                            // See if need to close previous xml elements.
                            if (line.Level <= prevLevel)
                            {
                                // Determine how many elements to close.
                                int count = prevLevel - line.Level + 1;
                                for (int i = 0; i < count; i++)
                                    writer.WriteEndElement();
                            }

                            // Create new xml element.
                            writer.WriteStartElement(line.Tag);
                            writer.WriteAttributeString("Value", line.Data);
                            
                            prevLevel = line.Level;
                        }
                    }
                }

                // Close the last element.
                writer.WriteEndElement();

                // Close the root element.
                writer.WriteEndElement();

                // Write to the file system.
                writer.Flush();
                writer.Close();

                if (combineSplitValues)
                    CombineSplitValues(xmlFilePath);
            }
        }

        /// <summary>
        /// GEDCOM lines have a max length of 255 characters, this goes through
        /// the XML files and combines all of the split lines which makes the
        /// XML file easier to process.
        /// </summary>
        static private void CombineSplitValues(string xmlFilePath)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlFilePath);

            // Get all nodes that contain child continue nodes.
            XmlNodeList list = doc.SelectNodes("//CONT/.. | //CONC/..");

            // Go through each node and append child continue nodes.
            foreach (XmlNode node in list)
                AppendValues(node);

            // Write the updates to the file system.
            doc.Save(xmlFilePath);
        }

        /// <summary>
        /// Append child continue nodes to the parent.
        /// </summary>
        static private void AppendValues(XmlNode node)
        {
            // Get the value for the parent node.
            StringBuilder sb = new StringBuilder(node.Attributes["Value"].Value);

            // Find all of the child continue nodes.
            XmlNodeList list = node.SelectNodes("CONT | CONC");
            foreach (XmlNode childNode in list)
            {
                switch (childNode.Name)
                {
                    // Concatenate.
                    case "CONC":
                        sb.Append(childNode.Attributes["Value"].Value);
                        break;

                    // Continue, add line return and then the text.
                    case "CONT":
                        sb.AppendFormat("\r{0}", childNode.Attributes["Value"].Value);
                        break;
                }

                // Remove all of the child continue nodes.
                node.RemoveChild(childNode);
            }

            // Update the parent node value.
            node.Attributes["Value"].Value = sb.ToString();
        }
    }

    /// <summary>
    /// Parses one line in a GEDCOM file.
    /// </summary>
    class GedcomLine
    {
        #region fields

        // Parts of the GEDCOM line.
        private int level;
        private string tag;
        private string data;

        // Expression pattern used to parse the GEDCOM line.
        private readonly Regex regSplit = new Regex(
            @"(?<level>\d+)\s+(?<tag>[\S]+)(\s+(?<data>.+))?");

        // Expression pattern used to clean up the GEDCOM line.
        // Only allow viewable characters.
        private readonly Regex regClean = new Regex(@"[^\x20-\x7e]");

        // Expression pattern used to clean up the GEDCOM tag.
        // Tag can contain alphanumeric characters, _, ., or -.
        private readonly Regex regTag = new Regex(@"[^\w.-]");

        #endregion

        #region properties

        /// <summary>
        /// Level of the tag.
        /// </summary>
        public int Level
        {
            get { return this.level; }
            set { this.level = value; }
        }

        /// <summary>
        /// Line tag.
        /// </summary>
        public string Tag
        {
            get { return this.tag; }
            set { this.tag = value; }
        }

        /// <summary>
        /// Data of the tag.
        /// </summary>
        public string Data
        {
            get { return this.data; }
            set { this.data = value; }
        }

        #endregion

        /// <summary>
        /// Parse the Level, Tag, and Data fields from the GEDCOM line.
        /// The following is a sample GEDCOM line:
        /// 
        ///    2 NAME Personal Ancestral File
        ///    
        /// The Level = 2, Tag = NAME, and Data = Personal Ancestral File.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public bool Parse(string text)
        {
            try
            {
                // Init values.
                Clear();

                // Return right away is nothing to parse.
                if (string.IsNullOrEmpty(text))
                    return false;

                // Clean up the line by only allowing viewable characters.
                text = regClean.Replace(text, "");

                // Get the parts of the line.
                Match match = regSplit.Match(text);
                this.level = Convert.ToInt32(match.Groups["level"].Value, CultureInfo.InvariantCulture);
                this.tag = match.Groups["tag"].Value.Trim();
                this.data = match.Groups["data"].Value.Trim();
                
                // The pointer reference is specified in the tag, and the tag in the data,
                // swap these two values to make it more consistent, the tag contains the 
                // tag and data contains the pointer reference.
                if (this.tag[0] == '@')
                {
                    string temp = this.tag;
                    this.tag = this.data;
                    this.data = temp;
                    int pos = this.tag.IndexOf(' ');

                    // Some GEDCOM files have additional info, 
                    // we only handle the tag info.
                    if (pos != -1)
                        this.tag = this.tag.Substring(0, pos);
                }

                // Make sure there are not any invalid characters in the tag.
                this.tag = regTag.Replace(this.tag, "");

                return true;
            }
            catch
            {
                // This line is invalid, clear all values.
                Clear();
                return false;
            }
        }

        /// <summary>
        /// Reset all values.
        /// </summary>
        private void Clear()
        {
            this.Level = 0;
            this.Tag = "";
            this.Data = "";
        }
    }

}
