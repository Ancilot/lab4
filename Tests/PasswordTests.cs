using NUnit.Framework;
using Lab.Interfaces;
using System;
using Lab.Implementations.GenCode2;

namespace Tests;

[TestFixture]
public class PasswordTests
{
    private IPassword _password = new Password();


    // --------- Тесты "чёрного ящика" ---------

    //проверака на положительный результат от 8 символов
    [Test]
    public void Passwordlength_ReturnsTrue()
    {
        var result = _password.Passwordlength("PsssUsdf");
        Assert.That(result, Is.True);

    }

    //проверака на отрицательный результат при количестве символов меньше 8
    [Test]
    public void Passwordlength_ReturnsFalse()
    {
        var result = _password.Passwordlength("1e!");
        Assert.That(result, Is.False);

    }

    //проверка на null
    [Test]
    public void Passwordlength_FalseNull()
    {
        var result = _password.Passwordlength(null);
        Assert.That(result, Is.False);

    }

    //проверка на сложный пароль
    [Test]
    public void DiversityOfSymbols_ReturnTrue()
    {
        var result = _password.DiversityOfSymbols("!sdje340PdF");
        Assert.That(result, Is.True);

    }

    //проверка на пароль состояший из цифр
    [Test]
    public void DiversityOfSymbols_ReturnFalseNumber()
    {
        var result = _password.DiversityOfSymbols("12345678");
        Assert.That(result, Is.False);

    }

    //проверка на пароль состояший из букв
    [Test]
    public void DiversityOfSymbols_ReturnFalseLetters()
    {
        var result = _password.DiversityOfSymbols("asdfghjk");
        Assert.That(result, Is.False);

    }

    //проверка на пароль состояший из символов
    [Test]
    public void DiversityOfSymbols_ReturnFalseSimvols()
    {
        var result = _password.DiversityOfSymbols("!@#<?@!>");
        Assert.That(result, Is.False);

    }

    //проверка хэширования
    [Test]
    public void HashPassword_AreNotEqual()
    {
        string password1 = "MyPassword123";
        string password2 = "AnotherPassword456";

        string hash1 = _password.HashPassword(password1);
        string hash2 = _password.HashPassword(password2);

        // разные пароли должны давать разные хэши
        Assert.That(hash1, Is.Not.EqualTo(hash2));
    }

    [Test]
    public void HashPassword_IsNotEmpty()
    {
        string hash = _password.HashPassword("Pass!word03");
        Assert.That(hash, Is.Not.Empty);

    }

    [Test]
    public void HashPassword_NotNull()
    {
        string hash = _password.HashPassword("Pass!word03");
        Assert.That(hash, Is.Not.Null);
    }


    //Успешная смена пароля
    [Test]
    public void ChangePassword_ValidData_PasswordChanged()
    {
        string oldPassword = "OldPa05ss123!";
        string newPassword = "New200Pass456!";

        Assert.That(() => _password.ChangePassword(oldPassword, newPassword), Throws.Nothing);

    }

    //Неверный старый пароль
    [Test]
    public void ChangePassword_WrongOldPassword_NotChanged()
    {
        _password.ChangePassword("JNHBk123", "OldPass123!");

        Assert.That(() =>
        _password.ChangePassword("Wrong123!", "NewPass456!"),
        Throws.ArgumentException);
    }

    //Новый пароль слишком короткий
    [Test]
    public void ChangePassword_ShortPassword_NotChanged()
    {
        string oldPassword = "OldPass123!";
        string newPassword = "123";
        Assert.That(() =>
        _password.ChangePassword(oldPassword, newPassword),
        Throws.ArgumentException);
    }


    //Новый пароль не соответствует сложности
    [Test]
    public void ChangePassword_WeakPassword_NotChanged()
    {
        string oldPassword = "OldPass123!";
        string newPassword = "password";

        Assert.That(() =>
        _password.ChangePassword(oldPassword, newPassword),
        Throws.ArgumentException);
    }

    //// --------- тесты белого ящика ---------

    // Проверка на пустой старый пароль. Должно выдать ошибку
    [Test]
    public void ChangePassword_Exception()
    {
        Assert.That(() =>
         _password.ChangePassword("", "StrongPass1!"),
         Throws.ArgumentException);
    }
}