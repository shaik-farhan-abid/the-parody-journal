using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ML;
using theParodyJournal.Models;
using NewsAPI;
using NewsAPI.Models;
using NewsAPI.Constants;
using theParodyJournal.Models.ML;
using theParodyJournal.Services.ML;
using Newtonsoft.Json;

namespace theParodyJournal.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly NewsApiClient _newsApiClient;
        private readonly IConfiguration _configuration;
        private readonly MLContext _mlContext;
        private ITransformer _mlModel;
        private Dictionary<string, string> _articleCategories = new Dictionary<string, string>();

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _newsApiClient = new NewsApiClient(configuration["NewsAPI:ApiKey"]);
            _mlContext = new MLContext();

            // Load the trained ML model
            LoadMLModel();
        }
        public IActionResult TrackArticleClick(int userId, string articleUrl)
        {
            var storedCategories = HttpContext.Session.GetString("ArticleCategories");

            if (!string.IsNullOrEmpty(storedCategories))
            {
                _articleCategories = JsonConvert.DeserializeObject<Dictionary<string, string>>(storedCategories);
            }
            var ratingService = new NewsRatingService();
            ratingService.UpdateArticleScore(userId, articleUrl,_articleCategories);

            return Redirect(articleUrl); // Redirect user to the actual news article
        }
        [HttpPost]
        public IActionResult DecreaseArticleScore([FromBody] ArticleRequest request)
        {
            var storedCategories = HttpContext.Session.GetString("ArticleCategories");

            if (!string.IsNullOrEmpty(storedCategories))
            {
                _articleCategories = JsonConvert.DeserializeObject<Dictionary<string, string>>(storedCategories);
            }

            var ratingService = new NewsRatingService();
            ratingService.ResetArticleScore(request.UserId, request.ArticleUrl, _articleCategories);

            return Ok(new { message = "Article score reset successfully." });
        }

        public class ArticleRequest
        {
            public int UserId { get; set; }
            public string ArticleUrl { get; set; }
        }



        public async Task<IActionResult> Index(string category = "Home")
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            ViewBag.UserId = userId;
            try
            {
                var allArticles = await GetAllNewsArticles(); // Fetch all articles

                List<Article> displayedArticles;

                if (category == "Home")
                {
                    // Ensure we have articles before applying recommendation
                    displayedArticles = allArticles.Any() ? GetRecommendedNews(allArticles) : new List<Article>();
                }
                else
                {
                    // Filter articles based on category stored in _articleCategories
                    displayedArticles = allArticles
                        .Where(a => _articleCategories.ContainsKey(a.Url) &&
                                    _articleCategories[a.Url].Equals(category, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                ViewBag.Category = category; // Pass category name to the view
                return View(displayedArticles);
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
                    foreach (var art in topNews.Articles)
                    {
                        if (!string.IsNullOrEmpty(art.Url)) // Ensure URL is not null
                        {
                            _articleCategories[art.Url] = "GOOD NEWS"; // Store the category
                        }
                    }
                }

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
                    foreach (var art in businessNews.Articles)
                    {
                        if (!string.IsNullOrEmpty(art.Url)) // Ensure URL is not null
                        {
                            _articleCategories[art.Url] = "BUSINESS"; // Store the category
                        }
                    }


                }

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
                    foreach (var art in techNews.Articles)
                    {
                        if (!string.IsNullOrEmpty(art.Url)) // Ensure URL is not null
                        {
                            _articleCategories[art.Url] = "TECH"; // Store the category
                        }
                    }
                }

                var sportNews = await Task.Run(() => _newsApiClient.GetTopHeadlines(new TopHeadlinesRequest
                {
                    Country = Countries.US,
                    Category = Categories.Sports,
                    PageSize = 6,
                    Language = Languages.EN
                }));

                if (sportNews.Status == Statuses.Ok)
                {
                    articles.AddRange(sportNews.Articles);
                }

                var sciNews = await Task.Run(() => _newsApiClient.GetTopHeadlines(new TopHeadlinesRequest
                {
                    Country = Countries.US,
                    Category = Categories.Science,
                    PageSize = 6,
                    Language = Languages.EN
                }));

                if (sciNews.Status == Statuses.Ok)
                {
                    articles.AddRange(sciNews.Articles);
                    foreach (var art in sciNews.Articles)
                    {
                        if (!string.IsNullOrEmpty(art.Url)) // Ensure URL is not null
                        {
                            _articleCategories[art.Url] = "SCIENCE"; // Store the category
                        }
                    }
                }

                var healthNews = await Task.Run(() => _newsApiClient.GetTopHeadlines(new TopHeadlinesRequest
                {
                    Country = Countries.US,
                    Category = Categories.Health,
                    PageSize = 6,
                    Language = Languages.EN
                }));

                if (healthNews.Status == Statuses.Ok)
                {
                    articles.AddRange(healthNews.Articles);
                    foreach (var art in healthNews.Articles)
                    {
                        if (!string.IsNullOrEmpty(art.Url)) // Ensure URL is not null
                        {
                            _articleCategories[art.Url] = "HEALTHY LIVING"; // Store the category
                        }
                    }
                }

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
                    foreach (var art in generalNews.Articles)
                    {
                        if (!string.IsNullOrEmpty(art.Url)) // Ensure URL is not null
                        {
                            _articleCategories[art.Url] = "GOOD NEWS"; // Store the category
                        }
                    }
                }

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
                    foreach (var art in entertainmentNews.Articles)
                    {
                        if (!string.IsNullOrEmpty(art.Url)) // Ensure URL is not null
                        {
                            _articleCategories[art.Url] = "ENTERTAINMENT"; // Store the category
                        }
                    }
                }

                articles = articles
                    .Where(a => !string.IsNullOrEmpty(a.Url))
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
                //foreach (var kvp in _articleCategories)
                //{
                //    _logger.LogInformation("Article URL: {Key}, Category: {Value}", kvp.Key, kvp.Value);
                //}
                //_logger.LogInformation("Total entries in _articleCategories: {Count}", _articleCategories.Count);


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllNewsArticles");
                return new List<Article>();
            }
            HttpContext.Session.SetString("ArticleCategories", JsonConvert.SerializeObject(_articleCategories));

            return articles;
        }

        // ✅ New method: Load the ML model
        private void LoadMLModel()
        {
            try
            {
                string modelPath = Path.Combine(Directory.GetCurrentDirectory(), "MLModel.zip");

                if (System.IO.File.Exists(modelPath))
                {
                    _mlModel = _mlContext.Model.Load(modelPath, out _);
                    _logger.LogInformation(" ML Model loaded successfully from file.");
                }
                else
                {
                    _logger.LogWarning(" ML Model file not found. Training a new model...");
                    var recommendationService = new NewsRecommendationService();
                    _mlModel = recommendationService.TrainModel(); // This will train and save the model
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " Failed to load the ML model.");
            }
        }


        // Get recommended news using ML
        private List<Article> GetRecommendedNews(List<Article> articles)
        {

            var predictionEngine = _mlContext.Model.CreatePredictionEngine<NewsRating, NewsRatingPrediction>(_mlModel);
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0; // Use HttpContext instead



            var recommendedArticles = articles

.Select(a =>
{
    // Try to get the category safely
    if (!_articleCategories.TryGetValue(a.Url, out string category))
    {
        category = "UNKNOWN"; // Default category if not found
    }

    return new
    {
        Article = a,
        Prediction = predictionEngine.Predict(new NewsRating { UserId = userId, ArticleTag = category })
    };
})
                .Where(p => p.Prediction.Score >= 4) // Threshold rating: 3.5
                .Select(p => p.Article)
                .ToList();

            _logger.LogInformation($"Total recommended articles: {recommendedArticles.Count}");
            return recommendedArticles;
        }

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
