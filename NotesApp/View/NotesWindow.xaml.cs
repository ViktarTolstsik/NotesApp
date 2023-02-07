using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Speech.Recognition;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NotesApp.View
{
    /// <summary>
    /// Логика взаимодействия для NotesWindow.xaml
    /// </summary>
    public partial class NotesWindow : Window
    {
        SpeechRecognitionEngine recognizer;
        public NotesWindow()
        {
            InitializeComponent();

            var currentCulture = CultureInfo.GetCultureInfo("en-US");
            recognizer = new SpeechRecognitionEngine(currentCulture);

            GrammarBuilder builder = new GrammarBuilder();
            builder.Culture = currentCulture;
            builder.AppendDictation();
            Grammar grammar = new Grammar(builder);

            recognizer.LoadGrammar(grammar);
            recognizer.SetInputToDefaultAudioDevice();
            recognizer.SpeechRecognized += Recognizer_SpeechRecognized;
        }

        private void Recognizer_SpeechRecognized(object? sender, SpeechRecognizedEventArgs e)
        {
            string recognizedText = e.Result.Text;

            richTextBoxContent.Document.Blocks.Add(new Paragraph(new Run(recognizedText)));
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        bool isRecognizing = false;
        private void SpeechButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isRecognizing)
            {
                recognizer.RecognizeAsync(RecognizeMode.Multiple);
                richTextBoxContent.Document.Blocks.Add(new Paragraph(new Run("uiyu")));

                isRecognizing = true;
            }
            else
            {
                recognizer.RecognizeAsyncStop();
                isRecognizing = false;
            }
        }

        private void richTextBoxContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            int amountOfCharacters = new TextRange(richTextBoxContent.Document.ContentStart, richTextBoxContent.Document.ContentEnd).Text.Length;

            statusTextBlock.Text = $"Document length: {amountOfCharacters} characters";
        }

        private void BoldButton_Click(object sender, RoutedEventArgs e)
        {
            richTextBoxContent.Selection.ApplyPropertyValue(Inline.FontWeightProperty, FontWeights.Bold);

        }
    }
}
