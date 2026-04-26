using System;
using System.Text.RegularExpressions;
using Lab.Interfaces;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Lab.Implementations.GenCode1;

public class Password : IPassword
{
    // Хранит хэш текущего пароля
    private string _hashedPassword;

    // Проверка длины пароля (минимум 8 символов)
    public bool Passwordlength(string password)
    {
        if (string.IsNullOrEmpty(password))
            return false;

        return password.Length >= 8;
    }

    // Проверка сложности пароля
    public bool DiversityOfSymbols(string password)
    {
        if (string.IsNullOrEmpty(password))
            return false;

        bool hasUpper = password.Any(char.IsUpper);
        bool hasLower = password.Any(char.IsLower);
        bool hasDigit = password.Any(char.IsDigit);
        bool hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

        // Общие шаблоны пароля, которых избегаем
        string[] commonPatterns = { "password", "qwerty", "abc123", "123456", "admin" };
        bool containsCommon = commonPatterns.Any(p => password.ToLower().Contains(p));

        return hasUpper && hasLower && hasDigit && hasSpecial && !containsCommon;
    }

    // Хэширование пароля через SHA256
    public string HashPassword(string password)
    {
        using SHA256 sha256 = SHA256.Create();
        byte[] bytes = Encoding.UTF8.GetBytes(password);
        byte[] hash = sha256.ComputeHash(bytes);

        StringBuilder builder = new StringBuilder();
        foreach (byte b in hash)
            builder.Append(b.ToString("x2"));

        return builder.ToString();
    }

    // Изменение пароля
    public void ChangePassword(string oldPassword, string newPassword)
    {
        // Если пароль ещё не установлен
        if (_hashedPassword == null)
        {
            if (!Passwordlength(newPassword) || !DiversityOfSymbols(newPassword))
            {
                Console.WriteLine("Новый пароль не соответствует требованиям безопасности.");
                return;
            }

            _hashedPassword = HashPassword(newPassword);
            Console.WriteLine("Пароль создан.");
            return;
        }

        // Проверяем старый пароль
        string oldHash = HashPassword(oldPassword);
        if (_hashedPassword != oldHash)
        {
            Console.WriteLine("Старый пароль неверный.");
            return;
        }

        // Проверяем новый пароль
        if (!Passwordlength(newPassword) || !DiversityOfSymbols(newPassword))
        {
            Console.WriteLine("Новый пароль не соответствует требованиям безопасности.");
            return;
        }

        _hashedPassword = HashPassword(newPassword);
        Console.WriteLine("Пароль успешно изменён.");
    }
}