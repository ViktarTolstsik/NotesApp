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

		public ObservableCollection<Note> Notes { get; set; }
		public NewNotebookCommand NewNotebookCommand { get; set; }
		public NewNoteCommand NewNoteCommand { get; set; }
		public EditCommand EditCommand { get; set; }
		public EndEditingCommand EndEditingCommand { get; set; }

		public event PropertyChangedEventHandler? PropertyChanged;

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

        public void CreateNotebook()
        {
            Notebook newNotebook = new Notebook()
            {
                Name = "New notebook"
            };

            DatabaseHelper.Insert(newNotebook);

			GetNotebooks();
        }

        public void CreateNote(int notebookId)
		{
			Note newNote = new Note()
			{
				NotebookId = notebookId,
				CreatedAt = DateTime.Now,
				UpdatedAt = DateTime.Now,
				Title = $"New note for {DateOnly.FromDateTime(DateTime.Now)}"
			};

			DatabaseHelper.Insert(newNote);

			GetNotes();
		}

		private void GetNotebooks()
		{
			var notebooks = DatabaseHelper.Read<Notebook>();

			Notebooks.Clear();
			foreach(var notebook in notebooks) 
			{
				Notebooks.Add(notebook);
			}
		}

        private void GetNotes()
        {
			if (SelectedNotebook != null)
			{
				var notes = DatabaseHelper.Read<Note>().Where(n => n.NotebookId == SelectedNotebook.Id).ToList();

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
		}
        public void StopEditing(Notebook notebook)
        {
            IsVisible = Visibility.Collapsed;
			DatabaseHelper.Update(notebook);
			GetNotebooks();
        }
    }
}
