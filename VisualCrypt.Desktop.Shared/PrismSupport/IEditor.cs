﻿using System.Windows;
using System.Windows.Controls;

namespace VisualCrypt.Desktop.Shared.PrismSupport
{
	public interface IEditor
	{
		TextBox TextBox1 { get; }

		TextBox TextBoxFind { get; }

		TextBox TextBoxFindReplace { get; }

		TextBox TextBoxGoTo { get; }

		UIElement EditorControl { get; }
	}
}