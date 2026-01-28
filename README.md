# WrapPanel2D
A custom WrapPanel for Avalonia UI, which allows user to navigate the selection using keyboard, in all four directions.

## WrapPanel Limitation
In WrapPanel, you can navigate your selection left or right, but not up and down. WrapPanel2D removes this limitation.

## Adding Library
Import the nuget to your .NET project:
> https://www.nuget.org/packages/IND.NC.Avalonia.Controls

## Usage
Add the namespace declaration to XAML. Use it inside a collection control, like `ListBox`

```xml
...
  xmlns:controls="using:IND.NC.Avalonia.Controls"
...

<ListBox Name="listBox"
			ItemsSource="{Binding MyCollection}"
			SelectionMode="Single"
			SelectedItem="{Binding SelectedItem}"
			>
	<ListBox.ItemsPanel>
		<ItemsPanelTemplate>
			<!-- ==========   !!!! HERE!!!!   ========== -->
			<controls:WrapPanel2D />
		</ItemsPanelTemplate>
	</ListBox.ItemsPanel>

	<ListBox.ItemTemplate>
		<!-- ... -->
	</ListBox.ItemTemplate>

</ListBox>
```

