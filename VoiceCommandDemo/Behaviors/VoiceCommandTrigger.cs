using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Globalization;
using Windows.Media.SpeechRecognition;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace VoiceCommandsDemo.Behaviors {

    // TODO: What can I say? => https://docs.microsoft.com/en-us/windows/uwp/design/input/speech-interactions

    public class VoiceCommandTrigger : Trigger {

        
        private static readonly Dictionary<string, List<VoiceCommandTrigger>> _triggers = new Dictionary<string, List<VoiceCommandTrigger>>(StringComparer.InvariantCultureIgnoreCase);
        private static ISpeechRecognizer _speechRecognizer;

        public static ISpeechRecognizer SpeechRecognizer {
            get {
                return _speechRecognizer;
            }
            set {
                if (value != _speechRecognizer) {
                    if (_speechRecognizer is object) {
                        _speechRecognizer.Recognized -= SpeechRecognizer_Recognized;
                    }
                    _speechRecognizer = value;
                    if (_speechRecognizer is object) {
                        _speechRecognizer.Recognized += SpeechRecognizer_Recognized;
                    }
                }
            }
        }

        private static void SpeechRecognizer_Recognized(ISpeechRecognizer sender, RecognizedEventArgs e) {
            Debug.WriteLine(e.Result.Text);
            if (_triggers.TryGetValue(e.Result.Text, out var list)) {
                foreach (var trigger in list) {
                    _ = trigger.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                        Interaction.ExecuteActions(trigger.AssociatedObject, trigger.Actions, e);
                    });
                }
            }
        }

        public string Text {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(VoiceCommandTrigger), new PropertyMetadata(default(string), OnTextPropertyChanged));

        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is VoiceCommandTrigger source) {
                var newValue = (string)e.NewValue;
                var oldValue = (string)e.OldValue;
                if (!string.IsNullOrEmpty(oldValue)) source.Remove(oldValue);
                if (!string.IsNullOrEmpty(newValue)) source.Add(newValue);
            }
        }


        protected override void OnDetaching() {
            Remove(Text);
        }

        private void Add(string text) {
            foreach (var item in text.Split('|')) {
                if (_triggers.TryGetValue(item, out var list)) {
                    list.Add(this);
                } else {
                    list = new List<VoiceCommandTrigger>() {
                        this
                    };
                    _triggers[item] = list;
                }
            }
        }

        private void Remove(string text) {
            foreach (var item in text.Split('|')) {
                if (_triggers.TryGetValue(item, out var list)) {
                    list.Remove(this);
                    if (list.Count == 0) _triggers.Remove(item);
                }
            }
        }
    }
}
