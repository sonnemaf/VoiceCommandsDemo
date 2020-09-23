using System;

namespace VoiceCommandsDemo.Behaviors {
    public class RecognizedEventArgs : EventArgs {

        public ISpeechRecognitionResult Result { get; }

        public RecognizedEventArgs(ISpeechRecognitionResult propertyParameter) {
            this.Result = propertyParameter;
        }

    }

    public delegate void RecognizedEventHandler(ISpeechRecognizer sender, RecognizedEventArgs e);

}
