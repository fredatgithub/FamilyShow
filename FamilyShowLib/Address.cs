using System;

namespace Microsoft.FamilyShowLib
{
  [Serializable]
  public class Address
  {
    #region Fields and Constants
    private string address1;
    private string address2;
    private string city;
    private string country;
    private string zipCode;
    #endregion

    #region Properties
    public string Address1
    {
      get { return address1; }
      set { address1 = value; }
    }

    public string Address2
    {
      get { return address2; }
      set { address2 = value; }
    }

    public string City
    {
      get { return city; }
      set { city = value; }
    }

    public string Country
    {
      get { return country; }
      set { country = value; }
    }

    public string ZipCode
    {
      get { return zipCode; }
      set { zipCode = value; }
    }
    #endregion
  }
}
