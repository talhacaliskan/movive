using Moovoo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moovoo.Abstract
{
    internal interface IUserRepository
    {
        bool SignIn(User user);
        bool SignUp(User user);
        User GetUser(string email);
    }
}
