using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballManager.Models
{
    public class UserModel : Firebase.Auth.User
    {
        
    }

    public class Customer : Firebase.Auth.User
    {
        public string Sex { get; set; } // true male false female
        public string Address { get; set; }
    }
}
