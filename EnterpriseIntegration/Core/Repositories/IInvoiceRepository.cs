using Dapper;
using System.Data;
using Microsoft.Data.SqlClient;
using EnterpriseIntegration.Core.Models;

namespace EnterpriseIntegration.Core.Repositories
{
    public interface IInvoiceRepository
    {
        Task<PagedResult<InvoiceHeaderDto>> GetPendingInvoicesAsync(int FromDate,int ToDate,int pageNumber, int pageSize);

        Task<PagedResult<InvoiceProducts>> GetPendingInvoiceProductssAsync(int FromDate, int ToDate, int type, int pageNumber, int pageSize);

        Task<PagedResult<InoviceWarehouse>> GetPendingInvoicewarehausesAsync(int FromDate, int ToDate, int pageNumber, int pageSize);
    }  
}
