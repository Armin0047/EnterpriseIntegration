using EnterpriseIntegration.Core.Models;

namespace EnterpriseIntegration.Core.Repositories
{
    public interface IReportRepository
    {
        Task<List<ReportModel>> currentStockList(int FromProductKey,int ToProductKey,int FromWarehouse,int ToWarehouse);
        Task<List<ReportModel>> Stagnantgoodslist(int fromDate, int toDate);
    }
}
