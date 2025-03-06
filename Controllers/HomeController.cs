using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using theParodyJournal.Models;
using NewsAPI;
using NewsAPI.Models;
using NewsAPI.Constants;

namespace theParodyJournal.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly NewsApiClient _newsApiClient;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            _newsApiClient = new NewsApiClient(configuration["NewsAPI:ApiKey"]);
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var articles = await GetAllNewsArticles();

                foreach (var article in articles.Take(5))
                {
                    _logger.LogInformation($"Article URL: {article.Url}");
                }
                return View(articles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching news in Index action");
                return View(new List<Article>());
            }
        }

        private async Task<List<Article>> GetAllNewsArticles()
        {
            var articles = new List<Article>();

            try
            {
                // Top headline er jnno
                var topNews = await Task.Run(() => _newsApiClient.GetTopHeadlines(new TopHeadlinesRequest
                {
                    Country = Countries.US,
                    PageSize = 4,
                    Language = Languages.EN
                }));

                if (topNews.Status == Statuses.Ok)
                {
                    _logger.LogInformation($"Fetched {topNews.Articles.Count} top headlines");
                    articles.AddRange(topNews.Articles);
                }

                // Business news er jnno
                var businessNews = await Task.Run(() => _newsApiClient.GetTopHeadlines(new TopHeadlinesRequest
                {
                    Country = Countries.US,
                    Category = Categories.Business,
                    PageSize = 6,
                    Language = Languages.EN
                }));

                if (businessNews.Status == Statuses.Ok)
                {
                    articles.AddRange(businessNews.Articles);
                }

                // technology news er jnno
                var techNews = await Task.Run(() => _newsApiClient.GetTopHeadlines(new TopHeadlinesRequest
                {
                    Country = Countries.US,
                    Category = Categories.Technology,
                    PageSize = 6,
                    Language = Languages.EN
                }));

                if (techNews.Status == Statuses.Ok)
                {
                    articles.AddRange(techNews.Articles);
                }

                // general news sob
                var generalNews = await Task.Run(() => _newsApiClient.GetTopHeadlines(new TopHeadlinesRequest
                {
                    Country = Countries.US,
                    Category = Categories.Business,
                    PageSize = 10,
                    Language = Languages.EN
                }));

                if (generalNews.Status == Statuses.Ok)
                {
                    articles.AddRange(generalNews.Articles);
                }

                // entertainment news section eta
                var entertainmentNews = await Task.Run(() => _newsApiClient.GetTopHeadlines(new TopHeadlinesRequest
                {
                    Country = Countries.US,
                    Category = Categories.Entertainment,
                    PageSize = 10,
                    Language = Languages.EN
                }));

                if (entertainmentNews.Status == Statuses.Ok)
                {
                    articles.AddRange(entertainmentNews.Articles);
                }

                // Process and validate articles
                articles = articles
                    .Where(a => !string.IsNullOrEmpty(a.Url)) // valid url check krtsi ekhane
                    .DistinctBy(a => a.Url)
                    .Select(a => new Article
                    {
                        Title = a.Title ?? "No Title Available",
                        Description = a.Description ?? "No Description Available",
                        Url = a.Url ?? "#",
                        UrlToImage = a.UrlToImage,
                        PublishedAt = a.PublishedAt,
                        Author = a.Author,
                        Content = a.Content
                    })
                    .ToList();

                _logger.LogInformation($"Total articles fetched: {articles.Count}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllNewsArticles");
                // error handeling korar jnno
                return new List<Article>();
            }

            return articles;
        }

        // Method to get news by category
        [HttpGet]
        public async Task<IActionResult> GetNewsByCategory(string category)
        {
            try
            {
                var selectedCategory = GetCategory(category);
                var response = await Task.Run(() => _newsApiClient.GetTopHeadlines(new TopHeadlinesRequest
                {
                    Country = Countries.US,
                    Category = selectedCategory,
                    PageSize = 10,
                    Language = Languages.EN
                }));

                if (response.Status == Statuses.Ok)
                {
                    return Json(new { success = true, data = response.Articles });
                }

                return Json(new { success = false, error = $"Failed to fetch {category} news" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching news for category {category}");
                return Json(new { success = false, error = "Failed to fetch category news" });
            }
        }

        private Categories GetCategory(string category)
        {
            return category?.ToLower() switch
            {
                "business" => Categories.Business,
                "entertainment" => Categories.Entertainment,
                "health" => Categories.Health,
                "science" => Categories.Science,
                "sports" => Categories.Sports,
                "technology" => Categories.Technology,
                _ => Categories.Business
            };
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}