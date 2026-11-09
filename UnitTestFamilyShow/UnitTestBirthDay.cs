namespace UnitTestFamilyShow
{
  [TestClass]
  public class UnitTestBirthDay
  {
    [TestMethod]
    public void TestMethod_StringToDate_no_slash()
    {
      const string source = "26061965";
      DateTime expected = new(1, 1, 1);
      DateTime result = StringToDate(source);
      Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void TestMethod_StringToDate_with_slash()
    {
      const string source = "08/05/1950";
      DateTime expected = new(1950, 5, 8);
      DateTime result = StringToDate(source);
      Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void TestMethod_StringToDate_string_empty()
    {
      const string source = "";
      DateTime expected = new(1, 1, 1);
      DateTime result = StringToDate(source);
      Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void TestMethod_StringToDate_with_dash()
    {
      const string source = "08-05-1950";
      DateTime expected = new(1950, 5, 8);
      DateTime result = StringToDate(source);
      Assert.AreEqual(expected, result);
    }

    [TestMethod]
    public void TestMethod_StringToDate_with_dash_1_1_1()
    {
      const string source = "01-01-0001";
      DateTime expected = new(1, 1, 1);
      DateTime result = StringToDate(source);
      Assert.AreEqual(expected, result);
    }

    public static DateTime StringToDate(string dateString)
    {
      // Append first month and day if just the year was entered.
      if (dateString.Length == 4)
      {
        dateString = "1/1/" + dateString;
      }

      _ = DateTime.TryParse(dateString, out DateTime date);

      return date;
    }
  }
}
