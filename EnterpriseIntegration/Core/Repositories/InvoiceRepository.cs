using Dapper;
using EnterpriseIntegration.Core.Models;
using System.Data;

namespace EnterpriseIntegration.Core.Repositories
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly IDbConnection _dbConnection;

        public InvoiceRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<PagedResult<InvoiceHeaderDto>> GetPendingInvoicesAsync(int fromDate, int toDate, int pageNumber, int pageSize)
        {
            int startRow = ((pageNumber - 1) * pageSize) + 1;
            int endRow = pageNumber * pageSize;

            // کوئری با ساختار تفکیک هدر و اقلام با کارایی بالا
            string sql = @" 
                		Select CodeFacHeder into #t From FacRadif WITH (NOLOCK) group by Codefacheder

		                Select  COUNT(1)								
		                From FacHeder H WITH (NOLOCK)
		                inner join #t T on T.CodeFacHeder=H.CodeFacHeder
		                Left Join Users U on U.ID=H.CodeUser
		                left join Sazeman Z on Z.CodeSazeMan = isnull(H.CodeSazeMan,55)
		                left join Sandogh S WITH (NOLOCK) on S.Sh_Noskhe = H.Sh_Noskhe
		                Left Join Users US on US.ID=S.UserId
		                left join Kheyrie K on K.KheyrieCode=S.KheyrieCode
		                WHERE H.state = 0 
                        AND H.DateFac BETWEEN @FromDate and @ToDate		

		                ;WITH PagedInvoices AS (
			                Select  
			                ROW_NUMBER() OVER (ORDER BY H.Sh_noskhe ASC) AS Rownumber,
			                H.Sh_noskhe as InvoiceNumber ,
			                H.DateNoskhe as PrescriptionDate,
			                H.datefac as InvoiceDate,
			                Z.Sazeman as InsuranceName,
			                U.Lname as RegistrarName,
			                H.BimarName as PatientName,
			                H.ShDaftarcheh as NationalCode,
			                H.Mobile as Mobile,
			                isnull( (Select Top 1 1 From Sandogh Sind Where Sind.Sh_Noskhe = H.sh_noskhe and Sind.state=1),0) as Paid,
			                'TypePaid' = Case When S.State = 50 Then 0 when S.state in (1,91,90 ) then 1 else null end,
			                S.Mablagh as Cash,
			                STUFF(STUFF(CAST(ISNULL(dbo.Milady2Shams(S.TimeSandogh), 0) AS VARCHAR(10)), 5, 0, '/'), 8, 0, '/') + ' ' + CONVERT(varchar,Ntime,108) as PaidTime,
			                US.Lname as Cashier,
			                S.MablaghBaKart as Cardpayment,
			                K.Kheyrie		
			                From FacHeder H WITH (NOLOCK)
			                inner join #t T on T.CodeFacHeder=H.CodeFacHeder
			                Left Join Users U on U.ID=H.CodeUser
			                left join Sazeman Z on Z.CodeSazeMan = isnull(H.CodeSazeMan,55)
			                left join Sandogh S WITH (NOLOCK) on S.Sh_Noskhe = H.Sh_Noskhe
			                Left Join Users US on US.ID=S.UserId
			                left join Kheyrie K on K.KheyrieCode=S.KheyrieCode
			                WHERE H.state = 0  AND H.DateFac BETWEEN @FromDate and @ToDate                            
		                )

		                 SELECT 
		                 *
		                 FROM PagedInvoices
		                 WHERE Rownumber BETWEEN @StartRow AND @EndRow
                            order by InvoiceNumber,PaidTime;

		                 Drop Table #t
                        ";

            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();

            using var multi = await _dbConnection.QueryMultipleAsync(sql, new { StartRow = startRow, EndRow = endRow , FromDate =fromDate , ToDate = toDate});

            int totalRecords = await multi.ReadFirstAsync<int>();
            var headers = (await multi.ReadAsync<dynamic>()).ToList();

            if (!headers.Any())
            {
                return new PagedResult<InvoiceHeaderDto>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalRecords,
                    Data = Enumerable.Empty<InvoiceHeaderDto>()
                };
            }

           // تجمیع اقلام در هدرها
            var resultList = new List<InvoiceHeaderDto>();
            foreach (var h in headers)
            {
                var invoiceDto = new InvoiceHeaderDto
                {
                    Rownumber =Convert.ToInt32(h.Rownumber),
                    InvoiceNumber = Convert.ToString(h.InvoiceNumber),
                    PrescriptionDate = Convert.ToString(h.PrescriptionDate),
                    InvoiceDate = Convert.ToString(h.InvoiceDate),
                    InsuranceName = Convert.ToString(h.InsuranceName),
                    RegistrarName = Convert.ToString(h.RegistrarName),
                    NationalCode = Convert.ToString(h.NationalCode),                    
                    PatientName = h.BimarName,
                    Mobile = h.Mobile,
                    Paid = Convert.ToInt32(h.Paid),
                    TypePaid =h.TypePaid,
                    Cash = Convert.ToDecimal(h.Cash),
                    PaidTime = Convert.ToString(h.PaidTime),
                    Cashier = Convert.ToString(h.Cashier),
                    Cardpayment = Convert.ToDecimal(h.Cardpayment),
                    Kheyrie =h.Kheyrie,
                };
                resultList.Add(invoiceDto);
            }

            return new PagedResult<InvoiceHeaderDto>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalRecords,
                Data = resultList
            };
        }

        public async Task<PagedResult<InvoiceProducts>> GetPendingInvoiceProductssAsync(int fromDate, int toDate, int type, int pageNumber, int pageSize)
        {
            int startRow = ((pageNumber - 1) * pageSize) + 1;
            int endRow = pageNumber * pageSize;

            // کوئری با ساختار تفکیک هدر و اقلام با کارایی بالا
            string sql = @" 
                				Select  COUNT(1)			From FacHeder H		 WITH (NOLOCK)		
		                            inner join FacRadif  R WITH (NOLOCK) on H.CodeFacHeder=R.CodeFacHeder
		                            inner join Kala K on K.Code=R.Code
		                            left join Users U on U.Id = R.codeuser
		                            left join Sazeman S on Isnull(H.CodeSazeMan,55)=S.CodeSazeMan
		                            Where H.State = 0 And H.DateFac between @FromDate and @ToDate and isnull(K.Type,0)=@Type and r.ted>0		

		                            ;WITH PagedInvoices AS (
			                            Select  
			                            ROW_NUMBER() OVER (ORDER BY H.Sh_noskhe ASC) AS Rownumber,
			                            H.sh_noskhe as InvoiceNumber,
			                            H.DateFac as InvoiceDate,
			                            S.Sazeman as InsuranceName,
			                            K.Code as ProductKey,
			                            K.FaName as ProductName,
			                            U.lname as RegistrarName,
			                            R.Ted as Num,
			                            isnull(R.JamRadif,0)+isnull(R.EzafeDariafty,0) as Price 
			                            From FacHeder H  WITH (NOLOCK)
			                            inner join FacRadif  R  WITH (NOLOCK) on H.CodeFacHeder=R.CodeFacHeder
			                            inner join Kala K on K.Code=R.Code
			                            left join Users U on U.Id = R.codeuser
			                            left join Sazeman S on Isnull(H.CodeSazeMan,55)=S.CodeSazeMan
			                            Where H.State = 0 And H.DateFac between @FromDate and @ToDate and isnull(K.Type,0)=@Type and r.ted>0	
			
		                            )

		                             SELECT 
		                             *
		                             FROM PagedInvoices
		                             WHERE Rownumber BETWEEN @StartRow AND @EndRow
		                             order by InvoiceNumber;

                        ";

            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();

            using var multi = await _dbConnection.QueryMultipleAsync(sql, new { StartRow = startRow, EndRow = endRow,Type=type, FromDate = fromDate, ToDate = toDate });

            int totalRecords = await multi.ReadFirstAsync<int>();
            var headers = (await multi.ReadAsync<dynamic>()).ToList();

            if (!headers.Any())
            {
                return new PagedResult<InvoiceProducts>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalRecords,
                    Data = Enumerable.Empty<InvoiceProducts>()
                };
            }

            // تجمیع اقلام در هدرها
            var resultList = new List<InvoiceProducts>();
            foreach (var h in headers)
            {
                var invoiceDto = new InvoiceProducts
                {
                    Rownumber = Convert.ToInt32(h.Rownumber),
                    InvoiceNumber = Convert.ToString(h.InvoiceNumber),
                    InvoiceDate = Convert.ToString(h.InvoiceDate),
                    InsuranceName = Convert.ToString(h.InsuranceName),
                    RegistrarName = Convert.ToString(h.RegistrarName),
                    ProductKey = Convert.ToString(h.ProductKey),
                    ProductName = h.ProductName,
                    Num = Convert.ToInt32(h.Num),
                    Price = Convert.ToDecimal(h.Price),
                };
                resultList.Add(invoiceDto);
            }

            return new PagedResult<InvoiceProducts>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalRecords,
                Data = resultList
            };
        }

        public async Task<PagedResult<InoviceWarehouse>> GetPendingInvoicewarehausesAsync(int fromDate, int toDate, int pageNumber, int pageSize)
        {
            int startRow = ((pageNumber - 1) * pageSize) + 1;
            int endRow = pageNumber * pageSize;

            // کوئری با ساختار تفکیک هدر و اقلام با کارایی بالا
            string sql = @" 
                			Select  COUNT(1) From Fac_Heder H with (NoLock)
									Left join Fard F on F.Codem = H.Code_M
									left join Users U on U.Id = H.IdUser
									Where H.state = 1 And Date_fac between @FromDate and @ToDate

		                            ;WITH PagedInvoices AS (
			                            Select  
			                            ROW_NUMBER() OVER (ORDER BY H.Code ASC) AS Rownumber,
			                            H.Code as InvoiceNumber,
			                            H.Date_Fac as InvoiceDate,
										U.lname as RegistrarName,
			                            F.Name as CompanyName,
			                            H.ShFacMoshatri as Code_Invoice_Company,
			                            H.JamFac as GrandTotal,
			                            H.TakhfifRadif as Discount,
										H.TakhfifHeader as Cash_Discount,
										H.ArzeshAfzodeh as tax,
										H.JamKhales as Payable
			                           From
			                            Fac_Heder H with (Nolock)
										Left join Fard F on F.Codem = H.Code_M
										left join Users U on U.Id = H.IdUser
										Where H.state = 1 And Date_fac between @FromDate and @ToDate
                                        )

		                             SELECT 
		                             *
		                             FROM PagedInvoices
		                             WHERE Rownumber BETWEEN @StartRow AND @EndRow
		                             order by InvoiceNumber;
                        ";

            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();

            using var multi = await _dbConnection.QueryMultipleAsync(sql, new { StartRow = startRow, EndRow = endRow, FromDate = fromDate, ToDate = toDate });

            int totalRecords = await multi.ReadFirstAsync<int>();
            var headers = (await multi.ReadAsync<dynamic>()).ToList();

            if (!headers.Any())
            {
                return new PagedResult<InoviceWarehouse>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalRecords,
                    Data = Enumerable.Empty<InoviceWarehouse>()
                };
            }

            // تجمیع اقلام در هدرها
            var resultList = new List<InoviceWarehouse>();
            foreach (var h in headers)
            {
                var invoiceDto = new InoviceWarehouse
                {
                    Rownumber = Convert.ToInt32(h.Rownumber),
                    InvoiceNumber = Convert.ToString(h.InvoiceNumber),                 
                    InvoiceDate = Convert.ToString(h.InvoiceDate),                    
                    RegistrarName = Convert.ToString(h.RegistrarName),
                    CompanyName = Convert.ToString(h.CompanyName),
                    Code_Invoice_Company = Convert.ToString(h.Code_Invoice_Company),
                    GrandTotal = Convert.ToDecimal(h.GrandTotal),
                    Discount = Convert.ToDecimal(h.Discount),
                    Cash_Discount = Convert.ToDecimal(h.Cash_Discount),
                    tax = Convert.ToDecimal(h.tax),
                    Payable = Convert.ToDecimal(h.Payable),


                };
                resultList.Add(invoiceDto);
            }

            return new PagedResult<InoviceWarehouse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalRecords,
                Data = resultList
            };
        }
    }
}
