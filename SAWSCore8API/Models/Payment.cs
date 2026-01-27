using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SAWSCore8API.Models
{
    public class Payment
    {
        public string? returnUrl { get; set; }
        public string? cancelUrl { get; set; }
        public string notifyUrl { get; set; }
        public string name_first { get; set; }
        public string name_last { get; set; }
        public string email_address { get; set; }
        public string m_payment_id { get; set; }
        public string item_name { get; set; }
        public string item_description { get; set; }
        public bool email_confirmation { get; set; }
        public string confirmation_email { get; set; }
        public double amount { get; set; }
        public double recurring_amount { get; set; }
        public string frequency { get; set; }
        public int userId { get; set; }
        public int package_id { get; set; }
        public int subscription_amount { get; set; }
        public string package_name { get; set; }
        public string subscription_type { get; set; }

    }
}
