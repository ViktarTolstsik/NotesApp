﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace NotesApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string UserId = string.Empty;
        public static string UserName = string.Empty;
        public static string WelcomeMessagePath = System.IO.Path.Combine(Environment.CurrentDirectory, $"WelcomeMessage.rtf");
    }
}
