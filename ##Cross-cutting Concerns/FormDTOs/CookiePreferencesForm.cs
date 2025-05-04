using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace __Cross_cutting_Concerns.FormDTOs
{
    public class CookiePreferencesForm
    {
        public string Consent { get; set; }
        public bool Functional { get; set; }
        public bool Analytics { get; set; }
        public bool Marketing { get; set; }
    }
}
