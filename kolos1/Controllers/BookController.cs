using kolos1.Models.DTOs;
using kolos1.Repositories;
using Microsoft.AspNetCore.Mvc;
namespace kolos1.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class BookController : ControllerBase
{

    private readonly IBookRepository _bookRepository;

    public BookController(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetBook(int id)
    {
        if (!await _bookRepository.DoesBookExist(id))
            return NotFound($"Book with given ID - {id} doesn't exist");
        
        var book = await _bookRepository.GetBook(id);
        return Ok(book);
    }

    [HttpPost]
    public async Task<IActionResult> AddBook(NewBookWithAuthors newBookWithAuthors)
    {
        await _bookRepository.AddNewBookWithAuthors(newBookWithAuthors);
        return Created(Request.Path.Value ?? "api/books", newBookWithAuthors);
    }
}