using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmbeddedMail {
  public interface ISmtpAuthorization {
    bool SmtpAuthorization(string username, string password);
  }
}
