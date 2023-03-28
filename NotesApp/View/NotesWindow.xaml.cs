﻿using NotesApp.ViewModel;
using NotesApp.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Speech.Recognition;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        NotesVM viewModel;

        SpeechRecognitionEngine recognizer;
        public NotesWindow()
        {
            InitializeComponent();

            viewModel = Resources["vm"] as NotesVM;
            viewModel.SelectedNoteChanged += ViewModel_SelectedNoteChanged;

            var currentCulture = CultureInfo.GetCultureInfo("en-US");
            recognizer = new SpeechRecognitionEngine(currentCulture);

            GrammarBuilder builder = new GrammarBuilder();
            builder.Culture = currentCulture;
            builder.AppendDictation();
            Grammar grammar = new Grammar(builder);

            recognizer.LoadGrammar(grammar);
            recognizer.SetInputToDefaultAudioDevice();
            recognizer.SpeechRecognized += Recognizer_SpeechRecognized;

            var fontFamilies = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
            fontFamilyComboBox.ItemsSource = fontFamilies;
            //Bings answer
            /*In the above example, the ItemTemplate property of the fontFamilyComboBox is set to a new DataTemplate. 
             *The FrameworkElementFactory class is used to create a new TextBlock element, 
             *which is then used to display the font family name and set the font family of the text to the font family being displayed.*/
            fontFamilyComboBox.ItemTemplate = new DataTemplate();
            FrameworkElementFactory textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
            textBlockFactory.SetBinding(TextBlock.TextProperty, new Binding());
            textBlockFactory.SetBinding(TextBlock.FontFamilyProperty, new Binding());
            fontFamilyComboBox.ItemTemplate.VisualTree = textBlockFactory;
            //

            List<double> fontSizes = new List<double>() { 8, 9, 10, 11, 12, 14, 16, 18, 24, 28, 32 };
            fontSizeComboBox.ItemsSource = fontSizes;
            NoteToolBar.IsEnabled = false;
            richTextBoxContent.AppendText("Welcome to Noteify!");
            TextRange textRange = new TextRange(richTextBoxContent.Document.ContentStart, richTextBoxContent.Document.ContentEnd);
            textRange.ApplyPropertyValue(TextElement.FontSizeProperty, fontSizes[9]);
            textRange.ApplyPropertyValue(Paragraph.TextAlignmentProperty, TextAlignment.Center);
            richTextBoxContent.IsEnabled = false;
            statusTextBlock.Visibility = Visibility.Hidden;
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            if (!string.IsNullOrEmpty(App.UserId))
            {
                viewModel.GetNotebooks();
            }
        }

        private void ViewModel_SelectedNoteChanged(object? sender, EventArgs e)
        {
            richTextBoxContent.Document.Blocks.Clear();
            if (viewModel.SelectedNote != null)
            {
                if (!string.IsNullOrEmpty(viewModel.SelectedNote.FileLocation))
                {
                    using (FileStream filestream = new FileStream(viewModel.SelectedNote.FileLocation, FileMode.Open))
                    {
                        var contents = new TextRange(richTextBoxContent.Document.ContentStart, richTextBoxContent.Document.ContentEnd);
                        contents.Load(filestream, DataFormats.Rtf);
                    };

                }
                NoteToolBar.IsEnabled = true;
                richTextBoxContent.IsEnabled = true;
                statusTextBlock.Visibility = Visibility.Visible;
            }
            
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
            bool isButtonChecked = ((ToggleButton)sender).IsChecked ?? false;

            if (isButtonChecked)
            {
                richTextBoxContent.Selection.ApplyPropertyValue(Inline.FontWeightProperty, FontWeights.Bold);
            }
            else
            {
                richTextBoxContent.Selection.ApplyPropertyValue(Inline.FontWeightProperty, FontWeights.Normal);
            }
        }

        private void richTextBoxContent_SelectionChanged(object sender, RoutedEventArgs e)
        {
            var selectedWeight = richTextBoxContent.Selection.GetPropertyValue(FontWeightProperty);
            boldButton.IsChecked = (selectedWeight != DependencyProperty.UnsetValue) && selectedWeight.Equals(FontWeights.Bold);
           
            var selectedStyle = richTextBoxContent.Selection.GetPropertyValue(FontStyleProperty);
            italicButton.IsChecked = (selectedStyle != DependencyProperty.UnsetValue) && selectedStyle.Equals(FontStyles.Italic);
           
            var selectedDecorations = richTextBoxContent.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
            underlineButton.IsChecked = (selectedDecorations != DependencyProperty.UnsetValue) && selectedDecorations.Equals(TextDecorations.Underline);

            fontFamilyComboBox.SelectedItem = richTextBoxContent.Selection.GetPropertyValue(Inline.FontFamilyProperty);
            fontSizeComboBox.Text = richTextBoxContent.Selection.GetPropertyValue(Inline.FontSizeProperty).ToString();
        }

        private void italicButton_Click(object sender, RoutedEventArgs e)
        {
            bool isButtonChecked = ((ToggleButton)sender).IsChecked ?? false;

            if (isButtonChecked)
            {
                richTextBoxContent.Selection.ApplyPropertyValue(Inline.FontStyleProperty, FontStyles.Italic);
            }
            else
            {
                EditingCommands.ToggleBullets.Execute(null, richTextBoxContent);
                richTextBoxContent.Selection.ApplyPropertyValue(Inline.FontStyleProperty, FontStyles.Normal);
            }

        }

        private void underlineButton_Click(object sender, RoutedEventArgs e)
        {
            bool isButtonChecked = ((ToggleButton)sender).IsChecked ?? false;

            if (isButtonChecked)
            {
                richTextBoxContent.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Underline);
                //EditingCommands.ToggleBullets.Execute(null, richTextBoxContent); BULLET LIST WORKS
            }
            else
            {
                richTextBoxContent.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, null);
            }

        }

        private void fontFamilyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (fontFamilyComboBox.SelectedItem != null)
            {
                richTextBoxContent.Selection.ApplyPropertyValue(Inline.FontFamilyProperty, fontFamilyComboBox.SelectedItem);
            }
        }

        private void fontSizeComboBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(richTextBoxContent.Selection.Text))
            {
                richTextBoxContent.Selection.ApplyPropertyValue(Inline.FontSizeProperty, fontSizeComboBox.Text);
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string rtfFile = System.IO.Path.Combine(Environment.CurrentDirectory, $"{viewModel.SelectedNote.Id}.rtf");
            viewModel.SelectedNote.FileLocation = rtfFile;
            await DatabaseHelper.Update(viewModel.SelectedNote);

            FileStream fileStream = new FileStream(rtfFile, FileMode.Create);
            var contents = new TextRange(richTextBoxContent.Document.ContentStart, richTextBoxContent.Document.ContentEnd);
            contents.Save(fileStream, DataFormats.Rtf);
        }
        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
