namespace DentalHouseWebAPI.Models
{
    public class SaleTransaction
    {
        public Sale Master { get; set; }
        public IEnumerable<SaleDetail> Details { get; set; }
    }
}
