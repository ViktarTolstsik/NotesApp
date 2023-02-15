using NotesApp.Model;
using NotesApp.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NotesApp.ViewModel
{
    public class LoginVM : INotifyPropertyChanged
    {
		private bool isShowingRegister = false;
		private User user;

		public User	User
		{
			get { return user; }
			set 
			{ 
				user = value;
				OnPropertyChanged("User");
			}
		}

		private string username;

		public string Username
		{
			get { return username; }
			set 
			{ 
				username = value;
				User = new User 
				{ 
					Username= username,
					Password = this.Password,
                    Name = this.Name,
                    LastName = this.LastName,
                    ConfirmPassword = this.ConfirmPassword

                };
				OnPropertyChanged("Username");
			}
		}

		private string password;

		public string Password
		{
			get { return password; }
			set 
			{ 
				password = value;
                User = new User
                {
                    Password = password,
                    Username = this.Username
                };
                OnPropertyChanged("Password");
			}
		}



		private Visibility loginVis;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Visibility LoginVis
		{
			get { return loginVis; }
			set 
			{ 
				loginVis = value; 
				OnPropertyChanged("LoginVis");
			}
		}
		private Visibility registerVis;
        public Visibility RegisterVis
        {
            get { return registerVis; }
            set
            {
                registerVis = value;
                OnPropertyChanged("RegisterVis");
            }
        }

        public RegisterCommand RegisterCommand { get; set; }

		public LoginCommand LoginCommand { get; set; }
		public ShowRegisterCommand ShowRegisterCommand { get; set; }

		public LoginVM()
		{
			LoginVis = Visibility.Visible;
			RegisterVis = Visibility.Collapsed;

			RegisterCommand = new RegisterCommand(this);
			LoginCommand = new LoginCommand(this);
			ShowRegisterCommand = new ShowRegisterCommand(this);

			User = new User();
		}

		public void SwitchViews()
		{
			isShowingRegister = !isShowingRegister;

			if(isShowingRegister)
			{
				RegisterVis = Visibility.Visible;
				LoginVis = Visibility.Collapsed; 
			}
			else
			{
                RegisterVis = Visibility.Collapsed;
                LoginVis = Visibility.Visible;
            }
		}

		public void Login()
		{
			//TODO
		}

        public void Register()
        {
            //TODO
        }

        private void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
