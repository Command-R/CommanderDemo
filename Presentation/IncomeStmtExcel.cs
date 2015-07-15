/*
    NOTE how the UI would execute IncomeStmtExcel which formats the report into Excel (as
    opposed to a PDF or HTML page), which first executes IncomeStmtRpt to build the report model,
    but only after calling the CalcIncomeStmt proc command to get the data from the database. Each also
    uses CopyTo to pass along all the matching arguments to the other commands.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using MediatR;

namespace Dashboard
{
    public class IncomeStmtExcel : IRequest<StreamInfo>
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

        internal class Handler : IRequestHandler<IncomeStmtExcel, StreamInfo>
        {
            private readonly IMediator _mediator;
            private IncomeStmtRpt.Report _rpt;
            private Excel _excel;

            public Handler(IMediator mediator)
            {
                _mediator = mediator;
            }

            public StreamInfo Handle(IncomeStmtExcel cmd)
            {
                _rpt = _mediator.Send(cmd.CopyTo(new IncomeStmtRpt()));
                using (_excel = new Excel())
                {
                    BuildHeader();
                    if (cmd.GainLoss)
                    {
                        DisplaySection("GAIN/LOSS", _rpt.GainLosses, _rpt.NetGainLoss);
                    }
                    else
                    {
                        DisplaySection("INCOME", _rpt.Incomes, _rpt.TotalIncome);
                        DisplaySection("EXPENSE", _rpt.Expenses, _rpt.TotalExpense);
                        if (_rpt.OtherIncomes.Count > 0 || _rpt.OtherExpenses.Count > 0)
                        {
                            DisplayRow(_rpt.OperatingGainLoss, true);
                        }
                        DisplaySection("OTHER INCOME", _rpt.OtherIncomes, _rpt.TotalOtherIncome);
                        DisplaySection("OTHER EXPENSE", _rpt.OtherExpenses, _rpt.TotalOtherExpense);
                        DisplayRow(_rpt.NetGainLoss, true);
                    }
                    _excel.FormatAll(new Excel.Style { Format = NumberFormat });
                    return _excel.GetStreamInfo("IncomeSmtRpt.xlsx");
                }
            }

            private void BuildHeader()
            {
                _excel.NextCol(" ");
                foreach (var col in _rpt.Columns)
                {
                    _excel.NextCol(col + "\nActual");
                }
                if (_rpt.ShowMonthlyBudget)
                {
                    _excel.NextCol(_rpt.Columns.LastOrDefault() + "\nBudget");
                    _excel.NextCol("Monthly\nVariance");
                }
                _excel.NextCol(" ");
                _excel.NextCol("Total\nActual");
                _excel.NextCol("Total\nBudget");
                _excel.NextCol("Total\nVariance");
                _excel.FormatRow(new Excel.Style { Bold = true });
            }

            private void DisplaySection(string title, List<IncomeStmtRpt.Row> rows, IncomeStmtRpt.Row total)
            {
                if (rows.Count == 0)
                    return;

                _excel.NextRow();
                _excel.NextCol(title);
                _excel.FormatRow(new Excel.Style { Bold = true, Merge = true }, _rpt.Columns.Count + 7);

                foreach (var row in rows)
                {
                    DisplayRow(row, false);
                }
                DisplayRow(total, true);
            }

            private void DisplayRow(IncomeStmtRpt.Row row, bool header)
            {
                _excel.NextRow();
                _excel.NextCol(row.Text);
                foreach (var value in row.Values)
                {
                    _excel.NextCol(value);
                }
                if (_rpt.ShowMonthlyBudget)
                {
                    _excel.NextCol(row.MonthlyBudget);
                    _excel.NextCol(row.MonthlyVariance);
                }
                _excel.NextCol(" ");
                _excel.NextCol(row.TotalActual);
                _excel.NextCol(row.TotalBudget);
                _excel.NextCol(row.TotalVariance);

                if (header)
                    _excel.FormatRow(new Excel.Style { Bold = true });
            }

            private const string NumberFormat = @"_(* #,##0_);_(* -#,##0_);_(* "" - ""_);_(@_)";
        };
    };
}
