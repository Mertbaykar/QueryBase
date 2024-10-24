using Microsoft.AspNetCore.Mvc;
using QueryBase.Examples.Core;
using QueryBase.Filter;
using QueryMapper.Examples.Core;

namespace QueryBase.Examples.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly IBookRepository BookRepository;

        public BookController(IBookRepository bookRepository)
        {
            this.BookRepository = bookRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Read()
        {
            var result = await BookRepository.Read<Book, ReadBookResponse>(queryOrders: [
                new QueryOrder() { PropertyName = "Name"},
                new QueryOrder() { PropertyName = "PublishYear", Asc = false},
              ]);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> ReadById(int id)
        {
            var result = await BookRepository.ReadById<Book, ReadBookResponse, int>(id);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Paginate(QueryPaginationFilter<PaginateBookFilter> paginationFilter)
        {
            var result = await BookRepository.ReadFilteredPaginate<Book, ReadBookResponse, PaginateBookFilter>(paginationFilter);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateBookRequest request)
        {
            var result = await BookRepository.Create<Book, CreateBookRequest>(request);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdateBookRequest request)
        {
            var result = await BookRepository.Update(request);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> BulkUpdate()
        {
            await BookRepository.BulkUpdate<Book>(x=> x.IsActive, updater => updater
            .Update(book=> book.IsActive, false)
            .Update(book=> book.ShelfLocation, book => book.ShelfLocation + " 2024")
            );
            return Ok();
        }
    }
}
