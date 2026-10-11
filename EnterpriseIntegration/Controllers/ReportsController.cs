using EnterpriseIntegration.Core.Models;
using EnterpriseIntegration.Core.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseIntegration.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly IReportRepository _repository;
        
        public ReportsController(IReportRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// گزارش موجودی محصولات
        /// </summary>
        /// <remarks>
        /// نمونه درخواست:
        /// 
        ///     GET /api/Report/StockList?fromProductKey=1&amp;toProductKey=10&amp;fromWarehouse=1&amp;toWarehouse=50
        /// 
        /// نکات فنی:
        /// - مقادیر دارای پیش فرض هستند که در راهنما موجود هست
        /// - اگر مقادیر خالی باشد برای فیلدهایی که با from شروع میشوند اولین مقدار ،و برای پارامترهایی که با to شروع میشوند مقدار آخرین آنها در نظر گرفته میشوند 
        /// - همه پارامترها خالی موجودی همه اقلام از همه انبارها را برمیگرداند
        /// 
        /// </remarks>
        /// <param name="fromProductKey">کد محصولی که میخواهعید گزارش بگیرید (مثال: 10)</param>
        /// <param name="toProductKey">کد محصولی که میخواهعید گزارش بگیرید (مثال: 10)</param>
        /// <param name="fromWarehouse">کد انباری که میخواهعید گزارش بگیرید (مثال: 1)</param>
        /// <param name="toWarehouse">کد انباری که میخواهعید گزارش بگیرید (مثال: 1)</param>
        /// <response code="200">عملیات موفقیت‌آمیز بود و لیست بازگردانده شد.</response>
        /// <response code="400">فرمت ارسالی نامعتبر است.</response>
        /// <response code="401">توکن احراز هویت نامعتبر یا منقضی شده است.</response>
        [HttpGet("StockList")]
        [ProducesResponseType(typeof(List<ReportModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]

        public async Task<IActionResult> GetStockList([FromQuery] int fromProductKey=0, [FromQuery] int toProductKey= 2147483647, [FromQuery] int fromWarehouse = 0, [FromQuery] int toWarehouse = 2147483647)
        {           

            var result = await _repository.currentStockList(fromProductKey, toProductKey, fromWarehouse, toWarehouse);
            return Ok(result);
        }

        /// <summary>
        /// گزارش کالاهای راکد
        /// </summary>
        /// <remarks>
        /// نمونه درخواست:
        /// 
        ///     GET /api/Report/Stagnantgoods?fromDate=14010105&amp;toDate=14010105
        /// 
        /// نکات فنی:
        /// - شرط مصرف کالا ثبت صندوق میباشد
        /// - کالایی که در بازه فاقد فروش ثبت صندوق شده باشد ،بعنوان کالای راکد شناسایی میشود
        /// 
        /// </remarks>
        /// <param name="fromDate">تاریخ فاکتور به فرمت شمسی پیوسته (مثال: 14010105)</param>
        /// <param name="toDate">تاریخ فاکتور به فرمت شمسی پیوسته (مثال: 14010105)</param>    
        /// <response code="200">عملیات موفقیت‌آمیز بود و لیست بازگردانده شد.</response>
        /// <response code="400">فرمت ارسالی نامعتبر است.</response>
        /// <response code="401">توکن احراز هویت نامعتبر یا منقضی شده است.</response>
        [HttpGet("Stagnantgoods")]
        [ProducesResponseType(typeof(List<ReportModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]

        public async Task<IActionResult> GetStagnantgoods([FromQuery] int fromDate, [FromQuery] int toDate )
        {

            var result = await _repository.Stagnantgoodslist(fromDate, toDate);
            return Ok(result);
        }
    }
}
