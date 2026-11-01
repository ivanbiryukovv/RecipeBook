using LiteDB;
using RecipeBook.Models;

namespace RecipeBook.Data;

public class LiteDbRecipeRepository : IRecipeRepository
{
    private readonly LiteDatabase _database;
    private readonly ILiteCollection<Recipe> _collection;
    private bool _disposed;

    public LiteDbRecipeRepository(string? databasePath = null)
    {
        var dbPath = string.IsNullOrWhiteSpace(databasePath)
            ? RecipeConstants.DatabaseFileName
            : databasePath;

        _database = new LiteDatabase(dbPath);
        _collection = _database.GetCollection<Recipe>(RecipeConstants.RecipeCollectionName);

        EnsureIndicesCreated();
        SeedInitialDataIfEmpty();
    }

    private void EnsureIndicesCreated()
    {
        _collection.EnsureIndex(r => r.Title);
        _collection.EnsureIndex(r => r.Category);
        _collection.EnsureIndex(r => r.IsFavorite);
        _collection.EnsureIndex(r => r.CookingTimeMinutes);
        _collection.EnsureIndex(r => r.Difficulty);
    }

    public IReadOnlyList<Recipe> GetAllRecipes()
    {
        ThrowIfDisposed();
        return _collection.FindAll().OrderBy(r => r.Id).ToList().AsReadOnly();
    }

    public Recipe? GetRecipeById(int id)
    {
        ThrowIfDisposed();
        if (id <= 0) return null;
        return _collection.FindById(id);
    }

    public int AddRecipe(Recipe recipe)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(recipe);
        var newId = _collection.Insert(recipe);
        return newId.AsInt32;
    }

    public bool UpdateRecipe(Recipe recipe)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(recipe);
        return _collection.Update(recipe);
    }

    public bool DeleteRecipe(int id)
    {
        ThrowIfDisposed();
        if (id <= 0) return false;
        return _collection.Delete(id);
    }

    public IReadOnlyList<Recipe> SearchRecipes(string? searchTitle, string? category = null, bool? onlyFavorites = null)
    {
        return SearchRecipes(new RecipeFilterCriteria
        {
            SearchText = searchTitle,
            Category = category,
            OnlyFavorites = onlyFavorites ?? false
        });
    }

    public IReadOnlyList<Recipe> SearchRecipes(RecipeFilterCriteria criteria)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(criteria);

        var query = _collection.Query();

        if (!string.IsNullOrWhiteSpace(criteria.SearchText))
        {
            var clean = criteria.SearchText.Trim();
            query = query.Where(r => r.Title.Contains(clean, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(criteria.Category) && criteria.Category != RecipeConstants.CategoryAll)
        {
            query = query.Where(r => r.Category == criteria.Category);
        }

        if (criteria.OnlyFavorites)
        {
            query = query.Where(r => r.IsFavorite);
        }

        if (criteria.MinCookingTime.HasValue)
        {
            query = query.Where(r => r.CookingTimeMinutes >= criteria.MinCookingTime.Value);
        }

        if (criteria.MaxCookingTime.HasValue)
        {
            query = query.Where(r => r.CookingTimeMinutes <= criteria.MaxCookingTime.Value);
        }

        if (criteria.Difficulties.Count > 0)
        {
            query = query.Where(r => criteria.Difficulties.Contains(r.Difficulty));
        }

        if (criteria.Servings.HasValue && criteria.Servings.Value > 0)
        {
            query = query.Where(r => r.Servings == criteria.Servings.Value);
        }

        return query.OrderBy(r => r.Id).ToList().AsReadOnly();
    }

    public int GetCountByCategory(string category)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrWhiteSpace(category);
        return _collection.Count(r => r.Category == category);
    }

    public int GetFavoritesCount()
    {
        ThrowIfDisposed();
        return _collection.Count(r => r.IsFavorite);
    }

    public int GetTotalCount()
    {
        ThrowIfDisposed();
        return _collection.Count();
    }

    private void SeedInitialDataIfEmpty()
    {
        if (_collection.Count() > 0) return;

        var sampleRecipes = new List<Recipe>
        {
            new()
            {
                Id = 101,
                Title = "Паста Карбонара со сливками",
                Category = RecipeConstants.CategoryMainDishes,
                Description = "Традиционное блюдо итальянской кухни со сливочным соусом, обжаренным копченым беконом и сыром пармезан. Готовится быстро и просто.",
                CookingTimeMinutes = 25,
                Servings = 4,
                Difficulty = 2,
                IsFavorite = true,
                ImagePath = "carbonara.jpg",
                Ingredients =
                [
                    new("Спагетти из твердых сортов", 400, "грамм"),
                    new("Бекон копченый", 150, "грамм"),
                    new("Яичные желтки", 3, "штук"),
                    new("Сыр Пармезан тертый", 80, "грамм"),
                    new("Сливки 20%", 100, "миллилитров")
                ],
                Instructions =
                [
                    "Отварить спагетти в кипящей подсоленной воде до состояния al dente (около 8 минут).",
                    "Нарезать бекон тонкими брусочками и обжарить на сковороде на среднем огне до легкой золотистой корочки.",
                    "В миске взбить яичные желтки со сливками, добавить тертый сыр пармезан и свежемолотый перец.",
                    "Снять сковороду с огня, добавить горячую пасту к бекону, влить соус и непрерывно перемешивать до загустения."
                ]
            },
            new()
            {
                Id = 102,
                Title = "Борщ украинский с пампушками",
                Category = RecipeConstants.CategorySoups,
                Description = "Наваристый традиционный суп со свёклой, говядиной и свежей капустой. Подаётся с чесночными пампушками и сметаной.",
                CookingTimeMinutes = 90,
                Servings = 6,
                Difficulty = 3,
                IsFavorite = true,
                ImagePath = "borscht.jpg",
                Ingredients =
                [
                    new("Говядина на кости", 500, "грамм"),
                    new("Свёкла средняя", 2, "штук"),
                    new("Капуста белокочанная", 300, "грамм"),
                    new("Картофель", 4, "штук"),
                    new("Морковь", 1, "штук")
                ],
                Instructions =
                [
                    "Сварить насыщенный мясной бульон из говядины на слабом огне в течение 1.5 часов.",
                    "Свёклу натереть на терке и тушить с томатной пастой и каплей уксуса для сохранения яркого цвета.",
                    "Добавить в бульон нарезанный картофель и нашинкованную капусту, варить 15 минут.",
                    "Ввести свёклу и пассерованные лук с морковью, варить ещё 10 минут, дать настояться."
                ]
            },
            new()
            {
                Id = 103,
                Title = "Салат «Цезарь» с курицей",
                Category = RecipeConstants.CategorySalads,
                Description = "Популярный хрустящий салат с нежным куриным филе, сухариками, листьями Романо и фирменным соусом.",
                CookingTimeMinutes = 30,
                Servings = 2,
                Difficulty = 1,
                IsFavorite = false,
                ImagePath = "caesar.jpg",
                Ingredients =
                [
                    new("Куриное филе", 300, "грамм"),
                    new("Салат Романо", 1, "кочан"),
                    new("Багет для сухариков", 100, "грамм"),
                    new("Помидоры черри", 8, "штук"),
                    new("Соус Цезарь", 50, "грамм")
                ],
                Instructions =
                [
                    "Куриное филе нарезать, посолить, поперчить и обжарить на сковороде-гриль до готовности.",
                    "Багет нарезать кубиками и подсушить в духовке с оливковым маслом и чесноком.",
                    "Листья салата промыть, обсушить и порвать руками в глубокую салатницу.",
                    "Выложить теплую курицу, сухарики, черри и полить соусом перед подачей."
                ]
            },
            new()
            {
                Id = 104,
                Title = "Классический тирамису",
                Category = RecipeConstants.CategoryDesserts,
                Description = "Воздушный итальянский десерт на основе сыра маскарпоне, печенья савоярди и крепкого эспрессо.",
                CookingTimeMinutes = 40,
                Servings = 6,
                Difficulty = 3,
                IsFavorite = true,
                ImagePath = "tiramisu.jpg",
                Ingredients =
                [
                    new("Сыр Маскарпоне", 500, "грамм"),
                    new("Печенье Савоярди", 250, "грамм"),
                    new("Яйца куриные", 4, "штук"),
                    new("Кофе эспрессо крепкий", 200, "миллилитров"),
                    new("Какао-порошок", 30, "грамм")
                ],
                Instructions =
                [
                    "Желтки взбить с сахаром до белой пены, ввести маскарпоне и аккуратно перемешать.",
                    "Отдельно взбить белки в крепкую пену и ввести в крем.",
                    "Палочки савоярди быстро окунать в остывший кофе и выкладывать слоями в форму.",
                    "Покрыть кремом и убрать в холодильник минимум на 4 часа, посыпать какао."
                ]
            },
            new()
            {
                Id = 105,
                Title = "Пышная яблочная шарлотка",
                Category = RecipeConstants.CategoryBaking,
                Description = "Ароматный домашний пирог с кисло-сладкими яблоками и нежным бисквитным тестом.",
                CookingTimeMinutes = 50,
                Servings = 8,
                Difficulty = 2,
                IsFavorite = false,
                ImagePath = "sharlotka.jpg",
                Ingredients =
                [
                    new("Яблоки кислые", 4, "штук"),
                    new("Мука пшеничная", 160, "грамм"),
                    new("Сахар", 160, "грамм"),
                    new("Яйца куриные", 4, "штук"),
                    new("Разрыхлитель", 1, "чайная ложка")
                ],
                Instructions =
                [
                    "Яблоки очистить от сердцевины и нарезать тонкими ломтиками.",
                    "Взбить яйца с сахаром миксером не менее 7 минут до пышной светлой массы.",
                    "Просеять муку с разрыхлителем и аккуратно перемешать лопаткой снизу вверх.",
                    "Выложить яблоки в смазанную форму, залить тестом и выпекать 35 минут при 180°C."
                ]
            },
            new()
            {
                Id = 106,
                Title = "Стейк Рибай прожарки Medium",
                Category = RecipeConstants.CategoryMainDishes,
                Description = "Сочный мраморный говяжий стейк со сливочным маслом, чесноком и розмарином.",
                CookingTimeMinutes = 20,
                Servings = 2,
                Difficulty = 4,
                IsFavorite = true,
                ImagePath = "ribeye.jpg",
                Ingredients =
                [
                    new("Стейк Рибай (мраморная говядина)", 600, "грамм"),
                    new("Масло сливочное", 40, "грамм"),
                    new("Чеснок", 3, "зубчика"),
                    new("Розмарин свежий", 2, "веточки"),
                    new("Морская соль и перец", 1, "щепотка")
                ],
                Instructions =
                [
                    "Мясо достать из холодильника за 30 минут до жарки, чтобы оно согрелось до комнатной температуры.",
                    "Раскалить чугунную сковороду, смазать стейк растительным маслом и обжаривать по 2.5 минуты с каждой стороны.",
                    "Добавить в сковороду сливочное масло, раздавленный чеснок и розмарин, поливать мясо растопленным маслом.",
                    "Переложить стейк на теплую доску, накрыть фольгой и дать отдохнуть 5 минут перед нарезкой."
                ]
            }
        };

        _collection.InsertBulk(sampleRecipes);
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(LiteDbRecipeRepository));
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _database.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
