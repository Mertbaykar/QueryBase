using Microsoft.EntityFrameworkCore;
using QueryBase;
using QueryBase.Examples.Core;

namespace QueryMapper.Examples.Core
{
    public class BookRepository : QueryRepoBase<BookContext>, IBookRepository
    {

        public BookRepository(BookContext bookContext, IQueryMapper queryMapper) : base(bookContext, queryMapper)
        {
        }

        public async Task<Book> Update(UpdateBookRequest request)
        {
            var book = await base.Update<Book, UpdateBookRequest, int>(request, false, b => b.Include(x => x.Author).Include(x => x.CreatedBy));
            book.Author.DeActivate();
            await Save();
            return book;
        }
    }

    public interface IBookRepository : IQueryRepoBase
    {
        Task<Book> Update(UpdateBookRequest request);
    }
}
