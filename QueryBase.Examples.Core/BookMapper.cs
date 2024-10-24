
namespace QueryBase.Examples.Core
{
    public class BookMapper : QueryMapper.QueryMapper
    {
        public BookMapper()
        {
            
            Configure<Book, ReadBookResponse>(config =>
            {
                config
                .Match(x => x.Author.FirstName + " " + x.Author.LastName, y => y.AuthorName)
                .Match(x => x.CreatedBy.FirstName + " " + x.CreatedBy.LastName, y => y.CreatedByName)
                ;
            });

            Configure<Note, ReadNoteResponse>(config =>
            {
                config
                .Match(x => x.User.FirstName + " " + x.User.LastName, y => y.UserName)
                .Match(x => x.User.ShareId, y => y.ShareId)
                ;
            });
        }
    }
}
