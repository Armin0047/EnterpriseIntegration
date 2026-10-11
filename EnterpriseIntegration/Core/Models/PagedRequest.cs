namespace EnterpriseIntegration.Core.Models
{
    public class PagedRequest
    {
        private int _pageSize = 50;
        public int PageNumber { get; set; } = 1;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > 200 ? 200 : value; // جلوگیری از درخواست‌های سنگین
        }

        public long? FromDate { get; set; } // به صورت عددی YYYYMMDD سازگار با SQL
        public long? ToDate { get; set; }
    }

    public class PagedResult<T>
    {
        /// <summary>
        /// تعداد کل ردیفهای موجود
        /// </summary>
        public int TotalCount { get; set; }
        /// <summary>
        /// شماره صفحه 
        /// </summary>
        public int PageNumber { get; set; }
        /// <summary>
        /// تعداد رکورد در هر صفحه  
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// کل صفحات  
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public IEnumerable<T> Data { get; set; } = new List<T>();
    }
}
