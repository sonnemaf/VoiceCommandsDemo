using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoiceCommandsDemo.Behaviors {

    public interface ISpeechRecognizer {
        event RecognizedEventHandler Recognized;
    }

}
