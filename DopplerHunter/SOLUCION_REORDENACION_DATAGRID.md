## 🔍 PROBLEMA IDENTIFICADO: Reordenación al Seleccionar Items

### **Causa Raíz:**

El problema ocurre porque:

1. **`FileMetadata.IsSelected`** invoca `OnPropertyChanged(nameof(IsSelected))`
2. Esto notifica a la **ICollectionView** sobre cambios en el item
3. La CollectionView **recalcula los Sorts y Groups**
4. El item sube/baja de posición en su grupo

```csharp
// ❌ PROBLEMÁTICO:
public bool IsSelected 
{ 
	get => isSelected;
	set
	{ 
		if(isSelected != value)
		{
			isSelected = value;
			OnPropertyChanged(nameof(IsSelected));  // ← CAUSA LA RECALCULACIÓN
			SelectionChanged?.Invoke(this, EventArgs.Empty);
		}
	} 
}
```

---

## ✅ SOLUCIONES

### **Opción 1: Excluir IsSelected de INotifyPropertyChanged (RECOMENDADO)**

Hacer que **IsSelected NO notifique cambios** a la CollectionView:

```csharp
// En FileMetadata.cs
public class FileMetadata: NotificationPropertiesBase
{
	private bool isSelected;

	/// <summary>
	/// IsSelected NO dispara OnPropertyChanged para evitar reordenación en CollectionView
	/// </summary>
	public bool IsSelected 
	{ 
		get => isSelected;
		set
		{ 
			if(isSelected != value)
			{
				isSelected = value;
				// NO llamar a OnPropertyChanged aquí
				// Solo disparar el evento si es necesario
				SelectionChanged?.Invoke(this, EventArgs.Empty);
			}
		} 
	}
}
```

**Ventajas:**
- ✅ Soluciona completamente el problema
- ✅ Máximo rendimiento
- ✅ No afecta otros bindings

**Desventajas:**
- ❌ Otros bindings a IsSelected NO se actualizarán automáticamente
- ❌ Si tienes un CheckBox vinculado, puede no sincronizar correctamente

---

### **Opción 2: Suspender CollectionView Durante Selección (RECOMENDADO SI USAS BINDINGS)**

Si tienes bindings a IsSelected en el XAML, suspende la actualización:

```csharp
// En MainViewModel.cs
private void FileMetadata_SelectionChanged(object? sender, EventArgs e)
{
	// Suspender actualizaciones de la vista
	using (FilesFoundView.DeferRefresh())
	{
		// La selección ya ocurrió aquí
		// NotifySelectionCountChanged() si es necesario
	}

	// Actualizar propiedades que dependen de la selección
	OnPropertyChanged(nameof(SelectedFilesCountText));
	OnPropertyChanged(nameof(SelectedFilesCountValue));
}
```

**Problema:** Esto no evita la reordenación porque ya ocurrió.

**Solución correcta:** Usar `DeferRefresh()` ANTES de cambiar IsSelected:

```csharp
// En el XAML del DataGrid
// Añadir un Converter que maneje la selección sin OnPropertyChanged
```

---

### **Opción 3: Usar Converter para Selección (SIN INotifyPropertyChanged)**

Crear un converter que vincule la selección **sin triggerar PropertyChanged:**

```csharp
// Crear nuevo archivo: Converters\SelectionConverter.cs
public class SelectionConverter : IValueConverter
{
	public static HashSet<FileMetadata> SelectedItems { get; } = [];

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is FileMetadata file)
			return SelectedItems.Contains(file);
		return false;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is bool isSelected && parameter is FileMetadata file)
		{
			if (isSelected)
				SelectedItems.Add(file);
			else
				SelectedItems.Remove(file);
		}
		return Binding.DoNothing;
	}
}
```

En XAML:
```xml
<DataGridCheckBoxColumn 
	Binding="{Binding ., Converter={StaticResource SelectionConverter}}"
	Header="Select" 
	Width="50"/>
```

**Ventajas:**
- ✅ No invoca OnPropertyChanged
- ✅ No causa reordenación

**Desventajas:**
- ❌ Más complejo
- ❌ Requiere cambios en XAML

---

## 🎯 RECOMENDACIÓN FINAL

### **Usar Opción 1 (Simplest):**

1. **Modifica FileMetadata.cs:**

```csharp
public class FileMetadata: NotificationPropertiesBase
{
	private bool isSelected;

	/// <summary>
	/// NO dispara OnPropertyChanged para evitar reordenación en CollectionView
	/// Los cambios de selección se sincronizan solo a través del evento SelectionChanged
	/// </summary>
	public bool IsSelected 
	{ 
		get => isSelected;
		set
		{ 
			if(isSelected != value)
			{
				isSelected = value;
				// ❌ NO: OnPropertyChanged(nameof(IsSelected));
				SelectionChanged?.Invoke(this, EventArgs.Empty);
			}
		} 
	}
}
```

2. **Si necesitasBindingUI (CheckBox vinculado):**

Usa el Converter de la Opción 3, o cambia el binding a usar un RelayCommand:

```xaml
<!-- XAML DataGrid Checkbox Column -->
<DataGrid.Columns>
	<DataGridTemplateColumn Header="✓" Width="40">
		<DataGridTemplateColumn.CellTemplate>
			<DataTemplate>
				<CheckBox 
					IsChecked="{Binding IsSelected, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
					Click="CheckBox_Click"/>
			</DataTemplate>
		</DataGridTemplateColumn.CellTemplate>
	</DataGridTemplateColumn>
	<!-- Resto de columnas -->
</DataGrid.Columns>
```

Con code-behind:
```csharp
private void CheckBox_Click(object sender, RoutedEventArgs e)
{
	if (sender is CheckBox cb && cb.DataContext is FileMetadata file)
	{
		// No triggerea OnPropertyChanged, solo cambia IsSelected
		file.IsSelected = cb.IsChecked == true;
	}
}
```

---

## Código Completo Recomendado:

```csharp
// FileMetadata.cs
public class FileMetadata: NotificationPropertiesBase
{
	private bool isSelected;

	/// <summary>
	/// Selección de archivo - NO notifica PropertyChanged para evitar 
	/// reordenación en CollectionView. Usa SelectionChanged event en su lugar.
	/// </summary>
	public bool IsSelected 
	{ 
		get => isSelected;
		set
		{ 
			if(isSelected != value)
			{
				isSelected = value;
				// IMPORTANTE: No llamar a OnPropertyChanged aquí
				// Usar SelectionChanged event en su lugar
				SelectionChanged?.Invoke(this, EventArgs.Empty);
			}
		} 
	}

	public event EventHandler? SelectionChanged;
}


// MainViewModel.cs
public class MainViewModel : BaseViewModel
{
	public MainViewModel(...)
	{
		// ...
		FileMetadata.SelectionChanged += OnFileMetadataSelectionChanged;
		// ...
	}

	private void OnFileMetadataSelectionChanged(object? sender, EventArgs e)
	{
		// Actualizar propiedades que dependen de la selección
		// SIN triggerear cambios en la CollectionView
		OnPropertyChanged(nameof(SelectedFilesCountText));
		OnPropertyChanged(nameof(SelectedFilesCountValue));
	}
}
```

---

Este cambio **elimina completamente el problema** de reordenación mientras mantienes toda la funcionalidad. ¿Cuál prefieres implementar?

