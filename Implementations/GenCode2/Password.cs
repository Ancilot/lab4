using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Lab.Interfaces;

namespace Lab.Implementations.GenCode2;

public class Password : IPassword
{
    // Минимальная длина пароля
    private const int MinPasswordLength = 8;

    // Список общих шаблонов паролей, которые следует избегать
    private readonly HashSet<string> _commonPatterns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "password", "qwerty", "abc123", "123456", "admin",
            "letmein", "welcome", "monkey", "dragon", "master",
            "hello", "freedom", "whatever", "qazwsx", "trustno1"
        };

    public bool Passwordlength(string password)
    {
        if (string.IsNullOrEmpty(password))
            return false;

        return password.Length >= MinPasswordLength;
    }

    public bool DiversityOfSymbols(string password)
    {
        if (string.IsNullOrEmpty(password))
            return false;

        // Проверка наличия различных типов символов
        bool hasUpperCase = password.Any(char.IsUpper);
        bool hasLowerCase = password.Any(char.IsLower);
        bool hasDigit = password.Any(char.IsDigit);
        bool hasSpecialChar = password.Any(ch => !char.IsLetterOrDigit(ch));

        // Проверка на общие шаблоны
        bool isCommonPattern = _commonPatterns.Contains(password.ToLower());

        // Проверка на последовательности (например, "abcdef", "123456")
        bool hasSequentialChars = HasSequentialCharacters(password);

        // Проверка на повторяющиеся символы
        bool hasRepeatingChars = HasRepeatingCharacters(password);

        // Пароль считается сложным, если:
        // 1. Содержит как минимум 3 из 4 типов символов
        // 2. Не является общим шаблоном
        // 3. Не содержит последовательных символов
        // 4. Не содержит повторяющихся символов

        int criteriaCount = 0;
        if (hasUpperCase) criteriaCount++;
        if (hasLowerCase) criteriaCount++;
        if (hasDigit) criteriaCount++;
        if (hasSpecialChar) criteriaCount++;

        return criteriaCount >= 3 &&
               !isCommonPattern &&
               !hasSequentialChars &&
               !hasRepeatingChars;
    }

    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Пароль не может быть пустым");

        // Используем современный алгоритм хэширования PBKDF2
        byte[] salt = GenerateSalt();
        byte[] hash = GenerateHash(password, salt);

        // Сохраняем соль вместе с хэшем для последующей проверки
        byte[] hashBytes = new byte[salt.Length + hash.Length];
        Array.Copy(salt, 0, hashBytes, 0, salt.Length);
        Array.Copy(hash, 0, hashBytes, salt.Length, hash.Length);

        return Convert.ToBase64String(hashBytes);
    }

    public void ChangePassword(string oldPassword, string newPassword)
    {
        if (string.IsNullOrEmpty(oldPassword))
            throw new ArgumentException("Старый пароль не может быть пустым");

        if (string.IsNullOrEmpty(newPassword))
            throw new ArgumentException("Новый пароль не может быть пустым");

        // Проверяем, что новый пароль соответствует требованиям
        if (!Passwordlength(newPassword))
            throw new ArgumentException($"Пароль должен содержать минимум {MinPasswordLength} символов");

        if (!DiversityOfSymbols(newPassword))
            throw new ArgumentException("Пароль недостаточно сложный. Используйте разные типы символов и избегайте общих шаблонов");

        // Проверяем, что новый пароль отличается от старого
        if (oldPassword == newPassword)
            throw new ArgumentException("Новый пароль должен отличаться от старого");

        // Здесь обычно происходит сохранение нового пароля в базе данных
        // и обновление хэша
        Console.WriteLine("Пароль успешно изменен");
    }

    // Вспомогательные методы

    private byte[] GenerateSalt()
    {
        byte[] salt = new byte[16]; // 128 бит
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }
        return salt;
    }

    private byte[] GenerateHash(string password, byte[] salt)
    {
        using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256))
        {
            return pbkdf2.GetBytes(32); // 256 бит
        }
    }

    private bool HasSequentialCharacters(string password)
    {
        string lowerPassword = password.ToLower();

        // Проверка на последовательные буквы (abc, bcd, etc.)
        for (int i = 0; i < lowerPassword.Length - 2; i++)
        {
            if (lowerPassword[i] + 1 == lowerPassword[i + 1] &&
                lowerPassword[i] + 2 == lowerPassword[i + 2])
            {
                if (char.IsLetter(lowerPassword[i]))
                    return true;
            }
        }

        // Проверка на последовательные цифры (123, 234, etc.)
        for (int i = 0; i < lowerPassword.Length - 2; i++)
        {
            if (char.IsDigit(lowerPassword[i]) &&
                lowerPassword[i] + 1 == lowerPassword[i + 1] &&
                lowerPassword[i] + 2 == lowerPassword[i + 2])
            {
                return true;
            }
        }

        // Проверка на последовательности на клавиатуре (qwerty, asdf, etc.)
        string[] keyboardSequences = { "qwerty", "asdfgh", "zxcvbn", "йцукен", "фывапр" };
        foreach (var sequence in keyboardSequences)
        {
            if (lowerPassword.Contains(sequence))
                return true;
        }

        return false;
    }

    private bool HasRepeatingCharacters(string password)
    {
        // Проверка на 3 и более повторяющихся символа подряд
        for (int i = 0; i < password.Length - 2; i++)
        {
            if (password[i] == password[i + 1] && password[i] == password[i + 2])
            {
                return true;
            }
        }
        return false;
    }

    // Дополнительный метод для проверки пароля (верификация)
    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
            return false;

        byte[] hashBytes = Convert.FromBase64String(hashedPassword);

        // Извлекаем соль (первые 16 байт)
        byte[] salt = new byte[16];
        Array.Copy(hashBytes, 0, salt, 0, 16);

        // Извлекаем оригинальный хэш (следующие 32 байта)
        byte[] originalHash = new byte[32];
        Array.Copy(hashBytes, 16, originalHash, 0, 32);

        // Вычисляем хэш для проверяемого пароля
        byte[] testHash = GenerateHash(password, salt);

        // Сравниваем хэши
        return originalHash.SequenceEqual(testHash);
    }
}