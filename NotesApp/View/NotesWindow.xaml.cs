using Microsoft.Win32;
using NotesApp.Model;
using NotesApp.View.UserControls;
using NotesApp.ViewModel;
using NotesApp.ViewModel.Commands;
using NotesApp.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;

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
            //recognizer.SetInputToDefaultAudioDevice();
            recognizer.SpeechRecognized += Recognizer_SpeechRecognized;

            var fontFamilies = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
            fontFamilyComboBox.ItemsSource = fontFamilies;
            /*ItemTemplate property of the fontFamilyComboBox is set to a new DataTemplate. 
             *The FrameworkElementFactory class is used to create a new TextBlock element, 
             *which is then used to display the font family name and set the font family of the text to the font family being displayed.*/
            fontFamilyComboBox.ItemTemplate = new DataTemplate();
            FrameworkElementFactory textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
            textBlockFactory.SetBinding(TextBlock.TextProperty, new Binding());
            textBlockFactory.SetBinding(TextBlock.FontFamilyProperty, new Binding());
            fontFamilyComboBox.ItemTemplate.VisualTree = textBlockFactory;

            List<double> fontSizes = new List<double>() { 8, 9, 10, 11, 12, 14, 16, 18, 24, 28, 32 };
            fontSizeComboBox.ItemsSource = fontSizes;
            NoteToolBar.IsEnabled = false;
            if (!string.IsNullOrEmpty(App.WelcomeMessagePath))
            {
                using (FileStream filestream = new FileStream(App.WelcomeMessagePath, FileMode.Open))
                {
                    var contents = new TextRange(richTextBoxContent.Document.ContentStart, richTextBoxContent.Document.ContentEnd);
                    contents.Load(filestream, DataFormats.XamlPackage);
                    filestream.Close();
                };
            }
            richTextBoxContent.Focusable = false;
            statusTextBlock.Visibility = Visibility.Hidden;
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            if (!string.IsNullOrEmpty(App.UserId))
            {
                viewModel.GetNotebooks();
                UserNameWelcome.Text = App.UserName;

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
                        contents.Load(filestream, DataFormats.XamlPackage);
                        filestream.Close();
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
            System.Windows.Application.Current.Shutdown();
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
                richTextBoxContent.Selection.ApplyPropertyValue(Inline.FontStyleProperty, FontStyles.Normal);
            }

        }

        private void underlineButton_Click(object sender, RoutedEventArgs e)
        {
            bool isButtonChecked = ((ToggleButton)sender).IsChecked ?? false;

            if (isButtonChecked)
            {
                richTextBoxContent.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Underline);
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

            using (FileStream fileStream = new FileStream(rtfFile, FileMode.Create))
            {
                var contents = new TextRange(richTextBoxContent.Document.ContentStart, richTextBoxContent.Document.ContentEnd);
                contents.Save(fileStream, DataFormats.XamlPackage);
                fileStream.Close();
            }
        }
        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        private void OnNotebookMouseDown(object sender, MouseButtonEventArgs e)
        {
            NotebookControl listItem = (NotebookControl)sender;
            listItem.EditNotebookButton.Visibility = Visibility.Visible;
            listItem.DeleteNotebookButton.Visibility = Visibility.Visible;

            listItem.EditNotebookButton.Click += new RoutedEventHandler(EditActionsAssign);
            listItem.DeleteNotebookButton.InputBindings.Add(new MouseBinding(viewModel.DeleteNotebookCommand, new MouseGesture(MouseAction.LeftClick)) { CommandParameter = listItem.DataContext });
            //listItem.DeleteNotebookButton.Command = viewModel.DeleteNotebookCommand;

            var notebookControls = FindNotebookControls(NotebooksListView);

            foreach (NotebookControl item in notebookControls)
            {
                if (item != listItem)
                {
                    Button otherEditButton = (Button)item.FindName("EditNotebookButton");
                    Button otherDeleteButton = (Button)item.FindName("DeleteNotebookButton");
                    otherDeleteButton.Visibility = Visibility.Hidden;
                    otherEditButton.Visibility = Visibility.Hidden;
                }
            }

            NotesListView.SelectedItems.Clear();
            CheckFocus();
        }

        private void EditActionsAssign(object sender, RoutedEventArgs e)
        {
            viewModel.StartEditing();
        }

        //private void DeleteActionsAssign(object sender, RoutedEventArgs e, Notebook notebook)
        //{

        //    if (MessageBox.Show("Do you want to delete this notebook?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
        //    {
        //        //do no stuff
        //    }
        //    else
        //    {
        //        viewModel.DeleteNotebook(notebook);
        //    }

        //}

        private List<NotebookControl> FindNotebookControls(DependencyObject parent)
        {
            var list = new List<NotebookControl> { };
            for (int count = 0; count < VisualTreeHelper.GetChildrenCount(parent); count++)
            {
                var child = VisualTreeHelper.GetChild(parent, count);
                if (child is NotebookControl)
                {
                    list.Add(child as NotebookControl);
                }
                list.AddRange(FindNotebookControls(child));
            }
            return list;
        }

        private void btnWindowState_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
        }

        private void alignLeftButton_Click(object sender, RoutedEventArgs e)
        {
            EditingCommands.AlignLeft.Execute(null, richTextBoxContent);
        }

        private void alignCenterButton_Click(object sender, RoutedEventArgs e)
        {
            EditingCommands.AlignCenter.Execute(null, richTextBoxContent);
        }

        private void alignJustifyButton_Click(object sender, RoutedEventArgs e)
        {
            EditingCommands.AlignJustify.Execute(null, richTextBoxContent);
        }

        private void alignRightButton_Click(object sender, RoutedEventArgs e)
        {
            EditingCommands.AlignRight.Execute(null, richTextBoxContent);
        }

        private void orderedListButton_Click(object sender, RoutedEventArgs e)
        {
            EditingCommands.ToggleNumbering.Execute(null, richTextBoxContent);
        }

        private void unorderedListButton_Click(object sender, RoutedEventArgs e)
        {
            EditingCommands.ToggleBullets.Execute(null, richTextBoxContent);
        }

        private void imageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.bmp;*.jpg;*.jpeg;*.png;*.gif;*.tiff;...";

            openFileDialog.DefaultExt = ".bmp";

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                //new InlineUIContainer
                //(new System.Windows.Controls.Image()
                //{
                //    Source = new BitmapImage(new Uri(filePath, UriKind.RelativeOrAbsolute)),
                //    Margin = new Thickness(60, 0, 60, 0)
                //}
                //, richTextBoxContent.CaretPosition);
                InlineUIContainer container = new InlineUIContainer();
                Border border = new Border()
                {
                    CornerRadius = new CornerRadius(10),
                    Background = new ImageBrush(new BitmapImage(new Uri(filePath, UriKind.RelativeOrAbsolute))) { Stretch = Stretch.UniformToFill},
                    Margin = new Thickness(60, 0, 60, 0),
                    Width = 600,
                    Height = 400
                };

                container.Child = border;
                richTextBoxContent.CaretPosition.InsertTextInRun("");
                richTextBoxContent.CaretPosition.Paragraph.Inlines.Add(container);
            }
        }

        private void subscriptButton_Click(object sender, RoutedEventArgs e)
        {
            if ((BaselineAlignment)richTextBoxContent.Selection.GetPropertyValue(Inline.BaselineAlignmentProperty) == BaselineAlignment.Subscript)
            {
                richTextBoxContent.Selection.ApplyPropertyValue(Inline.BaselineAlignmentProperty, BaselineAlignment.Baseline);
            }
            else
            {
                richTextBoxContent.Selection.ApplyPropertyValue(Inline.BaselineAlignmentProperty, BaselineAlignment.Subscript);
            }
        }

        private void superscriptButton_Click(object sender, RoutedEventArgs e)
        {
            if ((BaselineAlignment)richTextBoxContent.Selection.GetPropertyValue(Inline.BaselineAlignmentProperty) == BaselineAlignment.Superscript)
            {
                richTextBoxContent.Selection.ApplyPropertyValue(Inline.BaselineAlignmentProperty, BaselineAlignment.Baseline);
            }
            else
            {
                richTextBoxContent.Selection.ApplyPropertyValue(Inline.BaselineAlignmentProperty, BaselineAlignment.Superscript);
            }
        }

        private void NotesListView_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckFocus();
        }

        private void CheckFocus()
        {
            if (NotesListView.SelectedItems.Count == 0)
            {
                richTextBoxContent.IsEnabled = false;
            }
            else
            {
                richTextBoxContent.Focusable = true;
            }
        }
    }
}
