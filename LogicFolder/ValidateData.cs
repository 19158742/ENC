using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ENC
{
    public class ValidateData
    {
        internal string validateSenderInformation(string senderFirstName, string senderLastName, string senderEmail, string senderSecretWord)
        {
            try
            {
                return "success";
            }
            catch (Exception ex)
            {
                return "failure";
            }
        }
    }
}