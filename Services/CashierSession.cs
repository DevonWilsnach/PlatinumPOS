using PlatinumPOS.Models;
using System;

namespace PlatinumPOS.Services
{
    public class CashierSession
    {
        public Cashier? CurrentCashier { get; private set; }
        public bool IsLoggedIn => CurrentCashier != null;
        public bool IsManager => CurrentCashier?.Role == "Manager";

        public event Action? OnChange;

        public void Login(Cashier cashier)
        {
            CurrentCashier = cashier;
            NotifyStateChanged();
        }

        public void Logout()
        {
            CurrentCashier = null;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
