using DopplerHunter.Models;
using System.Runtime.CompilerServices;

namespace DopplerHunter.ViewModels
{
    /// <summary>
    /// Clase base para todos los ViewModels con soporte para INotifyPropertyChanged.
    /// </summary>
    public abstract class BaseViewModel : NotificationPropertiesBase
    {

        /// <summary>
        /// Establece un valor en una propiedad y notifica si cambió.
        /// </summary>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName!);
            return true;
        }
    }
}
