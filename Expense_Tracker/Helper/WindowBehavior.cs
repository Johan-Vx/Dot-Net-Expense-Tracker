using System.Windows;

namespace Expense_Tracker.Behavior
{
    public static class WindowBehavior
    {
        public static readonly DependencyProperty DialogResultProperty =
            DependencyProperty.RegisterAttached(
                "DialogResult",
                typeof(bool?),
                typeof(WindowBehavior),
                new PropertyMetadata(null, OnDialogResultChanged));
        public static void SetDialogResult(DependencyObject d, bool? value)
        {
            d.SetValue(DialogResultProperty, value);
        }
        public static bool? GetDialogResult(DependencyObject d)
        {
            return (bool?)d.GetValue(DialogResultProperty);
        }
        private static void OnDialogResultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window)
            {
                if (e.NewValue != null)
                {
                    try
                    {
                        window.DialogResult = e.NewValue as bool?;
                    }
                    catch
                    {
                        window.Close();
                    }
                }
            }
        }
    }
}