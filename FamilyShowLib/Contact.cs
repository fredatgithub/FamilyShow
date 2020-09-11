using System;

namespace Microsoft.FamilyShowLib
{
  [Serializable]
  public class Contact
  {
    private string mail;
    private Address address;
    private string phone;

    public string Mail
    {
      get { return mail; }
      set { mail = value; }
    }

    public Address Address
    {
      get { return address; }
      set { address = value; }
    }

    public string Phone
    {
      get { return phone; }
      set { phone = value; }
    }
  }
}
