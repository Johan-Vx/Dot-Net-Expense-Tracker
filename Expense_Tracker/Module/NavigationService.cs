using System;

namespace Expense_Tracker.Module
{
    public static class NavigationService
    {
        public static Action<BaseViewModel> NavigateToViewModel { get; set; }

        public static void NavigateTo(BaseViewModel viewModel)
        {
            NavigateToViewModel?.Invoke(viewModel);
        }
    }
}
