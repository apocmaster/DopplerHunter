using System.Collections.ObjectModel;

namespace DopplerHunter.Extensions
{
    public static class ObservableCollectionExtensions
    {
        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
        {
            ArgumentNullException.ThrowIfNull(collection);
            if (items == null) return;

            foreach (var item in items)
            {  
                collection.Add(item); 
            }
        }
    }
}
