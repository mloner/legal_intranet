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
        
        [ExcelColumn("E", "Родительская задача")]
        public int? ParentTaskId { get; set; }
    
        [ExcelColumn("F", "Тип задачи")]
        public string? TaskType { get; set; }
        
        [ExcelColumn("G", "Исполнитель")]
        public string? PerformerFullName { get; set; }
        
        [ExcelColumn("H", "Статус")]
        public string? Status { get; set; }
        
        [ExcelColumn("I", "Срок")]
        public DateTime? Deadline { get; set; }
        
        [ExcelColumn("J", "Дата последнего изменения")]
        public DateTime? LastChangeDateTime { get; set; }
    }
}