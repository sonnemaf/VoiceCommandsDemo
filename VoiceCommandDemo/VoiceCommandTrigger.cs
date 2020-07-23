using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Globalization;
using Windows.Media.SpeechRecognition;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace VoiceCommandsDemo {

    public class VoiceCommandTrigger : Trigger {

        private static SpeechRecognizer _sr;
        private static readonly Dictionary<string, VoiceCommandTrigger> _triggers = new Dictionary<string, VoiceCommandTrigger>(StringComparer.InvariantCultureIgnoreCase);

        static VoiceCommandTrigger() {
            Task.Run(async () => {

                foreach (var item in Windows.System.UserProfile.GlobalizationPreferences.Languages) {
                    var language = new Windows.Globalization.Language(item);
                    try {
                        _sr = new SpeechRecognizer(language);
                        break;
                    } catch (Exception ex) {
                        Debug.WriteLine(ex);
                    }
                }
                if (_sr is null) {
                    _sr = new SpeechRecognizer();
                }
                Debug.WriteLine($"SpeechRecognizer Language: { _sr.CurrentLanguage.DisplayName}");

                _sr.ContinuousRecognitionSession.AutoStopSilenceTimeout = TimeSpan.MaxValue;
                await _sr.CompileConstraintsAsync();
                _sr.ContinuousRecognitionSession.ResultGenerated += ContinuousRecognitionSession_ResultGenerated;
                await _sr.ContinuousRecognitionSession.StartAsync();
            });
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
                if (!string.IsNullOrEmpty(oldValue)) {
                    _triggers.Remove(oldValue);
                }
                if (!string.IsNullOrEmpty(newValue)) {
                    _triggers[newValue] = source;
                }
            }
        }

        private static void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args) {
            Debug.WriteLine(args.Result.Text);
            if (_triggers.TryGetValue(args.Result.Text, out var trigger)) {
                _ = trigger.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                    Interaction.ExecuteActions(trigger.AssociatedObject, trigger.Actions, args);
                });
            }
        }

        protected override void OnAttached() {
            base.OnAttached();
            _triggers[this.Text] = this;
        }

        protected override void OnDetaching() {
            if (_triggers[this.Text] == this) {
                _triggers.Remove(this.Text);
            }
        }
    }
}
