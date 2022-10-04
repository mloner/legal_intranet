using System;
using Utg.Common.Packages.ExcelReportBuilder.Report;

namespace Utg.LegalService.Common.Models.Report.Dtos 
{
    public class TaskReportDto : ExcelReportDto
    {
        [ExcelColumn("A", "№")]
        public int RowNumber { get; set; }
    
        [ExcelColumn("B", "Дата")]
        public DateTime? CreationDate { get; set; }
    
        [ExcelColumn("C", "Автор")]
        public string? AuthorFullName { get; set; }
    
        [ExcelColumn("E", "Тип задачи")]
        public string? TaskType { get; set; }
        
        [ExcelColumn("F", "Исполнитель")]
        public string? PerformerFullName { get; set; }
        
        [ExcelColumn("G", "Статус")]
        public string? Status { get; set; }
        
        [ExcelColumn("H", "Срок")]
        public DateTime? Deadline { get; set; }
        
        [ExcelColumn("I", "Дата последнего изменения")]
        public DateTime? LastChangeDateTime { get; set; }
    }
}