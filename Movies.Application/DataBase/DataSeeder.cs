using Dapper;
using Movies.Application.Auth;
using Movies.Application.Models;
using System.Text.RegularExpressions;

namespace Movies.Application.DataBase
{
    public class DataSeeder
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public DataSeeder(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task SeedAsync(bool forceMovies = false, CancellationToken token = default)
        {
            await using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

            // 1. Seed Enterprise Users
            var usersCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(id) FROM users;");
            if (usersCount == 0)
            {
                var defaultUsers = new[]
                {
                    new
                    {
                        Id = Guid.Parse("b9787b32-8558-4a34-9c43-095b1280c790"),
                        Email = "admin@cinemovies.io",
                        FullName = "System Administrator",
                        PasswordHash = PasswordHasher.Hash("Admin123!"),
                        Role = UserRoles.Admin,
                        CreatedAt = DateTime.UtcNow
                    },
                    new
                    {
                        Id = Guid.Parse("7c9e6679-7425-40de-944b-e07fc1f90ae7"),
                        Email = "critic@cinemovies.io",
                        FullName = "Senior Film Critic",
                        PasswordHash = PasswordHasher.Hash("Critic123!"),
                        Role = UserRoles.TrustedMember,
                        CreatedAt = DateTime.UtcNow
                    },
                    new
                    {
                        Id = Guid.Parse("e24a87b3-c19d-4882-9e3f-6781298c5ab2"),
                        Email = "member@cinemovies.io",
                        FullName = "Cinema Enthusiast",
                        PasswordHash = PasswordHasher.Hash("Member123!"),
                        Role = UserRoles.Member,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                await connection.ExecuteAsync("""
                    INSERT INTO users (id, email, fullname, passwordhash, role, createdat)
                    VALUES (@Id, LOWER(@Email), @FullName, @PasswordHash, @Role, @CreatedAt)
                    ON CONFLICT (id) DO NOTHING;
                    """, defaultUsers);
            }

            // 2. Seed Movies, Synopses, Official Trailers, and Genres
            var moviesCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(id) FROM movies;");
            var unpopulatedCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(id) FROM movies WHERE description IS NULL OR trailerurl IS NULL;");

            if (moviesCount < 20 || unpopulatedCount > 0 || forceMovies)
            {
                var initialMovies = new (Guid Id, string Title, int Year, string Description, string TrailerUrl, string[] Genres)[]
                {
                    (
                        Guid.Parse("11111111-1111-1111-1111-111111111111"),
                        "Inception",
                        2010,
                        "Dom Cobb is a skilled thief, the absolute best in the dangerous art of extraction, stealing valuable secrets from deep within the subconscious during the dream state. Cobb's rare ability has made him a coveted player in corporate espionage, but it has also made him an international fugitive and cost him everything he loves. Now Cobb is being offered a chance at redemption: one last job could give him his life back, only if he can accomplish the impossible—inception.",
                        "https://www.youtube.com/watch?v=YoHD9XEInc0",
                        new[] { "Action", "Sci-Fi", "Thriller" }
                    ),
                    (
                        Guid.Parse("22222222-2222-2222-2222-222222222222"),
                        "Interstellar",
                        2014,
                        "In Earth's future, a global crop blight and second Dust Bowl are slowly rendering the planet uninhabitable. Professor Brand, a brilliant NASA physicist, is working on plans to save mankind by transporting Earth's population to a new home via a wormhole. But first, Brand must send former NASA pilot Cooper and a team of researchers through the wormhole and across the galaxy to find out which of three planets could be mankind's new home.",
                        "https://www.youtube.com/watch?v=zSWdZVtXT7E",
                        new[] { "Adventure", "Drama", "Sci-Fi" }
                    ),
                    (
                        Guid.Parse("33333333-3333-3333-3333-333333333333"),
                        "The Dark Knight",
                        2008,
                        "With the help of allies Lt. Jim Gordon and DA Harvey Dent, Batman has been able to keep a tight lid on crime in Gotham City. But when a vile young criminal calling himself the Joker suddenly throws the town into chaos, the Caped Crusader begins to tread a fine line between heroism and vigilantism in a psychological war for Gotham's soul.",
                        "https://www.youtube.com/watch?v=EXeTwQWrcwY",
                        new[] { "Action", "Crime", "Drama" }
                    ),
                    (
                        Guid.Parse("44444444-4444-4444-4444-444444444444"),
                        "Oppenheimer",
                        2023,
                        "During World War II, Lt. Gen. Leslie Groves Jr. appoints physicist J. Robert Oppenheimer to work on the top-secret Manhattan Project. Oppenheimer and a team of scientists spend years developing and designing the atomic bomb. Their work comes to fruition on July 16, 1945, as they witness the world's first nuclear explosion, forever changing the course of history.",
                        "https://www.youtube.com/watch?v=uYPbbksJxIg",
                        new[] { "Biography", "Drama", "History" }
                    ),
                    (
                        Guid.Parse("55555555-5555-5555-5555-555555555555"),
                        "The Matrix",
                        1999,
                        "Thomas A. Anderson is a man living two lives. By day he is an average computer programmer and by night a hacker known as Neo. Neo has always questioned his reality, and when the mysterious Morpheus contacts him, Neo discovers that humanity is trapped inside a simulated reality created by sentient machines—and he may be the One prophesied to end the war.",
                        "https://www.youtube.com/watch?v=vKQi3bBA1y8",
                        new[] { "Action", "Sci-Fi" }
                    ),
                    (
                        Guid.Parse("66666666-6666-6666-6666-666666666666"),
                        "Pulp Fiction",
                        1994,
                        "Vincent Vega and Jules Winnfield are hitmen with a penchant for philosophical discussions. In this ultra-hip, multi-strand crime movie, their storyline is interwoven with those of their boss, gangster Marsellus Wallace; his actress wife, Mia; struggling boxer Butch Coolidge; master fixer Winston Wolfe; and a nervous pair of diner robbers.",
                        "https://www.youtube.com/watch?v=s7EdQ4FqbhY",
                        new[] { "Crime", "Drama" }
                    ),
                    (
                        Guid.Parse("77777777-7777-7777-7777-777777777777"),
                        "The Godfather",
                        1972,
                        "Widely regarded as one of the greatest films of all time, this mob drama focuses on the powerful Italian-American crime family of Don Vito Corleone. When the don's youngest son, Michael, reluctantly joins the Mafia after an assassination attempt on his father, he is drawn into the inevitable cycle of violence and betrayal.",
                        "https://www.youtube.com/watch?v=UaVTIH8mujA",
                        new[] { "Crime", "Drama" }
                    ),
                    (
                        Guid.Parse("88888888-8888-8888-8888-888888888888"),
                        "Fight Club",
                        1999,
                        "A depressed man suffering from chronic insomnia meets a strange soap salesman named Tyler Durden and soon finds himself living in his squalid house after his perfect apartment is destroyed. The two bored men form an underground club with strict rules and fight other men who are fed up with their mundane, consumerist lives.",
                        "https://www.youtube.com/watch?v=qtRKdVHc-cE",
                        new[] { "Drama", "Thriller" }
                    ),
                    (
                        Guid.Parse("99999999-9999-9999-9999-999999999999"),
                        "Forrest Gump",
                        1994,
                        "Slow-witted Forrest Gump has never thought of himself as disadvantaged, and thanks to his supportive mother, he leads anything but a restricted life. Whether dominating on the gridiron as a college football star, fighting in Vietnam, or captaining a shrimp boat, Forrest inspires people with his childlike optimism across decades of American history.",
                        "https://www.youtube.com/watch?v=bLvqoHBptjg",
                        new[] { "Drama", "Romance" }
                    ),
                    (
                        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                        "Spirited Away",
                        2001,
                        "During her family's move to the suburbs, a sullen 10-year-old girl named Chihiro wanders into a world ruled by gods, witches, and spirits, where humans are changed into beasts. Forced to work in an enchanted bathhouse run by the sorceress Yubaba, Chihiro must summon her inner courage to free herself and her parents.",
                        "https://www.youtube.com/watch?v=ByXuk9QqQkk",
                        new[] { "Animation", "Adventure", "Fantasy" }
                    ),
                    (
                        Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                        "Parasite",
                        2019,
                        "Greed and class discrimination threaten the newly formed symbiotic relationship between the wealthy Park family and the destitute Kim clan. As the Kims infiltrate the Park household one by one by posing as unrelated, highly qualified professionals, a shocking discovery in the mansion's basement unravels their scheme.",
                        "https://www.youtube.com/watch?v=5xH0HfJHsaY",
                        new[] { "Drama", "Thriller" }
                    ),
                    (
                        Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                        "Gladiator",
                        2000,
                        "Commodus takes power and strips rank from Maximus, one of the favored generals of his predecessor and father, Emperor Marcus Aurelius. Maximus is then relegated to fighting to the death in the gladiator arenas, where his tactical brilliance and defiance turn him into a hero of the Roman people and a mortal threat to the corrupt emperor.",
                        "https://www.youtube.com/watch?v=owK1qxDselE",
                        new[] { "Action", "Adventure", "Drama" }
                    ),
                    (
                        Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                        "Blade Runner 2049",
                        2017,
                        "Officer K, a new blade runner for the Los Angeles Police Department, unearths a long-buried secret that has the potential to plunge what's left of society into chaos. His discovery leads him on a quest to find Rick Deckard, a former LAPD blade runner who has been missing for thirty years.",
                        "https://www.youtube.com/watch?v=gCcx85zbxz4",
                        new[] { "Action", "Drama", "Sci-Fi" }
                    ),
                    (
                        Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                        "Whiplash",
                        2014,
                        "Andrew Neiman is an ambitious young jazz drummer, single-minded in his pursuit to rise to the top of his elite East Coast music conservatory. Terence Fletcher, an instructor known for his terrifying teaching methods, discovers Andrew and transfers the aspiring drummer into the top jazz ensemble, forever changing the young man's life.",
                        "https://www.youtube.com/watch?v=7d_jQycdQGo",
                        new[] { "Drama", "Music" }
                    ),
                    (
                        Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                        "The Prestige",
                        2006,
                        "In end-of-the-century London, two rival magicians, Robert Angier and Alfred Borden, become obsessed with creating the ultimate stage illusion. As their competition escalates from harmless pranks to deadly sabotage, both men sacrifice everything they hold dear in pursuit of supremacy and the secret of the 'Transported Man'.",
                        "https://www.youtube.com/watch?v=RLtaA9fFNXU",
                        new[] { "Drama", "Mystery", "Sci-Fi" }
                    ),
                    (
                        Guid.Parse("10101010-1111-2222-3333-444444444441"),
                        "The Shawshank Redemption",
                        1994,
                        "Andy Dufresne, a successful young banker, is arrested for the murders of his wife and her lover, and is sentenced to life imprisonment at the Shawshank prison. Over the following two decades, he befriends a fellow prisoner, contraband smuggler Ellis 'Red' Redding, and becomes instrumental in a money laundering operation while quietly plotting his freedom.",
                        "https://www.youtube.com/watch?v=PLl99DlL6b4",
                        new[] { "Drama" }
                    ),
                    (
                        Guid.Parse("10101010-1111-2222-3333-444444444442"),
                        "Dune: Part Two",
                        2024,
                        "Paul Atreides unites with Chani and the Fremen while on a warpath of revenge against the conspirators who destroyed his family. Facing a choice between the love of his life and the fate of the known universe, he endeavors to prevent a terrible future only he can foresee.",
                        "https://www.youtube.com/watch?v=Way9Dexny3w",
                        new[] { "Action", "Adventure", "Sci-Fi" }
                    ),
                    (
                        Guid.Parse("10101010-1111-2222-3333-444444444443"),
                        "The Lord of the Rings: The Fellowship of the Ring",
                        2001,
                        "A meek Hobbit from the Shire and eight companions set out on a perilous journey across Middle-earth to destroy the One Ring and the Cracks of Doom, while the Dark Lord Sauron marshals his armies to reclaim his lost power.",
                        "https://www.youtube.com/watch?v=V75dMMIW2B4",
                        new[] { "Action", "Adventure", "Fantasy" }
                    ),
                    (
                        Guid.Parse("10101010-1111-2222-3333-444444444444"),
                        "Spider-Man: Across the Spider-Verse",
                        2023,
                        "After reuniting with Gwen Stacy, Brooklyn's full-time, friendly neighborhood Spider-Man is catapulted across the Multiverse, where he encounters the Spider Society, a team of Spider-People charged with protecting the Multiverse's very existence. When the heroes clash on how to handle a new threat, Miles must redefine what it means to be a hero.",
                        "https://www.youtube.com/watch?v=cqGjhVJWtEg",
                        new[] { "Animation", "Action", "Adventure" }
                    ),
                    (
                        Guid.Parse("10101010-1111-2222-3333-444444444445"),
                        "Mad Max: Fury Road",
                        2015,
                        "Years after the collapse of civilization, the tyrannical Immortan Joe enslaves apocalypse survivors inside the desert fortress the Citadel. When the warrior Imperator Furiosa leads the despot's five wives in a daring escape, she forges an alliance with Max Rockatansky, a loner and former captive.",
                        "https://www.youtube.com/watch?v=hEJnMQG9ev8",
                        new[] { "Action", "Adventure", "Sci-Fi" }
                    ),
                    (
                        Guid.Parse("10101010-1111-2222-3333-444444444446"),
                        "Se7en",
                        1995,
                        "When retiring police Detective William Somerset tackles a final case with the aid of newly transferred David Mills, they discover a number of elaborate andgrisly murders. They soon realize they are dealing with a serial killer who is targeting people he thinks represent one of the seven deadly sins.",
                        "https://www.youtube.com/watch?v=znmZoVkCjpI",
                        new[] { "Crime", "Drama", "Mystery" }
                    ),
                    (
                        Guid.Parse("10101010-1111-2222-3333-444444444447"),
                        "Goodfellas",
                        1990,
                        "A young man grows up in the mob and works very hard to advance himself through the ranks. He enjoys his life of money and luxury, but is oblivious to the horror that he causes. A drug addiction and a few mistakes ultimately unravel his climb to the top of the Lucchese crime family.",
                        "https://www.youtube.com/watch?v=2ilzidi_J8Q",
                        new[] { "Biography", "Crime", "Drama" }
                    ),
                    (
                        Guid.Parse("10101010-1111-2222-3333-444444444448"),
                        "Everything Everywhere All at Once",
                        2022,
                        "When an interdimensional rupture unravels reality, an unlikely hero—an exhausted Chinese-American laundromat owner who can't even finish her taxes—must channel her newfound powers to fight bizarre and bewildering dangers from the multiverse as the fate of the world hangs in the balance.",
                        "https://www.youtube.com/watch?v=wxN1T1uxQ2g",
                        new[] { "Action", "Adventure", "Sci-Fi" }
                    ),
                    (
                        Guid.Parse("10101010-1111-2222-3333-444444444449"),
                        "Arrival",
                        2016,
                        "Linguistics professor Louise Banks leads an elite team of investigators when gigantic spaceships touch down in 12 locations around the world. As nations teeter on the verge of global war, Banks and her crew must race against time to find a way to communicate with the extraterrestrial visitors.",
                        "https://www.youtube.com/watch?v=tFMo3UJ4B4g",
                        new[] { "Drama", "Mystery", "Sci-Fi" }
                    )
                };

                foreach (var item in initialMovies)
                {
                    var slug = GenerateSlug(item.Title, item.Year);

                    // Upsert movie & enrich description/trailerUrl if missing
                    await connection.ExecuteAsync("""
                        INSERT INTO movies (id, slug, title, yearofrelease, description, trailerurl)
                        VALUES (@Id, @Slug, @Title, @Year, @Description, @TrailerUrl)
                        ON CONFLICT (id) DO UPDATE
                        SET description = COALESCE(movies.description, EXCLUDED.description),
                            trailerurl = COALESCE(movies.trailerurl, EXCLUDED.trailerurl);
                        """, new { item.Id, Slug = slug, item.Title, item.Year, item.Description, item.TrailerUrl });

                    // Also update by slug in case movie was added manually with a different ID
                    await connection.ExecuteAsync("""
                        UPDATE movies
                        SET description = COALESCE(description, @Description),
                            trailerurl = COALESCE(trailerurl, @TrailerUrl)
                        WHERE slug = @Slug;
                        """, new { Slug = slug, item.Description, item.TrailerUrl });

                    var existingGenres = await connection.ExecuteScalarAsync<int>(
                        "SELECT COUNT(1) FROM genres WHERE movieId = @Id;", new { item.Id });

                    if (existingGenres == 0)
                    {
                        var movieExists = await connection.ExecuteScalarAsync<bool>(
                            "SELECT EXISTS(SELECT 1 FROM movies WHERE id = @Id);", new { item.Id });
                        if (movieExists)
                        {
                            var genreItems = item.Genres.Select(g => new { MovieId = item.Id, Name = g });
                            await connection.ExecuteAsync("""
                                INSERT INTO genres (movieId, name)
                                VALUES (@MovieId, @Name);
                                """, genreItems);
                        }
                    }
                }

                // 3. Seed Ratings
                var adminId = Guid.Parse("b9787b32-8558-4a34-9c43-095b1280c790");
                var criticId = Guid.Parse("7c9e6679-7425-40de-944b-e07fc1f90ae7");
                var memberId = Guid.Parse("e24a87b3-c19d-4882-9e3f-6781298c5ab2");

                var ratings = new (Guid UserId, Guid MovieId, int Rating)[]
                {
                    (criticId, Guid.Parse("11111111-1111-1111-1111-111111111111"), 5),
                    (memberId, Guid.Parse("11111111-1111-1111-1111-111111111111"), 5),
                    (criticId, Guid.Parse("22222222-2222-2222-2222-222222222222"), 5),
                    (memberId, Guid.Parse("22222222-2222-2222-2222-222222222222"), 4),
                    (criticId, Guid.Parse("33333333-3333-3333-3333-333333333333"), 5),
                    (memberId, Guid.Parse("33333333-3333-3333-3333-333333333333"), 5),
                    (adminId,  Guid.Parse("33333333-3333-3333-3333-333333333333"), 5),
                    (criticId, Guid.Parse("44444444-4444-4444-4444-444444444444"), 5),
                    (criticId, Guid.Parse("55555555-5555-5555-5555-555555555555"), 5),
                    (memberId, Guid.Parse("55555555-5555-5555-5555-555555555555"), 4),
                    (criticId, Guid.Parse("66666666-6666-6666-6666-666666666666"), 5),
                    (criticId, Guid.Parse("77777777-7777-7777-7777-777777777777"), 5),
                    (adminId,  Guid.Parse("77777777-7777-7777-7777-777777777777"), 5),
                    (criticId, Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 5),
                    (memberId, Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), 5),
                    (criticId, Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), 5),
                    (criticId, Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"), 5),
                    (criticId, Guid.Parse("10101010-1111-2222-3333-444444444441"), 5),
                    (memberId, Guid.Parse("10101010-1111-2222-3333-444444444441"), 5),
                    (adminId,  Guid.Parse("10101010-1111-2222-3333-444444444441"), 5),
                    (criticId, Guid.Parse("10101010-1111-2222-3333-444444444442"), 5),
                    (memberId, Guid.Parse("10101010-1111-2222-3333-444444444442"), 5),
                    (criticId, Guid.Parse("10101010-1111-2222-3333-444444444443"), 5),
                    (criticId, Guid.Parse("10101010-1111-2222-3333-444444444444"), 5),
                    (memberId, Guid.Parse("10101010-1111-2222-3333-444444444445"), 4),
                    (criticId, Guid.Parse("10101010-1111-2222-3333-444444444446"), 5),
                    (criticId, Guid.Parse("10101010-1111-2222-3333-444444444447"), 5),
                    (criticId, Guid.Parse("10101010-1111-2222-3333-444444444448"), 5),
                    (criticId, Guid.Parse("10101010-1111-2222-3333-444444444449"), 5)
                };

                foreach (var r in ratings)
                {
                    await connection.ExecuteAsync("""
                        INSERT INTO ratings (userId, movieId, rating)
                        SELECT @UserId, @MovieId, @Rating
                        WHERE EXISTS (SELECT 1 FROM movies WHERE id = @MovieId)
                        ON CONFLICT (userId, movieId) DO NOTHING;
                        """, new { r.UserId, r.MovieId, r.Rating });
                }
            }
        }

        private static string GenerateSlug(string title, int year)
        {
            var raw = $"{title} {year}".ToLowerInvariant();
            var cleaned = Regex.Replace(raw, @"[^a-z0-9\s-]", "");
            return Regex.Replace(cleaned, @"\s+", "-").Trim('-');
        }
    }
}
