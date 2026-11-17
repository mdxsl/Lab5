using Lab5WebApplication;

namespace Lab5WebApplication.Services
{
    public class LibraryService
    {
        private readonly string booksFilePath = Path.Combine("Data", "Books.csv");
        private readonly string usersFilePath = Path.Combine("Data", "Users.csv");
        private List<Book> books = new List<Book>();
        private List<User> users = new List<User>();
        private Dictionary<User, List<Book>> borrowedBooks = new Dictionary<User, List<Book>>();

        public LibraryService() {
            ReadBooks();
            ReadUsers();
        }

        public List<Book> GetBooks() => books;
        public List<User> GetUsers() => users;
        public Dictionary<User, List<Book>> GetBorrowedBooks() => borrowedBooks;

        public void WriteBooks()
        {
            try
            {
                var lines = books.Select(b => $"{b.Id}, {b.Title}, {b.Author}, {b.ISBN}");
                File.WriteAllLines(booksFilePath, lines);
            }
            catch (Exception ex) 
            {
                Console.Write($"Error writing books: {ex.Message}");
            }
        }

        public void ReadBooks()
        {
            try
            {
                foreach (var line in File.ReadAllLines("./Data/Books.csv"))
                {
                    var fields = line.Split(',');

                    if (fields.Length >= 4)
                    {
                        var book = new Book
                        {
                            Id = int.Parse(fields[0].Trim()),
                            Title = fields[1].Trim(),
                            Author = fields[2].Trim(),
                            ISBN = fields[3].Trim()
                        };

                        books.Add(book);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        public void WriteUsers()
        {
            try
            {
                var lines = users.Select(u => $"{u.Id}, {u.Name}, {u.Email}");
                File.WriteAllLines(usersFilePath, lines);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing users: {ex.Message}");
            }
        }

        public void ReadUsers()
        {
            try
            {
                foreach (var line in File.ReadAllLines("./Data/Users.csv"))
                {
                    var fields = line.Split(',');

                    if (fields.Length >= 3) // Ensure there are enough fields
                    {
                        users.Add(new User
                        {
                            Id = int.Parse(fields[0].Trim()),
                            Name = fields[1].Trim(),
                            Email = fields[2].Trim()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        public void AddBook(Book book)
        {
            book.Id = books.Any() ? books.Max(b => b.Id) + 1 : 1;
            books.Add(book);
            WriteBooks();
        }

        public void EditBook(Book updatedBook)
        {
            var book = books.FirstOrDefault(b => b.Id == updatedBook.Id);
            if (book != null)
            {
                book.Title = updatedBook.Title;
                book.Author = updatedBook.Author;
                book.ISBN = updatedBook.ISBN;
                WriteBooks();
            }
        }

        public void DeleteBook(int id)
        {
            var book = books.FirstOrDefault(b => b.Id == id);
            if (book != null)
            {
                books.Remove(book);
                WriteBooks();
            }
        }

        public void AddUser(User user)
        {
            user.Id = users.Any() ? users.Max(u => u.Id) + 1 : 1;
            users.Add(user);
            WriteUsers();
        }

        public void EditUser(User updatedUser)
        {
            var user = users.FirstOrDefault(u => u.Id == updatedUser.Id);
            if (user != null)
            {
                user.Name = updatedUser.Name;
                user.Email = updatedUser.Email;
                WriteUsers();
            }
        }

        public void DeleteUser(int id)
        {
            var user = users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                users.Remove(user);
                WriteUsers();
            }
        }

        public void BorrowBook(int userId, int bookId)
        {
            var user = users.FirstOrDefault(u => u.Id == userId);
            var book = books.FirstOrDefault(b => b.Id == bookId);

            if (book != null && user != null)
            {
                if (!borrowedBooks.ContainsKey(user))
                {
                    borrowedBooks[user] = new List<Book>();
                }
                borrowedBooks[user].Add(book);
                books.Remove(book);
                WriteBooks();
            }
        }

        public void ReturnBook(Book book, int userId)
        {
            var user = users.FirstOrDefault(u => u.Id == userId);
            if (user != null && borrowedBooks.ContainsKey(user))
            {
                borrowedBooks[user].Remove(book);
                books.Add(book);

                if (borrowedBooks[user].Count == 0)
                {
                    borrowedBooks.Remove(user);
                }
                WriteBooks();
            }
        }
    }
}
