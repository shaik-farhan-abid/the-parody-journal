namespace theParodyJournal.Models.ML
{
    public class NewsRatingPrediction
    {
        public float Label { get; set; }  // Actual rating
        public float Score { get; set; }  // Predicted rating
    }
}
