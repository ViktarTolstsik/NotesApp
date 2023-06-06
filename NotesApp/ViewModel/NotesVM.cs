using NotesApp.Model;
using NotesApp.ViewModel.Commands;
using NotesApp.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Xps;

namespace NotesApp.ViewModel
{
	public class NotesVM : INotifyPropertyChanged
	{
		public ObservableCollection<Notebook> Notebooks { get; set; }
		private Notebook selectedNotebook;

        public Notebook SelectedNotebook
		{
			get { return selectedNotebook; }
			set
			{
				selectedNotebook = value;
				OnPropertyChanged("SelectedNotebook");
				GetNotes();
			}
		}

		private Note selectedNote;

		public Note SelectedNote
		{
			get { return selectedNote; }
			set 
			{ 
				selectedNote = value;
                OnPropertyChanged("SelectedNote");
				SelectedNoteChanged?.Invoke(this, new EventArgs());
            }
		}


		private Visibility	isVisible;
		public Visibility IsVisible
		{
			get { return isVisible; }
			set 
			{
				isVisible = value;
                OnPropertyChanged("IsVisible");
            }
		}

		private Visibility altVis;

		public Visibility AltVis
        {
			get { return altVis; }
			set { altVis = value;
                OnPropertyChanged("AltVis");
            }
		}


		public ObservableCollection<Note> Notes { get; set; }
		public NewNotebookCommand NewNotebookCommand { get; set; }
		public NewNoteCommand NewNoteCommand { get; set; }
		public EditCommand EditCommand { get; set; }
		public EndEditingCommand EndEditingCommand { get; set; }

		public event PropertyChangedEventHandler? PropertyChanged;
		public event EventHandler SelectedNoteChanged;

        public NotesVM()
		{
			NewNoteCommand = new NewNoteCommand(this);
			NewNotebookCommand = new NewNotebookCommand(this);
			EditCommand = new EditCommand(this);
			EndEditingCommand = new EndEditingCommand(this);

			Notebooks = new ObservableCollection<Notebook>();
			Notes = new ObservableCollection<Note>();

			IsVisible = Visibility.Collapsed;

			GetNotebooks();
		}

        public async void CreateNotebook()
        {
			Notebook newNotebook = new()
			{
				Name = "New notebook",
				UserId = App.UserId
            };

            await DatabaseHelper.Insert(newNotebook);

			GetNotebooks();
        }

        public async void CreateNote(string notebookId)
		{
			Note newNote = new()
			{
				NotebookId = notebookId,
				CreatedAt = DateTime.Now,
				UpdatedAt = DateTime.Now,
				Title = $"New note for {DateOnly.FromDateTime(DateTime.Now)}"
			};

			await DatabaseHelper.Insert(newNote);

			GetNotes();
		}

		public async void GetNotebooks()
		{
			var notebooks = (await DatabaseHelper.Read<Notebook>()).Where(n => n.UserId == App.UserId).ToList();

			Notebooks.Clear();
			foreach(var notebook in notebooks) 
			{
				Notebooks.Add(notebook);
			}
		}

        private async void GetNotes()
        {
			if (SelectedNotebook != null)
			{
				var notes = (await DatabaseHelper.Read<Note>()).Where(n => n.NotebookId == SelectedNotebook.Id).ToList();

				Notes.Clear();
				foreach (var note in notes)
				{
					Notes.Add(note);
				}
			}
        }
		private void OnPropertyChanged(string propertyName) 
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		public void StartEditing()
		{
			IsVisible = Visibility.Visible;
			AltVis = Visibility.Collapsed;
		}
        public async void StopEditing(Notebook notebook)
        {
            IsVisible = Visibility.Collapsed;
			await DatabaseHelper.Update(notebook);
			GetNotebooks();
            AltVis = Visibility.Visible;
        }
    }
}
