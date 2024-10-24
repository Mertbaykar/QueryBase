using System;

namespace QueryBase.Examples.Core.Domain.DTO
{
    public class UserReadDTO
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public QueryBase.Examples.Core.NoteShareSetting ShareId { get; set; }
    }
}