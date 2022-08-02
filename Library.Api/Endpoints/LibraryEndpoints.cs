﻿using FluentValidation;
using FluentValidation.Results;
using Library.Api.Models;
using Library.Api.Services;
using Microsoft.AspNetCore.Cors;

namespace Library.Api.Endpoints;

public static class LibraryEndpoints
{
    public static void AddLibraryEndPoints(this IServiceCollection services)
    {
        services.AddSingleton<IBookService, BookService>();
    }

    public static void UseLibraryEndPoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("books",
                // [Authorize(AuthenticationSchemes = ApiKeySchemeConstants.SchemeName)]
                // [AllowAnonymous]
                CreateBookAsync).WithName("CreateBook")
            .Accepts<Book>("application/json")
            .Produces<Book>(201)
            .Produces<IEnumerable<ValidationFailure>>(400)
            .WithTags("Books");
// .AllowAnonymous();

        app.MapGet("books", async (IBookService bookService, string? searchTerm) =>
            {
                if (searchTerm is not null && !string.IsNullOrWhiteSpace(searchTerm))
                {
                    var matchedBooks = await bookService.SearchByTitleAsync(searchTerm);
                    return Results.Ok(matchedBooks);
                }

                var books = await bookService.GetAllAsync();
                return Results.Ok(books);
            }).WithName("GetBooks")
            .Produces<IEnumerable<Book>>(200)
            .WithTags("Books");

        app.MapGet("books/{isbn}", async (string isbn, IBookService bookService) =>
            {
                var book = await bookService.GetByIsbnAsync(isbn);
                return book is not null ? Results.Ok(book) : Results.NotFound();
            }).WithName("GetBook")
            .Produces<Book>(200)
            .Produces(404)
            .WithTags("Books");

        app.MapPut("books/{isbn}", async (string isbn, Book book, IBookService bookService,
                IValidator<Book> validator) =>
            {
                book.Isbn = isbn;
                var validationResult = await validator.ValidateAsync(book);
                if (!validationResult.IsValid)
                    return Results.BadRequest(validationResult.Errors);
                var updated = await bookService.UpdateAsync(book);
                return updated ? Results.Ok(book) : Results.NotFound();
            }).WithName("UpdateBook")
            .Accepts<Book>("application/json")
            .Produces<Book>(200)
            .Produces<IEnumerable<ValidationFailure>>(400)
            .WithTags("Books");

        app.MapDelete("books/{isbn}", async (string isbn, IBookService bookService) =>
            {
                var deleted = await bookService.DeleteAsync(isbn);
                return deleted ? Results.NoContent() : Results.NotFound();
            }).WithName("DeleteBook")
            .Produces(204)
            .Produces(404)
            .WithTags("Books");

        app.MapGet("status", [EnableCors("AnyOrigin")]() =>
        {
            return Results.Extensions.Html(@"<!doctype html>
<html>
    <head>
        <title>Status page</title>
    </head>
    <body>
        <h1>Status</h1>
        <p>The server is working fine. Bye bye!</p>
    </body>
</html>");
        }).ExcludeFromDescription();
// .RequireCors("AnyOrigin");
    }

    private static async Task<IResult> CreateBookAsync(Book book, IBookService bookService, IValidator<Book> validator)
    {
        var validationResult = await validator.ValidateAsync(book);
        if (!validationResult.IsValid)
            return Results.BadRequest(validationResult.Errors);

        var created = await bookService.CreateAsync(book);
        if (!created)
            return Results.BadRequest(new List<ValidationFailure>
            {
                new("Isbn", "A book with this ISBN-13 already exists")
            });
        return Results.CreatedAtRoute("GetBook", new {isbn = book.Isbn}, book);
        // return Results.Created($"/books/{book.Isbn}", book);
    }
}