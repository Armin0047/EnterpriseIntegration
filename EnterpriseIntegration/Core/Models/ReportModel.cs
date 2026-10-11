namespace EnterpriseIntegration.Core.Models
{
    public class ReportModel
    {
        /// <summary>
        /// شناسه محصول
         /// </summary>
        public string ProductKey { get; set; }
        /// <summary>
        /// نام محصول
        /// </summary>
        public string ProductName { get; set; }
        /// <summary>
        /// آخرین قیمت خرید
        /// </summary>
        public decimal ProductBuy { get; set; }
        /// <summary>
        /// قیمت فروش محصول
        /// </summary>
        public decimal ProductPrice { get; set; }
        /// <summary>
        /// کد انبار
        /// </summary>
        public int      WarehouseCode { get; set; }
        /// <summary>
        /// موجودی محصول
        /// </summary>
        public decimal Stock {  get; set; }
    }
}
