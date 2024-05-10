using kolos1.Models.DTOs;

namespace kolos1.Repositories;

public interface IBookRepository
{
    Task<bool> DoesBookExist(int id);
    Task<BookWithAuthors> GetBook(int id);
    Task AddNewBookWithAuthors(NewBookWithAuthors newBookWithAuthors);
}