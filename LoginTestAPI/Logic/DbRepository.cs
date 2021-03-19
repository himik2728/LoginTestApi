using LoginTest2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LoginTest2.Logic
{
    public class DbRepository
    {
        private dbLoginTestEntities db = new dbLoginTestEntities();

        public Users LoginUser(string Username, String PasswordHash, out string errmsg)
        {
            errmsg = "success";
            Users u = new Users();
            u = db.Users.Where(x => x.Username == Username).FirstOrDefault();
            if (u != null)
            {
                if (u.Password != PasswordHash)
                    errmsg = "Invalid Password";
            }
            else
            {
                errmsg = "Invalid Username";
            }
            return u;
        }

        public Users RegisterUser(Users model, out string errmsg)
        {
            errmsg = "";
            try
            {
                string PasswordHash = "";
                PasswordHash = CryptoHashing.Encrypt(model.Password);

                model.Password = PasswordHash;
                db.Users.Add(model);
                db.SaveChanges();
                errmsg = "success";
            }
            catch (Exception ex)
            {
                errmsg = ex.Message;
            }
            return model;
        }
    }
}