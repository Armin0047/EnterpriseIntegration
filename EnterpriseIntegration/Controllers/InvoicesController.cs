using EnterpriseIntegration.Core.Models;
using EnterpriseIntegration.Core.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseIntegration.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // تمام اکشن‌های این کنترلر نیازمند JWT معتبر هستند
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceRepository _repository;

        public InvoicesController(IInvoiceRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// دریافت ریزفاکتورها در بازه زمانی مشخص همراه با صفحه‌بندی
        /// </summary>
        /// <remarks>
        /// نمونه درخواست:
        /// 
        ///     GET /api/Invoices?fromDate=14010105&amp;toDate=14010105&amp;pageNumber=1&amp;pageSize=50
        /// 
        /// نکات فنی:
        /// - تاریخ باید به صورت عدد ۸ رقمی شمسی باشد (مانند 14010105).
        /// - وضعیت فاکتورها فقط مواردی که فروش هستند واکشی می‌شود. *** توجه : پیش فاکتور ها لیست نخواهند شد.
        /// </remarks>
        /// <param name="fromDate">تاریخ فاکتور به فرمت شمسی پیوسته (مثال: 14010105)</param>
        /// <param name="toDate">تاریخ فاکتور به فرمت شمسی پیوسته (مثال: 14010105)</param>
        /// <param name="pageNumber">شماره صفحه (پیش‌فرض: 1)</param>
        /// <param name="pageSize">تعداد رکورد در هر صفحه (پیش‌فرض: 50 و  سقف برای حفظ سلامت مموری سرور برابر 200) </param>
        /// <response code="200">عملیات موفقیت‌آمیز بود و لیست بازگردانده شد.</response>
        /// <response code="400">فرمت تاریخ ارسالی نامعتبر است.</response>
        /// <response code="401">توکن احراز هویت نامعتبر یا منقضی شده است.</response>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<InvoiceHeaderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]

        public async Task<IActionResult> GetInvoices([FromQuery] int fromDate, [FromQuery] int toDate ,[FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize > 200) pageSize = 200; // سقف برای حفظ سلامت مموری سرور

            var result = await _repository.GetPendingInvoicesAsync(fromDate, toDate, pageNumber, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// دریافت ریزفاکتورها برای یک روز
        /// </summary>
        /// /// <remarks>
        /// نمونه درخواست:
        /// 
        ///     GET /api/Invoices/daily?date=14010105
        /// 
        /// نکات فنی:
        /// - تاریخ باید به صورت عدد ۸ رقمی شمسی باشد (مانند 14010105).
        /// - وضعیت فاکتورها فقط مواردی که فروش هستند واکشی می‌شود. *** توجه : پیش فاکتور ها لیست نخواهند شد.
        /// </remarks>
        /// <param name="date">تاریخ فاکتور به فرمت شمسی پیوسته (مثال: 14010105)</param>
        /// <response code="200">عملیات موفقیت‌آمیز بود و لیست بازگردانده شد.</response>
        /// <response code="400">فرمت تاریخ ارسالی نامعتبر است.</response>
        /// <response code="401">توکن احراز هویت نامعتبر یا منقضی شده است.</response>
        [HttpGet("daily")]
        [ProducesResponseType(typeof(PagedResult<InvoiceHeaderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetInvoices([FromQuery] int date)
        {           

            var result = await _repository.GetPendingInvoicesAsync(date, date, 1,999999);
            return Ok(result);
        }

        /// <summary>
        /// دریافت  گزارش کالاهای هدف بر اساس زیر گروه در بازه زمانی مشخص همراه با صفحه بندی
        /// </summary>
        /// <remarks>
        /// نمونه درخواست:
        /// 
        ///     GET /api/Invoices/ProductsType?fromDate=14010105&amp;toDate=14010105&amp;type=1&amp;pageNumber=1&amp;pageSize=50
        /// 
        /// نکات فنی:
        /// - تاریخ باید به صورت عدد ۸ رقمی شمسی باشد (مانند 14010105).
        /// - وضعیت فاکتورها فقط مواردی که فروش هستند واکشی می‌شود. *** توجه : پیش فاکتور ها لیست نخواهند شد.
        /// </remarks>
        /// <param name="fromDate">تاریخ فاکتور به فرمت شمسی پیوسته (مثال: 14010105)</param>
        /// <param name="toDate">تاریخ فاکتور به فرمت شمسی پیوسته (مثال: 14010105)</param>
        /// /// <param name="type">کدزیرگروه</param>
        /// <param name="pageNumber">شماره صفحه (پیش‌فرض: 1)</param>
        /// <param name="pageSize">تعداد رکورد در هر صفحه (پیش‌فرض: 50 و  سقف برای حفظ سلامت مموری سرور برابر 200) </param>
        /// <response code="200">عملیات موفقیت‌آمیز بود و لیست بازگردانده شد.</response>
        /// <response code="400">فرمت تاریخ ارسالی نامعتبر است.</response>
        /// <response code="401">توکن احراز هویت نامعتبر یا منقضی شده است.</response>
        [HttpGet("ProductsType")]
        [ProducesResponseType(typeof(PagedResult<InvoiceProducts>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]

        public async Task<IActionResult> GetProductsType([FromQuery] int fromDate, [FromQuery] int toDate, [FromQuery] int type, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize > 200) pageSize = 200; // سقف برای حفظ سلامت مموری سرور

            var result = await _repository.GetPendingInvoiceProductssAsync(fromDate, toDate,type, pageNumber, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// دریافت  گزارش کالاهای هدف بر اساس زیر گروه برای یک روز
        /// </summary>
        /// <remarks>
        /// نمونه درخواست:
        ///
        ///     GET /api/Invoices/ProductsType/daily?date=14010105&amp;type=1
        /// 
        /// نکات فنی:
        /// - تاریخ باید به صورت عدد ۸ رقمی شمسی باشد (مانند 14010105).
        /// - وضعیت فاکتورها فقط مواردی که فروش هستند واکشی می‌شود. *** توجه : پیش فاکتور ها لیست نخواهند شد.
        /// </remarks>
        /// <param name="date">تاریخ فاکتور به فرمت شمسی پیوسته (مثال: 14010105)</param>
        /// /// <param name="type">کدزیرگروه</param>
        /// <response code="200">عملیات موفقیت‌آمیز بود و لیست بازگردانده شد.</response>
        /// <response code="400">فرمت تاریخ ارسالی نامعتبر است.</response>
        /// <response code="401">توکن احراز هویت نامعتبر یا منقضی شده است.</response>
        [HttpGet("ProductsType/daily")]
        [ProducesResponseType(typeof(PagedResult<InvoiceProducts>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]

        public async Task<IActionResult> GetProductsType([FromQuery] int date, [FromQuery] int type)
        {


            var result = await _repository.GetPendingInvoiceProductssAsync(date, date, type, 1, 999999);
            return Ok(result);
        }

        /// <summary>
        /// دریافت ریزفاکتورها خرید در بازه زمانی مشخص همراه با صفحه‌بندی
        /// </summary>
        /// <remarks>
        /// نمونه درخواست:
        /// 
        ///     GET /api/Invoices/Warehouse?fromDate=14010105&amp;toDate=14010105&amp;pageNumber=1&amp;pageSize=50
        /// 
        /// نکات فنی:
        /// - تاریخ باید به صورت عدد ۸ رقمی شمسی باشد (مانند 14010105).        
        /// </remarks>
        /// <param name="fromDate">تاریخ فاکتور به فرمت شمسی پیوسته (مثال: 14010105)</param>
        /// <param name="toDate">تاریخ فاکتور به فرمت شمسی پیوسته (مثال: 14010105)</param>
        /// <param name="pageNumber">شماره صفحه (پیش‌فرض: 1)</param>
        /// <param name="pageSize">تعداد رکورد در هر صفحه (پیش‌فرض: 50 و  سقف برای حفظ سلامت مموری سرور برابر 200) </param>
        /// <response code="200">عملیات موفقیت‌آمیز بود و لیست بازگردانده شد.</response>
        /// <response code="400">فرمت تاریخ ارسالی نامعتبر است.</response>
        /// <response code="401">توکن احراز هویت نامعتبر یا منقضی شده است.</response>
        [HttpGet("Warehouse")]
        [ProducesResponseType(typeof(PagedResult<InoviceWarehouse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]

        public async Task<IActionResult> GetInvoicesWarehouse([FromQuery] int fromDate, [FromQuery] int toDate, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize > 200) pageSize = 200; // سقف برای حفظ سلامت مموری سرور

            var result = await _repository.GetPendingInvoicewarehausesAsync(fromDate, toDate, pageNumber, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// دریافت ریزفاکتورها خرید برای یک روز
        /// </summary>
        /// <remarks>
        /// نمونه درخواست:
        /// 
        ///     GET /api/Invoices/Warehouse/daily?date=14010105
        /// 
        /// نکات فنی:
        /// - تاریخ باید به صورت عدد ۸ رقمی شمسی باشد (مانند 14010105).        
        /// </remarks>
        /// <param name="date">تاریخ فاکتور به فرمت شمسی پیوسته (مثال: 14010105)</param>       
        /// <response code="200">عملیات موفقیت‌آمیز بود و لیست بازگردانده شد.</response>
        /// <response code="400">فرمت تاریخ ارسالی نامعتبر است.</response>
        /// <response code="401">توکن احراز هویت نامعتبر یا منقضی شده است.</response>
        [HttpGet("Warehouse/daily")]
        [ProducesResponseType(typeof(PagedResult<InoviceWarehouse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]

        public async Task<IActionResult> GetInvoicesWarehouse([FromQuery] int date)
        {         

            var result = await _repository.GetPendingInvoicewarehausesAsync(date, date, 1, 9999999);
            return Ok(result);
        }
    }
}
