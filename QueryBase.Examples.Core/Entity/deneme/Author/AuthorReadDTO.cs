using System;

namespace QueryBase.Examples.Core.Domain.DTO
{
    public class AuthorReadDTO
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}