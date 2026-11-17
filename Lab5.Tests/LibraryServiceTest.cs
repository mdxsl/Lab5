 namespace Lab5.Tests;

using System.Reflection;
using Lab5WebApplication;
using Lab5WebApplication.Components.Pages;
using Lab5WebApplication.Services;

[TestClass]
public sealed class LibraryServiceTest
{
    private string originalDirectory = string.Empty;
    private string tempDirectory = string.Empty;
    private LibraryService service = null!;
    [TestInitialize]
    public void Setup()
    {
        originalDirectory = Environment.CurrentDirectory;
        tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(Path.Combine(tempDirectory, "Data"));

        File.WriteAllLines(Path.Combine(tempDirectory, "Data", "Books.csv"), new[]
        {
            "1,Dune,Frank Herbert,111",
            "2,Dune,Frank Herbert,111",
            "3,Emma,Jane Austen,222"
        });
        File.WriteAllLines(Path.Combine(tempDirectory, "Data", "Users.csv"), new[]
        {
            "1,Ada Lovelace,ada@example.com",
            "2,Alan Turing,alan@example.com"
        });

        Environment.CurrentDirectory = tempDirectory;
        service = new LibraryService();
    }
    [TestCleanup]
    public void Cleanup()
    {
        Environment.CurrentDirectory = originalDirectory;

        if (Directory.Exists(tempDirectory))
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }
    [TestMethod]
    public void GetBooks_LoadsSeededDataCorrectly()
    {
        var books = service.GetBooks();

        Assert.AreEqual(3, books.Count);
        Assert.AreEqual("Dune", books[0].Title);
        Assert.AreEqual("Emma", books[2].Title);
    }

    [TestMethod]
    public void GetUsers_LoadsSeededDataCorrectly()
    {
        var users = service.GetUsers();

        Assert.AreEqual(2, users.Count);
        Assert.AreEqual("Ada Lovelace", users[0].Name);
        Assert.AreEqual("Alan Turing", users[1].Name);
    }

    [TestMethod]
    public void AddBook_AppendsNewBookandAssignsId()
    {
        var newBook = new Book { Title = "Poop", Author = "Beetle Juice", ISBN = "6767"};

        service.AddBook(newBook);
        var books = service.GetBooks();

        Assert.AreEqual(99, books.Count);
        Assert.AreEqual("Poop", books.Last().Title);
        Assert.AreEqual(4, books.Last().Id);
    }

    [TestMethod]
    public void AddUser_AppendsNewUserandAssignsId()
    {
        var newUser = new User { Name = "Harry Maguire", Email = "harry@example.com"};

        service.AddUser(newUser);
        var users = service.GetUsers();

        Assert.AreEqual(3, users.Count);
        Assert.AreEqual("Harry Maguire", users.Last().Name);
        Assert.AreEqual(3, users.Last().Id);
    }

    [TestMethod]
    public void DeleteBook_DeletesBookCorrectly()
    {
        service.DeleteBook(3);
        var books = service.GetBooks();

        Assert.AreEqual(2, books.Count);
        Assert.IsFalse(books.Any(b => b.Id == 3));
    }

    [TestMethod]
    public void DeleteUser_DeletesUserCorrectly()
    {
        service.DeleteUser(1);
        var users = service.GetUsers();

        Assert.AreEqual(1, users.Count);
        Assert.IsFalse(users.Any(u => u.Id == 1));
    }

    [TestMethod]
    public void EditUser_UpdatesExistingUser()
    {
        var updatedUser = new User { Id = 1, Name = "Ada Byron", Email = "ada.byron@example.com"};
        service.EditUser(updatedUser);
        var user = service.GetUsers().First(u => u.Id == 1);

        Assert.AreEqual("Ada Byron", user.Name);
        Assert.AreEqual("ada.byron@example.com", user.Email);
    }

    [TestMethod]
    public void EditBook_UpdatesExistingBook()
    {
        var updatedBook = new Book { Id = 3, Title = "Glory and Fame", Author = "Jane Austen", ISBN = "999"};
        service.EditBook(updatedBook);
        var book = service.GetBooks().First(u => u.Id == 3);

        Assert.AreEqual("Glory and Fame", book.Title);
        Assert.AreEqual("999", book.ISBN);
    }

    [TestMethod]
    public void WriteBooks_CreatesChangesToDisk()
    {
        service.AddBook(new Book { Title = "The War in the West", Author = "James Holland", ISBN = "444"});

        var newServiceInstance = new LibraryService();
        var reloadedBooks = newServiceInstance.GetBooks();

        Assert.AreEqual(4, reloadedBooks.Count);
        Assert.AreEqual("The War in the West", reloadedBooks.Last().Title);
    }

    [TestMethod]
    public void WriteUsers_CreatesChangesToDisk()
    {
        service.AddUser(new User { Name = "LeBron James", Email = "james@example.com"});

        var newServiceInstance = new LibraryService();
        var reloadedUsers = newServiceInstance.GetUsers();

        Assert.AreEqual(3, reloadedUsers.Count);
        Assert.AreEqual("LeBron James", reloadedUsers.Last().Name);
    }

    [TestMethod]
    public void BorrowBook_MovesBookToBorrowedDictionary()
    {
        service.BorrowBook(1, 1);

        Assert.AreEqual(2, service.GetBooks().Count);

        var borrowedDict = service.GetBorrowedBooks();
        Assert.AreEqual(1, borrowedDict.Count());

        var userKey = borrowedDict.Keys.First();
        Assert.AreEqual("Ada Lovelace", userKey.Name);
        Assert.AreEqual("Dune", borrowedDict[userKey][0].Title);
    }

    [TestMethod]
    public void ReturnBook_RestoresBookToLibraryAndCleansUpDictionary()
    {
        service.BorrowBook(1, 1);
        var borrowedBook = service.GetBorrowedBooks().Keys
        .SelectMany(u => service.GetBorrowedBooks()[u])
        .First();

        service.ReturnBook(borrowedBook, 1);

        Assert.AreEqual(3, service.GetBooks().Count);

        Assert.IsEmpty(service.GetBorrowedBooks());
    }
}
