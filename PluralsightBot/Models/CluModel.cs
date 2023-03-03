namespace PluralsightBot.Models
{

    public class CluModel
    {
        public string Kind { get; set; }
        public Result Result { get; set; }
    }

    public class Result
    {
        public string Query { get; set; }
        public Prediction Prediction { get; set; }
    }

    public class Prediction
    {
        public string TopIntent { get; set; }
        public string ProjectKind { get; set; }
        public Intent[] Intents { get; set; }
        public Entity[] Entities { get; set; }
    }

    public class Intent
    {
        public string Category { get; set; }
        public float ConfidenceScore { get; set; }
    }

    public class Entity
    {
        public string Category { get; set; }
        public string Text { get; set; }
        public int Offset { get; set; }
        public int Length { get; set; }
        public int ConfidenceScore { get; set; }
        public Extrainformation[] ExtraInformation { get; set; }
    }

    public class Extrainformation
    {
        public string ExtraInformationKind { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }

}
