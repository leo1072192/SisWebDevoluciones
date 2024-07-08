using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string CardCode { get; set; }
        public string CardName { get; set; }
        public string FederalTaxID { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }
}
