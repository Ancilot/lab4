using Lab.Interfaces;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Lab.Implementations.GenCode3;

public class Password : IPassword
{
    private string _currentPasswordHash;

    public bool Passwordlength(string password)
    {
        return !string.IsNullOrEmpty(password) && password.Length >= 8;
    }

    public bool DiversityOfSymbols(string password)
    {
        if (string.IsNullOrEmpty(password))
            return false;

        // Проверка на наличие разных типов символов
        bool hasUpper = password.Any(char.IsUpper);
        bool hasLower = password.Any(char.IsLower);
        bool hasDigit = password.Any(char.IsDigit);
        bool hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

        // Проверка на запрещённые шаблоны
        string[] forbiddenPatterns = { "password", "qwerty", "abc123", "12345678" };
        bool hasForbiddenPattern = forbiddenPatterns.Any(pattern =>
            password.ToLower().Contains(pattern));

        return hasUpper && hasLower && hasDigit && hasSpecial && !hasForbiddenPattern;
    }

    public string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }

    public void ChangePassword(string oldPassword, string newPassword)
    {
        // Проверяем, что старый пароль корректен (если уже есть установленный пароль)
        if (!string.IsNullOrEmpty(_currentPasswordHash))
        {
            var oldPasswordHash = HashPassword(oldPassword);
            if (oldPasswordHash != _currentPasswordHash)
                throw new ArgumentException("Неверный текущий пароль.");
        }

        // Валидация нового пароля
        if (!Passwordlength(newPassword))
            throw new ArgumentException("Пароль должен содержать не менее 8 символов.");

        if (!DiversityOfSymbols(newPassword))
            throw new ArgumentException(
                "Пароль должен содержать заглавные и строчные буквы, цифры и специальные символы. " +
                "Избегайте распространённых шаблонов (например, 'password', 'qwerty').");

        // Устанавливаем новый пароль (сохраняем его хэш)
        _currentPasswordHash = HashPassword(newPassword);
    }
}