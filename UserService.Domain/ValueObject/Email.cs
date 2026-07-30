using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService.Domain.ValueObject
{
    public class Email
    {
        public string Value { get; set; } = string.Empty;
        public Email(string email) 
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentNullException("Email не может быть пустым");
            }
            if (!email.Contains("@"))
            {
                throw new ArgumentException("Email не может не содержать @");
            }

            Value = email;
        }
        public override string ToString()=>Value;
    }
}
