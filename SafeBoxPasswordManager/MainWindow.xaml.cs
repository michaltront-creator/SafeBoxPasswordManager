using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SafeBoxPasswordManager.Services;
using SafeBoxPasswordManager.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace SafeBoxPasswordManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MasterPasswordService _masterPasswordService = new MasterPasswordService();
        private readonly PasswordStorageService _passwordStorageService = new PasswordStorageService();
        private readonly TotpService _totpService = new TotpService();

        private List<PasswordEntry> _entries = new List<PasswordEntry>();
        private PasswordEntry? _editedEntry;




        public MainWindow()
        {
            InitializeComponent();
        }
        private void ShowVaultPanel()
        {
            _entries = _passwordStorageService.LoadEntries();
            RefreshEntriesList();

            LoginPanel.Visibility = Visibility.Collapsed;
            VaultPanel.Visibility = Visibility.Visible;
            MasterPasswordBox.Clear();
            StatusTextBlock.Text = "";
        }

        private void ShowLoginPanel()
        {
            VaultPanel.Visibility = Visibility.Collapsed;
            LoginPanel.Visibility = Visibility.Visible;
            MasterPasswordBox.Clear();
            StatusTextBlock.Text = "";
            StatusTextBlock.Foreground = Brushes.Red;
        }
        private void ShowTotpPanel()
        {
            LoginPanel.Visibility = Visibility.Collapsed;
            VaultPanel.Visibility = Visibility.Collapsed;
            TotpPanel.Visibility = Visibility.Visible;

            TotpCodeTextBox.Clear();
            TotpStatusTextBlock.Text = "";

            if (!_totpService.IsTotpConfigured())
            {
                string secret = _totpService.CreateSecret();

                TotpSecretLabel.Visibility = Visibility.Visible;
                TotpSecretTextBox.Visibility = Visibility.Visible;
                TotpSecretTextBox.Text = secret;

                TotpInfoTextBlock.Text = "Dodaj ten klucz ręcznie w Google Authenticator, a potem wpisz wygenerowany kod.";
            }
            else
            {
                TotpSecretLabel.Visibility = Visibility.Collapsed;
                TotpSecretTextBox.Visibility = Visibility.Collapsed;
                TotpSecretTextBox.Text = "";

                TotpInfoTextBlock.Text = "Wpisz kod z Google Authenticator.";
            }
        }

        private void VerifyTotpButton_Click(object sender, RoutedEventArgs e)
        {
            string code = TotpCodeTextBox.Text.Trim();

            if (_totpService.VerifyCode(code))
            {
                TotpPanel.Visibility = Visibility.Collapsed;
                ShowVaultPanel();
            }
            else
            {
                TotpStatusTextBlock.Foreground = Brushes.Red;
                TotpStatusTextBlock.Text = "Nieprawidłowy kod 2FA.";
            }
        }

        private void LockButton_Click(object sender, RoutedEventArgs e)
        {
            ShowLoginPanel();
        }
        private void AddEntryButton_Click(object sender, RoutedEventArgs e)
        {
            EntryStatusTextBlock.Foreground = Brushes.Red;

            string serviceName = ServiceNameTextBox.Text.Trim();
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordTextBox.Text.Trim();
            string url = UrlTextBox.Text.Trim();
            string notes = NotesTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(serviceName))
            {
                EntryStatusTextBlock.Text = "Podaj nazwę serwisu.";
                return;
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                EntryStatusTextBlock.Text = "Podaj login lub e-mail.";
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                EntryStatusTextBlock.Text = "Podaj hasło.";
                return;
            }

            if (_editedEntry == null)
            {
                PasswordEntry newEntry = new PasswordEntry
                {
                    ServiceName = serviceName,
                    Username = username,
                    Password = password,
                    Url = url,
                    Notes = notes
                };

                _entries.Add(newEntry);

                EntryStatusTextBlock.Foreground = Brushes.Green;
                EntryStatusTextBlock.Text = "Wpis został dodany.";
            }
            else
            {
                _editedEntry.ServiceName = serviceName;
                _editedEntry.Username = username;
                _editedEntry.Password = password;
                _editedEntry.Url = url;
                _editedEntry.Notes = notes;

                EntryStatusTextBlock.Foreground = Brushes.Green;
                EntryStatusTextBlock.Text = "Wpis został zaktualizowany.";
            }

            _passwordStorageService.SaveEntries(_entries);
            RefreshEntriesList();
            ResetEntryFormToAddMode();
        }
        private List<PasswordEntry> GetFilteredEntries()
        {
            string searchText = SearchTextBox.Text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                return _entries;
            }

            return _entries
                .Where(entry =>
                    entry.ServiceName.ToLower().Contains(searchText) ||
                    entry.Username.ToLower().Contains(searchText) ||
                    entry.Url.ToLower().Contains(searchText))
                .ToList();
        }
        private void RefreshEntriesList()
        {
            EntriesListBox.Items.Clear();

            foreach (PasswordEntry entry in GetFilteredEntries())
            {
                EntriesListBox.Items.Add(entry);
            }
        }
        private void SearchTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            RefreshEntriesList();
            ClearSelectedEntryDetails();
        }
        private void EntriesListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (EntriesListBox.SelectedItem is not PasswordEntry selectedEntry)
            {
                return;
            }
            _editedEntry = selectedEntry;

            ServiceNameTextBox.Text = selectedEntry.ServiceName;
            UsernameTextBox.Text = selectedEntry.Username;
            PasswordTextBox.Text = selectedEntry.Password;
            UrlTextBox.Text = selectedEntry.Url;
            NotesTextBox.Text = selectedEntry.Notes;

            AddEntryButton.Content = "Zapisz zmiany";

            SelectedServiceTextBlock.Text = selectedEntry.ServiceName;
            SelectedUsernameTextBlock.Text = selectedEntry.Username;
            SelectedPasswordTextBlock.Text = selectedEntry.Password;
            SelectedUrlTextBlock.Text = selectedEntry.Url;
            SelectedNotesTextBlock.Text = selectedEntry.Notes;
        }
        private void DeleteEntryButton_Click(object sender, RoutedEventArgs e)
        {
            if (EntriesListBox.SelectedItem is not PasswordEntry selectedEntry)
            {
                EntryStatusTextBlock.Foreground = Brushes.Red;
                EntryStatusTextBlock.Text = "Najpierw wybierz wpis do usunięcia.";
                return;
            }

            _entries.Remove(selectedEntry);
            _passwordStorageService.SaveEntries(_entries);

            RefreshEntriesList();
            ClearSelectedEntryDetails();

            EntryStatusTextBlock.Foreground = Brushes.Green;
            EntryStatusTextBlock.Text = "Wpis został usunięty.";
        }
        private void ClearSelectedEntryDetails()
        {
            SelectedServiceTextBlock.Text = "-";
            SelectedUsernameTextBlock.Text = "-";
            SelectedPasswordTextBlock.Text = "-";
            SelectedUrlTextBlock.Text = "-";
            SelectedNotesTextBlock.Text = "-";
        }


        private void ClearEntryForm()
        {
            ServiceNameTextBox.Clear();
            UsernameTextBox.Clear();
            PasswordTextBox.Clear();
            UrlTextBox.Clear();
            NotesTextBox.Clear();
        }
        private void ResetEntryFormToAddMode()
        {
            _editedEntry = null;
            EntriesListBox.SelectedItem = null;

            ClearEntryForm();
            ClearSelectedEntryDetails();

            AddEntryButton.Content = "Dodaj wpis";
            EntryStatusTextBlock.Text = "";
            EntryStatusTextBlock.Foreground = Brushes.Red;
        }

        private void NewEntryButton_Click(object sender, RoutedEventArgs e)
        {
            ResetEntryFormToAddMode();
        }
        private void GeneratePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            PasswordTextBox.Text = GenerateStrongPassword(16);
        }

        private string GenerateStrongPassword(int length)
        {
            const string lowercase = "abcdefghijklmnopqrstuvwxyz";
            const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digits = "0123456789";
            const string specialCharacters = "!@#$%^&*()-_=+[]{};:,.<>?";

            string allCharacters = lowercase + uppercase + digits + specialCharacters;

            char[] password = new char[length];

            password[0] = lowercase[RandomNumberGenerator.GetInt32(lowercase.Length)];
            password[1] = uppercase[RandomNumberGenerator.GetInt32(uppercase.Length)];
            password[2] = digits[RandomNumberGenerator.GetInt32(digits.Length)];
            password[3] = specialCharacters[RandomNumberGenerator.GetInt32(specialCharacters.Length)];

            for (int i = 4; i < length; i++)
            {
                password[i] = allCharacters[RandomNumberGenerator.GetInt32(allCharacters.Length)];
            }

            return new string(password.OrderBy(_ => RandomNumberGenerator.GetInt32(int.MaxValue)).ToArray());
        }
        private void UnlockButton_Click(object sender, RoutedEventArgs e)
        {
            string masterPassword = MasterPasswordBox.Password;

            StatusTextBlock.Foreground = Brushes.Red;

            if (string.IsNullOrWhiteSpace(masterPassword))
            {
                StatusTextBlock.Text = "Wpisz hasło główne.";
                return;
            }

            if (masterPassword.Length < 8)
            {
                StatusTextBlock.Text = "Hasło główne powinno mieć co najmniej 8 znaków.";
                return;
            }

            if (!_masterPasswordService.IsMasterPasswordSet())
            {
                _masterPasswordService.CreateMasterPassword(masterPassword);

                ShowTotpPanel();


                return;

            }

            if (_masterPasswordService.VerifyMasterPassword(masterPassword))
            {
                ShowTotpPanel();



            }
            else
            {
                StatusTextBlock.Text = "Nieprawidłowe hasło główne.";
            }
        }


    }
}
