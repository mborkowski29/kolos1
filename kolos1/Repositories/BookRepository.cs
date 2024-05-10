//using System.Data.SqlClient;
using kolos1.Models.DTOs;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Http.HttpResults;
using kolos1.Repositories;
namespace kolos1.Repositories;

public class BookRepository : IBookRepository
{
    private readonly IConfiguration _configuration;

    public BookRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public async Task<bool> DoesBookExist(int id)
    {
        var query = "SELECT 1 FROM Books WHERE ID = @ID";
        await using SqlConnection connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@ID", id);

        await connection.OpenAsync();
        var res = await command.ExecuteScalarAsync();

        return res is not null;
    }

    public async Task<BookWithAuthors> GetBook(int id)
    {
        var query = @"SELECT books.PK, books.title, [authors].first_name, [authors].last_name 
                        FROM book 
                        JOIN books_authors ON books_authors.FK_book = books.PK
                        JOIN [authors] ON [authors].PK = books_authors.FK_author";
        
        await using SqlConnection connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@ID", id);

        await connection.OpenAsync();

        var reader = await command.ExecuteReaderAsync();

        var idBookOrdinal = reader.GetOrdinal("BookId");
        var titleBookOrdinal = reader.GetOrdinal("BookTitle");
        var firstNameAuthorOrdinal = reader.GetOrdinal("AuthorFirstName");
        var lastNameAuthorOrdinal = reader.GetOrdinal("AuthorLastName");

        BookWithAuthors bookWithAuthor = null;

        while (await reader.ReadAsync())
        {
            if (bookWithAuthor is not null)
            {
                bookWithAuthor.Authors.Add(new Author()
                {
                    FirstName = reader.GetString(firstNameAuthorOrdinal),
                    LastName = reader.GetString(lastNameAuthorOrdinal)
                });
            }
            else
            {
                bookWithAuthor = new BookWithAuthors()
                {
                    Id = reader.GetInt32(idBookOrdinal),
                    Title = reader.GetString(titleBookOrdinal),
                    Authors = new List<Author>()
                    {
                        new Author()
                        {
                            FirstName = reader.GetString(firstNameAuthorOrdinal),
                            LastName = reader.GetString(lastNameAuthorOrdinal)
                        }
                    }
                };
            }
        }

        if (bookWithAuthor is null) throw new Exception();

        return bookWithAuthor;
    }

    public async Task AddNewBookWithAuthors(NewBookWithAuthors newBookWithAuthors)
    {
        var insert = @"INSERT INTO books VALUES(@Title); 
                            SELECT @@IDENTITY AS ID;";

        await using SqlConnection connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = insert;

        command.Parameters.AddWithValue("@Title", newBookWithAuthors.Title);

        await connection.OpenAsync();

        var transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;

        try
        {
            var id = await command.ExecuteScalarAsync();

            foreach (var author in newBookWithAuthors.Authors)
            {
                command.Parameters.Clear();
                command.CommandText = "INSERT INTO authors VALUES(@FirstName, @LastName)";
                command.Parameters.AddWithValue("@FirstName", author.FirstName);
                command.Parameters.AddWithValue("@LastName", author.LastName);

                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}