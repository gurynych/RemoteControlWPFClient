using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteControlWPFClient.MVVM.Models
{
    public partial class User
    {
        public string Login { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public byte[] AuthToken { get; set; }

        public User()
        {            

        }

        public User(string email, string password) : this()
        {
            Email = email;
            Password = password;
        }

        public User(string login, string email, string password) : this(email, password)
        {
            Login = login;
            Email = email;
            Password = password;
        }

    }
}
