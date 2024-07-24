using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Application.DTOs
{
    public class SapUserDto
    {
        public string Id { get; set; }
        public string Role { get; set; }
        public string CardCode { get; set; }
        public string CardName { get; set; }
        public string CardType { get; set; }
        public int GroupCode { get; set; }
        public string Address { get; set; }
        public string Phone1 { get; set; }
        public string ContactPerson { get; set; }
        public string Notes { get; set; }
        public int PayTermsGrpCode { get; set; }
        public double CreditLimit { get; set; }
        public double MaxCommitment { get; set; }
        public string FederalTaxID { get; set; }
        public string Cellular { get; set; }
        public string EmailAddress { get; set; }
        public string CardForeignName { get; set; }
        public string DebitorAccount { get; set; }
    }
}