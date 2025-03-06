using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using System.Formats.Asn1;
using System.Diagnostics.Eventing.Reader;

public class NewsRatingService
{
    private readonly string filePath = Path.Combine("Data", "news_ratings.csv");
    public void ResetArticleScore(int userId, string articleUrl, Dictionary<string, string> articleCategories)
    {
        // Ensure the Data folder exists
        Directory.CreateDirectory("Data");

        if (!articleCategories.TryGetValue(articleUrl, out string? articleCategory))
        {
            Console.WriteLine($"[WARNING] Category not found for URL: {articleUrl}");
            return;
        }

        List<NewsRating> ratings = new List<NewsRating>();

        try
        {
            // Read existing ratings from CSV if the file exists
            if (File.Exists(filePath))
            {
                using (var reader = new StreamReader(filePath))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                {
                    ratings = csv.GetRecords<NewsRating>().ToList();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to read CSV file: {ex.Message}");
            return;
        }

        // Find the existing rating entry for the user and category
        var existingRating = ratings.FirstOrDefault(r => r.UserId == userId && r.ArticleTag == articleCategory);

        if (existingRating != null)
        {
            existingRating.Score = 0; // ✅ Only updating the Score column
        }



        try
        {
            // Write updated ratings back to CSV
            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                csv.WriteRecords(ratings);
            }

            Console.WriteLine($"[INFO] Updated Score - UserID: {userId}, Category: {articleCategory}, Score: {existingRating?.Score ?? 1}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to write to CSV file: {ex.Message}");
            Console.WriteLine($"[INFO] Updated Score - UserID: {userId}, Category: {articleCategory}, Score: {existingRating?.Score ?? 1}");

        }
    }

    public void UpdateArticleScore(int userId, string articleUrl, Dictionary<string, string> articleCategories)
    {
        // Ensure the Data folder exists
        Directory.CreateDirectory("Data");

        if (!articleCategories.TryGetValue(articleUrl, out string? articleCategory))
        {
            Console.WriteLine($"[WARNING] Category not found for URL: {articleUrl}");
            return;
        }

        List<NewsRating> ratings = new List<NewsRating>();

        try
        {
            // Read existing ratings from CSV if the file exists
            if (File.Exists(filePath))
            {
                using (var reader = new StreamReader(filePath))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                {
                    ratings = csv.GetRecords<NewsRating>().ToList();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to read CSV file: {ex.Message}");
            return;
        }

        // Find the existing rating entry for the user and category
        var existingRating = ratings.FirstOrDefault(r => r.UserId == userId && r.ArticleTag == articleCategory);

        if (existingRating != null)
        {
            existingRating.Score += 1; // ✅ Only updating the Score column
        }
        


        try
        {
                // Write updated ratings back to CSV
                using (var writer = new StreamWriter(filePath))
                using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
                {
                    csv.WriteRecords(ratings);
                }

                Console.WriteLine($"[INFO] Updated Score - UserID: {userId}, Category: {articleCategory}, Score: {existingRating?.Score ?? 1}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to write to CSV file: {ex.Message}");
            Console.WriteLine($"[INFO] Updated Score - UserID: {userId}, Category: {articleCategory}, Score: {existingRating?.Score ?? 1}");

        }
    }
}
public class NewsRating
{
    [ColumnName("UserId"),LoadColumn(0)]
    public float UserId { get; set; }

    [LoadColumn(1)]
    public string ArticleTag { get; set; }

    [LoadColumn(2)]
    public float Score { get; set; }
}
