namespace VoiceCommandsDemo.Behaviors {
    public interface ISpeechRecognitionResult {
        string Text { get; }
        double RawConfidence { get; }
    }

}
