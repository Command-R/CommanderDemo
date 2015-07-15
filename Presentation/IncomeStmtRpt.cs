/*
    NOTE how the IncomeStmtRpt uses MediatR to the CalcIncomeStmt proc
    command to retrieve the data.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using MediatR;

namespace Dashboard
{
    public class IncomeStmtRpt : IRequest<IncomeStmtRpt.Report>
    {
        public int? UserId { get; set; }
        public int? ClientId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool GainLoss { get; set; }
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
        public string RowLink { get; set; }
        public string ValueLink { get; set; }

        public class Report
        {
            public string RowView { get; set; }
            public string ColView { get; set; }
            public string RowLink { get; set; }
            public string ValueLink { get; set; }
            public List<string> Columns { get; set; }
            public List<Row> Incomes { get; set; }
            public Row TotalIncome { get; set; }
            public List<Row> Expenses { get; set; }
            public Row TotalExpense { get; set; }
            public Row OperatingGainLoss { get; set; }
            public List<Row> OtherIncomes { get; set; }
            public Row TotalOtherIncome { get; set; }
            public List<Row> OtherExpenses { get; set; }
            public Row TotalOtherExpense { get; set; }
            public Row NetGainLoss { get; set; }
            public List<Row> GainLosses { get; set; }

            public bool ShowMonthlyBudget
            {
                get { return ColView == "Month"; }
            }

            public string Fmt(decimal? num)
            {
                return num == null || num == 0m ? null : num.ToString("#,##0");
            }

            public string GetValueLink(Row row, Ext.ForEachItem<decimal> value)
            {
                if (row == null || value.Item == 0m)
                    return null;

                if (ValueLink == null)
                    return Fmt(value.Item);

                var url = string.Format(ValueLink, row.Id, Columns[value.Index]);
                return string.Format("<a href='{0}' target='_blank'>{1}</a>",
                    url, Fmt(value.Item));
            }
        };

        public class Row
        {
            public int Id { get; set; }
            public string Text { get; set; }
            public List<decimal> Values { get; set; }
            public decimal MonthlyBudget { get; set; }
            public decimal MonthlyVariance { get; set; }
            public decimal TotalActual { get; set; }
            public decimal TotalBudget { get; set; }
            public decimal TotalVariance { get; set; }
        };

        internal class Handler : IRequestHandler<IncomeStmtRpt, Report>
        {
            private readonly IAppContext _appContext;
            private readonly IMediator _mediator;
            private List<CalcIncomeStmt.Item> _rows;
            private List<string> _columns;

            public Handler(IAppContext appContext, IMediator mediator)
            {
                _appContext = appContext;
                _mediator = mediator;
            }

            public Report Handle(IncomeStmtRpt request)
            {
                if (request.UserId == null)
                    request.UserId = _appContext.User.Id;

                if (request.ClientId == null)
                    request.ClientId = _appContext.ClientId;

                _rows = _mediator.Send(request.CopyTo(new CalcIncomeStmt()));
                _columns = _rows.Where(x => x.WorksheetType == "Columns").Select(x => x.Col).ToList();

                var report = new Report
                {
                    RowView = request.RowView,
                    ColView = request.ColView,
                    RowLink = request.RowLink,
                    ValueLink = request.ValueLink,
                    Columns = _columns,
                    Incomes = new List<Row>(),
                    TotalIncome = NewRow(0, "Total Income"),
                    Expenses = new List<Row>(),
                    TotalExpense = NewRow(0, "Total Expense"),
                    OperatingGainLoss = NewRow(0, "Operating Gain/Loss"),
                    OtherIncomes = new List<Row>(),
                    TotalOtherIncome = NewRow(0, "Total Other Income"),
                    OtherExpenses = new List<Row>(),
                    TotalOtherExpense = NewRow(0, "Total Other Expense"),
                    NetGainLoss = NewRow(0, "Net Gain/Loss"),
                    GainLosses = new List<Row>(),
                };

                if (request.GainLoss)
                {
                    ComputeGainLossRows(report.GainLosses, report.NetGainLoss);
                }
                else
                {
                    ComputeRows(Account.INCOME, report.Incomes, report.TotalIncome);
                    ComputeRows(Account.EXPENSE, report.Expenses, report.TotalExpense);
                    ComputeDifference(NewRow(0, ""), report.TotalIncome, report.TotalExpense, report.OperatingGainLoss);
                    ComputeRows(Account.OTHER_INCOME, report.OtherIncomes, report.TotalOtherIncome);
                    ComputeRows(Account.OTHER_EXPENSE, report.OtherExpenses, report.TotalOtherExpense);
                    ComputeDifference(report.OperatingGainLoss, report.TotalOtherIncome, report.TotalOtherExpense, report.NetGainLoss);
                }

                return report;
            }

            private void ComputeRows(string type, List<Row> section, Row total)
            {
                var accounts = _rows.Where(x => x.WorksheetType == Worksheet.ACTUAL && x.AccountType == type)
                                    .Select(x => new {x.Id, x.Row})
                                    .Distinct();

                foreach (var account in accounts)
                {
                    var row = NewRow(account.Id, account.Row);
                    var budgets = _rows.Where(x => x.WorksheetType == Worksheet.BUDGET
                                                   && x.AccountType == type && x.Id == row.Id)
                                       .ToList();

                    foreach (var col in _columns.ToFor())
                    {
                        var actual = _rows.Where(x => x.WorksheetType == Worksheet.ACTUAL
                                                     && x.AccountType == type && x.Id == row.Id &&
                                                     x.Col == col.Item)
                                         .Select(x => (decimal?)x.Value)
                                         .SingleOrDefault() ?? 0;

                        if (col.Last)
                        {
                            var budget = budgets.Where(x => x.Col == col.Item)
                                                .Select(x => (decimal?)x.Value)
                                                .SingleOrDefault() ?? 0;

                            row.MonthlyBudget = budget;
                            row.MonthlyVariance = budget - actual;
                        }

                        row.Values[col.Index] = actual;
                        total.Values[col.Index] += actual;
                    }

                    row.TotalActual = row.Values.Sum();
                    row.TotalBudget = budgets.Sum(x => x.Value);
                    row.TotalVariance = row.TotalActual - row.TotalBudget;

                    total.MonthlyBudget += row.MonthlyBudget;
                    total.MonthlyVariance += row.MonthlyVariance;
                    total.TotalActual += row.TotalActual;
                    total.TotalBudget += row.TotalBudget;
                    total.TotalVariance += row.TotalVariance;

                    section.Add(row);
                }
            }

            private void ComputeDifference(Row init, Row income, Row expense, Row gainLoss)
            {
                foreach (var col in _columns.ToFor())
                {
                    gainLoss.Values[col.Index] = init.Values[col.Index] + income.Values[col.Index] - expense.Values[col.Index];
                }

                gainLoss.MonthlyBudget = init.MonthlyBudget + income.MonthlyBudget - expense.MonthlyBudget;
                gainLoss.MonthlyVariance = init.MonthlyVariance + income.MonthlyVariance - expense.MonthlyVariance;
                gainLoss.TotalActual = init.TotalActual + income.TotalActual - expense.TotalActual;
                gainLoss.TotalBudget = init.TotalBudget + income.TotalBudget - expense.TotalBudget;
                gainLoss.TotalVariance = init.TotalVariance + income.TotalVariance - expense.TotalVariance;
            }

            private void ComputeGainLossRows(List<Row> section, Row total)
            {
                var accounts = _rows.Where(x => x.WorksheetType == Worksheet.ACTUAL)
                                    .Select(x => new { x.Id, x.Row })
                                    .Distinct();

                foreach (var account in accounts)
                {
                    var row = NewRow(account.Id, account.Row);
                    var incomeBudgets = _rows.Where(x => x.WorksheetType == Worksheet.BUDGET && x.Id == row.Id
                                                         && (x.AccountType == Account.INCOME || x.AccountType == Account.OTHER_INCOME))
                                             .ToList();
                    var expenseBudgets = _rows.Where(x => x.WorksheetType == Worksheet.BUDGET && x.Id == row.Id
                                                          && (x.AccountType == Account.EXPENSE || x.AccountType == Account.OTHER_EXPENSE))
                                              .ToList();

                    foreach (var col in _columns.ToFor())
                    {
                        var incomeActual = _rows.Where(x => x.WorksheetType == Worksheet.ACTUAL && x.Id == row.Id
                                                             && (x.AccountType == Account.INCOME || x.AccountType == Account.OTHER_INCOME)
                                                             && x.Col == col.Item)
                                                 .Select(x => (decimal?)x.Value)
                                                 .SingleOrDefault() ?? 0;

                        var expenseActual = _rows.Where(x => x.WorksheetType == Worksheet.ACTUAL && x.Id == row.Id
                                                              && (x.AccountType == Account.EXPENSE || x.AccountType == Account.OTHER_EXPENSE)
                                                              && x.Col == col.Item)
                                                  .Select(x => (decimal?)x.Value)
                                                  .SingleOrDefault() ?? 0;
                        var actual = incomeActual - expenseActual;

                        if (col.Last)
                        {
                            var incomeBudget = incomeBudgets.Where(x => x.Col == col.Item)
                                                            .Sum(x => (decimal?)x.Value) ?? 0;
                            var expenseBudget = expenseBudgets.Where(x => x.Col == col.Item)
                                                              .Sum(x => (decimal?)x.Value) ?? 0;

                            row.MonthlyBudget = incomeBudget - expenseBudget;
                            row.MonthlyVariance = row.MonthlyBudget - actual;
                        }

                        row.Values[col.Index] = actual;
                        total.Values[col.Index] += actual;
                    }

                    row.TotalActual = row.Values.Sum();
                    row.TotalBudget = incomeBudgets.Sum(x => x.Value) - expenseBudgets.Sum(x => x.Value);
                    row.TotalVariance = row.TotalActual - row.TotalBudget;

                    total.MonthlyBudget += row.MonthlyBudget;
                    total.MonthlyVariance += row.MonthlyVariance;
                    total.TotalActual += row.TotalActual;
                    total.TotalBudget += row.TotalBudget;
                    total.TotalVariance += row.TotalVariance;

                    section.Add(row);
                }
            }

            private Row NewRow(int id, string text)
            {
                return new Row
                {
                    Id = id,
                    Text = text,
                    Values = new List<decimal>(new decimal[_columns.Count]),
                };
            }
        };
    };
}
