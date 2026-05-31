//
//============================================================
// AUTO-GENERATED TESTS. DO NOT EDIT MANUALLY.
// Source: spec/password.yaml
// Generator: gen_tests.py v1.0
// Generated at: 2026-05-31 19:52:12
//============================================================

using System;
using NUnit.Framework;
using Lab.Interfaces;
using Lab.Implementations.GenCode2;

namespace Tests
{
    [TestFixture]
    [Description("Автоматически сгенерированные тесты для Password")]
    public class PasswordTests_Generated
    {
        private IPassword _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new Password();
        }


        [Test(Description = "Пароль длиной 8 символов")]
        public void Password_Passwordlength_Parol_dlinoy_8_simvolov()
        {
            // Arrange
            // Ожидаемый результат: true
            
            // Act
            var result = _sut.Passwordlength("Password1");
            
            // Assert
            Assert.That(result, Is.True);
        }


        [Test(Description = "Пароль длиной менее 8 символов")]
        public void Password_Passwordlength_Parol_dlinoy_menee_8_simvolov()
        {
            // Arrange
            // Ожидаемый результат: false
            
            // Act
            var result = _sut.Passwordlength("Pass");
            
            // Assert
            Assert.That(result, Is.False);
        }


        [Test(Description = "Пароль null")]
        public void Password_Passwordlength_Parol_null()
        {
            // Arrange
            // Ожидаемый результат: false
            
            // Act
            var result = _sut.Passwordlength(null);
            
            // Assert
            Assert.That(result, Is.False);
        }


        [Test(Description = "Сложный пароль (буквы+цифры+символы)")]
        public void Password_DiversityOfSymbols_Slozhnyy_parol_bukvy_tsifry_simvoly()
        {
            // Arrange
            // Ожидаемый результат: true
            
            // Act
            var result = _sut.DiversityOfSymbols("P@ssw0rd!");
            
            // Assert
            Assert.That(result, Is.True);
        }


        [Test(Description = "Только буквы")]
        public void Password_DiversityOfSymbols_Tolko_bukvy()
        {
            // Arrange
            // Ожидаемый результат: false
            
            // Act
            var result = _sut.DiversityOfSymbols("OnlyLetters");
            
            // Assert
            Assert.That(result, Is.False);
        }


        [Test(Description = "Только цифры")]
        public void Password_DiversityOfSymbols_Tolko_tsifry()
        {
            // Arrange
            // Ожидаемый результат: false
            
            // Act
            var result = _sut.DiversityOfSymbols("12345678");
            
            // Assert
            Assert.That(result, Is.False);
        }


        [Test(Description = "Общий шаблон")]
        public void Password_DiversityOfSymbols_Obshchiy_shablon()
        {
            // Arrange
            // Ожидаемый результат: false
            
            // Act
            var result = _sut.DiversityOfSymbols("password123");
            
            // Assert
            Assert.That(result, Is.False);
        }


        [Test(Description = "Валидный пароль")]
        public void Password_HashPassword_Validnyy_parol()
        {
            // Arrange
            // Ожидаемый результат: непустая строка
            
            // Act
            var result = _sut.HashPassword("MySecureP@ss");
            
            // Assert
            Assert.That(result, Is.Not.Empty);
            Assert.That(result, Is.Not.Null);
        }


        [Test(Description = "Пустой пароль")]
        public void Password_HashPassword_Pustoy_parol()
        {
            // Arrange
            // Ожидаемый результат: ArgumentException
            
            // Act
            var result = _sut.HashPassword("");
            
            // Assert
            Assert.That(() => _sut.HashPassword(""), Throws.ArgumentException);
        }


        [Test(Description = "Успешная смена")]
        public void Password_ChangePassword_Uspeshnaya_smena()
        {
            // Arrange
            // Ожидаемый результат: без исключений
            
            // Act
            _sut.ChangePassword("OldP@ss1", "NewP@ss2");
            
            // Assert
            Assert.That(() => _sut.ChangePassword("OldP@ss1", "NewP@ss2"), Throws.Nothing);
        }


        [Test(Description = "Новый пароль короткий")]
        public void Password_ChangePassword_Novyy_parol_korotkiy()
        {
            // Arrange
            // Ожидаемый результат: ArgumentException
            
            // Act
            _sut.ChangePassword("OldP@ss1", "123");
            
            // Assert
            Assert.That(() => _sut.ChangePassword("OldP@ss1", "123"), Throws.ArgumentException);
        }


        [Test(Description = "Новый пароль слабый")]
        public void Password_ChangePassword_Novyy_parol_slabyy()
        {
            // Arrange
            // Ожидаемый результат: ArgumentException
            
            // Act
            _sut.ChangePassword("OldP@ss1", "weakpass");
            
            // Assert
            Assert.That(() => _sut.ChangePassword("OldP@ss1", "weakpass"), Throws.ArgumentException);
        }

    }
}
