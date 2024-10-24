using System;

namespace QueryBase.Examples.Core.Domain.DTO
{
    public class NoteReadDTO
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public string Text { get; set; }
        public bool IsShared { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public int UserId { get; set; }
        public int BookId { get; set; }
    }
}