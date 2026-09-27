# 🔍 ANÁLISIS PROFUNDO - EL VERDADERO PROBLEMA

## El Problema Real:

El CheckBox vinculado a **IsSelected** está usando **TwoWay binding**, lo que significa:

```xaml
<CheckBox IsChecked="{Binding IsSelected, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"/>
```

**¿QUÉ SUCEDE?**

1. Haces clic en CheckBox del ID 1
2. El binding TwoWay actualiza `IsSelected = true`
3. Aunque IsSelected no dispara OnPropertyChanged
4. **El hecho de que el objeto cambió se detecta como un CAMBIO EN LA COLECCIÓN**
5. **WPF recompila el GroupDescription basándose en DuplicateIndex**
6. Si hay un DuplicateIndex que cambia o se recalcula, los items se reordenan

---

## La Verdadera Solución:

El problema está en cómo **WPF agrupa los items**. Necesitamos:

1. **Usar DeferRefresh** cuando la selección cambia
2. O **usar PropertyGroupDescription sin DuplicateIndex**
3. O **usar un Converter que maneje la selección sin binding**

### **Opción 1: Usar DeferRefresh (RECOMENDADO)**

En `MainViewModel.cs`, envuelve la notificación en DeferRefresh:

```csharp
private void FileMetadata_SelectionChanged(object? sender, EventArgs e)
{
	// PAUSAR actualizaciones de la vista
	using (FilesFoundView.DeferRefresh())
	{
		// Los cambios ocurren sin actualizar la vista
		// La selección ya cambió, pero la lista no se recompila
	}

	// Después de DeferRefresh, notificar solo lo necesario
	OnPropertyChanged(nameof(SelectedFilesCountText));
	OnPropertyChanged(nameof(SelectedFilesCountValue));
}
```

### **Opción 2: Cambiar el Binding del CheckBox (MÁS LIMPIO)**

En lugar de usar binding bidireccional, usar un comando:

```xaml
<DataGridTemplateColumn Header="✓" Width="40">
	<DataGridTemplateColumn.CellTemplate>
		<DataTemplate>
			<CheckBox 
				IsChecked="{Binding IsSelected, Mode=OneWay}"
				Click="SelectionCheckBox_Click"/>
		</DataTemplate>
	</DataGridTemplateColumn.CellTemplate>
</DataGridTemplateColumn>
```

En `MainWindow.xaml.cs`:

```csharp
private void SelectionCheckBox_Click(object sender, RoutedEventArgs e)
{
	if (sender is CheckBox cb && cb.DataContext is FileMetadata file)
	{
		// Pausar la CollectionView
		if (DataContext is MainViewModel vm)
		{
			using (vm.FilesFoundView.DeferRefresh())
			{
				file.IsSelected = cb.IsChecked == true;
			}
		}
	}
}
```

### **Opción 3: Usar Converter sin INotifyPropertyChanged (DEFINITIVO)**

Crear un StatelessConverter que maneje la selección:

```csharp
public class SelectionConverter : IValueConverter
{
	// Almacenar las selecciones sin INotifyPropertyChanged
	public static HashSet<FileMetadata> SelectedItems { get; } = new();

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return value is FileMetadata file && SelectedItems.Contains(file);
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return Binding.DoNothing; // NO actualizar IsSelected
	}
}
```

En XAML:

```xaml
<DataGridCheckBoxColumn 
	Binding="{Binding ., Converter={StaticResource SelectionConverter}}"
	Header="✓" 
	Width="40"/>
```

---

## ✅ RECOMENDACIÓN FINAL:

**Usar Opción 2 (DeferRefresh en Code-Behind)** porque:

- ✅ Es sencillo
- ✅ No requiere cambios en el DataGrid
- ✅ Soluciona el problema completamente
- ✅ Mantiene la sincronización bidireccional

---

## IMPLEMENTACIÓN PASO A PASO:

### Paso 1: Actualizar MainWindow.xaml - Localizar el CheckBox

Busca en el DataGrid la columna de CheckBox y añade el evento Click:

```xaml
<DataGridTemplateColumn Header="Select" Width="50">
	<DataGridTemplateColumn.CellTemplate>
		<DataTemplate>
			<CheckBox 
				IsChecked="{Binding IsSelected, Mode=TwoWay}"
				Click="SelectionCheckBox_Click"/>
		</DataTemplate>
	</DataGridTemplateColumn.CellTemplate>
</DataGridTemplateColumn>
```

### Paso 2: Actualizar MainWindow.xaml.cs

Añade el code-behind con DeferRefresh:

```csharp
public partial class MainWindow : Window
{
	public MainWindow()
	{
		InitializeComponent();
	}

	// Event handler para el CheckBox
	private void SelectionCheckBox_Click(object sender, RoutedEventArgs e)
	{
		if (sender is CheckBox cb && cb.DataContext is FileMetadata file)
		{
			if (DataContext is MainViewModel vm)
			{
				// PAUSAR la actualización de CollectionView
				using (vm.FilesFoundView.DeferRefresh())
				{
					// El binding ya actualizó IsSelected aquí
					file.IsSelected = cb.IsChecked == true;
				}
				// REANUDAR: La vista se actualiza UNA sola vez
			}
		}
	}
}
```

### Paso 3: Revertir los cambios anteriores en FileMetadata

```csharp
// Puedes restaurar OnPropertyChanged si lo necesitas
public bool IsSelected 
{ 
	get => isSelected;
	set
	{ 
		if(isSelected != value)
		{
			isSelected = value;
			OnPropertyChanged(nameof(IsSelected)); // OK ahora porque usamos DeferRefresh
			SelectionChanged?.Invoke(this, EventArgs.Empty);
		}
	} 
}
```

---

## ¿POR QUÉ FUNCIONA DeferRefresh?

```
Usuario Click en CheckBox #2
	↓
Binding actualiza IsSelected
	↓
DeferRefresh PAUSA CollectionView
	↓
Notificaciones ocurren sin recalcular GroupBy/Sort
	↓
DeferRefresh se cierra (using)
	↓
CollectionView se actualiza UNA SOLA VEZ
	↓
Todos los items en posición correcta ✓
```

---

Intenta con **Opción 2** primero. Si funciona, el problema está resuelto.
