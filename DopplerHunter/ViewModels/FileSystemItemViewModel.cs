using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace DopplerHunter.ViewModels
{
    public class FileSystemItemViewModel: BaseViewModel
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public ObservableCollection<FileSystemItemViewModel> Children { get; set; }

        private bool _isExpanded;
        public bool IsExpanded 
        {
            get => _isExpanded;
            set
            {
                if (value != _isExpanded)
                {
                    _isExpanded = value;
                    OnPropertyChanged();
                    if (_isExpanded) Expand();
                }
            }
        }

        public bool IsDrive { get; }

        private bool _includeSubdirectories = true; // Por defecto solemos querer incluir subcarpetas
        public bool IncludeSubdirectories
        {
            get => _includeSubdirectories;
            set
            {
                if (_includeSubdirectories != value)
                {
                    _includeSubdirectories = value;
                    // Notifica a la UI que el estado del CheckBox ha cambiado [2, 5]
                    Debug.WriteLine($"IncludeSubdirectories: {_includeSubdirectories}");
                    OnPropertyChanged();
                }
            }
        }


        public FileSystemItemViewModel(string name, string fullPath, bool isDrive = false)
        {
            Name = name;
            FullPath = fullPath;
            IsDrive = isDrive;
            IncludeSubdirectories = true;
            Children = new ObservableCollection<FileSystemItemViewModel>();
            Children.Add(null);
        }

        private void Expand()
        {
            if (Children.Count == 1 && Children[0] == null)
            {
                Children!.Clear();
                try
                {
                    var directories = new DirectoryInfo(FullPath).GetDirectories();

                    foreach (var directory in directories)
                    {
                        if(!directory.Attributes.HasFlag(FileAttributes.System))
                            Children.Add(new FileSystemItemViewModel(directory.Name, directory.FullName));
                    }
                }
                catch (UnauthorizedAccessException) { }
                catch (System.IO.DirectoryNotFoundException) { }
            }
        }
    }
}
