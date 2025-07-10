using ECommerce.Models;

namespace ECommerce.Data;

public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;

    public DatabaseSeeder(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        if (!_context.Users.Any())
        {
            // Add users
            var users = new List<User>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Email = "admin@example.com",
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    FirstName = "Admin",
                    LastName = "User",
                    Address = "123 Admin St",
                    PhoneNumber = "1234567890",
                    Role = UserRole.Admin,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            // Add regular users
            for (int i = 1; i <= 10; i++)
            {
                users.Add(new User
                {
                    Id = Guid.NewGuid(),
                    Email = $"user{i}@example.com",
                    Username = $"user{i}",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("user123"),
                    FirstName = $"User{i}",
                    LastName = $"LastName{i}",
                    Address = $"{i} User St, City",
                    PhoneNumber = $"123456789{i}",
                    Role = UserRole.Customer,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();

            // Add categories
            var categories = new[]
            {
                new Category { Id = Guid.NewGuid(), Name = "Electronics", Description = "Electronic devices and accessories" },
                new Category { Id = Guid.NewGuid(), Name = "Clothing", Description = "Fashion and apparel" },
                new Category { Id = Guid.NewGuid(), Name = "Books", Description = "Books and literature" },
                new Category { Id = Guid.NewGuid(), Name = "Sports", Description = "Sports equipment and accessories" },
                new Category { Id = Guid.NewGuid(), Name = "Home & Garden", Description = "Home decor and gardening tools" },
                new Category { Id = Guid.NewGuid(), Name = "Toys", Description = "Toys and games" },
                new Category { Id = Guid.NewGuid(), Name = "Beauty", Description = "Beauty and personal care" },
                new Category { Id = Guid.NewGuid(), Name = "Automotive", Description = "Car parts and accessories" },
                new Category { Id = Guid.NewGuid(), Name = "Food", Description = "Groceries and gourmet food" },
                new Category { Id = Guid.NewGuid(), Name = "Health", Description = "Health and wellness products" }
            };

            foreach (var category in categories)
            {
                category.CreatedAt = DateTime.UtcNow;
                category.UpdatedAt = DateTime.UtcNow;
            }

            await _context.Categories.AddRangeAsync(categories);
            await _context.SaveChangesAsync();

            // Add products
            var products = new List<Product>();
            var random = new Random();

            string[] productNames = {
                "Smartphone Pro", "Laptop Elite", "Wireless Earbuds", "Smart Watch", "Tablet Ultra",
                "Designer T-Shirt", "Premium Jeans", "Running Shoes", "Winter Jacket", "Summer Dress",
                "Programming Guide", "Mystery Novel", "History Book", "Cookbook", "Science Fiction",
                "Tennis Racket", "Football", "Yoga Mat", "Dumbbells", "Basketball",
                "Garden Tools Set", "Plant Pots", "Outdoor Furniture", "Indoor Plants", "Wall Art",
                "Board Game", "Robot Toy", "Building Blocks", "Remote Car", "Educational Toy",
                "Face Cream", "Perfume", "Hair Care Set", "Makeup Kit", "Skincare Set",
                "Car Phone Mount", "Car Vacuum", "Seat Covers", "Air Freshener", "Tool Kit",
                "Organic Coffee", "Healthy Snacks", "Vitamin Pack", "Protein Powder", "Herbal Tea"
            };

            for (int i = 0; i < productNames.Length; i++)
            {
                var categoryIndex = i / 5; // 5 products per category
                var product = new Product
                {
                    Id = Guid.NewGuid(),
                    Name = productNames[i],
                    Description = $"High-quality {productNames[i]} with great features",
                    Price = random.Next(10, 2000) + 0.99m,
                    StockQuantity = random.Next(10, 200),
                    ImageUrl = $"https://example.com/images/{productNames[i].ToLower().Replace(" ", "-")}.jpg",
                    CategoryId = categories[categoryIndex].Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                products.Add(product);
            }

            await _context.Products.AddRangeAsync(products);
            await _context.SaveChangesAsync();

            // Add reviews
            var reviews = new List<Review>();
            foreach (var product in products)
            {
                // Add 2-3 reviews per product
                var numberOfReviews = random.Next(2, 4);
                for (int i = 0; i < numberOfReviews; i++)
                {
                    var review = new Review
                    {
                        Id = Guid.NewGuid(),
                        ProductId = product.Id,
                        UserId = users[random.Next(1, users.Count)].Id, // Skip admin user
                        Rating = random.Next(3, 6), // Ratings from 3 to 5
                        Comment = $"Review {i + 1} for {product.Name}: {GetRandomReviewComment()}",
                        CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 30)),
                        UpdatedAt = DateTime.UtcNow
                    };
                    reviews.Add(review);
                }
            }

            await _context.Reviews.AddRangeAsync(reviews);
            await _context.SaveChangesAsync();

            // Add orders
            var orders = new List<Order>();
            var orderItems = new List<OrderItem>();

            foreach (var user in users.Skip(1)) // Skip admin user
            {
                // 2-3 orders per user
                var numberOfOrders = random.Next(2, 4);
                for (int i = 0; i < numberOfOrders; i++)
                {
                    var order = new Order
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        OrderDate = DateTime.UtcNow.AddDays(-random.Next(1, 60)),
                        Status = (OrderStatus)random.Next(0, 5),
                        ShippingAddress = user.Address,
                        PaymentMethod = GetRandomPaymentMethod(),
                        PaymentStatus = "Paid",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    // 1-5 items per order
                    var numberOfItems = random.Next(1, 6);
                    decimal totalAmount = 0;
                    var orderProductIds = new HashSet<Guid>();

                    for (int j = 0; j < numberOfItems; j++)
                    {
                        var product = products[random.Next(products.Count)];
                        if (orderProductIds.Add(product.Id)) // Ensure unique products in order
                        {
                            var quantity = random.Next(1, 4);
                            var orderItem = new OrderItem
                            {
                                Id = Guid.NewGuid(),
                                OrderId = order.Id,
                                ProductId = product.Id,
                                Quantity = quantity,
                                UnitPrice = product.Price,
                                Subtotal = product.Price * quantity
                            };
                            orderItems.Add(orderItem);
                            totalAmount += orderItem.Subtotal;
                        }
                    }

                    order.TotalAmount = totalAmount;
                    orders.Add(order);
                }
            }

            await _context.Orders.AddRangeAsync(orders);
            await _context.OrderItems.AddRangeAsync(orderItems);
            await _context.SaveChangesAsync();

            // Add shopping carts
            var carts = new List<Cart>();
            var cartItems = new List<CartItem>();

            foreach (var user in users.Skip(1)) // Skip admin user
            {
                var cart = new Cart
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // 1-3 items in cart
                var numberOfItems = random.Next(1, 4);
                var cartProductIds = new HashSet<Guid>();

                for (int i = 0; i < numberOfItems; i++)
                {
                    var product = products[random.Next(products.Count)];
                    if (cartProductIds.Add(product.Id)) // Ensure unique products in cart
                    {
                        var cartItem = new CartItem
                        {
                            Id = Guid.NewGuid(),
                            CartId = cart.Id,
                            ProductId = product.Id,
                            Quantity = random.Next(1, 4),
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        cartItems.Add(cartItem);
                    }
                }

                carts.Add(cart);
            }

            await _context.Carts.AddRangeAsync(carts);
            await _context.CartItems.AddRangeAsync(cartItems);
            await _context.SaveChangesAsync();
        }
    }

    private static string GetRandomReviewComment()
    {
        string[] comments = {
            "Great product, exactly what I needed!",
            "Very satisfied with the quality.",
            "Good value for money.",
            "Exceeded my expectations.",
            "Would definitely recommend.",
            "Works perfectly, no complaints.",
            "Excellent product and fast delivery.",
            "Better than expected.",
            "Very happy with this purchase.",
            "Amazing quality and design."
        };
        return comments[new Random().Next(comments.Length)];
    }

    private static string GetRandomPaymentMethod()
    {
        string[] methods = {
            "Credit Card",
            "PayPal",
            "Bank Transfer",
            "Apple Pay",
            "Google Pay"
        };
        return methods[new Random().Next(methods.Length)];
    }
}
