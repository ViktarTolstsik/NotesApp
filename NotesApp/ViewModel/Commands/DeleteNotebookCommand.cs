using NotesApp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NotesApp.ViewModel.Commands
{
    public class DeleteNotebookCommand : ICommand
    {
        public NotesVM VM { get; set; }
        public event EventHandler? CanExecuteChanged;

        public DeleteNotebookCommand(NotesVM vm)
        {
            VM = vm;
        }
        public bool CanExecute(object? parameter)
        {
            Notebook selectedNotebook = parameter as Notebook;

            if (MessageBox.Show("Delete this notebook?","Attention", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.No && selectedNotebook != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Execute(object? parameter)
        {
            VM.DeleteNotebook(parameter as Notebook);
        }
    }
}
