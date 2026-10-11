using Dapper;
using EnterpriseIntegration.Core.Models;
using System.Data;
using System.Reflection.PortableExecutable;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EnterpriseIntegration.Core.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly IDbConnection _dbConnection;

        public ReportRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<List<ReportModel>> currentStockList(int FromProductKey, int ToProductKey, int FromWarehouse, int ToWarehouse)
        {
            string sql = @"exec sp_currentStockList 1,@anbCodeMin,@anbCodeMax,@KalaCodeMin,@KalaCodeMax,0,2147483647,0,2147483647,0,2147483647,1";
            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();

            using var multi = await _dbConnection.QueryMultipleAsync(sql, new { anbCodeMin = FromWarehouse, anbCodeMax = ToWarehouse, KalaCodeMin = FromProductKey, KalaCodeMax = ToProductKey });

            var stock = (await multi.ReadAsync<dynamic>()).ToList();

            if (!stock.Any())
            {
                return null;
            }

            var resultList = new List<ReportModel>();
            foreach (var h in stock)
            {
                var reportDto = new ReportModel
                {
                    ProductKey = Convert.ToString(h.code),
                    ProductName = Convert.ToString(h.Faname),
                    ProductBuy = Convert.ToDecimal(h.Buy),
                    ProductPrice = Convert.ToDecimal(h.Price),
                    WarehouseCode = Convert.ToInt32(h.anbCode),
                    Stock = Convert.ToDecimal(h.Mojodi),                    
                };
                resultList.Add(reportDto);
            }

            return resultList;

        }



        public async Task<List<ReportModel>> Stagnantgoodslist(int fromDate, int toDate)
        {
            string sql = @"select k.Code, K.FaName ,K.Buy ,K.Price, m.Mojodi 
                            from kala k
                            inner join (select * from mojodi where anbcode = 1 ) m on k.code = m.code
                            where 
                            k.Code not in (SELECT  DISTINCT  FacRadif.Code 
                                                 FROM       (select * from  dbo.FacRadif where anbcode =1) FacRadif
                                                 INNER JOIN dbo.Kala ON FacRadif.Code = dbo.Kala.Code 
                                                 INNER JOIN (select * from dbo.FacHeder where anb_code =1) FacHeder ON FacRadif.CodeFacHeder = FacHeder.CodeFacHeder
                                                 where FacRadif.State = 0  
                          
                                                       and  DateFac          between @Datemin and @DateMax 
                                                       and FacHeder.SH_Noskhe   in (select SH_Noskhe  from  Sandogh  ))
                            and isnull(m.mojodi , 0)>0";
            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();

            using var multi = await _dbConnection.QueryMultipleAsync(sql, new { Datemin = fromDate, DateMax = toDate });

            var stock = (await multi.ReadAsync<dynamic>()).ToList();

            if (!stock.Any())
            {
                return null;
            }

            var resultList = new List<ReportModel>();
            foreach (var h in stock)
            {
                var reportDto = new ReportModel
                {
                    ProductKey = Convert.ToString(h.Code),
                    ProductName = Convert.ToString(h.FaName),
                    ProductBuy = Convert.ToDecimal(h.Buy),
                    ProductPrice = Convert.ToDecimal(h.Price),
                    WarehouseCode = 1,
                    Stock = Convert.ToDecimal(h.Mojodi),
                };
                resultList.Add(reportDto);
            }

            return resultList;

        }
    }
}
