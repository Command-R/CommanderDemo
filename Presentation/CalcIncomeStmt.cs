/*
  You could easily code generate commands from t-sql Table-Valued Functions.
  The Request contains the TVF inputs, the Response is a list of the rows returned.
*/
using System;
using System.Collections.Generic;
using Common;
using MediatR;

namespace Dashboard
{
    public class CalcIncomeStmt : IRequest<List<CalcIncomeStmt.Item>>
    {
        public int? UserId { get; set; }
        public int? ClientId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? SheetDim1 { get; set; }
        public int? SheetDim2 { get; set; }
        public int? SheetDim3 { get; set; }
        public int? SheetDim4 { get; set; }
        public int? SheetDim5 { get; set; }
        public int? CategoryId { get; set; }
        public int? LineDim1 { get; set; }
        public int? LineDim2 { get; set; }
        public int? LineDim3 { get; set; }
        public string RowView { get; set; }
        public string ColView { get; set; }

        public class Item
        {
            public int Id { get; set; }
            public string WorksheetType { get; set; }
            public string AccountType { get; set; }
            public string Row { get; set; }
            public string Col { get; set; }
            public decimal Value { get; set; }
        };

        internal class Handler : IRequestHandler<CalcIncomeStmt, List<Item>>
        {
            private readonly IAppContext _appContext;
            private readonly DashboardDb _db;

            public Handler(IAppContext appContext, DashboardDb db)
            {
                _appContext = appContext;
                _db = db;
            }

            public List<Item> Handle(CalcIncomeStmt cmd)
            {
                if (cmd.UserId == null)
                    cmd.UserId = _appContext.User.Id;

                if (cmd.ClientId == null)
                    cmd.ClientId = _appContext.ClientId;

                var sql = @"SELECT * FROM CalcIncomeStmt({0})";
                return _db.Execute<Item>(sql, cmd);
            }
        }
    };
}
