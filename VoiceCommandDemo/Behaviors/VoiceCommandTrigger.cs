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

        private static SpeechRecognizer _sr;
        private static readonly Dictionary<string, List<VoiceCommandTrigger>> _triggers = new Dictionary<string, List<VoiceCommandTrigger>>(StringComparer.InvariantCultureIgnoreCase);
        private static bool _initialized;

        static VoiceCommandTrigger() {
            Task.Run(async () => {

                foreach (var item in Windows.System.UserProfile.GlobalizationPreferences.Languages) {
                    var language = new Language(item);
                    try {
                        _sr = new SpeechRecognizer(language);
                        break;
                    } catch (Exception ex) {
                        Debug.WriteLine(ex);
                    }
                }
                if (_sr is null) _sr = new SpeechRecognizer();
                Debug.WriteLine($"SpeechRecognizer Language: { _sr.CurrentLanguage.DisplayName}");

                _sr.ContinuousRecognitionSession.AutoStopSilenceTimeout = TimeSpan.MaxValue;
                await _sr.CompileConstraintsAsync();
                _sr.ContinuousRecognitionSession.ResultGenerated += ContinuousRecognitionSession_ResultGenerated;
                await _sr.ContinuousRecognitionSession.StartAsync();

                _initialized = true;
                Debug.WriteLine("VoiceCommandTrigger Initialized");
            });
        }

        internal static void Activate(WindowActivatedEventArgs e) {
            if (_initialized && e.WindowActivationState == CoreWindowActivationState.CodeActivated && _sr.State == SpeechRecognizerState.Idle) _ = _sr.ContinuousRecognitionSession.StartAsync();
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



        private static void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args) {
            Debug.WriteLine(args.Result.Text);
            if (_triggers.TryGetValue(args.Result.Text, out var list)) {
                foreach (var trigger in list) {
                    _ = trigger.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                        Interaction.ExecuteActions(trigger.AssociatedObject, trigger.Actions, args);
                    });
                }
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
