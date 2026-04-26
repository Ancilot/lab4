namespace Lab.Interfaces;

public interface IPassword
{
    bool Passwordlength(string password); //Длина пороля
    bool DiversityOfSymbols(string password); //Сложность пароля
    string HashPassword(string password); //Хэширование
    void ChangePassword(string oldPassword, string newPassword); //Создание нового пароля
}