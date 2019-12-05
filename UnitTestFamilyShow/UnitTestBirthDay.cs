using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

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
      string source = "26/06/1965";
      DateTime expected = new DateTime(1965, 6, 26);
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
      string source = "26-06-1965";
      DateTime expected = new DateTime(1965, 6, 26);
      DateTime result = StringToDate(source);
      Assert.AreEqual(result, expected);
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
