﻿namespace Utg.LegalService.Common.Models.Client
{
    public class TaskAccessRights
    {
        public bool CanShowDetails { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanMakeReport { get; set; }
        public bool CanPerform { get; set; }
        public bool CanReview { get; set; }
        public bool HasShortCycle { get; set; }
        public bool CanMoveToDone { get; set; }
    }
}