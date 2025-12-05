using Humanizer;
using InfoInfo2025.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Cryptography;
using System.Threading;
using System.Xml.Linq;
using static Azure.Core.HttpHeader;
using static System.Net.Mime.MediaTypeNames;
using Text = InfoInfo2025.Models.Text;

namespace InfoInfo2025.Data
{
    public class InfoSeeder
    {

        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var dbContext = services.GetRequiredService<ApplicationDbContext>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                    var userManager = services.GetRequiredService<UserManager<AppUser>>();

                    if (await dbContext.Database.CanConnectAsync())
                    {
                        await SeedRolesAsync(dbContext, roleManager);
                        await SeedUsersAsync(dbContext, userManager);
                        await SeedCategoriesAsync(dbContext);
                        await SeedTextsAsync(dbContext);
                        await SeedOpinionsAsync(dbContext, userManager);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Błąd podczas inicjalizacji danych: {ex.Message}");
                }
            }
        }


        private static async Task SeedRolesAsync(ApplicationDbContext dbContext, RoleManager<IdentityRole> roleManager)
        {
            try
            {
                string[] roleNames = { "admin", "author" };
                foreach (var roleName in roleNames)
                {
                    if (!await dbContext.Roles.AnyAsync(r => r.Name == roleName))
                    {
                        var role = new IdentityRole
                        {
                            Name = roleName,
                            NormalizedName = roleName.ToUpper()
                        };
                    var result = await roleManager.CreateAsync(role);
                        if (!result.Succeeded)
                        {
                            var errors = string.Join(", ", result.Errors.Select
                            (e => e.Description));
                            throw new Exception($"Nie udało się utworzyć roli {roleName}. Błędy: { errors }");
                        }
                    }
                }
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Błąd podczas tworzenia ról: {ex.Message}");
            }
        }


        private static async Task SeedUsersAsync(ApplicationDbContext dbContext, UserManager<AppUser> userManager)
        {
            // Lista użytkowników do utworzenia
            var usersToCreate = new List<(AppUser User, string Password, string Role)>
            {
                (new AppUser
                {
                    UserName = "autor1@portal.pl",
                    NormalizedUserName = "AUTOR1@PORTAL.PL",
                    Email = "autor1@portal.pl",
                    NormalizedEmail = "AUTOR1@PORTAL.PL",
                    EmailConfirmed = true,
                    LockoutEnabled = false,
                    FirstName = "Piotr",
                    LastName = "Pisarski",
                    Photo = "autor1.jpg",
                    Information = "Wszechstronny programista aplikacji sieciowych i internetowych. W portfolio ma kilka ciekawych projektów zrealizowanych dla firm z branży finansowej. Współpracuje z innowacyjnymi startupami."
                }, "Portalik1!", "author"),

                (new AppUser
                {
                    UserName = "autor2@portal.pl",
                    NormalizedUserName = "AUTOR2@PORTAL.PL",
                    Email = "autor2@portal.pl",
                    NormalizedEmail = "AUTOR2@PORTAL.PL",
                    EmailConfirmed = true,
                    LockoutEnabled = false,
                    FirstName = "Anna",
                    LastName = "Autorska",
                    Photo = "autor2.jpg",
                    Information = "Doświadczona programistka i projektantka stron internetowych oraz uznana blogierka. Specjalizuje się w HTML5, CSS3, JavaScript, jQuery i Bootstrap. Obecnie pracuje nad nowymi rozwiązaniami dla graczy."
                }, "Portalik1!", "author"),


                (new AppUser
                {
                    UserName = "admin@portal.pl",
                    NormalizedUserName = "ADMIN@PORTAL.PL",
                    Email = "admin@portal.pl",
                    NormalizedEmail = "ADMIN@PORTAL.PL",
                    EmailConfirmed = true,
                    LockoutEnabled = false,
                    FirstName = "Ewa",
                    LastName = "Ważna",
                    Photo = "woman.png",
                    Information = ""
                }, "Portalik1!", "admin")
                };

            try
            {
                    foreach (var (user, password, role) in usersToCreate)
                    {
                        if (!await dbContext.Users.AnyAsync(u => u.UserName == user.UserName))
                        {
                            var result = await userManager.CreateAsync(user, password);
                            if (result.Succeeded)
                            {
                                var roleResult = await userManager.AddToRoleAsync(user, role);
                                if (!roleResult.Succeeded)
                                {
                                    var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                                    throw new Exception($"Błąd podczas przypisywania roli dla { user.UserName }: { errors}");
                                }
                            }
                            else
                            {
                                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                                throw new Exception($"Błąd podczas tworzenia użytkownika { user.UserName }: { errors}");
                            }
                        }
                    }
                    await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Błąd podczas tworzenia użytkowników: { ex.Message }");
            }
        }


        private static async Task SeedCategoriesAsync(ApplicationDbContext dbContext)
        {
            try
            {
                if (!await dbContext.Categories.AnyAsync())
                {
                    var categories = new List<Category>
                    {
                        new Category
                        {
                        Name = "Wiadomości",
                        Active = true,
                        Display = true,
                        Icon = "chat-left-text",
                        Description = "Najświeższe wiadomości i informacje z dziedziny informatyki. Coś dla programistów i zwykłych użytkowników komputerów, tabletów oraz smartfonów."
                        },
                        new Category
                        {
                        Name = "Artykuły",
                        Active = true,
                        Display = true,
                        Icon = "journal-richtext",
                        Description = "Artykuły w naszym serwisie pisane są przez wybitnych znawców tematu, którzy z olbrzymią przenikliwością zgłębiają każdy temat."
                        },
                        new Category
                        {
                        Name = "Testy",
                        Active = true,
                        Display = true,
                        Icon = "speedometer",
                        Description = "Nasze laboratorium testuje dla Was najnowszy sprzęt, poddając go elektronicznym torturom i wyciskając siódme poty elektronów."
                        },
                        new Category
                        {
                        Name = "Porady",
                        Active = true,
                        Display = true,
                        Icon = "life-preserver",
                        Description = "Jeżeli wciąż masz problemy z obsługą komputerów lub chcesz pracować efektywnie zajrzyj do sekcji z poradami dla adminów i laików."
                        },
                        new Category
                        {
                            Name = "Tutoriale",
                            Active = true,
                            Display = true,
                            Icon = "display",
                            Description = "W tutorialach opisujemy krok po kroku, w jaki sposób rozwiązać zadania programistyczne praktycznie z każdej dziedziny."
                        },
                        new Category
                        {
                            Name = "Recenzje",
                            Active = true,
                            Display = true,
                            Icon = "controller",
                            Description = "Czytamy najciekawsze książki informatyczne i gramy dla Was w najnowsze gry, aby później opisać je dokładnie w tym dziale."
                        }
                    };

                    await dbContext.Categories.AddRangeAsync(categories);
                    await dbContext.SaveChangesAsync();
                        
                }
                  
            }

            catch (Exception ex)
            {
                throw new Exception($"Błąd podczas dodawania kategorii: { ex.Message}");
            }
        }

        private static async Task SeedTextsAsync(ApplicationDbContext dbContext)
        {
            try
            {
                if (!await dbContext.Texts.AnyAsync())
                {
                    var autor1Id = await dbContext.Users
                    .Where(u => u.UserName == "autor1@portal.pl")
                    .Select(u => u.Id)
                    .FirstOrDefaultAsync();
                    var autor2Id = await dbContext.Users
                    .Where(u => u.UserName == "autor2@portal.pl")
                    .Select(u => u.Id)
                    .FirstOrDefaultAsync();

                    if (autor1Id == null || autor2Id == null)
                    {
                        throw new Exception("Nie znaleziono wymaganych autorów w bazie danych");
                    }

                    var texts = new List<Text>();
                    // Generowanie tekstów dla obu autorów
                    for (int i = 1; i <= 6; i++) // sześć kategorii
                    {
                        // Teksty dla autora 1
                        for (int j = 0; j <= 4; j++)
                        {
                            texts.Add(new Text
                            {
                                Title = $"Tytuł{i}{j}",
                                Summary = $"Streszczenie tekstu o tytule Tytuł{i}{j}",
                                Keywords = $"tag{j}, tag{i + j}",
                                Content = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo.Nemo enim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos qui ratione voluptatem sequi nesciunt.Neque porro quisquam est, qui dolorem ipsum quia dolor sit amet, consectetur, adipisci velit, sed quia non numquam eius modi tempora incidunt ut labore et dolore magnam aliquam quaerat voluptatem. Ut enim ad minima veniam, quis nostrum exercitationem ullam corporis suscipit laboriosam, nisi ut aliquid ex ea commodi consequatur? Quis autem vel eum iure reprehenderit qui in ea voluptate velit esse quam nihil molestiae consequatur, vel illum qui dolorem eum fugiat quo voluptas nulla pariatur ? ",
                                AddedDate = DateTime.Now.AddDays(-i * j),
                                CategoryId = i,
                                UserId = autor1Id,
                                Active = true
                            });
                        }

                        // Teksty dla autora 2
                        for (int j = 5; j <= 9; j++)
                        {
                            texts.Add(new Text
                            {
                                Title = $"Tytuł{i}{j}",
                                Summary = $"Streszczenie tekstu o tytule Tytuł{i}{j}",
                                Keywords = $"tag{j}, tag{i + j}",
                                Content = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo.Nemo enim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos qui ratione voluptatem sequi nesciunt.Neque porro quisquam est, qui dolorem ipsum quia dolor sit amet, consectetur, adipisci velit, sed quia non numquam eius modi tempora incidunt ut labore et dolore magnam aliquam quaerat voluptatem. Ut enim ad minima veniam, quis nostrum exercitationem ullam corporis suscipit laboriosam, nisi ut aliquid ex ea commodi consequatur? Quis autem vel eum iure reprehenderit qui in ea voluptate velit esse quam nihil molestiae consequatur, vel illum qui dolorem eum fugiat quo voluptas nulla pariatur ? ",
                                AddedDate = DateTime.Now.AddDays(-i * j),
                                CategoryId = i,
                                UserId = autor2Id,
                                Active = true
                            });
                        }
                    }
                    await dbContext.Texts.AddRangeAsync(texts);
                    await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Błąd podczas dodawania tekstów: {ex.Message}");
            }
        }

        private static async Task SeedOpinionsAsync(ApplicationDbContext dbContext, UserManager<AppUser> userManager)
        {
            if (!dbContext.Opinions.Any())
            {
                var autor2 = await userManager.FindByEmailAsync("admin@portal.pl");
                var autor1 = await userManager.FindByEmailAsync("autor1@portal.pl");
                if (autor2 != null)
                {
                    var opinie1 = Enumerable.Range(1, 60).Select(i => new Opinion
                    {
                        Comment = "Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo.",
                        AddedDate = DateTime.Now.AddDays(-i),
                        UserId = autor2.Id,
                        TextId = i,
                        Rating = Rating.Excellent
                    });

                    await dbContext.Opinions.AddRangeAsync(opinie1);
                    await dbContext.SaveChangesAsync();
                }

                if (autor1 != null)
                {
                    var opinie2 = Enumerable.Range(1, 60).Select(i => new Opinion
                    {
                        Comment = "Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo.",
                        AddedDate = DateTime.Now.AddDays(-i),
                        UserId = autor1.Id,
                        TextId = i,
                        Rating = Rating.Good
                    });
                    await dbContext.Opinions.AddRangeAsync(opinie2);
                    await dbContext.SaveChangesAsync();
                }
            }
        }
    }
}
