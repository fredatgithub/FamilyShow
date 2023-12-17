using System.Globalization;

using FamilyShowLib;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestFamilyShow
{
  [TestClass]
  public class UnitTestBirthDay
  {
    [TestMethod]
    public void TestMethod_StringToDate_no_slash()
    {
      string source = "26061965";
      DateTime expected = new DateTime(1, 1, 1);
      DateTime result = StringToDate(source);
      Assert.AreEqual(result, expected);
    }

    [TestMethod]
    public void TestMethod_StringToDate_with_slash()
    {
      string source = "08/05/1950";
      DateTime expected = new DateTime(1950, 5, 8);
      DateTime result = StringToDate(source);
      Assert.AreEqual(result, expected);
    }

    [TestMethod]
    public void TestMethod_StringToDate_string_empty()
    {
      string source = "";
      DateTime expected = new DateTime(1, 1, 1);
      DateTime result = StringToDate(source);
      Assert.AreEqual(result, expected);
    }

    [TestMethod]
    public void TestMethod_StringToDate_with_dash()
    {
      string source = "08-05-1950";
      DateTime expected = new DateTime(1950, 5, 8);
      DateTime result = StringToDate(source);
      Assert.AreEqual(result, expected);
    }

    [TestMethod]
    public void TestMethod_StringToDate_with_dash_1_1_1()
    {
      string source = "01-01-0001";
      DateTime expected = new DateTime(1, 1, 1);
      DateTime result = StringToDate(source);
      Assert.AreEqual(result, expected);
    }

    [TestMethod]
    [DataRow(2019, 6, 21, "21 JUN 2019")]
    [DataRow(1982, 1, 9, "9 JAN 1982")]
    public void TestMethod_GEDCOM(int year, int month, int day, string expected)
    {
      DateTime date = new(year, month, day);
      var result = GedcomExport.ExportDate(date);
      Assert.AreEqual(expected, result);
      Assert.AreEqual(date, DateTime.Parse(result, CultureInfo.InvariantCulture));
    }

    public static DateTime StringToDate(string dateString)
    {
      // Append first month and day if just the year was entered.
      if (dateString.Length == 4)
      {
        dateString = "1/1/" + dateString;
      }

      DateTime.TryParse(dateString, out DateTime date);

      return date;
    }
  }
}
