using Moovoo.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moovoo.Models;
using System.Security.Cryptography;
using System.Web.Security;
using System.Web.Helpers;
using System.Net.Mail;
using System.Net;

namespace Moovoo.Concrete
{
    internal class UserConcrete : IUserRepository
    {
        public User GetUser(string email)
        {
            using (var db = new talhaDBEntities())
            {
                var user = db.Users.FirstOrDefault(p => p.Email == email);
                return user;
            }
        }
        public bool SignIn(User user)
        {
            using (var db = new talhaDBEntities())
            {
                var md5 = MD5.Create();
                if (db.Users.Any(p => p.Email == user.Email && user.SocialMediaAccountId != null))
                {
                    var usr = db.Users.FirstOrDefault(p => p.Email == user.Email);
                    if (usr != null)
                    {
                        usr.FirstName = user.FirstName;
                        usr.MiddleName = user.MiddleName;
                        usr.LastName = user.LastName;
                        usr.SocialMediaAccountId = user.SocialMediaAccountId;
                        usr.AccessToken = user.AccessToken;
                    }
                    db.SaveChanges();

                    return true;
                }
                else
                {
                    if (user.SocialMediaAccountId != null)
                    {
                        var request = db.Users.Any(p => p.SocialMediaAccountId.Equals(user.SocialMediaAccountId));
                        return request;
                    }
                    else if (user.Email != null && user.SocialMediaAccountId == null)
                    {
                        var userRequest = (from k in db.Users
                                           where k.Email == user.Email
                                           select k).FirstOrDefault();
                        var request = userRequest != null && Crypto.VerifyHashedPassword(userRequest.Password, user.Password);

                        return request;
                    }
                    else
                    {
                        return false;
                    }
                }

            }
        }
        public bool SignUp(User user)
        {
            using (var db = new talhaDBEntities())
            {
                try
                {
                    
                    if (db.Users.Any(p=>p.Email==user.Email))
                    {
                        return false;
                    }
                    else
                    {
                        if (user.Password != null)
                        {
                            var hash = Crypto.HashPassword(user.Password);
                            user.Password = hash;
                        }
                        db.Users.Add(user);
                        db.SaveChanges();
                        SendActivationMail(user);
                        return true;
                    }
                    
                }
                catch (Exception)
                {

                    return false;

                }
            }
        }
        public void SendActivationMail(User user)
        {
            const string body = "<p>Email From: {0} ({1})</p><p>Message:</p><p>{2}</p>";
            var message=new MailMessage();
            message.To.Add(new MailAddress(user.Email));  // replace with valid value 
            message.From = new MailAddress("welcome@movive.com");  // replace with valid value
            message.Subject = "Welcome to Movive";
            message.Body = string.Format(body, "Talha", "ÇALIŞKAN", "Siteme Hoşgeldniz");
            message.IsBodyHtml = true;

            using (var smtp = new SmtpClient())
            {
                var credential = new NetworkCredential
                {
                    UserName = "tlhcl7@gmail.com",  // replace with valid value
                    Password = "123456789+++"  // replace with valid value
                };
                smtp.Credentials = credential;
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                smtp.Send(message);
                

            }
            

        }
    }
}
