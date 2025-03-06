using Microsoft.ML;
using Microsoft.ML.Trainers;
using theParodyJournal.Models.ML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace theParodyJournal.Services.ML
{
    public class NewsRecommendationService
    {
        private readonly MLContext _mlContext;
        private readonly ITransformer _model;

        public NewsRecommendationService()
        {
            _mlContext = new MLContext();
            _model = TrainModel();
        }

        private Dictionary<string, int> _articleTagMapping = new Dictionary<string, int>();

        private int GetArticleTagId(string articleTag)
        {
            if (!_articleTagMapping.ContainsKey(articleTag))
            {
                _articleTagMapping[articleTag] = _articleTagMapping.Count + 1;
            }
            return _articleTagMapping[articleTag];
        }

        public ITransformer TrainModel()
        {
            string dataPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "news_ratings.csv");

            // Load dataset from CSV
            var dataView = _mlContext.Data.LoadFromTextFile<NewsRating>(
                path: dataPath,
                separatorChar: ',',
                hasHeader: true
            );

            //  FIXED COLUMN NAMES
            var pipeline = _mlContext.Transforms.Conversion
     .MapValueToKey("UserId", nameof(NewsRating.UserId))
     .Append(_mlContext.Transforms.Conversion.MapValueToKey("ArticleTag", nameof(NewsRating.ArticleTag)))
     .Append(_mlContext.Transforms.CopyColumns("Score", nameof(NewsRating.Score))) // Use the correct column for label
     .Append(_mlContext.Recommendation().Trainers.MatrixFactorization(new MatrixFactorizationTrainer.Options
     {
         LabelColumnName = nameof(NewsRating.Score),
         MatrixColumnIndexColumnName = nameof(NewsRating.UserId),
         MatrixRowIndexColumnName = nameof(NewsRating.ArticleTag),
         NumberOfIterations = 20,
         ApproximationRank = 100
     }));


            Console.WriteLine("Training ML Model... WAIT BRO");

            var model = pipeline.Fit(dataView);
            Console.WriteLine(" Model Training Complete.");

            // ✅ Ensure model is saved correctly
            string modelPath = Path.Combine(Directory.GetCurrentDirectory(), "MLModel.zip");
            _mlContext.Model.Save(model, dataView.Schema, modelPath);
            Console.WriteLine($" Model saved at {modelPath}");

            return model;
        }
    }
}




        //public float PredictRating(int userId, string articleTag)
        //{
        //    var predictionEngine = _mlContext.Model.CreatePredictionEngine<NewsRating, NewsRatingPrediction>(_model);
        //    var prediction = predictionEngine.Predict(new NewsRating { UserId = userId, ArticleTag = articleTag });
        //    return prediction.Score;
        //}

        //public List<NewsArticle> GetRecommendedArticles(List<NewsArticle> allArticles, int userId, float threshold = 3.5f)
        //{
        //    return allArticles
        //        .Where(article => PredictRating(userId, article.ArticleTag) >= threshold) // Updated to use ArticleTag
        //        .ToList();
        //}

