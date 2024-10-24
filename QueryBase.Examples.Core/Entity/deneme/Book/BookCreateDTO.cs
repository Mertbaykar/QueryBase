using System;

namespace QueryBase.Examples.Core.Domain.DTO
{
    public class BookCreateDTO
    {
        public string Name { get; set; }
        public string Summary { get; set; }
        public int PublishYear { get; set; }
        public string ShelfLocation { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string? CoverImagePath { get; set; }
        public int AuthorId { get; set; }
        public int CreatedById { get; set; }
    }
}