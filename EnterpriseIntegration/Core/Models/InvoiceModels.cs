using System.Text.Json.Serialization;

namespace EnterpriseIntegration.Core.Models
{
    public class InvoiceModels
    {
        /// <summary>
        /// شماره ردیف در صفحه بندی
        /// </summary>
        [JsonPropertyOrder(1)]
        public int Rownumber { get; set; }
        /// <summary>
        /// شماره نسخه یا شناسه فاکتور
        /// </summary>
        [JsonPropertyOrder(2)]
        public string InvoiceNumber { get; set; } = string.Empty;
        /// <summary>
        /// تاریخ فاکتور
        /// </summary>
        [JsonPropertyOrder(3)]
        public string InvoiceDate { get; set; } = string.Empty;

        /// <summary>
        /// کاربر ثبت کننده
        /// </summary>
        [JsonPropertyOrder(4)]
        public string RegistrarName { get; set; } = string.Empty;
    }

    public class InvoiceHeaderDto : InvoiceModels
    {
        /// <summary>
        /// تاریخ نسخه
        /// </summary>
        [JsonPropertyOrder(5)]
        public string PrescriptionDate { get; set; } = string.Empty;
        /// <summary>
        /// عنوان سازمان بیمه گر پایه
        /// </summary>
        [JsonPropertyOrder(6)]
        public string InsuranceName { get; set; } = string.Empty;
        /// <summary>
        /// کدملی( برابر با فیلد شماره دفترجهدر صفحه قیمت زنی )
        /// </summary>
        [JsonPropertyOrder(7)]
        public string? NationalCode { get; set; }
        /// <summary>
        /// نام بیمار یا مراجعه کننده
        /// </summary>
        [JsonPropertyOrder(8)]
        public string? PatientName { get; set; }
        /// <summary>
        /// تلفن همراه
        /// </summary>
        [JsonPropertyOrder(9)]
        public string? Mobile { get; set; }
        /// <summary>
        /// وضعیت صندوق ( 1 = ثبت شده در صندوق ، 0 = ثبت صندوق نشده )
        /// </summary>
        [JsonPropertyOrder(10)]
        public int Paid { get; set; }
        /// <summary>
        /// نوع وضعیت پرداختی ( 0 = تخفیف ، 1 = پرداختی(مثبت یا منفی) ، null = هیچکدام
        /// </summary>
        [JsonPropertyOrder(11)]
        public int? TypePaid { get; set; }
        /// <summary>
        /// مبلغ - درصورت وجود رکورد صندوق یا تخفیف 
        /// </summary>
        [JsonPropertyOrder(12)]
        public decimal? Cash { get; set; }
        /// <summary>
        /// زمان ثبت صندوق یا تخفیف
        /// </summary>
        [JsonPropertyOrder(13)]
        public string PaidTime { get; set; } = string.Empty;
        /// <summary>
        /// کاربر ثبت کننده صندوق یا تخفیف
        /// </summary>
        [JsonPropertyOrder(14)]
        public string Cashier { get; set; } = string.Empty;
        /// <summary>
        /// در صورت پرداخت به صورت با کارت (مقدار  cash - Cardpayment = مقدار پرداختی نقدی )  
        /// توجه داشته باشید فقط برای ردیفی که مقدار TypePaid = 1 میباشد
        /// </summary>
        [JsonPropertyOrder(15)]
        public decimal Cardpayment { get; set; }
        /// <summary>
        /// سازمان بیمه گر تکمیلی
        /// توجه داشته باشید فقط برای ردیفی که مقدار TypePaid = 0 میباشد
        /// </summary>
        [JsonPropertyOrder(16)]
        public string? Kheyrie { get; set; }
    }

    public class InvoiceProducts : InvoiceModels
    {
        /// <summary>
        /// عنوان سازمان بیمه گر پایه
        /// </summary>
        [JsonPropertyOrder(5)]
        public string InsuranceName { get; set; } = string.Empty;
        /// <summary>
        /// شناسه محصول
        /// </summary>
        [JsonPropertyOrder(6)]
        public string ProductKey { get; set; }

        /// <summary>
        /// نام محصول
        /// </summary>
        [JsonPropertyOrder(7)]
        public string ProductName { get; set; }

        /// <summary>
        /// تعداد فروش رفته
        /// </summary>
        [JsonPropertyOrder(8)]
        public int Num { get; set; }

        /// <summary>
        /// قیمت 
        /// </summary>
        [JsonPropertyOrder(9)]
        public decimal Price { get; set; }
    }

    public class InoviceWarehouse :InvoiceModels
    {
        /// <summary>
        /// نام شرکت
        /// </summary>
        [JsonPropertyOrder(5)]
        public string CompanyName { get; set; }
        /// <summary>
        /// کد فاکتور شرکت
        /// </summary>
        [JsonPropertyOrder(6)]
        public string Code_Invoice_Company { get; set; }
        /// <summary>
        /// جمع کل فاکتور ( بدون احتساب تخفیف و مالیات )
        /// </summary>
        [JsonPropertyOrder(7)]
        public decimal GrandTotal { get; set; }
        /// <summary>
        /// تخفیفات ( کالایی و تخفیفات مرتبط با ردیف محصول)
        /// </summary>
        [JsonPropertyOrder(8)]
        public decimal Discount { get; set; }
        /// <summary>
        /// تخفیفات نقدی
        /// </summary>
        [JsonPropertyOrder(9)]
        public decimal Cash_Discount { get; set; }
        /// <summary>
        /// ارزش افزوده / مالیات
        /// </summary>
        [JsonPropertyOrder(10)]
        public decimal tax { get; set; }
        /// <summary>
        /// قابل پرداخت
        /// </summary>
        [JsonPropertyOrder(11)]
        public decimal Payable { get; set; }
    }
}
