/*
 * GedcomConverter class
 * Converts a GEDCOM file to an XML file.
 * 
 * GedcomLine class
 * Parses one line in a GEDCOM file.
*/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Microsoft.FamilyShowLib
{
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
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
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
                    {
                        this.tag = this.tag.Substring(0, pos);
                    }
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