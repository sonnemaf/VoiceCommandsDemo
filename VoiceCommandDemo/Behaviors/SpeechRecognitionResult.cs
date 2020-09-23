namespace VoiceCommandsDemo.Behaviors {
    public class SpeechRecognitionResult : ISpeechRecognitionResult {

        public SpeechRecognitionResult(string text, double rawConfidence) {
            Text = text;
            RawConfidence = rawConfidence;
        }

        public string Text { get; }
        public double RawConfidence { get; }
    }

}
